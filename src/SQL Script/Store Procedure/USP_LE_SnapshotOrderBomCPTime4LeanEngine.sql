SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_LE_SnapshotOrderBomCPTime4LeanEngine')
	DROP PROCEDURE USP_LE_SnapshotOrderBomCPTime4LeanEngine
GO

CREATE PROCEDURE [dbo].[USP_LE_SnapshotOrderBomCPTime4LeanEngine] 
(
	@BatchNo int,
	@CreateUserId int,
	@CreateUserNm varchar(100)
) --WI
--WITH ENCRYPTION
AS
BEGIN
	SET NOCOUNT ON
	declare @Msg nvarchar(Max)
	
	set @Msg = N'快照OrderBomCPTime开始'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	Create table #VanProdLine
	(
		RowId int identity(1, 1),
		VanProdLine varchar(50)
	)
	
	insert into #VanProdLine(VanProdLine) select Code from SCM_FlowMstr where ProdLineType in (1,2,3,4)
	
	declare @RowId int
	declare @MaxRowId int
	declare @VanProdLine varchar(50)
	declare @DateTimeNow datetime = GETDATE()
	declare @LastUpdateDate datetime
	
	select @RowId = MIN(RowId), @MaxRowId = MAX(RowId) from #VanProdLine
	while @RowId <= @MaxRowId
	begin
		select @VanProdLine = VanProdLine from #VanProdLine where RowId = @RowId
		select top 1 @LastUpdateDate = CreateDate from ORD_OrderOpCPTime where VanProdLine = @VanProdLine order by CreateDate desc
		
		if not exists(select top 1 1 from ORD_OrderOpCPTime where VanProdLine = @VanProdLine) or (DATEADD(HOUR, -1, @DateTimeNow) > (select top 1 CreateDate from ORD_OrderOpCPTime where VanProdLine = @VanProdLine))
		begin
			set @Msg = N'生产线' + @VanProdLine + N' 最后刷新时间为' + convert(varchar, @LastUpdateDate, 120) + N'，已有1小时没有刷新OrderBomConsumeTime，强制进行刷新。'
			insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
			exec USP_Busi_UpdateOrderBomConsumeTime @VanProdLine, @CreateUserId, @CreateUserNm
			set @Msg = N'生产线' + @VanProdLine + N' 强制刷新完成。'
			insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
		end
		
		set @RowId = @RowId + 1
	end
	
	drop table #VanProdLine
	
	--truncate table ORD_OrderBomCPTime
	
	--insert into ORD_OrderBomCPTime(OrderNo, VanProdLine, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, CPTime,
	--Item, Uom, OpRef, OrderQty, Location, IsCreateOrder, BomId, DISPO, ManufactureParty, WorkCenter)
	--select tcpt.OrderNo, tcpt.VanProdLine, tcpt.AssProdLine, tcpt.Seq, tcpt.SubSeq, tcpt.VanOp, tcpt.AssOp, tcpt.Op, tcpt.OpTaktTime, tcpt.CPTime,
	--bom.Item, bom.Uom, bom.OpRef, bom.OrderQty, bom.Location, bom.IsCreateOrder, bom.Id, bom.DISPO, bom.ManufactureParty, bom.WorkCenter
	--from ORD_OrderOpCPTime as tcpt
	--inner join ORD_OrderBomDet as bom on tcpt.OrderNo = bom.OrderNo and tcpt.AssProdLine = bom.AssProdLine and tcpt.AssOp = bom.Op
	
	declare @trancount int = @@trancount
	begin try
		if @trancount = 0
		begin
			begin tran
		end
	        
		truncate table LE_OrderBomCPTimeSnapshot

		insert into LE_OrderBomCPTimeSnapshot(OrderNo, VanProdLine, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, 
		CPTime, Item, OpRef, OrderQty, Location, IsCreateOrder, BomId, DISPO, ManufactureParty, Uom, WorkCenter, BESKZ, SOBSL, 
		TraceCode, ZOPWZ, ZOPID, ZOPDS)
		select tcpt.OrderNo, tcpt.VanProdLine, tcpt.AssProdLine, tcpt.Seq, tcpt.SubSeq, tcpt.VanOp, tcpt.AssOp, tcpt.Op, tcpt.OpTaktTime, 
		tcpt.CPTime, bom.Item, bom.OpRef, bom.OrderQty, bom.Location, bom.IsCreateOrder, bom.Id as BomId, bom.DISPO, bom.ManufactureParty, bom.Uom, bom.WorkCenter, bom.BESKZ, bom.SOBSL,
		mstr.TraceCode, bom.ZOPWZ, bom.ZOPID, bom.ZOPDS
		from ORD_OrderOpCPTime as tcpt WITH(NOLOCK)
		inner join ORD_OrderBomDet as bom WITH(NOLOCK) on tcpt.OrderNo = bom.OrderNo and tcpt.AssProdLine = bom.AssProdLine and tcpt.AssOp = bom.Op
		inner join ORD_OrderMstr_4 as mstr WITH(NOLOCK) on bom.OrderNo = mstr.OrderNo
		where mstr.[Status] in (0, 1, 2)
		
		--按工位映射表更新
		update bom set OpRef = map.OpRef, RefOpRef = map.RefOpRef
		from LE_OrderBomCPTimeSnapshot as bom
		inner join CUST_OpRefMap as map on bom.Item = map.Item and bom.VanProdLine = map.ProdLine
		
		--为了防止相同工位的参考工位不一致，统一把参考工位更新成相同的
		update bom set RefOpRef = op.RefOpRef
		from LE_OrderBomCPTimeSnapshot as bom
		inner join (select Item, OpRef, MAX(RefOpRef) as RefOpRef from LE_OrderBomCPTimeSnapshot 
					group by Item, OpRef having count(distinct ISNULL(RefOpRef, '')) > 1) as op
		on bom.Item = op.Item and bom.OpRef = op.OpRef
		--insert into LE_OrderBomCPTimeSnapshot_Arch(BatchNo, OrderNo, VanProdLine, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, 
		--CPTime, Item, OpRef, OrderQty, Location, IsCreateOrder, BomId, DISPO, ManufactureParty, Uom, WorkCenter, BESKZ, SOBSL, 
		--TraceCode, ZOPWZ, ZOPID, ZOPDS, CreateDate) 
		--select @BatchNo, OrderNo, VanProdLine, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, 
		--CPTime, Item, OpRef, OrderQty, Location, IsCreateOrder, BomId, DISPO, ManufactureParty, Uom, WorkCenter, BESKZ, SOBSL, 
		--TraceCode, ZOPWZ, ZOPID, ZOPDS, CreateDate from LE_OrderBomCPTimeSnapshot
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
        
        set @Msg = N'快照OrderBomCPTime出现异常，异常信息：' + ERROR_MESSAGE()
		insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	end catch
	
	set @Msg = N'快照OrderBomCPTime结束'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
END
GO

