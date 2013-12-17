SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Busi_UpdateOpRefBalance4Subassembly')
	DROP PROCEDURE USP_Busi_UpdateOpRefBalance4Subassembly
GO

CREATE PROCEDURE [dbo].[USP_Busi_UpdateOpRefBalance4Subassembly]
(
	@Item varchar(50),
	@OpRef varchar(50),
	@ProdLine1 varchar(50),
	@TraceCode1 varchar(50),
	@ProdLine2 varchar(50),
	@TraceCode2 varchar(50),
	@InvQty decimal(18, 8),
	@CreateUserId int,
	@CreateUserNm varchar(100)
) --WITH ENCRYPTION
AS
BEGIN
	set nocount on
	Declare @ErrorMsg varchar(max)
	Declare @trancount int = @@trancount
	
	begin try
		if @trancount = 0
		begin
			begin tran
		end
			
		declare @Id int
		declare @Version int
		declare @Seq1 bigint
		declare @SubSeq1 int
		declare @PauseStatus1 tinyint
		declare @Seq2 bigint
		declare @SubSeq2 int
		declare @PauseStatus2 tinyint
		declare @TobeAssQty decimal(18, 8)
		declare @IntransitQty decimal(18, 8)
		
		select @Seq1 = seq.Seq, @SubSeq1 = seq.SubSeq, @PauseStatus1 = mstr.PauseStatus
		from ORD_OrderMstr_4 as mstr
		inner join ORD_OrderSeq as seq on seq.OrderNo = mstr.OrderNo
		where mstr.TraceCode = @TraceCode1 and mstr.Flow = @ProdLine1 --and bom.IsCreateOrder = 1
		
		if @Seq1 is null
		begin
			if not exists(select top 1 1 from ORD_OrderSeq where TraceCode = @TraceCode1)
			begin
				set @ErrorMsg = N'Van号' + @TraceCode1 + N'不存在。'
				RAISERROR(@ErrorMsg, 16, 1) 
			end
			else
			begin
				set @ErrorMsg = N'整车生产线' + @ProdLine1 + N'没有Van号' + @TraceCode1 + N'的生产单。'
				RAISERROR(@ErrorMsg, 16, 1) 
			end
		end
		
		select @Seq2 = seq.Seq, @SubSeq2 = seq.SubSeq, @PauseStatus2 = mstr.PauseStatus
		from ORD_OrderMstr_4 as mstr
		inner join ORD_OrderSeq as seq on seq.OrderNo = mstr.OrderNo
		where mstr.TraceCode = @TraceCode2 and mstr.Flow = @ProdLine2 --and bom.IsCreateOrder = 1
		
		if @Seq2 is null
		begin
			if not exists(select top 1 1 from ORD_OrderSeq where TraceCode = @TraceCode2)
			begin
				set @ErrorMsg = N'Van号' + @TraceCode2 + N'不存在。'
				RAISERROR(@ErrorMsg, 16, 1) 
			end
			else
			begin
				set @ErrorMsg = N'整车生产线' + @ProdLine2 + N'没有Van号' + @TraceCode2 + N'的生产单。'
				RAISERROR(@ErrorMsg, 16, 1) 
			end
		end
		
		select top 1 @Id = Id, @Version = [Version] from SCM_OpRefBalance where Item = @Item and OpRef = @OpRef
		
		--汇总已计算未装配的零件数量
		select @TobeAssQty = @TobeAssQty + ISNULL(SUM(bom.OrderQty), 0)
		from ORD_OrderBomDet as bom
		inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
		inner join ORD_OrderSeq as seq on seq.OrderNo = mstr.OrderNo
		where bom.Item = @Item and (seq.Seq > @Seq1 or (seq.Seq = @Seq1 and seq.SubSeq > @SubSeq1)) and mstr.Flow = @ProdLine1
		and bom.IsCreateOrder = 1
		
		select @TobeAssQty = @TobeAssQty + ISNULL(SUM(bom.OrderQty), 0)
		from ORD_OrderBomDet as bom
		inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
		inner join ORD_OrderSeq as seq on seq.OrderNo = mstr.OrderNo
		where bom.Item = @Item and (seq.Seq > @Seq2 or (seq.Seq = @Seq2 and seq.SubSeq > @SubSeq2)) and mstr.Flow = @ProdLine2
		and bom.IsCreateOrder = 1
		
		--汇总在途数量
		select @IntransitQty = ISNULL(SUM(det.OrderQty), 0)
		from (select SUM(det.OrderQty - det.RecQty) as OrderQty
			from ORD_OrderDet_1 as det
			inner join ORD_OrderMstr_1 as mstr on det.OrderNo = mstr.OrderNo
			where det.Item = @Item and ((det.WMSSeq is null and det.BinTo = @OpRef) or det.WMSSeq = @OpRef) and mstr.[Status] in (1, 2) and mstr.OrderStrategy = 3
			union all
			select SUM(det.OrderQty - det.RecQty) as OrderQty
			from ORD_OrderDet_2 as det
			inner join ORD_OrderMstr_2 as mstr on det.OrderNo = mstr.OrderNo
			where det.Item = @Item and ((det.WMSSeq is null and det.BinTo = @OpRef) or det.WMSSeq = @OpRef) and mstr.[Status] in (1, 2) and mstr.OrderStrategy = 3) as det
			
		declare @DateTimeNow datetime = GETDATE()
		if @Version is not null
		begin
			update SCM_OpRefBalance set Qty = @InvQty + @IntransitQty - @TobeAssQty, 
			LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, [Version] = [Version] + 1
			where Id = @Id and [Version] = @Version
			
			if @@ROWCOUNT = 0
			begin
				RAISERROR(N'工位余量已经被更新，请重试。', 16, 1)
			end
			
			insert into LOG_OpRefBalanceChange(Item, OpRef, Qty, [Status], [Version], CreateDate, CreateUserId, CreateUserNm)
			values(@Item, @OpRef, @InvQty + @IntransitQty - @TobeAssQty, 1, @Version + 1, @DateTimeNow, @CreateUserId, @CreateUserNm)
		end
		else
		begin
			insert into SCM_OpRefBalance(Item, OpRef, Qty, CreateDate, CreateUser, CreateUserNm, LastModifyDate, LastModifyUser, LastModifyUserNm, [Version])
			values(@Item, @OpRef, @InvQty + @IntransitQty - @TobeAssQty, @DateTimeNow, @CreateUserId, @CreateUserNm, @DateTimeNow, @CreateUserId, @CreateUserNm, 1)
			
			insert into LOG_OpRefBalanceChange(Item, OpRef, Qty, [Status], [Version], CreateDate, CreateUserId, CreateUserNm)
			values(@Item, @OpRef, @InvQty + @IntransitQty - @TobeAssQty, 0, 1, @DateTimeNow, @CreateUserId, @CreateUserNm)
		end
		
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
