
CREATE PROCEDURE [dbo].[USP_Busi_ScanJITControlPoint]
	@MESScanControlPointList varchar(4000),
	@CreateUserId int,
	@CreateUserNm varchar(100)
AS
BEGIN
	set nocount on
	begin try
		begin tran ScanJITControlPoint
		save tran ScanJITControlPoint_Point
		
		declare @DateTimeNow datetime = GetDate()
		declare @ErrorMsg nvarchar(MAX)
		
		
		
		
		-----------------------------↓查找过点扫描信息-----------------------------
		declare @SplitSymbol char(1) = '|'
		create table #tempScanControlPoint
		(
			RowId int identity(1,1),
			ScanControlPointId int,
			ProdLine varchar(50),
			StartDate date,
			ScanDateTime datetime,
			TraceCode varchar(50),
			ProdItem varchar(50),
			OrderNo varchar(50),
			Vanseries varchar(50),
			JITSeq int,
			IsEmpty bit,
			ProdCode varchar(50)
		)
		
		--循环拆分过点扫描Id插入缓存表中
		if ISNULL(@MESScanControlPointList, '') <> ''
		begin
			while(charindex(@SplitSymbol, @MESScanControlPointList) <> 0)
			begin
				insert #tempScanControlPoint(ScanControlPointId) values (substring(@MESScanControlPointList, 1, charindex(@SplitSymbol, @MESScanControlPointList) - 1))
				set @MESScanControlPointList = stuff(@MESScanControlPointList, 1, charindex(@SplitSymbol,@MESScanControlPointList), ' ')
			end
			insert #tempScanControlPoint(ScanControlPointId) values (Ltrim(@MESScanControlPointList))
		end
		
		--更新生产线
		update tscp set ProdLine = plc.ProdLine, StartDate = convert(datetime, scp.ScanDate), JITSeq = scp.JITSeq,
		ScanDateTime = convert(datetime, SUBSTRING(scp.ScanDate, 1, 4) + '-' + SUBSTRING(scp.ScanDate, 5, 2) + '-' + SUBSTRING(scp.ScanDate, 7, 2) + ' ' + SUBSTRING(scp.ScanTime, 1, 2) + ':' + SUBSTRING(scp.ScanTime, 3, 2) + ':' + SUBSTRING(scp.ScanTime, 5, 2)),
		TraceCode = i.VanSeries + scp.ProdSeq, ProdItem = scp.ProdItem, OrderNo = i.VanSeries + '1' + scp.ProdSeq,Vanseries = scp.Vanseries, 
		IsEmpty = 0, ProdCode = scp.ProdCode
		from #tempScanControlPoint as tscp
		inner join MES_ScanControlPoint as scp on scp.Id = tscp.ScanControlPointId
		inner join PRD_ProdLineControl as plc on scp.VanSeries = plc.VanSeries and scp.ControlPoint = plc.ControlPoint
		inner join MD_Item as i on scp.ProdItem = i.Code
		
		--更新空车的生产线
		update tscp set ProdLine = mstr.Code, StartDate = convert(datetime, scp.ScanDate), JITSeq = scp.JITSeq,
		ScanDateTime = convert(datetime, SUBSTRING(scp.ScanDate, 1, 4) + '-' + SUBSTRING(scp.ScanDate, 5, 2) + '-' + SUBSTRING(scp.ScanDate, 7, 2) + ' ' + SUBSTRING(scp.ScanTime, 1, 2) + ':' + SUBSTRING(scp.ScanTime, 3, 2) + ':' + SUBSTRING(scp.ScanTime, 5, 2)),
		Vanseries = scp.Vanseries, 
		IsEmpty = 1, ProdCode = scp.ProdCode
		from #tempScanControlPoint as tscp
		inner join MES_ScanControlPoint as scp on scp.Id = tscp.ScanControlPointId
		inner join SCM_FlowMstr as mstr on scp.ProdCode = mstr.BlankSeqProdCode
		where mstr.[Type] = 4 and mstr.ProdLineType = 1
		-----------------------------↑查找过点扫描信息-----------------------------	
		
		
		
		
		-----------------------------↓检查是否能够找到总装生产单-----------------------------
		declare @NoOrderPordCode varchar(50)
		declare @NoOrderPordSeq varchar(50)
		
		select top 1 @NoOrderPordCode = scp.ProdCode, @NoOrderPordSeq = scp.ProdSeq 
		from #tempScanControlPoint as tscp
		inner join MES_ScanControlPoint as scp on scp.Id = tscp.ScanControlPointId
		left join ORD_OrderMstr_4 as mstr on mstr.OrderNo = tscp.OrderNo --and tscp.ProdLine = mstr.Flow
		where mstr.OrderNo is null and tscp.IsEmpty = 0
		
		if @NoOrderPordCode is not null
		begin
			set @ErrorMsg = N'生产代码' + @NoOrderPordCode + N'车身号' + @NoOrderPordSeq + N'没有找到总装生产单。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		-----------------------------↑检查是否能够找到总装生产单-----------------------------	
		
		
		
		
		
		-----------------------------↓检查是否过了JIT点-----------------------------
		--已经过了JIT点
		declare @SeqErrProdCode varchar(50)
		declare @SeqErrProdSeq varchar(50)
		
		select @SeqErrProdCode= scp.ProdCode, @SeqErrProdSeq = scp.ProdSeq
		from #tempScanControlPoint as tscp
		inner join MES_ScanControlPoint as scp on scp.Id = tscp.ScanControlPointId
		inner join ORD_OrderMstr_4 as mstr on tscp.OrderNo = mstr.OrderNo and tscp.ProdLine = mstr.Flow
		inner join ORD_OrderSeq_4 as seq on seq.TraceCode = mstr.TraceCode and seq.ProdLine = mstr.Flow
		where seq.Seq < 59999999999999
		
		if @SeqErrProdCode is not null
		begin
			set @ErrorMsg = N'生产代码' + @SeqErrProdCode + N'车身号' + @SeqErrProdSeq + N'已经过了JIT点。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		-----------------------------↑检查是否过了JIT点-----------------------------	
		
		
		
		
		
		-----------------------------↓生产线变更-----------------------------
		--查找换生产线的生产单
		select Identity(int, 1, 1) RowId, mstr.OrderNo, det.Id as OrderDetId, det.Item, i.VanSeries, rm.Code as Routing, convert(date, mstr.StartTime) as StartDate, 
				mstr.Flow as OldProdLine, tscp.ProdLine as NewProdLine
		into #tempProdLineChange
		from #tempScanControlPoint as tscp
		inner join ORD_OrderMstr_4 as mstr on tscp.OrderNo = mstr.OrderNo
		inner join ORD_OrderDet_4 as det on mstr.OrderNo = det.OrderNo
		inner join MD_Item as i on det.Item = i.Code
		left join PRD_RoutingMstr as rm on rm.ProdLine = tscp.ProdLine and rm.VanSeries = tscp.VanSeries
		where tscp.ProdLine <> mstr.Flow
		
		if exists(select top 1 OrderNo from #tempProdLineChange where Routing is null)
		begin
			declare @ErrorOrderNo1 varchar(50)
			select top 1 @ErrorOrderNo1 = OrderNo from #tempProdLineChange where Routing is null
			
			set @ErrorMsg = N'没有找到生产单'+@ErrorOrderNo1+N'变更生产线后的工艺流程。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if exists(select top 1 OrderNo from #tempProdLineChange)
		begin

			CREATE TABLE #tempOrderBomDet  --订单BOM缓存表
			(
				RowId int identity(1, 1),
				ProdLine varchar(50),
				ProdLineType tinyint,	
				ProdItem varchar(50),
				ProdCode varchar(50),
				ProdSeq varchar(50),
				Seq int,
				Item varchar(50),
				ItemDesc varchar(100),
				RefItemCode varchar(50),
				Uom varchar(5),
				Op int,
				OpRef varchar(5),
				OrderQty decimal(18,8),
				OrgOrderQty decimal(18,8),
				Location varchar(50),
				PONo varchar(50),
				POLineNo varchar(50),
				UPGCode varchar(50),
				VanPO varchar(50),
				FlowStrategy tinyint,
				VanDiffId int,
				EOSwitchId int,
				EONo varchar(50),
				POChangeId int,
				MultiSupplyGroup varchar(50),       --LES多轨组号
				SubstituteGroup varchar(50),        --SAP替代组号
				ManufactureParty varchar(50),		--制造商，双轨使用
				VDMinusUpdateFlag bit default(0),    --订单车减料差异档更新标记
				OrderNo varchar(50),
				BomId varchar(50),
				Shelf varchar(50),
				ErrorId int,
				ErrorMsg varchar(100),
				EOCountingDown int,
				POCountingDown int
			)
			
			--生产线原材料库位缓存表，即拉料入库库位表
			CREATE TABLE #tempLocTo
			(
				LocTo varchar(50)
			)
			
			CREATE TABLE #tempMSGroupTargetOrder  --整车、替代组汇总临时表
			(
				MSGroupTargetOrderRowId int identity(1, 1),
				OrderNo varchar(50),
				MultiSupplyGroup varchar(50)
			)
			
			CREATE TABLE #tempMSOrderBomDetId  --一个双轨组下的整车+工位+替代物料表
			(
				MSOrderBomDetCycleId int identity(1, 1),
				RowId int,
				Item varchar(50),
				MultiSupplyGroup varchar(50),
				OrderQty decimal,
				ErrorId int,
				ErrorMsg varchar(100)
			)
			
			CREATE TABLE #tempMultiSupplyGroup
			(
				RowId int IDENTITY(1, 1),
				MultiSupplyGroup varchar(50)
			)
			
			declare @PLChangeRowId int
			declare @MaxPLChangeRowId int
			
			select @PLChangeRowId = MIN(RowId), @MaxPLChangeRowId = MAX(RowId) from #tempProdLineChange
			
			while (@PLChangeRowId <= @MaxPLChangeRowId)
			begin  --循环变更生产线
				Declare @OrderNo varchar(50)
				Declare @OrderDetId int
				Declare @ProdItem varchar(50)
				Declare @ProdLine varchar(50)
				Declare @EffDate datetime
				Declare @Routing  varchar(50)
				Declare @Region varchar(50)
				Declare @LocFrom varchar(50)
				Declare @LocTo varchar(50)
				Declare @ExtraDmdSource varchar(50)
				Declare @StartDate datetime
				Declare @ProdLineType tinyint

				--查找一张生产单
				select @OrderNo = OrderNo, @OrderDetId = OrderDetId, @ProdItem = Item, @ProdLine = NewProdLine, @EffDate = StartDate, @Routing = Routing
				from #tempProdLineChange where RowId = @PLChangeRowId
				
				--查找新线生产线区域、默认原材料消耗库位和成品入库库位和其它需求源，其它需求源作为其它需要考虑的原材料消耗库位
				select @Region = PartyFrom, @LocFrom = LocFrom, @LocTo = LocTo, @ExtraDmdSource = ExtraDmdSource, @ProdLineType = ProdLineType from SCM_FlowMstr where Code = @ProdLine			
				



				-----------------------------↓删除原生产线Bom-----------------------------
				--更新工位余量，把已经计算的JIT量全部作为正数记录到工位余量中
				update orb set Qty = Qty + bom.orderQty, [Version] = orb.[Version] + 1, 
				LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm
				from SCM_OpRefBalance as orb inner join
				(select bom.Item, bom.OpRef, SUM(bom.OrderQty) as orderQty
				from ORD_OrderBomDet_4 as bom
				where bom.OrderNo = @OrderNo and bom.IsCreateOrder = 1
				group by bom.Item, bom.OpRef) as bom on orb.Item = bom.Item and orb.OpRef = bom.OpRef
					
				--更新双轨循环量，把Bom用量更新到双轨的溢出量中
				update mss set SpillQty = SpillQty + bom.OrderQty, [Version] = mss.[Version] + 1
				from ORD_OrderBomDet_4 as bom
				inner join PRD_MultiSupplyItem as msi on bom.Item = msi.Item
				inner join PRD_MultiSupplySupplier as mss on msi.GroupNo = mss.GroupNo and msi.Supplier = mss.Supplier
				where bom.OrderNo = @OrderNo
				
				--删除原生产线Bom
				delete from ORD_OrderBomDet_4 where OrderNo = @OrderNo
				-----------------------------↑删除原生产线Bom-----------------------------
				
				
				
				-----------------------------↓新增新生产线Bom-----------------------------
				--缓存订单Bom
				truncate table #tempOrderBomDet
				insert into #tempOrderBomDet(Item, ItemDesc, RefItemCode, Uom,
											Op, OpRef, OrderQty, OrgOrderQty, Location,
											PONo, POLineNo, UPGCode, FlowStrategy,
											VanDiffId, EOSwitchId, EONo, POChangeId, 
											MultiSupplyGroup, ManufactureParty, SubstituteGroup,
											BomId, Shelf)
				select bi.Code as Item, bi.Desc1 as ItemDesc, bi.RefCode as RefItemCode, b.Uom, --BOM单位就是基本单位
				rd.Op, rd.OpRef, b.BomUnitQty as OrderQty, b.BomUnitQty as OrgOrderQty, @LocFrom as Location, --物料消耗库位
				null as PONo, null as POLineNo, b.UPGCode, 0 as FlowStrategy, --默认拉动方式手工的拉料方式
				0 as VanDiffId, 0 as EOSwitchId, null as EONo, 0 as POChangeId, 
				null as MultiSupplyGroup, null as ManufactureParty, b.SubstituteGroup,
				b.BomId, b.Shelf
				--整车上线计划
				from CUST_BomCache as b
				--工艺流程
				inner join PRD_RoutingMstr as rm on rm.Code = @Routing
				--工位表，取工位的顺序（工序）
				inner join PRD_RoutingDet as rd on rm.Code = rd.Routing and b.OpRef = rd.OpRef
				--BOM物料表，取描述、参考零件号、单位
				inner join MD_Item as bi on b.BomItem = bi.Code
				--生产线LayOut，取BOM消耗库位和物料拉动方式
				where b.ProdItem = @ProdItem and b.ProdLine = @ProdLine and b.EffDate = @EffDate
			
			
			
			
				-----------------------------↓更新Bom物料消耗库位-----------------------------
				Declare @SplitSymbol1 varchar(1) = ','
				Declare @SplitSymbol2 varchar(1) = '|'
				
				--把生产线入库库位插入生产线原材料库位缓存表中
				truncate table #tempLocTo
				insert into #tempLocTo values (@LocFrom)
				
				--循环拆分其它需求源中的库位并插入生产线原材料库位缓存表中
				if ISNULL(@ExtraDmdSource, '') <> ''
				begin
					if (charindex(@SplitSymbol1, @ExtraDmdSource) <> 0)
					begin
						while(charindex(@SplitSymbol1, @ExtraDmdSource) <> 0)
						begin
							insert #tempLocTo values (substring(@ExtraDmdSource, 1, charindex(@SplitSymbol1, @ExtraDmdSource) - 1))
							set @ExtraDmdSource = stuff(@ExtraDmdSource, 1, charindex(@SplitSymbol1,@ExtraDmdSource), ' ')
						end
					end
					else if (charindex(@SplitSymbol2, @ExtraDmdSource) <> 0)
					begin
						while(charindex(@SplitSymbol2, @ExtraDmdSource) <> 0)
						begin
							insert #tempLocTo values (substring(@ExtraDmdSource, 1, charindex(@SplitSymbol2, @ExtraDmdSource) - 1))
							set @ExtraDmdSource = stuff(@ExtraDmdSource, 1, charindex(@SplitSymbol2,@ExtraDmdSource), ' ')
						end
					end
					insert #tempLocTo values (Ltrim(@ExtraDmdSource))
				end
				update #tempLocTo set LocTo = Rtrim(Ltrim(LocTo))
			    
				--缓存拉料路线明细
				select det.Item, mstr.LocTo, stra.Strategy as FlowStrategy into #tempFlowDet 
				from SCM_FlowMstr as mstr 
				inner join SCM_FlowStrategy as stra on mstr.Code = stra.Flow
				inner join SCM_FlowDet as det on mstr.Code = det.Flow
				--先考虑路线明细的目的库位，如果路线明细库位为空在考虑路线头上的目的库位
				inner join #tempLocTo as loc on loc.LocTo = det.LocTo or (det.LocTo is null and loc.LocTo = mstr.LocTo)
				inner join #tempOrderBomDet as bom on bom.Item = det.Item
				where mstr.[Type] in (1, 2, 5, 6, 8) --拉料路线仅考虑采购、移库、委外、客供品、计划协议
				group by det.Item, mstr.LocTo, stra.Strategy
			    
				--更新订单BOM缓存表上的消耗库位和拉料方式
				--优先级为序列、JIT、看板、工位零件库位对照档、生产线默认库位
				update bom set FlowStrategy = 1, Location = flowDet.LocTo
				from #tempOrderBomDet as bom 
				inner join #tempFlowDet as flowDet on bom.Item = flowDet.Item
				where flowDet.FlowStrategy = 1 and bom.FlowStrategy = 0
			    
				update bom set FlowStrategy = 2, Location = flowDet.LocTo
				from #tempOrderBomDet as bom 
				inner join #tempFlowDet as flowDet on bom.Item = flowDet.Item
				where flowDet.FlowStrategy = 2 and bom.FlowStrategy = 0
			    
				update bom set FlowStrategy = 3, Location = flowDet.LocTo
				from #tempOrderBomDet as bom 
				inner join #tempFlowDet as flowDet on bom.Item = flowDet.Item
				where flowDet.FlowStrategy = 3 and bom.FlowStrategy = 0
			    
				--update bom set Location = layout.Location
				--from #tempOrderBomDet as bom 
				--inner join PRD_ProdLineLayOut as layout on bom.Item = layout.Item and bom.OpRef = layout.OpRef  --用零件+工位对应
				--where bom.FlowStrategy = 0
				update bom set Location = layout.Location
				from #tempOrderBomDet as bom 
				inner join PRD_ProdLineLayOut as layout on bom.Item = layout.Item and bom.OpRef = layout.OpRef  --用零件+工位对应
				where bom.FlowStrategy = 0				
				-----------------------------↑更新Bom物料消耗库位-----------------------------
				
				
				
				
				-----------------------------↓更新双轨-----------------------------
				declare @MSCycleRowId int
				declare @MaxMSCycleRowId int
				declare @MultiSupplyGroup varchar(50)
				declare @MSGroupTargetOrderRowId int
				declare @MaxMSGroupTargetOrderRowId int
				declare @MSEffSupplier varchar(50)
				declare @MSId int
				declare @MSSeq int
				declare @MSTargetCycleQty decimal(18, 8)
				declare @MSAccumulateQty decimal(18, 8)
				declare @MSVersion int
				declare @MSCycleOrderBomDetCycleId int
				declare @MaxMSCycleOrderBomDetCycleId int
				declare @MSRowId int
				declare @MSItem varchar(50)
				declare @MSSupplier varchar(50)
				declare @MSSubstituteGroup varchar(50)
				declare @MSOrderQty decimal(18, 8)
				declare @MSOrderBomDetCount int				
				
				--查找所有双轨组
				truncate table #tempMultiSupplyGroup
				insert into #tempMultiSupplyGroup
				select MultiSupplyGroup from #tempOrderBomDet 
				where OrderQty > 0 and FlowStrategy <> 1  --不考虑排序
				and ISNULL(MultiSupplyGroup, '') <> ''
				group by MultiSupplyGroup
				
				--更新双轨零件供应商，零件号相同的双轨零件随便找一个供应商
				update bom set ManufactureParty = SUBSTRING(item.Supplier, 1, 3)
				from #tempOrderBomDet as bom
				inner join PRD_MultiSupplyItem as item on bom.Item = item.Item 
				and bom.MultiSupplyGroup = item.GroupNo
				where bom.SubstituteGroup is not null and bom.SubstituteGroup <> ''
				
				--循环双轨组
				if exists (select top 1 RowId from #tempMultiSupplyGroup)
				begin
					select @MSCycleRowId = MIN(RowId), @MaxMSCycleRowId = MAX(RowId) from #tempMultiSupplyGroup
					while(@MSCycleRowId <= @MaxMSCycleRowId)
					begin
						--取得双轨组号
						select @MultiSupplyGroup = MultiSupplyGroup from #tempMultiSupplyGroup where RowId = @MSCycleRowId
						
						--查找当前供应商和循环量
						select @MSEffSupplier = EffSupplier, @MSTargetCycleQty = TargetCycleQty, @MSAccumulateQty = AccumulateQty,
						@MSVersion = [Version]
						from PRD_MultiSupplyGroup where GroupNo = @MultiSupplyGroup
						
						if isnull(@MSEffSupplier, '') = ''
						begin  --当前循环供应商没有指定
							select top 1 @MSEffSupplier = Supplier, @MSTargetCycleQty = CycleQty, @MSAccumulateQty = SpillQty 
							from PRD_MultiSupplySupplier where GroupNo = @MultiSupplyGroup and CycleQty > SpillQty and IsActive = 1 order by Seq, Id
							
							if isnull(@MSEffSupplier, '') = ''
							begin
								set @MSCycleRowId = @MSCycleRowId + 1
								continue
							end
							else
							begin
								update PRD_MultiSupplyGroup set EffSupplier = @MSEffSupplier, TargetCycleQty = @MSTargetCycleQty, AccumulateQty = @MSAccumulateQty, [Version] = @MSVersion + 1,
								LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, LastModifyDate = @DateTimeNow
								where GroupNo = @MultiSupplyGroup and [Version] = @MSVersion

								if @@rowcount = 0
								begin
									RAISERROR(N'双轨组已经被更新。', 16, 1)
								end
								
								set @MSVersion = @MSVersion + 1
							end
						end	
						
						--查找整车、替代组汇总插入临时表
						truncate table #tempMSGroupTargetOrder
						insert into #tempMSGroupTargetOrder(OrderNo, MultiSupplyGroup)
						select bom.OrderNo, bom.MultiSupplyGroup
						from #tempOrderBomDet as bom
						inner join PRD_MultiSupplyItem as item on bom.Item = item.Item and bom.MultiSupplyGroup = item.GroupNo --and bom.SubstituteGroup = item.SubstituteGroup
						where bom.MultiSupplyGroup = @MultiSupplyGroup and OrderQty > 0
						group by bom.Seq, bom.OrderNo, bom.MultiSupplyGroup
						order by bom.Seq, bom.OrderNo
						
						if exists(select top 1 MSGroupTargetOrderRowId from #tempMSGroupTargetOrder)
						begin
							--查找循环开始和结束标识
							select @MSGroupTargetOrderRowId = MIN(MSGroupTargetOrderRowId), @MaxMSGroupTargetOrderRowId = MAX(MSGroupTargetOrderRowId) from #tempMSGroupTargetOrder
							
							--循环查找整车+工位+替代物料组并更新双轨零件的数量
							while (@MSGroupTargetOrderRowId > 0 and @MSGroupTargetOrderRowId <= @MaxMSGroupTargetOrderRowId) 
							begin
								--查找同一台车同替代组下有多少个双轨物料
								truncate table #tempMSOrderBomDetId
								insert into #tempMSOrderBomDetId(RowId, Item, MultiSupplyGroup, OrderQty)
								select det.RowId, det.Item, det.MultiSupplyGroup, det.OrderQty
								from #tempOrderBomDet as det
								inner join #tempMSGroupTargetOrder as sub on det.OrderNo = sub.OrderNo and det.MultiSupplyGroup = sub.MultiSupplyGroup
								where sub.MSGroupTargetOrderRowId = @MSGroupTargetOrderRowId and det.OrderQty > 0
								
								select @MSOrderBomDetCount = COUNT(distinct Item) from #tempMSOrderBomDetId
								if @MSOrderBomDetCount = 1
								begin	--双轨零件号相同，再次更新BOM指定供应商
									--不管是否溢出都是当前双轨供应商供货
									if exists(select top 1 * from #tempMSOrderBomDetId as bom 
												inner join PRD_MultiSupplyItem as item on bom.MultiSupplyGroup = item.GroupNo and bom.Item = item.Item
												where item.Supplier = @MSEffSupplier)
									begin
										update #tempOrderBomDet set ManufactureParty = SUBSTRING(@MSEffSupplier, 1, 3) where RowId = (select top 1 RowId from #tempMSOrderBomDetId)														
									end
									--else
									--begin  --ErrorLog: 106.没有找到当前多轨供应商的物料
									--	update #tempOrderBomDet set ErrorId = 106, ErrorMsg = @MSEffSupplier where RowId = (select top 1 RowId from #tempMSOrderBomDetId)
									--end
								end
								else
								begin  --双轨零件不同
									--不管是否溢出都是当前双轨供应商供货
									if exists(select top 1 * from #tempMSOrderBomDetId as ms
												inner join PRD_MultiSupplyItem as item on ms.Item = item.Item and ms.MultiSupplyGroup = item.GroupNo
												where item.Supplier = @MSEffSupplier )
									begin  --有当前多轨供应商供的物料，把不是当前供应商的BOM用量更新为0
										update ms set OrderQty = 0
										from #tempMSOrderBomDetId as ms
										inner join PRD_MultiSupplyItem as item on ms.Item = item.Item and ms.MultiSupplyGroup = item.GroupNo
										where item.GroupNo = @MultiSupplyGroup and item.Supplier <> @MSEffSupplier
									end
									--else
									--begin  --ErrorLog: 106.没有找到当前多轨供应商的物料
									--	update ms set ErrorId = 106, ErrorMsg = @MSEffSupplier --,OrderQty = 0 
									--	from #tempMSOrderBomDetId as ms
									--	inner join PRD_MultiSupplyItem as item on ms.Item = item.Item and ms.MultiSupplyGroup = item.GroupNo
									--	where item.GroupNo = @MultiSupplyGroup and item.Supplier <> @MSEffSupplier
									--end
								end
								
								--查找双轨物料用量
								select @MSOrderQty = SUM(OrderQty) from #tempMSOrderBomDetId
								
								if @MSTargetCycleQty - @MSAccumulateQty > @MSOrderQty
								begin  --剩余循环量满足
									--更新双轨供应商零件的用量
									set @MSAccumulateQty = @MSAccumulateQty + @MSOrderQty
								end
								else
								begin  --剩余循环量不满足，但是用量不能拆分到两个供应商上面，切换供应商
									--记录供应商溢出量
									update PRD_MultiSupplySupplier set SpillQty = SpillQty + @MSOrderQty - (@MSTargetCycleQty - @MSAccumulateQty),AccumulateQty = AccumulateQty + @MSAccumulateQty + @MSOrderQty,
									LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, LastModifyDate = @DateTimeNow, [Version] = [Version] + 1
									where GroupNo = @MultiSupplyGroup and Supplier = @MSEffSupplier
									
									if @@rowcount = 0
									begin
										set @ErrorMsg = N'双轨组' + @MultiSupplyGroup + N'供应商' + @MSEffSupplier + N'没有找到。'
										RAISERROR(@ErrorMsg, 16, 1)
									end
									
									--切换供应商
									while 1 = 1
									begin 
										--查找供应商顺序号
										select @MSId = Id, @MSSeq = Seq from PRD_MultiSupplySupplier where GroupNo = @MultiSupplyGroup and Supplier = @MSEffSupplier
									
										if exists(select top 1 Supplier from PRD_MultiSupplySupplier where GroupNo = @MultiSupplyGroup and Seq > @MSSeq and IsActive = 1 and Id <> @MSId order by Seq)
										begin
											--查找下一个供应商
											select top 1 @MSEffSupplier = Supplier, @MSTargetCycleQty = CycleQty, @MSAccumulateQty = SpillQty
											from PRD_MultiSupplySupplier where GroupNo = @MultiSupplyGroup and Seq > @MSSeq and IsActive = 1 and Id <> @MSId order by Seq
										end
										else if exists(select top 1 Supplier from PRD_MultiSupplySupplier where GroupNo = @MultiSupplyGroup and Seq <= @MSSeq and IsActive = 1 and Id <> @MSId order by Seq)
										begin  --已经循环到最后一个供应商，从第一个开始
											select top 1 @MSEffSupplier = Supplier, @MSTargetCycleQty = CycleQty, @MSAccumulateQty = SpillQty 
											from PRD_MultiSupplySupplier where GroupNo = @MultiSupplyGroup and IsActive = 1 and Id <> @MSId order by Seq
										end
										else
										begin
											set @ErrorMsg = N'双轨组' + @MultiSupplyGroup + '没有有效的供应商。'
											RAISERROR(@ErrorMsg, 16, 1)
										end
							
										if @MSTargetCycleQty > @MSAccumulateQty
										begin --切换供应商，跳出循环
											break
										end
										else
										begin  --剩余循环量不足（溢出量大于等于循环量），再切换到下一个供应商
											--记录供应商溢出量，溢出量等于累计量 - 循环量的	
											update PRD_MultiSupplySupplier set SpillQty = (@MSAccumulateQty - @MSTargetCycleQty),
											LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, LastModifyDate = @DateTimeNow, [Version] = [Version] + 1
											where GroupNo = @MultiSupplyGroup and Supplier = @MSEffSupplier
											--再次循环
											
											if @@rowcount = 0
											begin
												set @ErrorMsg = N'双轨组' + @MultiSupplyGroup + N'供应商' + @MSEffSupplier + N'没有找到。'
												RAISERROR(@ErrorMsg, 16, 1)
											end
										end
									end
								end
								
								--更新BOM的双轨零件用量
								update bom set OrderQty = ms.OrderQty, ErrorId = case when ms.ErrorId IS not null then ms.ErrorId else bom.ErrorId end, ErrorMsg = case when ms.ErrorId IS not null then ms.ErrorMsg else bom.ErrorMsg end
								from #tempOrderBomDet as bom
								inner join #tempMSOrderBomDetId as ms on bom.RowId = ms.RowId
								
								set @MSGroupTargetOrderRowId = @MSGroupTargetOrderRowId + 1
							end
						end
						
						--把溢出量更新为0
						update PRD_MultiSupplySupplier set SpillQty = 0,
						LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, LastModifyDate = @DateTimeNow, [Version] = [Version] + 1
						where GroupNo = @MultiSupplyGroup and Supplier = @MSEffSupplier
						
						if @@rowcount = 0
						begin
							set @ErrorMsg = N'双轨组' + @MultiSupplyGroup + N'供应商' + @MSEffSupplier + N'没有找到。'
							RAISERROR(@ErrorMsg, 16, 1)
						end
											
						--更新双轨组
						update PRD_MultiSupplyGroup set EffSupplier = @MSEffSupplier, TargetCycleQty = @MSTargetCycleQty, AccumulateQty = @MSAccumulateQty, 
						LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, LastModifyDate = @DateTimeNow, [Version] = @MSVersion + 1
						where GroupNo = @MultiSupplyGroup and [Version] = @MSVersion
						
						if @@rowcount = 0
						begin
							RAISERROR(N'双轨组已经被更新。', 16, 1)
						end
						
						set @MSCycleRowId = @MSCycleRowId + 1
					end
				end
				-----------------------------↑更新双轨-----------------------------
				



				-----------------------------↓插入Bom-----------------------------
				declare @OrderBomCount int
				declare @BeginOrderBomId bigint
				declare @NextOrderBomId bigint = 1
				
				--查找订单Bom数量
				select @OrderBomCount = COUNT(*) from #tempOrderBomDet
				--锁定OrderBomId字段标识
				exec USP_SYS_BatchGetNextId 'ORD_OrderBomDet', @OrderBomCount, @NextOrderBomId output
				--查找开始标识
				set @BeginOrderBomId = @NextOrderBomId - @OrderBomCount
				
				select RowId, ROW_NUMBER() over (order by RowId) + @BeginOrderBomId as OrderBomId into #tempOrderBomId
				from #tempOrderBomDet order by RowId
				
				insert into ORD_OrderBomDet_4 (
				Id,
				OrderNo,
				OrderType,
				OrderSubType,
				OrderDetId,
				OrderDetSeq,
				Seq,
				Item,
				ItemDesc,
				Uom,
				BaseUom,
				ManufactureParty,
				Op,
				OpRef,
				OrderQty,
				OrgOrderQty,
				UnitQty,
				BomUnitQty,
				IsPrint,
				BackFlushMethod,
				FeedMethod,
				IsAutoFeed,
				IsScanHu,
				CreateUser,
				CreateUserNm,
				CreateDate,
				LastModifyUser,
				LastModifyUserNm,
				LastModifyDate,
				[Version],
				PartyFrom,
				PartyTo,
				SubstituteGroup,
				MultiSupplyGroup,
				UPGCode,
				PONo,
				POLineNo,
				Location,
				VanDiffId,
				EOSwitchId,
				EONo,
				POChangeId,
				ItemConsumeId,
				EOCountingDown,
				FlowStrategy,
				IsCreateOrder,
				BomId,
				Shelf,
				POCountingDown
				)
				select 
				bi.OrderBomId,
				@OrderNo,
				4,                           --类型，4生产单
				0,                           --子类型，0正常
				@OrderDetId,
				1,
				0,                           --BOM顺序号
				b.Item,
				b.ItemDesc,
				b.Uom,
				b.Uom,
				b.ManufactureParty,
				b.Op,
				b.OpRef,
				b.OrderQty,
				b.OrgOrderQty,
				1,
				b.OrderQty,
				0,
				0,
				0,
				0,
				0,
				@CreateUserId,
				@CreateUserNm,
				@DateTimeNow,
				@CreateUserId,
				@CreateUserNm,
				@DateTimeNow,
				1,
				@Region,
				@Region,
				b.SubstituteGroup,
				b.MultiSupplyGroup,
				b.UPGCode,
				b.PONo,
				b.POLineNo,
				b.Location,
				b.VanDiffId,
				b.EOSwitchId,
				b.EONo,
				b.POChangeId,
				0,                              --厂内外消化档
				ISNULL(b.EOCountingDown, 0),
				b.FlowStrategy,
				0,
				b.BomId,
				b.Shelf,
				ISNULL(b.POCountingDown, 0)
				from #tempOrderBomDet as b
				inner join #tempOrderBomId as bi on b.RowId = bi.RowId
				-----------------------------↑插入Bom-----------------------------
				
				-----------------------------↑新增新生产线Bom-----------------------------
				
				
				set @PLChangeRowId = @PLChangeRowId + 1
			end
			
			drop table #tempLocTo
			drop table #tempOrderBomDet
			
			--更新总装工艺流程
			update mstr set Routing = tplc.Routing
			from ORD_OrderMstr_4 as mstr inner join #tempProdLineChange as tplc on mstr.OrderNo = tplc.OrderNo
		end
		-----------------------------↑生产线变更-----------------------------
		
		
		
		
		-----------------------------↓更新整车物料号-----------------------------
		--更新订单明细的整车物料号
		update det set Item = tscp.ProdItem, ItemDesc = i.desc1
		from ORD_OrderDet_4 as det 
		inner join ORD_OrderMstr_4 as mstr on det.OrderNo = mstr.OrderNo
		inner join #tempScanControlPoint as tscp on tscp.OrderNo = mstr.OrderNo
		inner join MD_Item as i on tscp.ProdItem = i.Code
		where tscp.ProdItem <> det.Item
		
		--更新车序表的生产线
		--update seq set ProdLine = tscp.ProdLine
		--from ORD_OrderMstr_4 as mstr
		--inner join ORD_OrderSeq_4 as seq on mstr.TraceCode = seq.TraceCode and mstr.Flow = seq.ProdLine
		--inner join #tempScanControlPoint as tscp on tscp.OrderNo = mstr.OrderNo
		--where tscp.ProdItem <> mstr.Dock
		
		--更新生产单的整车物料号、颜色、生产代码
		update mstr set Dock = tscp.ProdItem,   --整车物料号
		CKDLot = scp.Color,   --颜色
		ProdCode = scp.ProdCode,  --生产代码
		Flow = tscp.ProdLine  --生产线
		from ORD_OrderMstr_4 as mstr
		inner join #tempScanControlPoint as tscp on tscp.OrderNo = mstr.OrderNo
		inner join MES_ScanControlPoint as scp on scp.Id = tscp.ScanControlPointId
		where tscp.ProdItem <> mstr.Dock
		-----------------------------↑更新整车物料号-----------------------------



		-----------------------------↓生成JIT车序/更新车序-----------------------------	
		declare @CycleProdLineId int
		declare @MaxProdLineId int
		--declare @ProdLine varchar(50)
		declare @ProdLineRegion varchar(50)
		declare @TaktTime int  --节拍时间（秒）
		
		Create table #tempJITOrderSeq
		(
			RowId int identity(1, 1),
			CPRowId int,
			TraceCode varchar(50),
			StartDate datetime,
			JITSeq int
		)
		
		select distinct identity(int, 1, 1) as RowId, ProdLine into #tempProdLine
		from #tempScanControlPoint order by ProdLine
		
		select @MaxProdLineId = MAX(RowId), @CycleProdLineId = MIN(RowId) from #tempProdLine
		
		while @MaxProdLineId >= @CycleProdLineId
		begin  --根据生产线循环
			declare @CycleCPRowId int = 0
			declare @MaxCPRowId int = 0
			declare @CPRowId int = 0
			declare @TraceCode varchar(50) = null
			declare @OrderSeqId int = 0
			declare @MaxSeq int = 0
			declare @OldStartDate datetime = null
			declare @NewStartDate datetime = null
			declare @JITSeq int = 0
			declare @ProdCode varchar(50) = null
			declare @ProdSeq varchar(50) = null
			
			--获取循环的生产线
			select @ProdLine = ProdLine from #tempProdLine where RowId = @CycleProdLineId
			
			--按生产线获取车头，根据过点时间排序
			truncate table #tempJITOrderSeq
			insert into #tempJITOrderSeq(CPRowId, TraceCode, StartDate, JITSeq)
			select tscp.RowId, tscp.TraceCode, tscp.StartDate, tscp.JITSeq
			from #tempScanControlPoint as tscp 
			where tscp.ProdLine = @ProdLine
			order by tscp.ScanDateTime, tscp.ScanControlPointId
			
			select @CycleCPRowId = Min(RowId), @MaxCPRowId = Max(RowId) from #tempJITOrderSeq
			while @CycleCPRowId <= @MaxCPRowId
			begin  --循环过点信息（包含空车），更新车序和JIT车序
				select @CPRowId = CPRowId, @TraceCode = TraceCode, @NewStartDate = StartDate, @JITSeq = JITSeq
				from #tempJITOrderSeq where RowId = @CycleCPRowId
				
				if (@OldStartDate is null or @OldStartDate <> @NewStartDate)
				begin  --查找生产线最大序号，顺序号前缀为50代表过JIT点
					if Exists (select top 1 Id from ORD_OrderSeq_4 where ProdLine = @ProdLine and Seq
								between Cast('50' + CONVERT(varchar(8) , @NewStartDate, 112) + '0000' as bigint) and Cast('50' + Cast(Cast(CONVERT(varchar(8) , @NewStartDate, 112) as bigint) + 1 as varchar(8)) + '0000' as bigint))
					begin  --下线日期已经有生产单
						select top 1 @MaxSeq = cast(substring(cast(Seq as varchar(14)), 11, 4) as int) from ORD_OrderSeq_4 where ProdLine = @ProdLine and Seq 
						between Cast('50' + CONVERT(varchar(8) , @NewStartDate, 112) + '0000' as bigint) and Cast('50' + Cast(Cast(CONVERT(varchar(8) , @NewStartDate, 112) as bigint) + 1 as varchar(8)) + '0000' as bigint)
						order by Seq desc
					end
					
					set @OldStartDate = @NewStartDate
				end
				
				--最大序号+1
				set @MaxSeq = @MaxSeq + 1
		
				if @TraceCode is null
				begin  --空车，插入一条空车序
					--锁定OrderDetId字段标识字段
					exec USP_SYS_GetNextId 'ORD_OrderSeq', @OrderSeqId output
				
					--插入空车车序
					insert into ORD_OrderSeq_4(Id, ProdLine, TraceCode, OrderType, PartyFrom, PartyTo, Seq, 
					SapSeq, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, [Version])
					select @OrderSeqId, mstr.Code, isnull(tscp.TraceCode, scp.ProdCode + scp.ProdSeq), 4, mstr.PartyFrom, mstr.PartyTo, cast('50' + CONVERT(varchar(8) , @NewStartDate, 112 ) + REPLICATE('0',4-len(@MaxSeq))+cast(@MaxSeq as varchar) as bigint),
					@JITSeq, @CreateUserId, @CreateUserNm, @DateTimeNow, @CreateUserId, @CreateUserNm, @DateTimeNow, 1
					from #tempScanControlPoint as tscp
					inner join MES_ScanControlPoint as scp on tscp.ScanControlPointId = scp.Id
					inner join SCM_FlowMstr as mstr on scp.ProdCode = mstr.BlankSeqProdCode
					where tscp.RowId = @CPRowId
					
					if @@ROWCOUNT <> 1
					begin
						set @ErrorMsg = N'插入空车失败，可能是生产线没有设置空车生产代码。'
						RAISERROR(@ErrorMsg, 16, 1)
					end
				end
				else
				begin  --更新车序和JIT车序					
					update ORD_OrderSeq_4 set ProdLine = @ProdLine, Seq = cast('50' + CONVERT(varchar(8) , @NewStartDate, 112 ) + REPLICATE('0',4-len(@MaxSeq))+cast(@MaxSeq as varchar) as bigint), SapSeq = @JITSeq,
					[Version] = [Version] + 1, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm
					where TraceCode = @TraceCode
					
					if @@ROWCOUNT <> 1
					begin
						select @ProdCode = scp.ProdCode, @ProdSeq = scp.ProdSeq 
						from #tempScanControlPoint as tscp
						inner join MES_ScanControlPoint as scp on tscp.ScanControlPointId = scp.Id
						where tscp.RowId = @CPRowId
						
						set @ErrorMsg = N'生产线' + @ProdLine + N'生产代码' + @ProdCode + N'车身号' + @ProdSeq + N'更新JIT过点出错。'
						RAISERROR(@ErrorMsg, 16, 1)
					end
				end
				
				set @CycleCPRowId = @CycleCPRowId + 1
			end
		
			set @CycleProdLineId = @CycleProdLineId + 1
		end
		-----------------------------↑生成JIT车序/更新车序-----------------------------	
		
		
		
		
		-----------------------------↓PO发布变更零件-----------------------------
		create table #tempOrderBomDet4PO
		(
			RowId int identity(1, 1),
			OrderNo varchar(50),
			OrderBomDetId int,
			Item varchar(50),
			ProdCode varchar(50),
			ProdSeq varchar(50),
			OrderQty decimal,
			FlowStrategy tinyint,
			VanPO varchar(50),
			SapSeq bigint
		)
		
		CREATE TABLE #tempPOOrder  --PO变更更新车身代码＋车身号循环表
		(
			POOrderCycleId int identity(1, 1),
			OrderNo varchar(50),
			ProdCode varchar(50),
			ProdSeq varchar(50)
		)
		
		CREATE TABLE #tempOpRef
		(
			Op int,             --工序
			OpRef varchar(50),  --工位
			TaktCount int,      --节拍数
		)
		
		CREATE TABLE #tempOrderOpTime
		(
			OrderNo varchar(50),
			Op int,             --工序
			TaktCount int,      --节拍数
			CPTime datetime     --过点时间
		)
		
		--工作日历临时表
		create table #tempWorkingCalendarView
		(
			RowId int Identity(1, 1),
			WorkingDate datetime,
			DateFrom datetime,
			DateTo datetime
		)
		
		create table #tempNoConsumePO
		(
			RowId int Identity(1, 1),
			Id int,
			OldItem varchar(50), 
			NewItem varchar(50), 
			Qty decimal(18,8), 
			ConsumeQty  decimal(18,8), 
			IsVoidVanPO bit, 
			PONo varchar(50), 
			POLineNo varchar(50), 
			POChangeNo varchar(50)
		)
		
		select @MaxProdLineId = MAX(RowId), @CycleProdLineId = MIN(RowId) from #tempProdLine
		
		while @MaxProdLineId >= @CycleProdLineId
		begin  --循环生产线和开始日期
			select @ProdLine = ProdLine from #tempProdLine where RowId = @CycleProdLineId	
			select @TaktTime = TaktTime, @ProdLineRegion = PartyFrom from SCM_FlowMstr where Code = @ProdLine
				
			truncate table #tempOrderBomDet4PO
			insert into #tempOrderBomDet4PO(OrderNo, OrderBomDetId, Item, ProdCode, ProdSeq, OrderQty, FlowStrategy, VanPO, SapSeq)
			select mstr.OrderNo, bom.Id, bom.Item, mstr.ProdCode, mstr.ProdSeq, bom.OrderQty, bom.FlowStrategy, mstr.VanPO, seq.SapSeq
			from ORD_OrderBomDet_4 as bom
			inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
			inner join ORD_OrderSeq_4 as seq on mstr.TraceCode = seq.TraceCode
			where  mstr.OrderNo in (select OrderNo from #tempScanControlPoint where ProdLine = @ProdLine)
			and bom.FlowStrategy <> 1					
			
			truncate table #tempNoConsumePO
			insert into #tempNoConsumePO(Id, OldItem, NewItem, Qty, ConsumeQty, IsVoidVanPO, PONo, POLineNo, POChangeNo)
			select cast(po.Id as int) as Id, po.OldItem, po.NewItem, po.Qty, po.ConsumeQty, po.IsVoidVanPO, po.PONo, po.POLineNo, po.POChangeNo
			from PRD_POChange as po
			inner join (select distinct Item from #tempOrderBomDet4PO where FlowStrategy <> 1) as a on po.OldItem = a.Item
			where po.Qty > po.ConsumeQty and po.ProdLine = @ProdLine --and po.EffDate <= @DateTimeNow --不是根据当前时间计算是否启用PO，而是根据零件的需求时间
			
			--ErrorLog: 108.BOM已经包含PO变更的新件
			INSERT INTO ERR_UpdateVanOrderBom
			(ProdLine, ProdLineType, StartDate, Item, ItemDesc, OrderQty, PONo, POLineNo, ErrorId, ErrorMsg, CreateDate)
            select @ProdLine, 1, @DateTimeNow, po.NewItem, i.Desc1, po.Qty, po.PONo, po.POLineNo, 108, po.POChangeNo, @DateTimeNow
			from #tempNoConsumePO as po inner join #tempOrderBomDet4PO as bom on po.NewItem = bom.Item
			inner join MD_Item as i on po.NewItem = i.Code
            where bom.OrderQty > 0
			
			if exists(select top 1 RowId from #tempNoConsumePO)
			begin
				declare @POCycleRowId int
				declare @MaxPOCycleRowId int
				declare @ThisPOConsumeQty int
				declare @OldPOItem varchar(50)
				declare @NewPOItem varchar(50)
				declare @NewPOItemDesc varchar(100)
				declare @NewPOItemRefCode varchar(50)
				declare @NewPOItemUom varchar(5)
				declare @POQty decimal
				declare @CosumePOQty decimal
				declare @RemainPOQty decimal
				declare @IsVoidVanPO bit
				declare @POPONo varchar(50)
				declare @POPOLineNo varchar(50)
				declare @POVersion int
				declare @POEffDate datetime
				declare @POOrderCycleId int
				declare @MaxPOOrderCycleId int
				declare @POOrderNo varchar(50)
				declare @POProdCode varchar(50)
				declare @POProdSeq varchar(50)
				declare @POId int
				declare @POMultiSupplyGroup varchar(50)
				declare @POMaxOrderBomDetId int
				declare @POBeginOrderBomDetId int
				declare @POOrderBomDetCount int
				declare @POFirstVanNo varchar(50)
				declare @POCPTime datetime
				
				--循环PO发布变更档
				select @POCycleRowId = MIN(RowId), @MaxPOCycleRowId = MAX(RowId) from #tempNoConsumePO
				while(@POCycleRowId <= @MaxPOCycleRowId)
				begin
					--PO变更是否开始启用
					declare @BeginConsume bit = 0
					
					--获取单条PO发布变更档标识
					select @POId = Id from #tempNoConsumePO where RowId = @POCycleRowId
					
					--PO发布变更装车台数清零
					set @ThisPOConsumeQty = 0
					select @OldPOItem = po.OldItem, @NewPOItem = po.NewItem,
					@NewPOItemDesc = i.Desc1, @NewPOItemRefCode = i.RefCode, @NewPOItemUom = i.Uom,
					@POQty = po.Qty, @CosumePOQty = po.ConsumeQty,
					@RemainPOQty = po.Qty - po.ConsumeQty,  @IsVoidVanPO = po.IsVoidVanPO,
					@POPONo = po.PONo, @POPOLineNo = po.POLineNo, @POVersion = po.[Version],
					@POEffDate = po.EffDate, @POFirstVanNo = FirstVanNo
					from PRD_POChange as po inner join MD_Item as i on po.NewItem = i.Code
					where Id = @POId
					
					if @CosumePOQty > 0
					begin
						set @BeginConsume = 1
					end
					
					--获取变更零件的多轨组号
					set @POMultiSupplyGroup = null
					select top 1 @POMultiSupplyGroup = GroupNo
					from PRD_MultiSupplyItem where Item = @NewPOItem
					
					--清空PO件更新BOM循环临时表
					truncate table #tempPOOrder
					--查找需要更新的订单BOM，PO变更是按照车辆台数计算消耗，所以要先按车辆代码+车身号汇总有多少台车符合PO变更要求
					if @IsVoidVanPO = 0
					begin  --不避开订单车
						insert into #tempPOOrder (OrderNo, ProdCode, ProdSeq)
						select OrderNo, ProdCode, ProdSeq from #tempOrderBomDet4PO 
						where Item = @OldPOItem and OrderQty > 0 and FlowStrategy <> 1 
						group by OrderNo, ProdCode, ProdSeq, SapSeq order by SapSeq
					end
					else
					begin  --避开订单车
						insert into #tempPOOrder (OrderNo, ProdCode, ProdSeq)
						select OrderNo, ProdCode, ProdSeq from #tempOrderBomDet4PO 
						where Item = @OldPOItem and OrderQty > 0 and FlowStrategy <> 1 and ISNULL(VanPO, '') = '' 
						group by OrderNo, ProdCode, ProdSeq, SapSeq order by SapSeq
					end
					
					if exists(select top 1 POOrderCycleId from #tempPOOrder)
					begin
						select @POOrderCycleId = MIN(POOrderCycleId), @MaxPOOrderCycleId = MAX(POOrderCycleId) from #tempPOOrder
						
						--循环需要更新的车辆代码+车身号
						while (@POOrderCycleId > 0 and @POOrderCycleId <= @MaxPOOrderCycleId) 
						begin
							if @RemainPOQty > 0
							begin
								set @ThisPOConsumeQty = @ThisPOConsumeQty + 1
								set @RemainPOQty = @RemainPOQty - 1
								
								select @POOrderNo = OrderNo, @POProdCode = ProdCode, @POProdSeq = ProdSeq from #tempPOOrder where POOrderCycleId = @POOrderCycleId
								
								if @BeginConsume = 0
								begin --PO变更没有开始执行，计算本台车的旧件工位，根据旧件工位的过点时间（需求时间）判断是否需要执行PO变更
									set @POCPTime = null
									exec USP_Busi_PreviewPOItemCPTime @ProdLine, @OldPOItem, @POOrderNo, @POCPTime output
									
									if @POCPTime >= @POEffDate
									begin  --过点时间大于PO变更的生效时间
										set @BeginConsume = 1
									end
									else
									begin  --过点时间小于PO变更的生效时间
										continue
									end
								end
								
								--查找新件数量
								select @POOrderBomDetCount = COUNT(*) from #tempOrderBomDet4PO as tbom
								inner join ORD_OrderBomDet_4 as bom on tbom.OrderBomDetId = bom.Id
								where tbom.ProdCode = @POProdCode and tbom.ProdSeq = @POProdSeq and bom.Item = @OldPOItem and bom.OrderQty > 0
								
								--锁定OrderBomDetId字段标识字段
								exec USP_SYS_BatchGetNextId 'ORD_OrderBomDet', @POOrderBomDetCount, @POMaxOrderBomDetId output
								
								--查找开始标识
								set @POBeginOrderBomDetId = @POMaxOrderBomDetId - @POOrderBomDetCount
							
								--按旧件订单数插入新件
								insert into ORD_OrderBomDet_4 (Id, OrderNo, OrderType, OrderSubType, OrderDetId, OrderDetSeq, 
								Seq, Item, ItemDesc, Uom, BaseUom, 
								ManufactureParty, Op, OpRef, OrderQty, OrgOrderQty, UnitQty, BomUnitQty, 
								IsPrint, BackFlushMethod, FeedMethod, IsAutoFeed, IsScanHu, 
								CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, [Version], 
								PartyFrom, PartyTo, SubstituteGroup, MultiSupplyGroup, UPGCode, PONo, POLineNo, Location, 
								VanDiffId, EOSwitchId, POChangeId, ItemConsumeId, EONo, EOCountingDown, FlowStrategy, IsCreateOrder, BomId, POCountingDown)
								select ROW_NUMBER() over (order by bom.Id) + @POBeginOrderBomDetId, bom.OrderNo, 4, 0, bom.OrderDetId, 1,
								bom.Seq, @NewPOItem as Item, @NewPOItemDesc as ItemDesc, @NewPOItemUom as Uom, @NewPOItemUom as BaseUom, --BOM单位就是基本单位
								null, bom.Op, bom.OpRef, bom.OrderQty, 0, 1, bom.OrderQty,
								0, 0, 0, 0, 0,
								@CreateUserId, @CreateUserNm, @DateTimeNow, @CreateUserId, @CreateUserNm, @DateTimeNow, 1,
								bom.PartyFrom, bom.PartyTo, null, @POMultiSupplyGroup, bom.UPGCode, null, null, bom.Location,
								0, 0, @POId, 0, null, 0, bom.FlowStrategy, 0, bom.BomId, @RemainPOQty
								from #tempOrderBomDet4PO as tbom
								inner join ORD_OrderBomDet_4 as bom on tbom.OrderBomDetId = bom.Id
								where tbom.ProdCode = @POProdCode and tbom.ProdSeq = @POProdSeq and bom.Item = @OldPOItem and bom.OrderQty > 0
							
								--更新工位余量，把已经计算的JIT量全部作为正数记录到工位余量中
								update orb set Qty = Qty + bom.orderQty, [Version] = orb.[Version] + 1, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm
								from SCM_OpRefBalance as orb inner join
								(select bom.Item, bom.OpRef, SUM(bom.OrderQty) as orderQty
								from ORD_OrderBomDet_4 as bom
								inner join #tempOrderBomDet4PO as tbom on bom.Id = tbom.OrderBomDetId
								where tbom.ProdCode = @POProdCode
								and tbom.ProdSeq = @POProdSeq
								and bom.Item = @OldPOItem
								and bom.OrderQty > 0
								and bom.IsCreateOrder = 1
								group by bom.Item, bom.OpRef) as bom on orb.Item = bom.Item and orb.OpRef = bom.OpRef
								
								--退回EO旧件库存
								update eo set ConsumeQty = ConsumeQty - bom.OrderQty, LastModifyDate = @DateTimeNow, 
								LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, [Version] = [Version] + 1
								from PRD_EOSwitch as eo
								inner join 
								(
								 select bom.EOSwitchId, SUM(bom.OrderQty) as OrderQty 
								 from ORD_OrderBomDet_4 as bom
								 inner join #tempOrderBomDet4PO as tbom on bom.Id = tbom.OrderBomDetId
								 where tbom.ProdCode = @POProdCode and tbom.ProdSeq = @POProdSeq
								 and bom.Item = @OldPOItem and bom.OrderQty > 0
								 group by EOSwitchId
								)as bom on eo.Id = bom.EOSwitchId
								
								--todo:退回已计算JIT的数量
								
								--把原旧件数量更新为0
								update bom set OrderQty = 0, POChangeId = @POId
								from ORD_OrderBomDet_4 as bom
								inner join #tempOrderBomDet4PO as tbom on bom.Id = tbom.OrderBomDetId
								where tbom.ProdCode = @POProdCode and tbom.ProdSeq = @POProdSeq
								and bom.Item = @OldPOItem and bom.OrderQty > 0
								
								if ISNULL(@POFirstVanNo, '') = ''
								begin  --记录首台车号
									set @POFirstVanNo = @POProdCode + @POProdSeq
								end
							end
							else
							begin
								break
							end
							
							set @POOrderCycleId = @POOrderCycleId + 1
						end
					end
					
					if @ThisPOConsumeQty > 0
					begin
						if @RemainPOQty = 0
						begin
							update PRD_POChange set ConsumeQty = ConsumeQty + @ThisPOConsumeQty, FirstVanNo = @POFirstVanNo,
							LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, LastModifyDate = @DateTimeNow,
							IsClose = 1, [Version] = @POVersion + 1
							where Id = @POId and [Version] = @POVersion
						end
						else
						begin
							update PRD_POChange set ConsumeQty = ConsumeQty + @ThisPOConsumeQty, FirstVanNo = @POFirstVanNo, 
							LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, LastModifyDate = @DateTimeNow,
							[Version] = @POVersion + 1
							where Id = @POId and [Version] = @POVersion
						end
						
						if @@rowcount = 0
						begin
							RAISERROR(N'PO发布变更档已经被更新。', 16, 1)
						end
					end
					
					set @POCycleRowId = @POCycleRowId + 1
				end
			end
			
			set @CycleProdLineId = @CycleProdLineId + 1
		end
		-----------------------------↑PO发布变更零件-----------------------------	


		
		-----------------------------↓释放生产单-----------------------------
		update mstr set Status = 1, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, LastModifyDate = @DateTimeNow,
		ReleaseUser = @CreateUserId, ReleaseUserNm = @CreateUserNm, ReleaseDate = tscp.ScanDateTime, [Version] = [Version] + 1
		from ORD_OrderMstr_4 as mstr inner join #tempScanControlPoint as tscp on mstr.OrderNo = tscp.OrderNo
		-----------------------------↑释放生产单-----------------------------	
		
		
		
		
		
		--最后一次过点站别
		update mstr set BillAddrDesc = scp.ControlPoint, PauseTime = tscp.ScanDateTime, ExtraDmdSource = scp.VanSeries
		from ORD_OrderMstr_4 as mstr 
		inner join #tempScanControlPoint as tscp on mstr.OrderNo = tscp.OrderNo
		inner join MES_ScanControlPoint as scp on scp.Id = tscp.ScanControlPointId
		
		--更新过点扫描成功标记
		update MES_ScanControlPoint set Status = 2 where Id in (select ScanControlPointId from #tempScanControlPoint)
		commit tran	ScanJITControlPoint
	end try
	begin catch
		rollback tran ScanJITControlPoint_Point
		commit tran ScanJITControlPoint
		set @ErrorMsg = Error_Message()
		RAISERROR(@ErrorMsg, 16, 1)
	end catch
END
