SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_MoveVanProdOrder') 
     DROP PROCEDURE USP_Busi_MoveVanProdOrder
GO

CREATE PROCEDURE [dbo].[USP_Busi_MoveVanProdOrder] 
(
	@ProdLine varchar(50),
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
      
		--更新生产线上所有整车生产单的当前工序
		update mstr set CurtOp = currOp.CurrOp, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, [Version] = mstr.[Version] + 1
		from ORD_OrderMstr_4 as mstr inner join
		(select mstr.OrderNo, MIN(op.Op) as CurrOp 
		from ORD_OrderMstr_4 as mstr WITH(NOLOCK) 
		inner join ORD_OrderOp as op WITH(NOLOCK) on mstr.OrderNo = op.OrderNo and ISNULL(mstr.CurtOp, 0) < op.Op
		where mstr.Flow = @ProdLine and mstr.[Status] = 2 and mstr.PauseStatus <> 2
		group by mstr.OrderNo) as currOp on mstr.OrderNo = currOp.OrderNo
		
		----工序物料记录反冲数量
		--update op set BackflushQty = 1, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm,
		--LastModifyDate = @DateTimeNow, [Version] = op.[Version] + 1
		--from ORD_OrderMstr_4 as mstr 
		--inner join ORD_OrderOp as op on mstr.OrderNo = op.OrderNo and mstr.CurtOp = op.Op
		--where mstr.Flow = @ProdLine and mstr.[Status] = 2 and BackflushQty = 0
		
		--插入暂停LOG记录
		insert into LOG_ProdOrderPauseResume(ProdLine, ProdLineDesc, OrderNo, VanCode, CurrentOperation, OprateType, CreateUserName, Seq, SubSeq) 
		select mstr.Flow, mstr.FlowDesc, mstr.OrderNo, mstr.TraceCode, mstr.CurtOp, 2, @CreateUserNm, seq.Seq, seq.SubSeq
		from ORD_OrderMstr_4 as mstr inner join ORD_OrderSeq as seq on mstr.OrderNo = seq.OrderNo
		where mstr.Flow = @ProdLine and mstr.[Status] = 2 and mstr.PauseStatus = 1 and mstr.CurtOp = PauseSeq

		--暂停计划暂停的订单
		update ORD_OrderMstr_4 set PauseStatus = 2, PauseTime = @DateTimeNow
		where Flow = @ProdLine and [Status] = 2 and PauseStatus = 1 and CurtOp = PauseSeq
		
		--生产单报工
		insert into ORD_OrderOpReport(OrderNo, OrderDetId, OrderOpId, Op, ReportQty, ScrapQty, BackflushQty, WorkCenter, Status, EffDate, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, Version)
		select op.OrderNo, op.OrderDetId, op.Id, op.Op, 1, 0, 0, op.WorkCenter, 0, @DateTimeNow, @CreateUserId, @CreateUserNm, @DateTimeNow, @CreateUserId, @CreateUserNm, @DateTimeNow, 1
		from ORD_OrderMstr_4 as mstr WITH(NOLOCK)
		inner join ORD_OrderOp as op WITH(NOLOCK) on mstr.OrderNo = op.OrderNo and mstr.CurtOp = op.Op
		left join ORD_OrderOpReport as rpt WITH(NOLOCK) on op.Id = rpt.OrderOpId  --预防重复报工
		where mstr.Flow = @ProdLine and mstr.[Status] = 2 and op.ReportQty = 0 and rpt.Id is null
		
		--生产单报工SAP
		insert into SAP_ProdOpReport(AUFNR, WORKCENTER, GAMNG, [Status], CreateDate, LastModifyDate, ErrorCount, SCRAP, [TEXT], IsCancel, OrderNo, OrderOpId, EffDate, [Version], OrderOpReportId)
		select mstr.ExtOrderNo, op.WorkCenter, 1, 0, @DateTimeNow, @DateTimeNow, 0, 0, '', 0, mstr.OrderNo, op.Id, @DateTimeNow, 1, rpt.Id
		from ORD_OrderMstr_4 as mstr WITH(NOLOCK)
		inner join ORD_OrderOp as op WITH(NOLOCK) on mstr.OrderNo = op.OrderNo and mstr.CurtOp = op.Op
		inner join ORD_OrderOpReport as rpt WITH(NOLOCK) on op.Id = rpt.OrderOpId
		where mstr.Flow = @ProdLine and mstr.[Status] = 2 and op.ReportQty = 0 and op.IsAutoReport = 1
		
		--更新生产单工序报工数量
		update op set ReportQty = 1, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm,
		LastModifyDate = @DateTimeNow, [Version] = op.[Version] + 1
		from ORD_OrderMstr_4 as mstr WITH(NOLOCK)
		inner join ORD_OrderOp as op WITH(NOLOCK) on mstr.OrderNo = op.OrderNo and mstr.CurtOp = op.Op
		where mstr.Flow = @ProdLine and mstr.[Status] = 2 and op.ReportQty = 0 --and op.IsAutoReport = 1
		
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
	
	--begin try
	--	--更新物料消耗时间
	--	exec USP_Busi_UpdateOrderBomConsumeTime @ProdLine, @CreateUserId, @CreateUserNm
	--end try
	--begin catch
	--	set @ErrorMsg = Error_Message() 
	--	RAISERROR(@ErrorMsg, 16, 1) 
	--end catch 
END 
