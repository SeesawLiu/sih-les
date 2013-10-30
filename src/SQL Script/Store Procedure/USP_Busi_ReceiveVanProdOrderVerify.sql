SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_ReceiveVanProdOrderVerify') 
     DROP PROCEDURE USP_Busi_ReceiveVanProdOrderVerify
GO

CREATE PROCEDURE [dbo].[USP_Busi_ReceiveVanProdOrderVerify] 
(
	@OrderNo varchar(50),
	@IsCheckIssue bit,
	@IsCheckItemTrace bit,
	@IsForce bit
) --WITH ENCRYPTION
AS 
BEGIN 
	set nocount on
	Declare @ErrorMsg nvarchar(MAX)
	Declare @Status tinyint
	Declare @ProdLine varchar(50)
	Declare @Seq bigint
	Declare @SubSeq int
	Declare @PauseStatus tinyint
	Declare @PauseSeq int
	Declare @TraceCode varchar(50)
	Declare @ProdlineType int
	
	Create table #tempErrMsg
	(
		ErrMsg varchar(Max)
	)
	
	select @Status = mstr.[Status], @ProdLine = mstr.Flow, @Seq = seq.Seq, @SubSeq = SubSeq, 
	@PauseStatus = PauseStatus, @PauseSeq = PauseSeq, @TraceCode = mstr.TraceCode, @ProdlineType = mstr.ProdLineType
	from ORD_OrderMstr_4 as mstr inner join ORD_OrderSeq as seq on mstr.OrderNo = seq.OrderNo 
	where mstr.OrderNo = @OrderNo
	
	if @Status <> 2
	begin
		if @Status = 3 or @Status = 4
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经下线。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		else
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'还未上线。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
	end
	
	if @PauseStatus = 2
	begin
		set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经暂停，不能下线。'
		RAISERROR(@ErrorMsg, 16, 1)
	end
	
	if @IsForce = 0 and not exists(select top 1 1 from CUST_ProductLineMap where CheckProdLine = @ProdLine or AssemblyProdLine = @ProdLine or SpecialProdLine = @ProdLine)
	begin
		if exists(select top 1 1 from ORD_OrderMstr_4 as mstr inner join ORD_OrderSeq as seq on mstr.OrderNo = seq.OrderNo 
					where mstr.Flow = @ProdLine and Status = 2 and PauseStatus <> 2 
					and (seq.Seq < @Seq or (seq.Seq = @Seq and seq.SubSeq < @SubSeq)))
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'不是生产线' + @ProdLine + N'第一张等待下线的生产单。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
	end
		
	--if exists(select top 1 1 from SCM_FlowMstr where Code = @ProdLine and IsPause = 1) and (@ProdlineType = 1 or @ProdlineType = 2)
	--begin
		--set @ErrorMsg = N'整车生产线' + @ProdLine + N'已经暂停，生产单' + @OrderNo + N'不能下线。'
		--RAISERROR(@ErrorMsg, 16, 1)
	--end
	
	if (@IsCheckIssue = 1 and exists(select top 1 * from ISS_IssueMstr where BackYards = @TraceCode and Status in (0, 1, 2)))
	begin
		insert into #tempErrMsg(ErrMsg) values(N'整车生产单' + @OrderNo + N'有未关闭的失效模式，不能下线。')
		insert into #tempErrMsg(ErrMsg) select Content from ISS_IssueMstr where BackYards = @TraceCode and Status in (0, 1, 2) 
	end
	
	if (@IsCheckItemTrace = 1 and exists(select top 1 * from ORD_OrderItemTrace where OrderNo = @OrderNo and ScanQty < Qty))
	begin
		insert into #tempErrMsg(ErrMsg) values(N'整车生产单' + @OrderNo + N'有未扫描的关键件，不能下线。')
		insert into #tempErrMsg(ErrMsg) 
		select N'关键件物料号' + Item + N'描述' + ISNULL(ItemDesc, '') + N'旧图号' + ISNULL(RefItemCode, '') + N'工位' + ISNULL(OpRef, '') 
		from ORD_OrderItemTrace where OrderNo = @OrderNo and ScanQty < Qty
	end
	
	select ErrMsg from #tempErrMsg
	drop table #tempErrMsg
END 
