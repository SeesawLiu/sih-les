SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Busi_UpdateOpRefBalance')
	DROP PROCEDURE USP_Busi_UpdateOpRefBalance
GO

CREATE PROCEDURE [dbo].[USP_Busi_UpdateOpRefBalance]
(
	@ProdLine varchar(50),
	@Item varchar(50),
	@OpRef varchar(50),
	@TraceCode varchar(50),
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
		declare @Seq bigint
		declare @SubSeq int
		declare @PauseStatus tinyint
		declare @TobeAssQty decimal(18, 8)
		declare @IntransitQty decimal(18, 8)
		
		select @Seq = seq.Seq, @SubSeq = seq.SubSeq, @PauseStatus = mstr.PauseStatus
		from ORD_OrderMstr_4 as mstr
		inner join ORD_OrderSeq as seq on seq.OrderNo = mstr.OrderNo
		where mstr.TraceCode = @TraceCode and mstr.Flow = @ProdLine --and bom.IsCreateOrder = 1
		
		if @Seq is null
		begin
			if not exists(select top 1 1 from ORD_OrderSeq where TraceCode = @TraceCode)
			begin
				set @ErrorMsg = N'Van号' + @TraceCode + N'不存在。'
				RAISERROR(@ErrorMsg, 16, 1) 
			end
			else
			begin
				set @ErrorMsg = N'整车生产线' + @ProdLine + N'没有Van号' + @TraceCode + N'的生产单。'
				RAISERROR(@ErrorMsg, 16, 1) 
			end
		end
		
		select top 1 @Id = Id, @Version = [Version] from SCM_OpRefBalance where Item = @Item and OpRef = @OpRef
		
		--汇总已计算未装配的零件数量
		select @TobeAssQty = ISNULL(SUM(bom.OrderQty), 0)
		from ORD_OrderBomDet as bom
		inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
		inner join ORD_OrderSeq as seq on seq.OrderNo = mstr.OrderNo
		where bom.Item = @Item and (seq.Seq > @Seq or (seq.Seq = @Seq and seq.SubSeq > @SubSeq)) and mstr.Flow = @ProdLine
		and bom.IsCreateOrder = 1
		
		--汇总在途数量
		select @IntransitQty = ISNULL(SUM(det.OrderQty), 0)
		from (select SUM(det.OrderQty - det.RecQty) as OrderQty
			from ORD_OrderDet_1 as det
			inner join ORD_OrderMstr_1 as mstr on det.OrderNo = mstr.OrderNo
			where det.Item = @Item and det.BinTo = @OpRef and mstr.[Status] in (1, 2) and mstr.OrderStrategy = 3
			union all
			select SUM(det.OrderQty - det.RecQty) as OrderQty
			from ORD_OrderDet_2 as det
			inner join ORD_OrderMstr_2 as mstr on det.OrderNo = mstr.OrderNo
			where det.Item = @Item and det.BinTo = @OpRef and mstr.[Status] in (1, 2) and mstr.OrderStrategy = 3) as det
			
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
