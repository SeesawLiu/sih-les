SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_LE_MakeupSequenceOrder')
	DROP PROCEDURE USP_LE_MakeupSequenceOrder	
GO

CREATE PROCEDURE [dbo].[USP_LE_MakeupSequenceOrder]
(
	@ProdLine varchar(50),
	@TraceCode varchar(50),
	@CreateUserId int,
	@CreateUserNm varchar(100)
) --WITH ENCRYPTION
AS
BEGIN
	set nocount on
	declare @DateTimeNow datetime = GetDate()
	declare @Msg nvarchar(Max)
	declare @trancount int
	
	begin try
		declare @OrderNo varchar(50)
		declare @Seq bigint
		declare @SubSeq int
		declare @RowId int
		declare @MaxRowId int
		
		create table #tempMsg
		(
			Lvl tinyint,
			Msg varchar(500)
		)
		
		create table #tempSeqGroup
		(
			RowId int identity(1, 1),
			SeqGroup varchar(50),
			OpRef varchar(50)
		)
		
		create table #tempSeqFlow
		(
			RowId int identity(1, 1),
			Flow varchar(50),
			[Type] tinyint,
			PartyFrom varchar(50),
			PartyTo varchar(50),
			LocFrom varchar(50),
			LocTo varchar(50),
			WinTimeDiff decimal(18, 8),
			EMLeadTime decimal(18, 8),
			LeadTime decimal(18, 8),
			MRPWeight int,
			MRPTotal decimal(18, 8),
			MRPTotalAdj decimal(18, 8),
			DeliveryCount int,
			DeliveryBalance int,
			IsCreatePickList bit
		)
		
		create table #tempAssOpRef
		(
			AssOpRef varchar(50)
		)
		
		create table #tempWMSKitWorkCenter
		(
			Flow varchar(50),
			WorkCenter varchar(50)
		)
		
		create table #tempFlowDet
		(
			FlowDetRowId int identity(1, 1) Primary Key,
			Flow varchar(50),		
			Item varchar(50), 
			ItemDesc varchar(100),
			RefItemCode varchar(50),
			Uom varchar(5),
			UC decimal(18, 8),
			MinUC decimal(18, 8),
			UCDesc varchar(50),
			Container varchar(50),
			ContainerDesc varchar(50),
			MrpTotal decimal(18, 8),
			MrpTotalAdj decimal(18, 8),
			MrpWeight int,
			LocFrom varchar(50),
			LocTo varchar(50)
		)
		
		create table #tempMultiSupplierGroup
		(
			RowId int identity(1, 1),
			Item varchar(50)
		)
		
		create table #tempMultiSupplierItem
		(
			RowId int identity(1, 1),
			FlowDetRowId int,
			Flow varchar(50),
			Item varchar(50), 
			MSGRowId int,
			MrpTotal decimal(18, 8),
			MrpTotalAdj decimal(18, 8),
			MrpWeight int,
			DeliveryCount int,
			DeliveryBalance int,
		)
		
		create table #tempSortedMultiSupplierItem
		(
			GID int, 
			FlowDetRowId int,
			Flow varchar(50)
		)
		
		create table #tempOrderBom
		(
			Id int identity(1, 1) Primary Key,
			OrderNo varchar(50),
			Seq bigint,
			SubSeq int,
			Flow varchar(50),
			Item varchar(50), 
			ItemDesc varchar(100),
			RefItemCode varchar(50),
			Uom varchar(5),
			UC decimal(18, 8),
			MinUC decimal(18, 8),
			UCDesc varchar(50),
			Container varchar(50),
			ContainerDesc varchar(50),
			OpRef varchar(50), 
			OrderQty decimal(18, 8), 
			Location varchar(50),
			CPTime datetime,
			BomId int, 
			ManufactureParty varchar(50),
			LocFrom varchar(50),
			LocTo varchar(50),
			ZOPWZ varchar(50),
			ZOPID varchar(50),
			ZOPDS varchar(50)
		)
		
		create table #tempSeqOrderDet
		(
			RowId int identity(1, 1),
			Id int,
			Seq int, 
			ExtNo varchar(50), 
			ExtSeq varchar(50),
			Item varchar(50), 
			ItemDesc varchar(100), 
			RefItemCode varchar(50), 
			ReqQty decimal(18, 8), 
			OrderQty decimal(18, 8), 
			Uom varchar(5), 
			UC decimal(18, 8), 
			MinUC decimal(18, 8),
			UCDesc varchar(50),
			Container varchar(50),
			ContainerDesc varchar(50),
			ManufactureParty varchar(50),
			OpRef varchar(50),
			LocFrom varchar(50),
			LocTo varchar(50),
			CPTime datetime,
			ZOPWZ varchar(50),
			ZOPID varchar(50),
			ZOPDS varchar(50),
			IsItemConsume bit
		)
		
		create table #tempItemConsume
		(
			RowId int identity(1, 1),
			Id int,
			Item varchar(50),
			RemainQty decimal(18, 8),
			[Version] int
		)
		
		create table #tempSeqOrderTrace
		(
			TraceCode varchar(50), 
			ProdLine varchar(50), 
			SeqGroup varchar(50), 
			DeliveryCount varchar(50)
		)
		
		create table #tempICSeqOrderDet
		(
			RowId int identity(1, 1),
			SeqRowId int
		)
		
		select @OrderNo = OrderNo, @Seq = Seq, @SubSeq = SubSeq from ORD_OrderSeq where TraceCode = @TraceCode and ProdLine = @ProdLine
		
		if @OrderNo is null
		begin
			set @Msg = N'整车生产线' + @ProdLine + N'没有找到Van号' + @TraceCode + N'的生产单。'
			RAISERROR(@Msg, 16, 1) 
		end
		
		insert into #tempSeqGroup(SeqGroup)
		select seq.Code
		from SCM_SeqGroup as seq
		left join ORD_SeqOrderTrace as trace on trace.TraceCode = @TraceCode and seq.Code = trace.SeqGroup
		where seq.ProdLine = @ProdLine and trace.Id is null
		and (seq.PrevSeq > @Seq or (seq.PrevSeq = @Seq and seq.PrevSubSeq >= @SubSeq))
		
		if not exists(select top 1 1 from #tempSeqGroup)
		begin
			set @Msg = N'整车生产线' + @ProdLine + N' Van号' + @TraceCode + N'没有未发出的排序单。'
			RAISERROR(@Msg, 16, 1) 
		end
		
		select @RowId = MIN(RowId), @MaxRowId = MAX(RowId) from #tempSeqGroup
		
		while @RowId <= @MaxRowId
		begin
			set @trancount = @@trancount
			
			begin try
				if @trancount = 0
				begin
					begin tran
				end
				
				-----------------------------↓循环生成排序单-----------------------------
				declare @SeqGroup varchar(50) = null
				declare @AssOpRef varchar(50) = null
				declare @ProdLineRegion varchar(50) = null
				declare @SplitSymbol1 char(1) = ','
				declare @SplitSymbol2 char(1) = '|'
				declare @IsWMSKitOrder bit = null
				
				-----------------------------↓获取排序组和排序路线-----------------------------
				--获取单个排序组
				select @SeqGroup = SeqGroup, @AssOpRef = OpRef from #tempSeqGroup where RowId = @RowId
				
				--生产线区域
				select @ProdLineRegion = PartyFrom from SCM_FlowMstr where Code = @ProdLine
				
				--获取排序路线
				truncate table #tempSeqFlow
				insert into #tempSeqFlow(Flow, [Type], PartyFrom, PartyTo, LocFrom, LocTo, WinTimeDiff, EMLeadTime, LeadTime, 
				MRPWeight, MRPTotal, MRPTotalAdj, 
				DeliveryCount, 
				DeliveryBalance,
				IsCreatePickList) 
				select stra.Flow, mstr.[Type], mstr.PartyFrom, mstr.PartyTo, mstr.LocFrom, mstr.LocTo, stra.WinTimeDiff, stra.EMLeadTime, stra.LeadTime, 
				stra.MRPWeight, ISNULL(stra.MRPTotal, 0), ISNULL(stra.MRPTotalAdj, 0), 
				(ISNULL(stra.MRPTotal, 0) + ISNULL(stra.MRPTotalAdj, 0)) / (case when stra.MRPWeight = 0 or stra.MRPWeight is null then 1 else stra.MRPWeight end) as DeliveryCount,
				(ISNULL(stra.MRPTotal, 0) + ISNULL(stra.MRPTotalAdj, 0)) % (case when stra.MRPWeight = 0 or stra.MRPWeight is null then 1 else stra.MRPWeight end) as DeliveryBalance,
				CASE WHEN mstr.Type = 2 and stra.IsCreatePL = 1 THEN 1 ELSE 0 END as IsCreatePickList
				from SCM_FlowStrategy as stra
				inner join SCM_FlowMstr as mstr on stra.Flow = mstr.Code
				where stra.SeqGroup = @SeqGroup and stra.Strategy = 4 and mstr.IsActive = 1
				
				if (select COUNT(1) from #tempSeqFlow where MRPWeight > 0) > 1
				begin  --如果设置了循环量,根据循环量选取路线
					-------------------↓根据循环量选取路线明细-----------------------
					declare @SelectedFlow varchar(50) = null
					
					select top 1 @SelectedFlow = Flow from #tempSeqFlow where MRPWeight >= 0
						order by DeliveryCount asc, MRPWeight desc, DeliveryBalance desc
				
					delete from #tempSeqFlow where MRPWeight >= 0 and Flow <> @SelectedFlow
					-------------------↑根据循环量选取路线明细-----------------------
				end
				
				if exists(select top 1 1 from CUST_ProductLineMap as map
						 inner join #tempSeqFlow as transFlow on map.TransFlow = transFlow.Flow)
				begin   --变速器
					set @IsWMSKitOrder = 1
				end
				else if exists(select top 1 1 from CUST_ProductLineMap as map
						 inner join #tempSeqFlow as saddleFlow on map.SaddleFlow = saddleFlow.Flow)
				begin	--鞍座
					set @IsWMSKitOrder = 1
				end
				else
				begin
					set @IsWMSKitOrder = 0
				end
				-----------------------------↑获取排序组和排序路线-----------------------------
				
				if @IsWMSKitOrder = 0
				begin  --正常排序件
					-----------------------------↓循环安装工位插入缓存表中-----------------------------
					truncate table #tempAssOpRef
					if (charindex(@SplitSymbol1, @AssOpRef) <> 0)
						begin
						while(charindex(@SplitSymbol1, @AssOpRef) <> 0)
						begin
							insert #tempAssOpRef(AssOpRef) values (substring(@AssOpRef, 1, charindex(@SplitSymbol1, @AssOpRef) - 1))
							set @AssOpRef = stuff(@AssOpRef, 1, charindex(@SplitSymbol1, @AssOpRef), ' ')
						end
					end
					else if (charindex(@SplitSymbol2, @AssOpRef) <> 0)
					begin
						while(charindex(@SplitSymbol2, @AssOpRef) <> 0)
						begin
							insert #tempAssOpRef(AssOpRef) values (substring(@AssOpRef, 1, charindex(@SplitSymbol2, @AssOpRef) - 1))
							set @AssOpRef = stuff(@AssOpRef, 1, charindex(@SplitSymbol2, @AssOpRef), ' ')
						end
					end
					
					if (ISNULL(@AssOpRef, '') <> '')
					begin
						insert #tempAssOpRef(AssOpRef) values (@AssOpRef)
					end
					-----------------------------↑循环安装工位插入缓存表中-----------------------------
					
					-----------------------------↓获取排序路线明细，考虑了多厂商供货的情况-----------------------------
					
					-------------------↓获取所有排序路线明细-----------------------
					truncate table #tempFlowDet
					insert into #tempFlowDet(Flow, Item, ItemDesc, RefItemCode, Uom, UC, MinUC,
					UCDesc, Container, ContainerDesc, MrpTotal, MrpTotalAdj, MrpWeight, LocFrom, LocTo)
					select fdet.Flow as Flow, fdet.Item, i.Desc1 as ItemDesc, i.RefCode as RefItemCode, fdet.Uom, fdet.UC, fdet.MinUC, 
					fdet.UCDesc, fdet.Container, fdet.ContainerDesc, fdet.MrpTotal, fdet.MrpTotalAdj, fdet.MrpWeight, mstr.LocFrom, mstr.LocTo
					from SCM_FlowDet as fdet
					inner join SCM_FlowMstr as mstr on fdet.Flow = mstr.Code
					inner join SCM_FlowStrategy as stra on stra.Flow = fdet.Flow
					inner join MD_Item as i on fdet.Item = i.Code
					where stra.Flow in (select Flow from #tempSeqFlow) and stra.Strategy = 4 --and fdet.IsActive = 1
					-------------------↑获取所有排序路线明细-----------------------
					
					
					-------------------↓获取引用路线明细-----------------------
					insert into #tempFlowDet(Flow, Item, ItemDesc, RefItemCode, Uom, UC, MinUC,
					UCDesc, Container, ContainerDesc, MrpTotal, MrpTotalAdj, MrpWeight, LocFrom, LocTo)
					select mstr.Code as Flow, rDet.Item, i.Desc1 as ItemDesc, i.RefCode as RefItemCode, rDet.Uom, rDet.UC, rDet.MinUC,
					rDet.UCDesc, rDet.Container, rDet.ContainerDesc, rDet.MrpTotal, rDet.MrpTotalAdj, rDet.MrpWeight, mstr.LocFrom, mstr.LocTo
					from SCM_FlowMstr as mstr
					inner join SCM_FlowStrategy as stra on stra.Flow = mstr.Code
					inner join SCM_FlowDet as rDet on mstr.RefFlow = rDet.Flow
					inner join MD_Item as i on rDet.Item = i.Code
					where stra.Flow in (select Flow from #tempSeqFlow) and stra.Strategy = 4 --and rDet.IsActive = 1
					and not Exists(select top 1 1 from #tempFlowDet as tDet where tdet.Item = rdet.Item)
					-------------------↑获取引用路线明细-----------------------
						
						
					-------------------↓先用配额选择多供应商供货的路线-----------------------
					if (select COUNT(1) from #tempSeqFlow) > 1
					begin
						delete det
						from #tempFlowDet as det 
						inner join SCM_FlowMstr as mstr on det.Flow = mstr.Code
						inner join (select det.FlowDetRowId, det.Item, lq.Supplier, det.LocTo 
									from #tempFlowDet as det 
									inner join SCM_FlowMstr as mstr on det.Flow = mstr.Code
									inner join LE_QuotaSnapShot as lq on det.Item = lq.Item and mstr.PartyFrom = lq.Supplier
									where mstr.[Type] = 1) as cDet --当前供货的采购路线明细
									on det.Item = cDet.Item and det.FlowDetRowId <> cDet.FlowDetRowId and mstr.PartyFrom <> cdet.Supplier --and det.LocTo = cDet.LocTo
						where mstr.[Type] = 1   --只过滤掉采购的路线
					end
					-------------------↑先用配额选择多供应商供货的路线-----------------------
						
						
					-------------------↓按零件分组-----------------------
					truncate table #tempMultiSupplierGroup
					insert into #tempMultiSupplierGroup(Item)
					select Item from #tempFlowDet
					group by Item having COUNT(*) > 1 
					
					insert into #tempMultiSupplierItem(FlowDetRowId, Flow, Item, MSGRowId, MrpTotal, MrpTotalAdj, MrpWeight)
					select det.FlowDetRowId, det.Flow, det.Item, msg.RowId, det.MrpTotal, det.MrpTotalAdj, det.MrpWeight 
					from #tempFlowDet as det inner join #tempMultiSupplierGroup as msg on det.Item = msg.Item
					-------------------↑按零件分组-----------------------
						
						
					-------------------↓零件和目的库位相同的路线都没有设置供货比例，按零件包装设置循环量-----------------------
					update tmp set MrpWeight = (CASE WHEN det.UC = 0 THEN 1 ELSE det.UC END)
					from #tempMultiSupplierItem as tmp
					inner join #tempFlowDet as det on det.FlowDetRowId = tmp.FlowDetRowId
					where MSGRowId in (select MSGRowId from #tempMultiSupplierItem group by MSGRowId having SUM(MrpWeight) = 0)
					-------------------↑零件和目的库位相同的路线都没有设置供货比例，按零件包装设置循环量-----------------------
						
						
					-------------------↓零件和目的库位相同的路线没有设置供货比例，忽略这些路线明细-----------------------
					delete #tempFlowDet where FlowDetRowId in (select FlowDetRowId from #tempMultiSupplierItem where MrpWeight = 0)
					delete #tempMultiSupplierItem where FlowDetRowId in (select FlowDetRowId from #tempMultiSupplierItem where MrpWeight = 0)
					-------------------↑零件和目的库位相同的路线没有设置供货比例，忽略这些路线明细-----------------------
					
					
					-------------------↓计算供货次数和余量-----------------------
					update #tempMultiSupplierItem set DeliveryCount = (ISNULL(MrpTotal, 0) + ISNULL(MrpTotalAdj, 0)) / MrpWeight, DeliveryBalance = (ISNULL(MrpTotal, 0) + ISNULL(MrpTotalAdj, 0)) % MrpWeight
					-------------------↑计算供货次数和余量-----------------------
					
					
					-------------------↓根据供货次数、循环量选取一条路线明细供货-----------------------
					truncate table #tempSortedMultiSupplierItem
					insert into #tempSortedMultiSupplierItem(GID, FlowDetRowId, Flow)
					select ROW_NUMBER() over(partition by MSGRowId order by DeliveryCount asc, MrpWeight desc, DeliveryBalance desc) as GID, FlowDetRowId, Flow 
					from #tempMultiSupplierItem
					
					insert into #tempMsg(Lvl, Msg)
					select distinct 0, N'排序组' + @SeqGroup + N'存在重复的零件' + Item + N'，可能由于没有维护配额值，系统自动删除路线' + Flow
					from #tempFlowDet where FlowDetRowId in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
					
					delete #tempFlowDet where FlowDetRowId in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
					delete #tempMultiSupplierItem where FlowDetRowId in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
					-------------------↑根据供货次数、循环量选取一条路线明细供货-----------------------
					
					
					-----------------------------↑获取排序路线明细，考虑了多厂商供货的情况-----------------------------
					--锁定并缓存Bom
					truncate table #tempOrderBom
					if exists(select top 1 1 from #tempAssOpRef)
					begin
						insert into #tempOrderBom(OrderNo, Seq, SubSeq, Flow, Item, ItemDesc, RefItemCode, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, OpRef, OrderQty, Location, CPTime, BomId, ManufactureParty, LocFrom, LocTo, ZOPWZ, ZOPID, ZOPDS)
						select bom.OrderNo, @Seq, @SubSeq, CASE WHEN flow.Flow is not null THEN flow.Flow ELSE fdet.Flow END as Flow, bom.Item, fdet.ItemDesc, fdet.RefItemCode, fdet.Uom, fdet.UC, fdet.MinUC, fdet.UCDesc, fdet.Container, fdet.ContainerDesc, bom.OpRef, bom.OrderQty, bom.Location, cp.CPTime, bom.Id, bom.ManufactureParty, fdet.LocFrom, fdet.LocTo, bom.ZOPWZ, bom.ZOPID, bom.ZOPDS
						from ORD_OrderBomDet as bom
						inner join ORD_OrderSeq as seq on seq.OrderNo = bom.OrderNo
						inner join #tempAssOpRef as opRef on bom.OpRef = opRef.AssOpRef
						inner join #tempFlowDet as fdet on fdet.Item = bom.Item
						inner join SCM_FlowStrategy as stra on stra.Flow = fdet.Flow
						left join LE_OrderBomCPTimeSnapshot as cp on bom.Id = cp.BomId
						left join #tempSeqFlow as flow on flow.PartyFrom = bom.ManufactureParty  --指定供应商取PartyFrom和指定供应商相同的路线代码
						where stra.SeqGroup = @SeqGroup and stra.Strategy = 4 
						--and bom.OrderNo = @OrderNo 
						and seq.TraceCode = @TraceCode 
						and bom.OrderQty > 0
					end
					else
					begin
						insert into #tempOrderBom(OrderNo, Seq, SubSeq, Flow, Item, ItemDesc, RefItemCode, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, OpRef, OrderQty, Location, CPTime, BomId, ManufactureParty, LocFrom, LocTo, ZOPWZ, ZOPID, ZOPDS)
						select bom.OrderNo, @Seq, @SubSeq, CASE WHEN flow.Flow is not null THEN flow.Flow ELSE fdet.Flow END as Flow, bom.Item, fdet.ItemDesc, fdet.RefItemCode, fdet.Uom, fdet.UC, fdet.MinUC, fdet.UCDesc, fdet.Container, fdet.ContainerDesc, bom.OpRef, bom.OrderQty, bom.Location, cp.CPTime, bom.Id, bom.ManufactureParty, fdet.LocFrom, fdet.LocTo, bom.ZOPWZ, bom.ZOPID, bom.ZOPDS
						from ORD_OrderBomDet as bom
						inner join ORD_OrderSeq as seq on seq.OrderNo = bom.OrderNo
						inner join #tempFlowDet as fdet on fdet.Item = bom.Item
						inner join SCM_FlowStrategy as stra on stra.Flow = fdet.Flow
						left join LE_OrderBomCPTimeSnapshot as cp on bom.Id = cp.BomId
						left join #tempSeqFlow as flow on flow.PartyFrom = bom.ManufactureParty  --指定供应商取PartyFrom和指定供应商相同的路线代码
						where stra.SeqGroup = @SeqGroup and stra.Strategy = 4 
						--and bom.OrderNo = @OrderNo 
						and seq.TraceCode = @TraceCode 
						and bom.OrderQty > 0
					end
				end
				else
				begin  --鞍座和变速器按工作中心找排序件
					if exists(select top 1 1 from (select ROW_NUMBER() over(order by Flow) as RowNm from #tempSeqFlow) as rn where rn.RowNm > 1)
					begin
						--记录日志
						set @Msg = N'找到多条鞍座和变速器的排序路线。'
						RAISERROR(@Msg, 16, 1) 
					end
					else
					begin
						declare @WMSKitOrderSeqFlow varchar(50) = null
						select top 1 @WMSKitOrderSeqFlow = Flow from #tempSeqFlow
					
						truncate table #tempWMSKitWorkCenter
						if exists(select top 1 1 from CUST_ProductLineMap as map
								 inner join #tempSeqFlow as transFlow on map.TransFlow = transFlow.Flow)
						begin   --变速器
							insert into #tempWMSKitWorkCenter(Flow, WorkCenter) 
							select map.TransFlow, pwc.WorkCenter from CUST_ProductLineMap as map
							inner join #tempSeqFlow as transFlow on map.TransFlow = transFlow.Flow
							inner join PRD_ProdLineWorkCenter as pwc on map.TransFlow = pwc.Flow
						end
						else if exists(select top 1 1 from CUST_ProductLineMap as map
								 inner join #tempSeqFlow as saddleFlow on map.SaddleFlow = saddleFlow.Flow)
						begin	--鞍座
							insert into #tempWMSKitWorkCenter(Flow, WorkCenter)
							select map.SaddleFlow, pwc.WorkCenter from CUST_ProductLineMap as map
							inner join #tempSeqFlow as saddleFlow on map.SaddleFlow = saddleFlow.Flow
							inner join PRD_ProdLineWorkCenter as pwc on map.SaddleFlow = pwc.Flow
						end
					
						--锁定并缓存Bom
						truncate table #tempOrderBom
						insert into #tempOrderBom(OrderNo, Seq, SubSeq, Flow, Item, ItemDesc, RefItemCode, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, OpRef, OrderQty, Location, CPTime, BomId, ManufactureParty, LocFrom, LocTo, ZOPWZ, ZOPID, ZOPDS)
						select bom.OrderNo, @Seq, @SubSeq, @WMSKitOrderSeqFlow as Flow, bom.Item, i.Desc1 as ItemDesc, i.RefCode as RefItemCode, i.Uom, i.UC, i.MinUC, null, i.Container, i.ContainerDesc, bom.OpRef, bom.OrderQty, bom.Location, cp.CPTime, bom.Id, bom.ManufactureParty, mstr.LocFrom, mstr.LocTo, bom.ZOPWZ, bom.ZOPID, bom.ZOPDS
						from ORD_OrderBomDet as bom
						inner join ORD_OrderSeq as seq on seq.OrderNo = bom.OrderNo
						inner join #tempWMSKitWorkCenter as pwc on bom.WorkCenter = pwc.WorkCenter
						inner join MD_Item as i on bom.Item = i.Code
						inner join SCM_FlowMstr as mstr on mstr.Code = pwc.Flow
						left join LE_OrderBomCPTimeSnapshot as cp on bom.Id = cp.BomId
						--where bom.OrderNo = @OrderNo and bom.OrderQty > 0
						where seq.TraceCode = @TraceCode and bom.OrderQty > 0
					end
				end
					
				if exists(select top 1 1 from #tempOrderBom)
				begin
					-----------------------------↓排序单结转-----------------------------
					
					--已经生成的排序件打标记，避免JIT也产生拉动
					update LE_OrderBomCPTimeSnapshot set IsCreateOrder = 1
					where BomId in (select BomId from #tempOrderBom)
					
					--已经生成的排序件打标记，避免JIT也产生拉动
					update ORD_OrderBomDet set IsCreateOrder = 1, IsCreateSeq = 1, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, [Version] = [Version] + 1
					where Id in (select BomId from #tempOrderBom)
						
					-----------------------------↓循环创建排序单-----------------------------													
					declare @SeqFlowRowId int = null
					declare @MaxSeqFlowRowId int = null
					
					select @SeqFlowRowId = MIN(RowId), @MaxSeqFlowRowId = MAX(RowId) from #tempSeqFlow
					
					truncate table #tempSeqOrderTrace
					while (@SeqFlowRowId <= @MaxSeqFlowRowId)
					begin
						declare @SeqOrderNo varchar(50) = null  --排序单号
						declare @SeqFlow varchar(50) = null  --排序路线
						declare @SeqFlowType tinyint = null  --排序路线类型
						declare @SeqFlowPartyFrom varchar(50) = null  --排序路线来源区域
						declare @SeqFlowPartyTo varchar(50) = null  --排序路线目的区域
						declare @SeqFlowLocFrom varchar(50) = null  --排序路线来源库位
						declare @SeqFlowLocTo varchar(50) = null  --排序路线目的库位
						declare @SeqIsCreatePickList bit = null  --是否要根据先进先出指定供应商
						
						select @SeqFlow = Flow, @SeqFlowType = [Type], @SeqFlowPartyFrom = PartyFrom,
						@SeqFlowPartyTo = PartyTo, @SeqFlowLocFrom = LocFrom, @SeqFlowLocTo = LocTo,
						@SeqIsCreatePickList = IsCreatePickList
						from #tempSeqFlow where RowId = @SeqFlowRowId
						
						if exists(select top 1 1 from #tempOrderBom where Flow = @SeqFlow)
						begin
							declare @LeadTime int = null
							declare @EMLeadTime int = null
							declare @WinTimeDiff int = null
							declare @StartTime datetime = null
							declare @EMStartTime datetime = null   --预计紧急发单时间
							declare @WindowTime datetime = null
							declare @IsUrgent bit = 0
							
							--获取提前期
							select @LeadTime = -LeadTime * 60 * 60, @EMLeadTime = -EMLeadTime * 60 * 60, @WinTimeDiff = -WinTimeDiff * 60 * 60 from #tempSeqFlow where Flow = @SeqFlow
							select @WindowTime = MIN(CPTime) from #tempOrderBom where Flow = @SeqFlow and CPTime is not null
							
							if @WindowTime is null
							begin
								set @Msg = N'排序组' + @SeqGroup + N'没有找到排序件的消耗时间。'
								RAISERROR(@Msg, 16, 1) 
							end
							
							--计算窗口时间
							exec USP_Busi_SubtractWorkingDate @WindowTime, @WinTimeDiff, @ProdLine, @ProdLineRegion, @WindowTime output					
						
							--计算发单时间
							exec USP_Busi_SubtractWorkingDate @WindowTime, @LeadTime, @ProdLine, @ProdLineRegion, @StartTime output
							
							if @StartTime < @DateTimeNow
							begin
								if @EMLeadTime > 0
								begin
									--提前期不能满足计算紧急提前期
									exec USP_Busi_SubtractWorkingDate @WindowTime, @EMLeadTime, @ProdLine, @ProdLineRegion, @EMStartTime output
									
									if @EMStartTime < @DateTimeNow
									begin
										--设置为紧急单
										set @IsUrgent = 1
								
										--紧急提前期也不能满足，设置开始时间为当前时间，窗口时间为当前时间+紧急提前期
										set @StartTime = @DateTimeNow
										set @EMLeadTime = 0 - @EMLeadTime
										exec USP_Busi_AddWorkingDate @StartTime, @EMLeadTime, @ProdLine, @ProdLineRegion, @WindowTime output
									end
								end
							end
							
							-----------------------------↓创建排序单头-----------------------------
							exec USP_GetDocNo_ORD @SeqFlow, 4, @SeqFlowType, 0, 0, 0, @SeqFlowPartyFrom, @SeqFlowPartyTo, @SeqFlowLocFrom, @SeqFlowLocTo, null, 0, @SeqOrderNo output
							
							if @SeqFlowType = 1
							begin
								insert into ORD_OrderMstr_1 (
								OrderNo,              --排序单号
								Flow,                 --排序路线
								TraceCode,			  --趟次
								OrderStrategy,        --策略，4
								RefOrderNo,			  --上趟最后台Van号
								ExtOrderNo,			  --本趟最后台Van号
								[Type],               --类型，1 采购
								SubType,              --子类型，0正常
								QualityType,          --质量状态，0良品
								StartTime,            --开始时间
								WindowTime,           --窗口时间
								PauseSeq,             --暂停工序，0
								IsQuick,              --是否快速，0
								[Priority],           --优先级，0
								[Status],             --状态，1释放
								PartyFrom,            --区域代码
								PartyFromNm,            --区域名称
								PartyTo,              --区域代码
								PartyToNm,              --区域名称
								--LocFrom,              --原材料库位
								--LocFromNm,			  --原材料库位
								LocTo,                --成品库位
								LocToNm,              --成品库位
								IsInspect,            --下线检验，0
								BillAddr,			  --开票地址
								Dock,				  --道口
								IsAutoRelease,        --自动释放，0
								IsAutoStart,          --自动上线，0
								IsAutoShip,           --自动发货，0
								IsAutoReceive,        --自动收货，0
								IsAutoBill,           --自动账单，0
								IsManualCreateDet,    --手工创建明细，0
								IsListPrice,          --显示价格单，0
								IsPrintOrder,         --打印排序单，0
								IsOrderPrinted,       --排序单已打印，0
								IsPrintAsn,           --打印ASN，0
								IsPrintRec,           --打印收货单，0
								IsShipExceed,         --允许超发，0
								IsRecExceed,          --允许超收，0
								IsOrderFulfillUC,     --整包装下单，0
								IsShipFulfillUC,      --整包装发货，0
								IsRecFulfillUC,       --整包装收货，0
								IsShipScanHu,         --发货扫描条码，0
								IsRecScanHu,          --收货扫描条码，0
								IsCreatePL,           --创建拣货单，0
								IsPLCreate,           --拣货单已创建，0
								IsShipFifo,           --发货先进先出，0
								IsRecFifo,            --收货先进先出，0
								IsShipByOrder,        --允许按订单发货，0
								IsOpenOrder,          --开口订单，0
								IsAsnUniqueRec,       --ASN一次性收货，0
								RecGapTo,             --收货差异处理，0
								RecTemplate,		  --收货单模板
								OrderTemplate,		  --订单模板
								AsnTemplate,		  --送货单模板
								HuTemplate,			  --条码模板
								BillTerm,             --结算方式，0
								CreateHuOpt,          --创建条码选项，0
								ReCalculatePriceOpt,  --重新计算价格单选项，0
								CreateUser,           --创建用户
								CreateUserNm,         --创建用户名称
								CreateDate,           --创建日期
								LastModifyUser,       --最后修改用户
								LastModifyUserNm,     --最后修改用户名称
								LastModifyDate,       --最后修改日期
								ReleaseUser,          --释放用户
								ReleaseUserNm,        --释放用户名称
								ReleaseDate,          --释放日期
								[Version],            --版本，1
								FlowDesc,			  --路线描述
								ProdLineType,         --生产线类型
								PauseStatus,          --暂停状态，0
								SeqGroup			  --排序组
								)
								select 
								@SeqOrderNo,          --排序单号
								mstr.Code,                 --排序路线
								null,				--趟次
								4,					  --策略，4
								null,				--上趟最后台Van号
								null,				--本趟最后台Van号
								@SeqFlowType,			--类型，1 采购
								0,					  --子类型，0正常
								0,					  --质量状态，0良品
								@StartTime,           --开始时间
								@WindowTime,          --窗口时间
								0,					  --暂停工序，0
								0,					  --是否快速，0
								@IsUrgent,				--优先级，0
								1,					  --状态，1释放
								mstr.PartyFrom,            --区域代码
								pf.Name,				--区域名称
								mstr.PartyTo,              --区域代码
								pt.Name,				--区域名称
								--mstr.LocFrom,              --原材料库位
								--lf.Name,              --原材料库位
								mstr.LocTo,                --成品库位
								lt.Name,              --成品库位
								mstr.IsInspect,            --下线检验，0
								mstr.BillAddr,			  --开票地址
								mstr.Dock,				  --道口
								mstr.IsAutoRelease,        --自动释放，0
								mstr.IsAutoStart,          --自动上线，0
								mstr.IsAutoShip,           --自动发货，0
								mstr.IsAutoReceive,        --自动收货，0
								mstr.IsAutoBill,           --自动账单，0
								mstr.IsManualCreateDet,    --手工创建明细，0
								mstr.IsListPrice,          --显示价格单，0
								mstr.IsPrintOrder,         --打印排序单，0
								0,					  --排序单已打印，0
								mstr.IsPrintAsn,           --打印ASN，0
								mstr.IsPrintRcpt,          --打印收货单，0
								mstr.IsShipExceed,         --允许超发，0
								mstr.IsRecExceed,          --允许超收，0
								mstr.IsOrderFulfillUC,     --整包装下单，0
								mstr.IsShipFulfillUC,      --整包装发货，0
								mstr.IsRecFulfillUC,       --整包装收货，0
								mstr.IsShipScanHu,         --发货扫描条码，0
								mstr.IsRecScanHu,          --收货扫描条码，0
								mstr.IsCreatePL,           --创建拣货单，0
								0,					  --拣货单已创建，0
								mstr.IsShipFifo,           --发货先进先出，0
								mstr.IsRecFifo,            --收货先进先出，0
								mstr.IsShipByOrder,        --允许按订单发货，0
								0,					  --开口订单，0
								mstr.IsAsnUniqueRec,       --ASN一次性收货，0
								mstr.RecGapTo,             --收货差异处理，0
								mstr.RecTemplate,		  --收货单模板
								mstr.OrderTemplate,		  --订单模板
								mstr.AsnTemplate,		  --送货单模板
								mstr.HuTemplate,			  --条码模板
								mstr.BillTerm,             --结算方式，0
								mstr.CreateHuOpt,          --创建条码选项，0
								0,					  --重新计算价格单选项，0
								@CreateUserId,        --创建用户
								@CreateUserNm,        --创建用户名称
								@DateTimeNow,         --创建日期
								@CreateUserId,        --最后修改用户
								@CreateUserNm,        --最后修改用户名称
								@DateTimeNow,         --最后修改日期
								@CreateUserId,        --释放用户
								@CreateUserNm,        --释放用户名称
								@DateTimeNow,         --释放日期
								1,					  --版本，1
								mstr.Desc1,				  --路线描述
								mstr.ProdLineType,         --生产线类型
								0,					  --暂停状态，0
								@SeqGroup			  --排序组
								from SCM_FlowMstr as mstr 
								inner join MD_Party as pf on mstr.PartyFrom = pf.Code
								inner join MD_Party as pt on mstr.PartyTo = pt.Code
								--left join MD_Location as lf on mstr.LocFrom = lf.Code
								left join MD_Location as lt on mstr.LocTo = lt.Code
								where mstr.Code = @SeqFlow
							end
							else if @SeqFlowType = 2
							begin 
								insert into ORD_OrderMstr_2 (
								OrderNo,              --排序单号
								Flow,                 --排序路线
								TraceCode,			  --趟次
								OrderStrategy,        --策略，4
								RefOrderNo,			  --上趟最后台Van号
								ExtOrderNo,			  --本趟最后台Van号
								[Type],               --类型，2 移库
								SubType,              --子类型，0正常
								QualityType,          --质量状态，0良品
								StartTime,            --开始时间
								WindowTime,           --窗口时间
								PauseSeq,             --暂停工序，0
								IsQuick,              --是否快速，0
								[Priority],           --优先级，0
								[Status],             --状态，1释放
								PartyFrom,            --区域代码
								PartyFromNm,            --区域名称
								PartyTo,              --区域代码
								PartyToNm,              --区域名称
								LocFrom,              --原材料库位
								LocFromNm,              --原材料库位
								LocTo,                --成品库位
								LocToNm,                --成品库位
								IsInspect,            --下线检验，0
								Dock,				  --道口
								IsAutoRelease,        --自动释放，0
								IsAutoStart,          --自动上线，0
								IsAutoShip,           --自动发货，0
								IsAutoReceive,        --自动收货，0
								IsAutoBill,           --自动账单，0
								IsManualCreateDet,    --手工创建明细，0
								IsListPrice,          --显示价格单，0
								IsPrintOrder,         --打印排序单，0
								IsOrderPrinted,       --排序单已打印，0
								IsPrintAsn,           --打印ASN，0
								IsPrintRec,           --打印收货单，0
								IsShipExceed,         --允许超发，0
								IsRecExceed,          --允许超收，0
								IsOrderFulfillUC,     --整包装下单，0
								IsShipFulfillUC,      --整包装发货，0
								IsRecFulfillUC,       --整包装收货，0
								IsShipScanHu,         --发货扫描条码，0
								IsRecScanHu,          --收货扫描条码，0
								IsCreatePL,           --创建拣货单，0
								IsPLCreate,           --拣货单已创建，0
								IsShipFifo,           --发货先进先出，0
								IsRecFifo,            --收货先进先出，0
								IsShipByOrder,        --允许按订单发货，0
								IsOpenOrder,          --开口订单，0
								IsAsnUniqueRec,       --ASN一次性收货，0
								RecGapTo,             --收货差异处理，0
								RecTemplate,		  --收货单模板
								OrderTemplate,		  --订单模板
								AsnTemplate,		  --送货单模板
								HuTemplate,			  --条码模板
								BillTerm,             --结算方式，0
								CreateHuOpt,          --创建条码选项，0
								ReCalculatePriceOpt,  --重新计算价格单选项，0
								CreateUser,           --创建用户
								CreateUserNm,         --创建用户名称
								CreateDate,           --创建日期
								LastModifyUser,       --最后修改用户
								LastModifyUserNm,     --最后修改用户名称
								LastModifyDate,       --最后修改日期
								ReleaseUser,          --释放用户
								ReleaseUserNm,        --释放用户名称
								ReleaseDate,          --释放日期
								[Version],            --版本，1
								FlowDesc,			  --路线描述
								ProdLineType,         --生产线类型
								PauseStatus,          --暂停状态，0
								SeqGroup			  --排序组
								)
								select 
								@SeqOrderNo,          --排序单号
								mstr.Code,                 --排序路线
								null,					--趟次
								4,					  --策略，4
								null,				--上趟最后台Van号
								null,				--本趟最后台Van号
								@SeqFlowType,		  --类型，1 移库
								0,					  --子类型，0正常
								0,					  --质量状态，0良品
								@StartTime,				--开始时间
								@WindowTime,          --窗口时间
								0,					  --暂停工序，0
								0,					  --是否快速，0
								@IsUrgent,			  --优先级，0
								1,					  --状态，1释放
								mstr.PartyFrom,            --区域代码
								pf.Name,				--区域名称
								mstr.PartyTo,              --区域代码
								pt.Name,				--区域名称
								mstr.LocFrom,              --原材料库位
								lf.Name,              --原材料库位
								mstr.LocTo,                --成品库位
								lt.Name,                --成品库位
								mstr.IsInspect,            --下线检验，0
								mstr.Dock,				  --道口
								mstr.IsAutoRelease,        --自动释放，0
								mstr.IsAutoStart,          --自动上线，0
								mstr.IsAutoShip,           --自动发货，0
								mstr.IsAutoReceive,        --自动收货，0
								mstr.IsAutoBill,           --自动账单，0
								mstr.IsManualCreateDet,    --手工创建明细，0
								mstr.IsListPrice,          --显示价格单，0
								mstr.IsPrintOrder,         --打印排序单，0
								0,					  --排序单已打印，0
								mstr.IsPrintAsn,           --打印ASN，0
								mstr.IsPrintRcpt,          --打印收货单，0
								mstr.IsShipExceed,         --允许超发，0
								mstr.IsRecExceed,          --允许超收，0
								mstr.IsOrderFulfillUC,     --整包装下单，0
								mstr.IsShipFulfillUC,      --整包装发货，0
								mstr.IsRecFulfillUC,       --整包装收货，0
								mstr.IsShipScanHu,         --发货扫描条码，0
								mstr.IsRecScanHu,          --收货扫描条码，0
								mstr.IsCreatePL,           --创建拣货单，0
								0,					  --拣货单已创建，0
								mstr.IsShipFifo,           --发货先进先出，0
								mstr.IsRecFifo,            --收货先进先出，0
								mstr.IsShipByOrder,        --允许按订单发货，0
								0,					  --开口订单，0
								mstr.IsAsnUniqueRec,       --ASN一次性收货，0
								mstr.RecGapTo,             --收货差异处理，0
								mstr.RecTemplate,		  --收货单模板
								mstr.OrderTemplate,		  --订单模板
								mstr.AsnTemplate,		  --送货单模板
								mstr.HuTemplate,			  --条码模板
								mstr.BillTerm,             --结算方式，0
								mstr.CreateHuOpt,          --创建条码选项，0
								0,					  --重新计算价格单选项，0
								@CreateUserId,        --创建用户
								@CreateUserNm,        --创建用户名称
								@DateTimeNow,         --创建日期
								@CreateUserId,        --最后修改用户
								@CreateUserNm,        --最后修改用户名称
								@DateTimeNow,         --最后修改日期
								@CreateUserId,        --释放用户
								@CreateUserNm,        --释放用户名称
								@DateTimeNow,         --释放日期
								1,					  --版本，1
								mstr.Desc1,				  --路线描述
								mstr.ProdLineType,         --生产线类型
								0,					  --暂停状态，0
								@SeqGroup			  --排序组
								from SCM_FlowMstr as mstr
								inner join MD_Party as pf on mstr.PartyFrom = pf.Code
								inner join MD_Party as pt on mstr.PartyTo = pt.Code
								left join MD_Location as lf on mstr.LocFrom = lf.Code
								left join MD_Location as lt on mstr.LocTo = lt.Code
								where mstr.Code = @SeqFlow
							end
							else
							begin
								RAISERROR(N'排序路线类型不正确。', 16, 1)
							end
							-----------------------------↑创建排序单头-----------------------------
							
							
							-----------------------------↓创建排序单明细-----------------------------
							declare @OrderDetCount int = 0
							declare @BeginOrderDetId int = 0
							declare @NextOrderDetId int = 0
							
							truncate table #tempSeqOrderDet
							insert into #tempSeqOrderDet(Seq, ExtNo, ExtSeq, 
							Item, ItemDesc, RefItemCode, ReqQty, OrderQty, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, ManufactureParty, OpRef, CPTime, ZOPWZ, ZOPID, ZOPDS, IsItemConsume)
							select ROW_NUMBER() over(order by bom.Item) as Seq, @TraceCode as ExtNo, (convert(varchar, (@Seq * 100 + @SubSeq))) as ExtSeq,
							bom.Item, bom.ItemDesc, bom.RefItemCode, bom.OrderQty, bom.OrderQty, bom.Uom, bom.UC, bom.MinUC, bom.UCDesc, bom.Container, bom.ContainerDesc, bom.ManufactureParty, bom.OpRef, bom.CPTime, bom.ZOPWZ, bom.ZOPID, bom.ZOPDS, 0
							from #tempOrderBom as bom
							where bom.Flow = @SeqFlow
							
							-----------------------------↓处理厂内消化-----------------------------
							truncate table #tempItemConsume
							insert into #tempItemConsume(Id, Item, RemainQty, [Version])
							select distinct con.Id, con.Item, con.Qty - con.ConsumeQty as RemainQty, con.[Version] 
							from #tempSeqOrderDet as det inner join PRD_ItemConsume as con on det.Item = con.Item
							where det.OrderQty > 0 and con.Qty > con.ConsumeQty order by con.Id
							
							if exists(select top 1 1 from #tempItemConsume)
							begin
								declare @ICRowId int = null
								declare @MaxICRowId int = null
								select @ICRowId = MIN(RowId), @MaxICRowId = MAX(RowId) from #tempItemConsume
								
								while @ICRowId <= @MaxICRowId
								begin
									declare @ICId int = null
									declare @ICItem varchar(50) = null
									declare @ICRemainQty decimal(18, 8) = null
									declare @ICVersion int = null
								
									select @ICId = Id, @ICItem = Item, @ICRemainQty = RemainQty, @ICVersion = [Version] 
									from #tempItemConsume where RowId = @ICRowId
																				
									truncate table #tempICSeqOrderDet
									insert into #tempICSeqOrderDet(SeqRowId)
									select RowId from #tempSeqOrderDet where Item = @ICItem and OrderQty > 0
									
									declare @ICSeqRowId int = null
									declare @MaxICSeqRowId int = null
									select @ICSeqRowId = MIN(RowId), @MaxICSeqRowId = MAX(RowId) from #tempICSeqOrderDet
									
									while @ICSeqRowId <= @MaxICSeqRowId
									begin
										if (@ICRemainQty > 0)
										begin
											declare @SeqRowId int = null
											declare @SeqRowSeq int = null
											declare @ICOrderQty decimal(18, 8) = null
											select @SeqRowId = SeqRowId from #tempICSeqOrderDet where RowId = @ICSeqRowId 
											select @SeqRowSeq = Seq, @ICOrderQty = OrderQty from #tempSeqOrderDet where RowId = @SeqRowId
											
											if (@ICRemainQty >= @ICOrderQty)
											begin
												update #tempSeqOrderDet set OrderQty = 0, IsItemConsume = 1 where RowId = @SeqRowId
												set @ICRemainQty = @ICRemainQty - @ICOrderQty
											end
											else
											begin
												update #tempSeqOrderDet set ReqQty = ReqQty - @ICRemainQty, OrderQty = OrderQty - @ICRemainQty where RowId = @SeqRowId
												
												update #tempSeqOrderDet set Seq = Seq + 1 where Seq > @SeqRowSeq
												
												insert into #tempSeqOrderDet(Seq, Item, ItemDesc, RefItemCode, ReqQty, OrderQty, 
																			Uom, UC, MinUC, UCDesc, Container, ContainerDesc, ManufactureParty, OpRef, CPTime, ZOPWZ, ZOPID, ZOPDS, IsItemConsume)
												select @SeqRowSeq + 1, Item, ItemDesc, RefItemCode, @ICRemainQty, 0, 
																			Uom, UC, MinUC, UCDesc, Container, ContainerDesc, ManufactureParty, OpRef, CPTime, ZOPWZ, ZOPID, ZOPDS, 1
												from #tempSeqOrderDet where RowId = @SeqRowId
											end
										end
										else
										begin
											break
										end
										
										set @ICSeqRowId = @ICSeqRowId + 1
									end
									
									update PRD_ItemConsume set ConsumeQty = Qty - @ICRemainQty, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, 
									LastModifyDate = @DateTimeNow, [Version] = [Version] + 1 
									where Id = @ICId and [Version] = @ICVersion
										
									if @@RowCount <> 1
									begin
										RAISERROR(N'厂内消化档已更新。', 16, 1) 
									end
									
									set @ICVersion = @ICVersion + 1
									set @ICRowId = @ICRowId + 1
								end
							end
							-----------------------------↑处理厂内消化-----------------------------
							
							--查找订单数量
							select @OrderDetCount = COUNT(*) from #tempSeqOrderDet
							--锁定OrderDetId字段标识字段
							exec USP_SYS_BatchGetNextId 'ORD_OrderDet', @OrderDetCount, @NextOrderDetId output
							--查找开始标识
							set @BeginOrderDetId = @NextOrderDetId - @OrderDetCount
						
							--更新生产单明细标识
							update #tempSeqOrderDet set Id = seq + @BeginOrderDetId
							
							if @SeqFlowType = 1
							begin
								insert into ORD_OrderDet_1 (
								Id,                         --生产单明细标识
								OrderNo,                    --生产单号
								OrderType,                  --类型，1采购
								OrderSubType,               --子类型，0正常
								Seq,						--行号，1
								ReserveNo,					--外部订单号，Van号
								ReserveLine,				--车辆流水号 = 顺序号+子顺序号
								ScheduleType,               --计划协议类型，0
								Item,                       --物料号
								RefItemCode,				--参考物料号
								ItemDesc,                   --物料描述
								Uom,                        --单位
								BaseUom,                    --基本单位
								UC,                         --包装，1
								MinUC,                      --最小包装，1
								UCDesc,						--包装描述
								Container,					--容器代码
								ContainerDesc,				--容器描述
								QualityType,                --质量状态，0
								ManufactureParty,           --制造商
								ReqQty,                     --需求数量，1
								OrderQty,                   --订单数量，1
								ShipQty,                    --发货数量，0
								RecQty,                     --收货数量，0
								RejQty,                     --次品数量，0
								ScrapQty,                   --废品数量，0
								PickQty,                    --拣货数量，0
								UnitQty,                    --单位用量，1
								IsInspect,                  --是否检验，0
								IsProvEst,                  --存是否已经生成过排序单
								IsIncludeTax,               --是否厂内消化
								IsScanHu,                   --是否扫描条码，0
								CreateUser,                 --创建用户
								CreateUserNm,               --创建用户名称
								CreateDate,                 --创建日期
								LastModifyUser,             --最后修改用户
								LastModifyUserNm,           --最后修改用户名称
								LastModifyDate,             --最后修改日期
								[Version],					--版本，1
								IsChangeUC,					--是否修改单包装，0
								BinTo,						--记录工位
								StartDate,					--记录排序件消耗时间
								--LocFrom,					--出库库位
								--LocFromNm,					--出库库位名称
								LocTo,						--入库库位
								LocToNm,					--入库库位名称
								BillAddr,					--开票地址
								BillTerm,					--结算方式
								ZOPWZ,						--
								ZOPID,						--
								ZOPDS						--
								)
								select 
								det.Id,                     --生产单明细标识
								@SeqOrderNo,				--生产单号
								@SeqFlowType,				--类型，1采购
								0,							--子类型，0正常
								det.Seq,					--行号，
								det.ExtNo,					--外部订单号，Van号
								det.ExtSeq,					--车辆流水号 = 顺序号+子顺序号
								0,							--计划协议类型，0
								det.Item,                   --物料号
								det.RefItemCode,			--参考物料号
								det.ItemDesc,               --物料描述
								det.Uom,                    --单位
								det.Uom,					--基本单位
								det.UC,                     --包装，1
								det.MinUC,                  --最小包装，1
								det.UCDesc,					--包装描述
								det.Container,				--容器代码
								det.ContainerDesc,			--容器描述
								0,							--质量状态，0
								det.ManufactureParty,       --制造商
								ISNULL(det.ReqQty, 0),		--需求数量，1
								det.OrderQty,				--订单数量，1
								0,							--发货数量，0
								0,							--收货数量，0
								0,							--次品数量，0
								0,							--废品数量，0
								0,							--拣货数量，0
								1,							--单位用量，1
								0,							--是否检验，0
								0,							--存是否已经生成过排序单
								det.IsItemConsume,			--是否厂内消化
								0,							--是否扫描条码，0
								@CreateUserId,				--创建用户
								@CreateUserNm,				--创建用户名称
								@DateTimeNow,				--创建日期
								@CreateUserId,				--最后修改用户
								@CreateUserNm,				--最后修改用户名称
								@DateTimeNow,				--最后修改日期
								1,							--版本，1
								0,							--是否修改单包装，0
								det.OpRef,					--记录工位
								det.CPTime,					--记录排序件消耗时间
								--ISNULL(det.LocFrom, mstr.LocFrom),--出库库位
								--ISNULL(lf.Name, mstr.LocFromNm),--出库库位
								ISNULL(det.LocTo, mstr.LocTo),	--入库库位
								ISNULL(lt.Name, mstr.LocToNm),--入库库位
								mstr.BillAddr,				--开票地址
								mstr.BillTerm,				--结算方式
								det.ZOPWZ,					--
								det.ZOPID,					--
								det.ZOPDS					--
								from #tempSeqOrderDet as det 
								inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = @SeqOrderNo
								--left join MD_Location as lf on det.LocFrom = lf.Code
								left join MD_Location as lt on det.LocTo = lt.Code
								
								--更新累计交货量
								update q set AccumulateQty = ISNULL(q.AccumulateQty, 0) + det.OrderQty, [Version] = q.[Version] + 1
								from SCM_Quota as q 
								inner join (select det.Item, mstr.PartyFrom, SUM(det.OrderQty) as OrderQty 
											from #tempSeqOrderDet as det 
											inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = @SeqOrderNo
											where det.OrderQty > 0
											group by det.Item, mstr.PartyFrom) as det on q.Item = det.Item and q.Supplier = det.PartyFrom
							end
							else
							begin
								insert into ORD_OrderDet_2 (
								Id,                         --生产单明细标识
								OrderNo,                    --生产单号
								OrderType,                  --类型，2移库
								OrderSubType,               --子类型，0正常
								Seq,						--行号，1
								ReserveNo,					--外部订单号，Van号
								ReserveLine,				--车辆流水号 = 顺序号+子顺序号
								ScheduleType,               --计划协议类型，0
								Item,                       --物料号
								RefItemCode,				--参考物料号
								ItemDesc,                   --物料描述
								Uom,                        --单位
								BaseUom,                    --基本单位
								UC,                         --包装，1
								MinUC,                      --最小包装，1
								UCDesc,						--包装描述
								Container,					--容器代码
								ContainerDesc,				--容器描述
								QualityType,                --质量状态，0
								ManufactureParty,           --制造商
								ReqQty,                     --需求数量，1
								OrderQty,                   --订单数量，1
								ShipQty,                    --发货数量，0
								RecQty,                     --收货数量，0
								RejQty,                     --次品数量，0
								ScrapQty,                   --废品数量，0
								PickQty,                    --拣货数量，0
								UnitQty,                    --单位用量，1
								IsInspect,                  --是否检验，0
								IsProvEst,                  --存是否已经生成过排序单
								IsIncludeTax,               --是否厂内消化，0
								IsScanHu,                   --是否扫描条码，0
								CreateUser,                 --创建用户
								CreateUserNm,               --创建用户名称
								CreateDate,                 --创建日期
								LastModifyUser,             --最后修改用户
								LastModifyUserNm,           --最后修改用户名称
								LastModifyDate,             --最后修改日期
								[Version],					--版本，1
								IsChangeUC,					--是否修改单包装，0
								BinTo,						--记录工位
								StartDate,					--记录排序件消耗时间
								LocFrom,					--出库库位
								LocFromNm,					--出库库位名称
								LocTo,						--入库库位
								LocToNm,					--入库库位名称
								--BillAddr					--开票地址
								ZOPWZ,						--
								ZOPID,						--
								ZOPDS						--
								)
								select 
								det.Id,						--生产单明细标识
								@SeqOrderNo,				--生产单号
								@SeqFlowType,				--类型，2移库
								0,							--子类型，0正常
								det.Seq,					--行号，
								det.ExtNo,					--外部订单号，Van号
								det.ExtSeq,					--车辆流水号 = 顺序号+子顺序号
								0,							--计划协议类型，0
								det.Item,					--整车物料号
								det.RefItemCode,			--参考物料号
								det.ItemDesc,				--整车物料描述
								det.Uom,					--单位
								det.Uom,					--基本单位
								det.UC,						--包装，1
								det.MinUC,					--最小包装，1
								det.UCDesc,					--包装描述
								det.Container,				--容器代码
								det.ContainerDesc,			--容器描述
								0,							--质量状态，0
								det.ManufactureParty,		--制造商
								ISNULL(det.ReqQty, 0),		--需求数量，1
								det.OrderQty,				--订单数量，1
								0,							--发货数量，0
								0,							--收货数量，0
								0,							--次品数量，0
								0,							--废品数量，0
								0,							--拣货数量，0
								1,							--单位用量，1
								0,							--是否检验，0
								0,							--存是否已经生成过排序单
								det.IsItemConsume,			--是否厂内消化
								0,							--是否扫描条码，0
								@CreateUserId,				--创建用户
								@CreateUserNm,				--创建用户名称
								@DateTimeNow,				--创建日期
								@CreateUserId,				--最后修改用户
								@CreateUserNm,				--最后修改用户名称
								@DateTimeNow,				--最后修改日期
								1,							--版本，1
								0,							--是否修改单包装，0
								det.OpRef,					--记录工位
								det.CPTime,					--记录排序件消耗时间
								ISNULL(det.LocFrom, mstr.LocFrom),--出库库位
								ISNULL(lf.Name, mstr.LocFromNm),--出库库位
								ISNULL(det.LocTo, mstr.LocTo),	--入库库位
								ISNULL(lt.Name, mstr.LocToNm),--入库库位
								--mstr.BillAddr				--开票地址
								det.ZOPWZ,					--
								det.ZOPID,					--
								det.ZOPDS					--
								from #tempSeqOrderDet as det 
								inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = @SeqOrderNo
								left join MD_Location as lf on det.LocFrom = lf.Code
								left join MD_Location as lt on det.LocTo = lt.Code
							end
							-----------------------------↑创建排序单明细-----------------------------
							
							--判断排序单是否直接关闭
							if not exists(select top 1 1 from #tempSeqOrderDet where OrderQty > 0)
							begin
								if @SeqFlowType = 1
								begin
									update ORD_OrderMstr_1 set [Status] = 4, CloseDate = @DateTimeNow, CloseUser = @CreateUserId, CloseUserNm = @CreateUserNm where OrderNo = @SeqOrderNo
								end
								else
								begin
									update ORD_OrderMstr_2 set [Status] = 4, CloseDate = @DateTimeNow, CloseUser = @CreateUserId, CloseUserNm = @CreateUserNm where OrderNo = @SeqOrderNo
								end
								
								set @Msg = N'关闭排序单' + @SeqOrderNo + '，因为订单数量全部为0'
								insert into #tempMsg(Lvl, Msg) values(0, @Msg)
							end
							
							if @SeqFlowType = 1
							begin  --记录创建计划协议的订单明细
								INSERT INTO SAP_CRSL(EINDT, FRBNR, LIFNR, MATNR, MENGE, SGTXT, WERKS, OrderDetId, [Status], ErrorCount, CreateDate, LastModifyDate)
								select CONVERT(varchar(8), @WindowTime, 112), @SeqOrderNo, mstr.PartyFrom, det.Item, det.OrderQty, det.Seq, r.Plant, det.Id, 0, 0, @DateTimeNow, @DateTimeNow
								from #tempSeqOrderDet as det
								inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = @SeqOrderNo
								inner join MD_Region as r on mstr.PartyTo = r.Code
								where det.OrderQty > 0
							end
							else
							begin
								if exists(select top 1 1 from SCM_FlowMstr where Code = @SeqFlow and PartyFrom in ('LOC', 'SQC'))
								begin
									INSERT INTO FIS_CreateSeqOrderDAT(SeqNo, Seq, Flow, StartTime, WindowTime, 
									PartyFrom, PartyTo, LocationTo, Container, CreateDate, Item, ManufactureParty, Qty, 
									SequenceNumber, Van, Line, Station, ErrorCount, UploadDate, FileName, IsCreateDat, OrderDetId)
									select @SeqOrderNo, det.Seq, mstr.SeqGroup, mstr.StartTime, mstr.WindowTime,
									ISNULL(det.LocFrom, mstr.LocFrom), ISNULL(det.LocTo, mstr.LocTo), ISNULL(det.OpRef, '未知工位'), '', @DateTimeNow, det.Item, det.ManufactureParty, det.OrderQty,
									det.ExtSeq, det.ExtNo, '', det.OpRef, 0, null, '', 0, det.Id
									from #tempSeqOrderDet as det
									inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = @SeqOrderNo
									left join MD_Location as lf on det.LocFrom = lf.Code
									left join MD_Location as lt on det.LocTo = lt.Code
								end
							end
							
							--更新排序策略上的累计量（按车辆数累计）
							update SCM_FlowStrategy set MRPTotal = ISNULL(MRPTotal, 0) + (select COUNT(distinct ExtNo) from #tempSeqOrderDet where OrderQty > 0)
							where Flow = @SeqFlow
							
							if @SeqIsCreatePickList = 1
							begin
								exec USP_LE_SpecifySequenceOrderCSSupplier @SeqOrderNo
							end
						
							--记录已经创建了排序单的VAN号
							insert into #tempSeqOrderTrace(TraceCode, ProdLine, SeqGroup, DeliveryCount) 
							values (@TraceCode, @ProdLine, @SeqGroup, null)
							-----------------------------↑循环创建排序单-----------------------------
							
							insert into #tempMsg(Lvl, Msg) values(0, N'排序组' + @SeqGroup + N'成功创建整车生产线' + @ProdLine + N' Van号' + @TraceCode + N'的排序单' + @SeqOrderNo + N'。')
						end
						
						set @SeqFlowRowId = @SeqFlowRowId + 1
					end
					
					--记录已经创建了排序单的VAN号
					insert into ORD_SeqOrderTrace(TraceCode, ProdLine, SeqGroup, DeliveryCount)
					select distinct TraceCode, ProdLine, SeqGroup, DeliveryCount from #tempSeqOrderTrace
				end
				else
				begin
					insert into #tempMsg(Lvl, Msg) values(0, N'排序组' + @SeqGroup + N'没有找到整车生产线' + @ProdLine + N' Van号' + @TraceCode + N'的排序件。')
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
		        
				set @Msg = Error_Message() + N'（排序组' + @SeqGroup + N'整车生产线' + @ProdLine + N' Van号' + @TraceCode + N'）'
				insert into #tempMsg(Lvl, Msg) values(1, @Msg)
			end catch
	
			set @RowId = @RowId + 1
		end
		
		select Lvl, Msg from #tempMsg
		
		drop table #tempItemConsume
		drop table #tempICSeqOrderDet
		drop table #tempMsg
		drop table #tempSeqGroup
		drop table #tempSeqFlow
		drop table #tempAssOpRef
		drop table #tempWMSKitWorkCenter
		drop table #tempFlowDet
		drop table #tempMultiSupplierGroup
		drop table #tempMultiSupplierItem
		drop table #tempSortedMultiSupplierItem
		drop table #tempOrderBom
		drop table #tempSeqOrderDet
	end try
	begin catch
		set @Msg = Error_Message() 
		RAISERROR(@Msg, 16, 1) 
	end catch
end
