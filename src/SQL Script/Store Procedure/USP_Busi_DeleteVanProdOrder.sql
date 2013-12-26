SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_DeleteVanProdOrder') 
     DROP PROCEDURE USP_Busi_DeleteVanProdOrder
GO

CREATE PROCEDURE [dbo].[USP_Busi_DeleteVanProdOrder] 
(
	@TraceCode varchar(50),
	@CreateUserId int,
	@CreateUserNm varchar(50)
) --WITH ENCRYPTION
AS 
BEGIN 
	set nocount on
	declare @DateTimeNow datetime = GetDate()
	declare @ErrorMsg nvarchar(MAX)
	declare @trancount int = @@trancount
		
	begin try
		if @trancount = 0
		begin
            begin tran
        end
        
        Create table #tempOrderNo
		(
			RowId int identity(1, 1),
			OrderNo varchar(50),
			ProdLine varchar(50)
		)
		
		Create table #tempSeqGroup
		(
			SeqGroup varchar(50),
			[Version] int	
		)
        
        insert into #tempOrderNo(OrderNo, ProdLine)
		select OrderNo, ProdLine from ORD_OrderSeq where TraceCode = @TraceCode
		
		if not exists(select top 1 1 from #tempOrderNo)
		begin
			set @ErrorMsg = N'Van号' + @TraceCode + N'不存在。'
			RAISERROR(@ErrorMsg, 16, 1) 
		end
		
		if exists(select top 1 1 from ORD_OrderMstr_4 as mstr inner join #tempOrderNo as t on mstr.OrderNo = t.OrderNo where mstr.[Status] <> 1)
		begin
			set @ErrorMsg = N'Van号' + @TraceCode + N'的生产单已经上线，不能删除。'
			RAISERROR(@ErrorMsg, 16, 1) 
		end
		
		--已经计算过的JIT零件要退回BOM用量至工位溢出量中
		update orb set Qty = Qty + bom.OrderQty, [Version] = orb.[Version] + 1, 
		LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm
		from ORD_OrderBomDet as bom 
		inner join SCM_OpRefBalance as orb on bom.Item = orb.Item and bom.OpRef = orb.OpRef
		where bom.OrderNo in (select OrderNo from #tempOrderNo) and bom.IsCreateOrder = 1 and bom.IsCreateSeq <> 1
		
		--新增不存在的工位余量
		insert into SCM_OpRefBalance(Item, OpRef, Qty, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, [Version])
		select bom.Item, bom.OpRef, SUM(bom.OrderQty) as Qty, @CreateUserId, @CreateUserNm, @DateTimeNow, @CreateUserId, @CreateUserNm, @DateTimeNow, 1
		from ORD_OrderBomDet as bom WITH(NOLOCK) 
		left join SCM_OpRefBalance as orb WITH(NOLOCK) on orb.Item = bom.Item and orb.OpRef = bom.OpRef
		where bom.OrderNo in (select OrderNo from #tempOrderNo) and bom.IsCreateOrder = 1 and bom.IsCreateSeq <> 1 and orb.Id is null
		group by bom.Item, bom.OpRef
		
		--检查删除车是否排序组中记录的最后一台车，如果是要把排序组中的更新成最后一台车的前一台，避免排序单结转出错
		declare @OrderNoRowId int
		declare @OrderNoMaxRowId int
		select @OrderNoRowId = MIN(RowId), @OrderNoMaxRowId = MAX(RowId) from #tempOrderNo
		
		while (@OrderNoRowId <= @OrderNoMaxRowId)
		begin
			declare @OrderNo varchar(50)
			declare @ProdLine varchar(50)
			select @OrderNo = OrderNo, @ProdLine = ProdLine from #tempOrderNo where RowId = @OrderNoRowId
			
			if exists(select top 1 1 from SCM_SeqGroup where PrevOrderNo = @OrderNo)
			begin
				declare @Seq bigint
				declare @SubSeq int
				declare @PrevOrderNo varchar(50)
				declare @PrevTraceCode varchar(50)
				declare @PrevSeq bigint
				declare @PrevSubSeq int
				
				--查找排序组中最后一台车的前一台
				select @Seq = Seq, @SubSeq = SubSeq from ORD_OrderSeq where OrderNo = @OrderNo
				
				select top 1 @PrevOrderNo = OrderNo, @PrevTraceCode = TraceCode, @PrevSeq = Seq, @PrevSubSeq = SubSeq
				from ORD_OrderSeq where (Seq < @Seq or (Seq = @Seq and SubSeq < @SubSeq)) and ProdLine = @ProdLine 
				order by Seq desc, SubSeq desc
			
				truncate table #tempSeqGroup
				insert into #tempSeqGroup(SeqGroup, [Version])
				select Code, [Version] from SCM_SeqGroup where PrevOrderNo = @OrderNo
				
				if exists(select top 1 1 from #tempSeqGroup)
				begin
					update seq set PrevOrderNo = @PrevOrderNo, PrevTraceCode = @PrevTraceCode, PrevSeq = @PrevSeq, PrevSubSeq = @PrevSubSeq, [Version] = seq.[Version] + 1
					from SCM_SeqGroup as seq inner join #tempSeqGroup as tSeq on seq.Code = tSeq.SeqGroup and seq.[Version] = tSeq.[Version]
					
					if (@@ROWCOUNT <> (select COUNT(1) from #tempSeqGroup))
					begin
						set @ErrorMsg = N'整车生产单' + @OrderNo + N' Van号' + @TraceCode + N'已经更新，请重新删除。'
						RAISERROR(@ErrorMsg, 16, 1)
					end
				end
			end
			
			set @OrderNoRowId = @OrderNoRowId + 1
		end
		
		delete from CUST_CabOut where OrderNo in (select OrderNo from #tempOrderNo)
		delete from ORD_OrderItemTraceResult where OrderNo in (select OrderNo from #tempOrderNo)
		delete from ORD_OrderItemTrace where OrderNo in (select OrderNo from #tempOrderNo)
		delete from ORD_OrderOp where OrderNo in (select OrderNo from #tempOrderNo)
		delete from ORD_OrderBomDet where OrderNo in (select OrderNo from #tempOrderNo)
		delete from ORD_OrderSeq where OrderNo in (select OrderNo from #tempOrderNo)
		delete from ORD_OrderDet_4 where OrderNo in (select OrderNo from #tempOrderNo)
		delete from ORD_OrderMstr_4 where OrderNo in (select OrderNo from #tempOrderNo)
		
		insert into LOG_DeleteVanProdOrder(TraceCode, CreateUser, CreateUserNm, CreateDate) values(@TraceCode, @CreateUserId, @CreateUserNm, @DateTimeNow)
		
		drop table #tempOrderNo
		drop table #tempSeqGroup
		
		if @trancount = 0 
		begin  
			commit
		end
	end try
	begin catch
        if @trancount = 0
        begin
            rollback
        end 
		
		set @ErrorMsg = Error_Message() 
		RAISERROR(@ErrorMsg, 16, 1) 
	end catch
END 
