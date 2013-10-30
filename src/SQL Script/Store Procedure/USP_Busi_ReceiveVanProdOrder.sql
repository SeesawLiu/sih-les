SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_ReceiveVanProdOrder') 
     DROP PROCEDURE USP_Busi_ReceiveVanProdOrder
GO

CREATE PROCEDURE [dbo].[USP_Busi_ReceiveVanProdOrder] 
(
	@OrderNo varchar(50),
	@ReceiveUserId int,
	@ReceiveUserNm varchar(50)
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
       
		Declare @Version int
		Declare @Status tinyint
		Declare @ProdLineType tinyint
		Declare @TraceCode varchar(50)
		
		select @Version = [Version], @Status = [Status], @ProdLineType = ProdLineType, @TraceCode = TraceCode
		from ORD_OrderMstr_4 WITH(NOLOCK)
		where OrderNo = @OrderNo
		
		if @Status = 1
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'还未上线。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		else if @Status = 3
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经下线。'
			RAISERROR(@ErrorMsg, 16, 1)
		end else if @Status = 4
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'物料已经反冲。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if (@ProdLineType = 3 or @ProdLineType = 4)
		begin  --如果是总装和特装要下线没有下线的底盘和驾驶室生产单，还要对检测生产单上线
			declare @CabOrderNo varchar(50)
			declare @CabOrderStatus tinyint
			select @CabOrderNo = OrderNo, @CabOrderStatus = [Status] from ORD_OrderMstr_4 WITH(NOLOCK)
			where TraceCode = @TraceCode and ProdLineType = 1
			if (@CabOrderNo is not null)
			begin
				if (@CabOrderStatus = 1)
				begin
					set @ErrorMsg = N'驾驶室生产单' + @CabOrderNo + N'还未上线。'
					RAISERROR(@ErrorMsg, 16, 1)
				end
				else if (@CabOrderStatus = 2)
				begin  --补驾驶室下线
					exec USP_Busi_ReceiveVanProdOrder @CabOrderNo, @ReceiveUserId, @ReceiveUserNm
				end
			end
			
			declare @ChassisOrderNo varchar(50)
			declare @ChassisOrderStatus tinyint
			select @ChassisOrderNo = OrderNo, @ChassisOrderStatus = [Status] from ORD_OrderMstr_4 WITH(NOLOCK)
			where TraceCode = @TraceCode and ProdLineType = 2
			if (@ChassisOrderNo is not null)
			begin
				if (@ChassisOrderStatus = 1)
				begin
					set @ErrorMsg = N'底盘生产单' + @ChassisOrderNo + N'还未上线。'
					RAISERROR(@ErrorMsg, 16, 1)
				end
				else if (@ChassisOrderStatus = 2)
				begin  --补驾驶室下线
					exec USP_Busi_ReceiveVanProdOrder @ChassisOrderNo, @ReceiveUserId, @ReceiveUserNm
				end
			end
			
			declare @CheckOrderNo varchar(50)
			declare @CheckOrderStatus tinyint
			declare @CheckOrderVersion tinyint
			select @CheckOrderNo = OrderNo, @CheckOrderStatus = [Status], @CheckOrderVersion = [Version] from ORD_OrderMstr_4 WITH(NOLOCK) 
			where TraceCode = @TraceCode and ProdLineType = 9
			if (@CheckOrderNo is not null)
			begin
				if (@CheckOrderStatus = 1)
				begin
					update ORD_OrderMstr_4 set [Status] = 2, StartDate = @DateTimeNow, StartUser = @ReceiveUserId, StartUserNm = @ReceiveUserNm, LastModifyDate = @DateTimeNow, LastModifyUser = @ReceiveUserId, LastModifyUserNm = @ReceiveUserNm, [Version] = [Version] + 1
					where OrderNo = @CheckOrderNo and [Version] = @CheckOrderVersion
					
					if @@rowcount = 0
					begin
						set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经更新，请重试下线。'
						RAISERROR(@ErrorMsg, 16, 1)
					end
				end
			end
		end
				
		--更新生产单的当前工序为最后一个工序
		update mstr set CurtOp = op.MaxOp, [Status] = 3,
		CompleteDate = @DateTimeNow, CompleteUser = @ReceiveUserId, CompleteUserNm = @ReceiveUserNm, 
		--CloseDate = @DateTimeNow, CloseUser = @ReceiveUserId, CloseUserNm = @ReceiveUserNm, 
		LastModifyDate = @DateTimeNow, LastModifyUser = @ReceiveUserId, LastModifyUserNm = @ReceiveUserNm, 
		[Version] = mstr.[Version] + 1
		from ORD_OrderMstr_4 as mstr inner join
		(select OrderNo, MAX(Op) as MaxOp from ORD_OrderOp WITH(NOLOCK) where OrderNo = @OrderNo group by OrderNo) as op 
		on mstr.OrderNo = op.OrderNo
		where mstr.OrderNo = @OrderNo and mstr.[Version] = @Version
		
		if @@rowcount = 0
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经更新，请重试下线。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		--未打回冲标记的工序全部打标记
		--update ORD_OrderOp set BackflushQty = 1, LastModifyUser = @ReceiveUserId, LastModifyUserNm = @ReceiveUserNm,
		--LastModifyDate = @DateTimeNow, [Version] = [Version] + 1
		--where OrderNo = @OrderNo and BackflushQty = 0
		
		--未报工的工序全部报工
		insert into ORD_OrderOpReport(OrderNo, OrderDetId, OrderOpId, Op, ReportQty, ScrapQty, BackflushQty, WorkCenter, Status, EffDate, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, Version)
		select op.OrderNo, op.OrderDetId, op.Id, op.Op, 1, 0, 0, op.WorkCenter, 0, @DateTimeNow, @ReceiveUserId, @ReceiveUserNm, @DateTimeNow, @ReceiveUserId, @ReceiveUserNm, @DateTimeNow, 1
		from ORD_OrderMstr_4 as mstr WITH(NOLOCK)
		inner join ORD_OrderOp as op WITH(NOLOCK) on mstr.OrderNo = op.OrderNo
		left join ORD_OrderOpReport as rpt WITH(NOLOCK) on op.Id = rpt.OrderOpId and rpt.[Status] = 0   --预防重复报工
		where mstr.OrderNo = @OrderNo and op.ReportQty = 0 and rpt.Id is null

		--未报工的工序全部报工SAP
		insert into SAP_ProdOpReport(AUFNR, WORKCENTER, GAMNG, [Status], CreateDate, LastModifyDate, ErrorCount, SCRAP, [TEXT], IsCancel, OrderNo, OrderOpId, EffDate, [Version], OrderOpReportId)
		select mstr.ExtOrderNo, rpt.WorkCenter, 1, 0, @DateTimeNow, @DateTimeNow, 0, 0, '', 0, mstr.OrderNo, op.Id, @DateTimeNow, 1, rpt.Id
		from ORD_OrderMstr_4 as mstr WITH(NOLOCK)
		inner join ORD_OrderOp as op WITH(NOLOCK) on mstr.OrderNo = op.OrderNo
		inner join ORD_OrderOpReport as rpt WITH(NOLOCK) on op.Id = rpt.OrderOpId
		left join SAP_ProdOpReport as por WITH(NOLOCK) on rpt.Id = por.OrderOpReportId
		where mstr.OrderNo = @OrderNo and por.Id is null and rpt.WorkCenter is not null
		
		--更新生产单工序报工标识
		update op set ReportQty = 1, LastModifyUser = @ReceiveUserId, LastModifyUserNm = @ReceiveUserNm,
		LastModifyDate = @DateTimeNow, [Version] = op.[Version] + 1
		from ORD_OrderMstr_4 as mstr WITH(NOLOCK) 
		inner join ORD_OrderOp as op WITH(NOLOCK) on mstr.OrderNo = op.OrderNo
		where mstr.OrderNo = @OrderNo and op.ReportQty = 0 --and op.WorkCenter is not null
				
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
