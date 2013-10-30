SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_LE_PreviewSequenceOrder')
	DROP PROCEDURE USP_LE_PreviewSequenceOrder
GO

Create PROCEDURE [dbo].[USP_LE_PreviewSequenceOrder]
(
	@Flow varchar(50),
	@TraceCode varchar(50),
	@Item varchar(50),
	@CPTimeFrom datetime,
	@CPTimeTo datetime,
	@PageSize int,
	@PageCount int
) --WITH ENCRYPTION
AS
BEGIN
	begin try
		set nocount on
		declare @ErrorMsg varchar(max)
		
		if ISNULL(@Flow, '') = ''
		begin
			set @ErrorMsg = '排序路线不能为空。'
			RAISERROR(@ErrorMsg, 16, 1) 
		end
		
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
			LeadTime decimal(18, 8)
		)
		
		create table #tempAssOpRef
		(
			AssOpRef varchar(50)
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
			LocFrom varchar(50),
			LocTo varchar(50)
		)
		
		create table #tempWMSKitWorkCenter
		(
			Flow varchar(50),
			WorkCenter varchar(50)
		)
		
		create table #tempBomCPTime
		(
			Id int identity(1, 1) Primary Key,
			OrderNo varchar(50),
			Seq bigint,
			SubSeq int,
			CPTime datetime,
			Flow varchar(50),
			Item varchar(50), 
			ItemDesc varchar(100),
			RefItemCode varchar(50),
			Uom varchar(5),
			UC decimal(18, 8),
			MinUC decimal(18, 8),
			OpRef varchar(50), 
			OrderQty decimal(18, 8), 
			Location varchar(50), 
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
			ManufactureParty varchar(50),
			OpRef varchar(50),
			CPTime datetime,
			LocFrom varchar(50),
			LocTo varchar(50),
			IsCreateSeq bit,
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
		
		create table #tempICSeqOrderDet
		(
			RowId int identity(1, 1),
			SeqRowId int
		)
		
		declare @SeqGroup varchar(50) = null
		declare @AssOpRef varchar(50) = null
		declare @ProdLine varchar(50) = null
		declare @SeqBatch int
		declare @PrevOrderNo varchar(50) = null
		declare @PrevSeq bigint = null
		declare @PrevSubSeq int = null
		declare @SplitSymbol1 char(1) = ','
		declare @SplitSymbol2 char(1) = '|'
		declare @IsWMSKitOrder bit = null
		
		-----------------------------↓获取排序组和排序路线-----------------------------
		--获取排序路线
		insert into #tempSeqFlow(Flow, [Type], PartyFrom, PartyTo, LocFrom, LocTo, WinTimeDiff, EMLeadTime, LeadTime) 
		select stra.Flow, mstr.[Type], mstr.PartyFrom, mstr.PartyTo, mstr.LocFrom, mstr.LocTo, stra.WinTimeDiff, stra.EMLeadTime, stra.LeadTime
		from SCM_FlowStrategy as stra
		inner join SCM_FlowMstr as mstr on stra.Flow = mstr.Code
		where mstr.Code = @Flow and stra.Strategy = 4 --and mstr.IsActive = 1
		
		--获取单个排序组
		select @AssOpRef = seq.OpRef, @ProdLine = seq.ProdLine, @SeqBatch = seq.SeqBatch, @PrevOrderNo = seq.PrevOrderNo, @SeqGroup = Code
		from SCM_SeqGroup as seq
		inner join SCM_FlowStrategy as stra on seq.Code = stra.SeqGroup
		where stra.Flow = @Flow
		
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
				
		if not exists(select top 1 1 from #tempSeqFlow)
		begin
			set @ErrorMsg = N'排序路线不存在或没有生效。'
			RAISERROR(@ErrorMsg, 16, 1) 
		end
		
		if ISNULL(@TraceCode, '') <> ''
		begin
			--指定开始顺序号
			select @PrevSeq = Seq, @PrevSubSeq = SubSeq from ORD_OrderSeq where ProdLine = @ProdLine and TraceCode = @TraceCode
			if @PrevSeq is null
			begin
				set @ErrorMsg = N'Van号不存在或Van号在整车生产线上' + @ProdLine + N'没有对应的生产单。'
				RAISERROR(@ErrorMsg, 16, 1) 
			end
		end
		else if (@PrevOrderNo is null)
		begin
			--没有设置上次结转的整车车序，系统自动设置为0'
			select top 1 @PrevSeq = Seq, @PrevSubSeq = SubSeq from LE_OrderBomCPTimeSnapshot where VanProdLine = @ProdLine order by CPTime
		end
		else
		begin
			--查找最新的顺序，为了防止暂停/恢复后原车序发生变化
			select @PrevSeq = Seq, @PrevSubSeq = SubSeq from ORD_OrderSeq where OrderNo = @PrevOrderNo
		end
				
		if @IsWMSKitOrder = 0
		begin  --正常排序件
			-----------------------------↓循环安装工位插入缓存表中-----------------------------
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
						
			-------------------↓获取所有排序路线明细-----------------------
			insert into #tempFlowDet(Flow, Item, ItemDesc, RefItemCode, Uom, UC,
			MinUC, LocFrom, LocTo)
			select fdet.Flow as Flow, fdet.Item, i.Desc1 as ItemDesc, i.RefCode as RefItemCode, fdet.Uom, fdet.UC, 
			fdet.MinUC, mstr.LocFrom, mstr.LocTo
			from SCM_FlowDet as fdet
			inner join SCM_FlowMstr as mstr on fdet.Flow = mstr.Code
			inner join SCM_FlowStrategy as stra on stra.Flow = fdet.Flow
			inner join MD_Item as i on fdet.Item = i.Code
			where stra.Flow = @Flow --and fdet.IsActive = 1
			-------------------↑获取所有排序路线明细-----------------------
			
			
			-------------------↓获取引用路线明细-----------------------
			insert into #tempFlowDet(Flow, Item, ItemDesc, RefItemCode, Uom, UC,
			MinUC, LocFrom, LocTo)
			select mstr.Code as Flow, rDet.Item, i.Desc1 as ItemDesc, i.RefCode as RefItemCode, rDet.Uom, rDet.UC, 
			rDet.MinUC, mstr.LocFrom, mstr.LocTo
			from SCM_FlowMstr as mstr
			inner join SCM_FlowStrategy as stra on stra.Flow = mstr.Code
			inner join SCM_FlowDet as rDet on mstr.RefFlow = rDet.Flow
			inner join MD_Item as i on rDet.Item = i.Code
			where stra.Flow = @Flow --and rDet.IsActive = 1
			and not Exists(select top 1 1 from #tempFlowDet as tDet where tdet.Item = rdet.Item)
			-------------------↑获取引用路线明细-----------------------
							
			--锁定并缓存Bom
			truncate table #tempBomCPTime
			if exists(select top 1 1 from #tempAssOpRef)
			begin
				insert into #tempBomCPTime(OrderNo, Seq, SubSeq, CPTime, Flow, Item, ItemDesc, RefItemCode, Uom, UC, MinUC, OpRef, OrderQty, Location, BomId, ManufactureParty, LocFrom, LocTo, ZOPWZ, ZOPID, ZOPDS)
				select bom.OrderNo, seq.Seq, seq.SubSeq, bom.CPTime, fdet.Flow, bom.Item, fdet.ItemDesc, fdet.RefItemCode, fdet.Uom, fdet.UC, fdet.MinUC, bom.OpRef, bom.OrderQty, bom.Location, bom.BomId, bom.ManufactureParty, fdet.LocFrom, fdet.LocTo, bom.ZOPWZ, bom.ZOPID, bom.ZOPDS
				from LE_OrderBomCPTimeSnapshot as bom
				inner join ORD_OrderSeq as seq on seq.OrderNo = bom.OrderNo
				inner join #tempAssOpRef as opRef on bom.OpRef = opRef.AssOpRef
				inner join #tempFlowDet as fdet on fdet.Item = bom.Item
				inner join SCM_FlowStrategy as stra on stra.Flow = fdet.Flow
				where stra.Flow = @Flow
				--and bom.VanProdLine = @ProdLine 
				and (seq.Seq > @PrevSeq or (seq.Seq = @PrevSeq and seq.SubSeq > @PrevSubSeq))
			end
			else
			begin
				insert into #tempBomCPTime(OrderNo, Seq, SubSeq, CPTime, Flow, Item, ItemDesc, RefItemCode, Uom, UC, MinUC, OpRef, OrderQty, Location, BomId, ManufactureParty, LocFrom, LocTo, ZOPWZ, ZOPID, ZOPDS)
				select bom.OrderNo, seq.Seq, seq.SubSeq, bom.CPTime, fdet.Flow, bom.Item, fdet.ItemDesc, fdet.RefItemCode, fdet.Uom, fdet.UC, fdet.MinUC, bom.OpRef, bom.OrderQty, bom.Location, bom.BomId, bom.ManufactureParty, fdet.LocFrom, fdet.LocTo, bom.ZOPWZ, bom.ZOPID, bom.ZOPDS
				from LE_OrderBomCPTimeSnapshot as bom
				inner join ORD_OrderSeq as seq on seq.OrderNo = bom.OrderNo
				inner join #tempFlowDet as fdet on fdet.Item = bom.Item
				inner join SCM_FlowStrategy as stra on stra.Flow = fdet.Flow
				where stra.Flow = @Flow
				--and bom.VanProdLine = @ProdLine 
				and (seq.Seq > @PrevSeq or (seq.Seq = @PrevSeq and seq.SubSeq > @PrevSubSeq))
			end
		end
		else
		begin  --鞍座和变速器按工作中心找排序件
			if exists(select top 1 1 from (select ROW_NUMBER() over(order by Flow) as RowNm from #tempSeqFlow) as rn where rn.RowNm > 1)
			begin
				set @ErrorMsg = N'找到多条鞍座和变速器的排序路线'
				RAISERROR(@ErrorMsg, 16, 1) 
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
				truncate table #tempBomCPTime
				insert into #tempBomCPTime(OrderNo, Seq, SubSeq, CPTime, Flow, Item, ItemDesc, RefItemCode, Uom, UC, MinUC, OpRef, OrderQty, Location, BomId, ManufactureParty, LocFrom, LocTo, ZOPWZ, ZOPID, ZOPDS)
				select bom.OrderNo, seq.Seq, seq.SubSeq, bom.CPTime, @WMSKitOrderSeqFlow as Flow, bom.Item, i.Desc1 as ItemDesc, i.RefCode as RefItemCode, i.Uom, i.UC, i.MinUC, bom.OpRef, bom.OrderQty, bom.Location, bom.BomId, bom.ManufactureParty, mstr.LocFrom, mstr.LocTo, bom.ZOPWZ, bom.ZOPID, bom.ZOPDS
				from LE_OrderBomCPTimeSnapshot as bom
				inner join ORD_OrderSeq as seq on seq.OrderNo = bom.OrderNo
				inner join #tempWMSKitWorkCenter as pwc on bom.WorkCenter = pwc.WorkCenter
				inner join MD_Item as i on bom.Item = i.Code
				inner join SCM_FlowMstr as mstr on mstr.Code = pwc.Flow
				--where bom.VanProdLine = @ProdLine and (seq.Seq > @PrevSeq or (seq.Seq = @PrevSeq and seq.SubSeq > @PrevSubSeq))
				where (seq.Seq > @PrevSeq or (seq.Seq = @PrevSeq and seq.SubSeq > @PrevSubSeq))
			end
		end
		
		--如果指定零件，删除没有指定的零件
		if ISNULL(@Item, '') <> ''
		begin
			delete from #tempBomCPTime where Item <> @Item
		end
								
		--把物料消耗时间全部更新为最小的消耗时间
		update bom set CPTime = mBom.MINCPTime
		from #tempBomCPTime as bom
		inner join (select OrderNo, MIN(CPTime) as MINCPTime from #tempBomCPTime where CPTime is not null group by OrderNo) as mBom on bom.OrderNo = mBom.OrderNo
		
		--获取排序件
		insert into #tempSeqOrderDet(Seq, ExtNo, ExtSeq, 
		Item, ItemDesc, RefItemCode, ReqQty, OrderQty, 
		Uom, UC, MinUC, ManufactureParty, OpRef, CPTime, IsCreateSeq, ZOPWZ, ZOPID, ZOPDS, IsItemConsume)
		select ROW_NUMBER() over (order by ord.Seq, ord.SubSeq) as Seq, ord.TraceCode as ExtNo,
		(convert(varchar, (ord.Seq * 100 + ord.SubSeq))) as ExtSeq,
		bom.Item, bom.ItemDesc, bom.RefItemCode, bom.OrderQty as ReqQty, CASE WHEN ord.IsCreateSeq = 0 then bom.OrderQty else 0 end as OrderQty,
		bom.Uom, bom.UC, bom.MinUC, bom.ManufactureParty, bom.OpRef, bom.CPTime, ord.IsCreateSeq, bom.ZOPWZ, bom.ZOPID, bom.ZOPDS, 0
		from
		(select seq.Seq, seq.SubSeq, seq.OrderNo, seq.TraceCode, CASE WHEN trace.Id is null THEN 0 ELSE 1 END as IsCreateSeq
		from ORD_OrderSeq as seq 
		inner join ORD_OrderMstr_4 as mstr on seq.OrderNo = mstr.OrderNo
		left join ORD_SeqOrderTrace as trace on trace.TraceCode = seq.TraceCode and trace.SeqGroup = @SeqGroup
		where seq.ProdLine = @ProdLine and (seq.Seq > @PrevSeq or (seq.Seq = @PrevSeq and seq.SubSeq > @PrevSubSeq))
		and mstr.PauseStatus in (0, 1)) as ord
		left join #tempBomCPTime as bom on ord.OrderNo = bom.OrderNo --and ord.IsCreateSeq = 0
		order by ord.Seq, ord.SubSeq
									
		--为空序列赋值，取排序路线上的第一个零件，数量为0
		update det set Item = '_0517LJ2', ItemDesc = '虚拟物料', RefItemCode = '虚拟物料', OrderQty = 0,
		Uom = 'ST', UC = 1, MinUC = 1
		from #tempSeqOrderDet as det,
		(select top 1 det.Item, i.Desc1, i.RefCode as RefItemCode, det.Uom, i.UC, det.MinUC 
		from #tempFlowDet as det
		inner join MD_Item as i on det.Item = i.Code) as fdet
		where det.Item is null					
		
		--如果还有零件为空的直接取第一个零件
		update det set Item = '_0517LJ2', ItemDesc = '虚拟物料', RefItemCode = '虚拟物料', OrderQty = 0,
		Uom = 'ST', UC = 1, MinUC = 1
		from #tempSeqOrderDet as det,
		(select top 1 Code as Item, Desc1, RefCode as RefItemCode, Uom, UC, MinUC from MD_Item) as fdet
		where det.Item is null
									
		if (@CPTimeFrom is not null)
		begin
			delete from #tempSeqOrderDet where Id < (select MIN(Id) from #tempSeqOrderDet where CPTime = (select MIN(CPTime) from #tempSeqOrderDet where CPTime >= @CPTimeFrom))
			
			--重置Seq，从1开始
			declare @MinSeq int
			select @MinSeq = MIN(Seq) from #tempSeqOrderDet
			update #tempSeqOrderDet set Seq = Seq - @MinSeq + 1
		end
		
		
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
			
				select @ICId = Id, @ICItem = Item, @ICRemainQty = RemainQty
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
														Uom, UC, MinUC, ManufactureParty, OpRef, CPTime, ZOPWZ, ZOPID, ZOPDS, IsItemConsume)
							select @SeqRowSeq + 1, Item, ItemDesc, RefItemCode, @ICRemainQty, 0, 
														Uom, UC, MinUC, ManufactureParty, OpRef, CPTime, ZOPWZ, ZOPID, ZOPDS, 1
							from #tempSeqOrderDet where RowId = @SeqRowId
						end
					end
					else
					begin
						break
					end
					
					set @ICSeqRowId = @ICSeqRowId + 1
				end
				
				set @ICRowId = @ICRowId + 1
			end
		end
		-----------------------------↑处理厂内消化-----------------------------		
									
		if (@CPTimeTo is not null)
		begin
			delete from #tempSeqOrderDet where Id > (select MAX(Id) from #tempSeqOrderDet where CPTime = (select MAX(CPTime) from #tempSeqOrderDet where CPTime <= @CPTimeTo))
		end
		
		select count(1) from #tempSeqOrderDet
		if (@PageSize is not null and @PageCount is not null)
		begin
			declare @SeqFrom int = @PageSize * (@PageCount - 1) + 1
			declare @SeqTo int = @PageSize * @PageCount
			select Seq, 
			ExtNo, 
			ExtSeq,
			Item, 
			ItemDesc, 
			RefItemCode, 
			OrderQty, 
			Uom,
			ManufactureParty,
			OpRef,
			CPTime,
			LocFrom,
			LocTo,
			IsCreateSeq,
			ISNULL(ReqQty,0) AS ReqQty,
			'' as CreatedOrderNo,
			IsItemConsume
			from #tempSeqOrderDet where Seq between @SeqFrom and @SeqTo
			order by Seq
		end
		else
		begin
			select Seq, 
			ExtNo, 
			ExtSeq,
			Item, 
			ItemDesc, 
			RefItemCode, 
			OrderQty, 
			Uom,
			ManufactureParty,
			OpRef,
			CPTime,
			LocFrom,
			LocTo,
			IsCreateSeq,
			ISNULL(ReqQty,0) AS ReqQty,
			'' as CreatedOrderNo,
			IsItemConsume
			from #tempSeqOrderDet
			order by Seq
		end
		
		drop table #tempSeqOrderDet
		drop table #tempBomCPTime
		drop table #tempAssOpRef
		drop table #tempSeqFlow
	end try
	begin catch
		set @ErrorMsg = Error_Message() 
		RAISERROR(@ErrorMsg, 16, 1) 
	end catch 
END