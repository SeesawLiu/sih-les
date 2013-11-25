SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_CreateOrUpdateVanProdOrder') 
     DROP PROCEDURE USP_Busi_CreateOrUpdateVanProdOrder
GO

CREATE PROCEDURE [dbo].[USP_Busi_CreateOrUpdateVanProdOrder] 
(
	@BatchNo int,
	@VanProdLine varchar(50),
	@VanProdLineNm varchar(100),
	@VanProdLine2 varchar(50),
	@CreateUserId int,
	@CreateUserNm varchar(50),
	@OrderNo varchar(50) output
) --WITH ENCRYPTION
AS 
BEGIN 
	set nocount on
	declare @DateTimeNow datetime = GetDate()
	declare @ErrorMsg nvarchar(MAX)
		
	begin try
		-----------------------------↓获取SAP生产单-----------------------------
		declare @SapProdLine varchar(50)  --SAP生产线代码A/B
		declare @SapOrderNo varchar(50)   --SAP整车生产单（总装的生产单号，不是底盘的生产单号）
		declare @SapVAN varchar(50)       --VAN号
		declare @SapSeq bigint			  --SAP顺序号
		declare @SapStartTime datetime    --SAP排产日期
		declare @SapProdItem varchar(50)  --SAP整车物料号  
		declare @SapProdItemDesc varchar(50)  --SAP整车物料描述
		declare @SapProdItemUom varchar(5)	--SAP整车物料单位
		declare @IsOrderExists bit = 0
		declare @OrderVersion int
		declare @DAUAT varchar(50)
		
		select @SapProdLine = ZLINE, @SapOrderNo = AUFNR, @SapVAN = CHARG, @SapStartTime = GSTRS, 
		@SapProdItem = MATNR, @SapProdItemDesc = MAKTX, @SapProdItemUom = GMEIN, @SapSeq = Convert(bigint, CY_SEQNR),
		@DAUAT = DAUAT
		from SAP_ProdOrder WITH(NOLOCK) where BatchNo = @BatchNo
		
		select @OrderNo = OrderNo, @OrderVersion = [Version] from ORD_OrderMstr_4 with(NOLOCK) where Flow = @VanProdLine and TraceCode = @SapVAN
		
		if @OrderNo is not null
		begin
			set @IsOrderExists = 1
		end
		
		declare @IsProdLineActive bit  --生产线是否有效
		declare @Routing varchar(50)  --生产线工艺流程
		declare @VanProdLineRegion varchar(50)  --生产线区域
		declare @LocTo varchar(50)     --生产线原材料
		declare @LocFrom varchar(50)   --生产线成品
		declare @ProdLineType tinyint   --生产线类型
		declare @VirtualOpRef varchar(50)   --虚拟工位
		declare @TaktTime int           --节拍时间（秒）
		declare @FlowDesc varchar(100)	--路线描述
		declare @OrderTemplate varchar(100)	--订单模板
		
		--获取生产线信息
		select @VanProdLineRegion = PartyFrom, @LocFrom = LocFrom, @LocTo = LocTo, @Routing = Routing, 
		@IsProdLineActive = IsActive, @ProdLineType = ProdLineType, @TaktTime = TaktTime, @VirtualOpRef = VirtualOpRef,@FlowDesc = Desc1,@OrderTemplate=OrderTemplate
		from SCM_FlowMstr WITH(NOLOCK) where Code = @VanProdLine
		-----------------------------↑获取SAP生产单-----------------------------
		
		
		
		
		-----------------------------↓生成工序临时表-----------------------------
		Create table #tempRoutingDet
		(
			RowId int Identity(1, 1) Primary Key,
			LoopCount int,
			AssProdLine varchar(50),
			Routing varchar(50),
			JointOp int,               --合装工序
			Op int,
			OpRef varchar(50),
			Location varchar(50),
			WorkCenter varchar(50)
		)
		
		--插入主线工序
		declare @LoopCount int = 1
		insert into #tempRoutingDet(LoopCount, AssProdLine, Routing, JointOp, Op, OpRef, Location, WorkCenter)
		select @LoopCount, @VanProdLine, Routing, Op, Op, OpRef, Location, WorkCenter 
		from PRD_RoutingDet WITH(NOLOCK) where Routing = @Routing
		
		--插入分装线工序
		while exists(select top 1 1 from #tempRoutingDet as tDet WITH(NOLOCK)
					inner join SCM_FlowBinding as bind WITH(NOLOCK) on tDet.AssProdLine = bind.MstrFlow
					inner join SCM_FlowMstr as subPL WITH(NOLOCK) on bind.BindFlow = subPL.Code
					where tDet.LoopCount = @LoopCount)
		begin
			set @LoopCount = @LoopCount + 1
			
			insert into #tempRoutingDet(LoopCount, AssProdLine, Routing, JointOp, Op, OpRef, Location, WorkCenter)
			select @LoopCount, subPL.Code, subPL.Routing, bind.JointOp, rDet.Op, rDet.OpRef, rDet.Location, rDet.WorkCenter 
			from #tempRoutingDet as tDet WITH(NOLOCK)
			inner join SCM_FlowBinding as bind WITH(NOLOCK) on tDet.AssProdLine = bind.MstrFlow
			inner join SCM_FlowMstr as subPL WITH(NOLOCK) on bind.BindFlow = subPL.Code
			inner join PRD_RoutingDet as rDet WITH(NOLOCK) on rDet.Routing = subPL.Routing
			where tDet.LoopCount = @LoopCount - 1
			group by subPL.Code, subPL.Routing, bind.JointOp, rDet.Op, rDet.OpRef, rDet.Location, rDet.WorkCenter
		end
		-----------------------------↑生成工序临时表-----------------------------
		
		
		
		
		-----------------------------↓生成生产单Bom临时表-----------------------------
		Create table #tempVanProdLine
		(
			RowId int Identity(1, 1) Primary Key,
			VanProdLine varchar(50)
		)
		insert into #tempVanProdLine(VanProdLine) values(@VanProdLine)
		if (ISNULL(@VanProdLine2, '') <> '')
		begin
			insert into #tempVanProdLine(VanProdLine) values(@VanProdLine2)
		end
		
		Create table #tempOrderBom
		(
			RowId int identity(1, 1) Primary Key,
			SapBomId bigint,
			Item varchar(50),
			ItemDesc varchar(100),
			RefItemCode varchar(50),
			UOM varchar(5),
			ManufactureParty varchar(50),
			AssProdLine varchar(50),
			JointOp int,
			Op int,
			OpRef varchar(50),
			RefOpRef varchar(256),
			OrderQty decimal(18, 8),
			Location varchar(50),
			ReserveNo varchar(50),
			ReserveLine varchar(50),
			ZOPWZ varchar(50),
			ZOPID varchar(50),
			ZOPDS varchar(50),
			AUFNR varchar(50),
			ICHARG varchar(50),
			BWART varchar(50),
			IsScanHu bit,
			WorkCenter varchar(50),
			DISPO varchar(50),
			BESKZ varchar(100),
			SOBSL varchar(100),
			PLNFL varchar(50),
			VORNR varchar(50),
			AUFPL varchar(50)
		)
		
		--查找Bom
		insert into #tempOrderBom(SapBomId, Item, ItemDesc, RefItemCode, UOM, ManufactureParty,
		AssProdLine, JointOp, Op, OpRef, OrderQty, Location,
		ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, AUFNR, ICHARG, BWART,
		IsScanHu, WorkCenter, DISPO, BESKZ, SOBSL, PLNFL, VORNR, AUFPL)
		select bom.Id as SapBomId, bom.MATERIAL as Item, bom.MAKTX as ItemDesc, bom.BISMT as RefItemCode, bom.MEINS as UOM, case when bom.LIFNR <> '' then bom.LIFNR else null end as ManufactureParty,
		null as ProdLine, null as JointOp, null as Op, case when ISNULL(GW, '') = '' then @VirtualOpRef when Len(GW) > 5 then SUBSTRING(GW, 1, 5) else GW end as OpRef, case when BWART = '531' then -MDMNG else MDMNG end as OrderQty, null as Location,
		bom.RSNUM as ReserveNo, bom.RSPOS as ReserveLine, null as ZOPWZ, bom.ZOPID, bom.ZOPDS, bom.AUFNR, bom.ICHARG, bom.BWART,
		CASE WHEN trace.Item is not null THEN 1 ELSE 0 END as IsScanHu, pwc.WorkCenter, bom.DISPO, bom.BESKZ, bom.SOBSL, bom.PLNFL, bom.VORNR, bom.AUFPL
		from SAP_ProdBomDet as bom WITH(NOLOCK)
		inner join SAP_ProdRoutingDet as routing WITH(NOLOCK) on bom.BatchNo = routing.BatchNo 
		and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  --用工艺路线编号、顺序和操作活动关联
		inner join PRD_ProdLineWorkCenter as pwc WITH(NOLOCK) on routing.ARBPL = pwc.WorkCenter
		left join CUST_ItemTrace as trace WITH(NOLOCK) on bom.MATERIAL = trace.Item
		where bom.BatchNo = @BatchNo and bom.MDMNG > 0 and bom.RGEKZ = 'X' and pwc.Flow in (select VanProdLine from #tempVanProdLine)
		
		--if exists(select 1 from CUST_ProductLineMap where SapProdLine = @SapProdLine and CabProdLine = @VanProdLine and [Type] = 1)
		--begin  --驾驶室
		--	--如果驾驶室没有加入驾驶室生产单中，把驾驶室加入
		--	insert into #tempOrderBom(SapBomId, Item, ItemDesc, RefItemCode, UOM, ManufactureParty,
		--	AssProdLine, JointOp, Op, OpRef, OrderQty, Location,
		--	ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, AUFNR, ICHARG, BWART,
		--	IsScanHu, WorkCenter, DISPO, BESKZ, SOBSL, PLNFL, VORNR, AUFPL)
		--	select bom.Id as SapBomId, bom.MATERIAL as Item, bom.MAKTX as ItemDesc, bom.BISMT as RefItemCode, bom.MEINS as UOM, case when bom.LIFNR <> '' then bom.LIFNR else null end as ManufactureParty,
		--	null as ProdLine, null as JointOp, null as Op, case when ISNULL(GW, '') = '' then @VirtualOpRef when Len(GW) > 5 then SUBSTRING(GW, 1, 5) else GW end as OpRef, case when tbom.BWART = '531' then -MDMNG else MDMNG end as OrderQty, null as Location,
		--	bom.RSNUM as ReserveNo, bom.RSPOS as ReserveLine, null as ZOPWZ, bom.ZOPID, bom.ZOPDS, bom.AUFNR, bom.ICHARG, bom.BWART,
		--	CASE WHEN trace.Item is not null THEN 1 ELSE 0 END as IsScanHu, routing.ARBPL as WorkCenter, bom.DISPO, bom.BESKZ, bom.SOBSL, bom.PLNFL, bom.VORNR, bom.AUFPL 
		--	from SAP_ProdBomDet as bom
		--	inner join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo and bom.AUFPL = routing.AUFPL and bom.PLNFL=routing.PLNFL and bom .VORNR=routing.VORNR
		--	and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  --用工艺路线编号、顺序和操作活动关联
		--	left join CUST_ItemTrace as trace on bom.MATERIAL = trace.Item
		--	left join #tempOrderBom as tbom on bom.Id = tbom.SapBomId
		--	where bom.BatchNo = @BatchNo and bom.MDMNG > 0
		--	and ((bom.DISPO = 'IAF' and bom.BESKZ = 'E' and ISNULL(bom.SOBSL, '') = '') or (bom.DISPO = 'L13'))
		--	and tbom.SapBomId is null
		--end
		--else
		--begin  --非驾驶室
		--	--删除驾驶室的Bom
		--	delete tBom 
		--	from #tempOrderBom as tBom
		--	inner join SAP_ProdBomDet as bom on tBom.SapBomId = bom.Id
		--	where bom.BatchNo = @BatchNo and bom.MDMNG > 0
		--	and ((bom.DISPO = 'IAF' and bom.BESKZ = 'E' and ISNULL(bom.SOBSL, '') = '') or (bom.DISPO = 'L13'))
		--end
		
		--查找负数Bom，记录错误日志
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg)
		select @SapOrderNo, @SapProdLine, @BatchNo, N'替换件的负数Bom(MATERIAL:' + Item + N', MDMNG:' + CONVERT(varchar, OrderQty) + N', AUFPL:' + AUFPL + N' PLNFL:' + PLNFL + N', VORNR:' + VORNR + N', RSNUM:' + ReserveNo + N', RSPOS:' + ReserveLine + N')没有找到对应正数Bom。' 
		from #tempOrderBom where OrderQty < 0
		
		--删除负数Bom
		delete from #tempOrderBom where OrderQty < 0
		
		if not exists(select top 1 1 from #tempOrderBom)
		begin
			if @IsOrderExists = 0
			begin
				if @ProdLineType = 2
				begin
					set @ErrorMsg = N'底盘生产单Bom不能为空。'
					insert into Log_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @VanProdLine, @BatchNo, @ErrorMsg)
					RAISERROR(@ErrorMsg, 16, 1)
				end
				else
				begin
					return
				end
			end
			else
			begin
				delete from CUST_CabOut where OrderNo = @OrderNo
				delete from ORD_OrderItemTraceResult where OrderNo = @OrderNo
				delete from ORD_OrderItemTrace where OrderNo = @OrderNo
				delete from ORD_OrderOp where OrderNo = @OrderNo
				delete from ORD_OrderBomDet where OrderNo = @OrderNo
				delete from ORD_OrderDet_4 where OrderNo = @OrderNo
				delete from ORD_OrderSeq where OrderNo = @OrderNo
				delete from ORD_OrderMstr_4 where OrderNo = @OrderNo
			end
		end
		
		--更新工序
		declare @MinOp int  --最小工序，从主线上取
		select @MinOp = MIN(Op) from PRD_RoutingDet WITH(NOLOCK) where Routing = @Routing
		
		--更新安装生产线、工序、工位和物料消耗库位，工序取合装工序
		update bom set AssProdLine = IsNULL(det.AssProdLine, @VanProdLine),--工艺流程没有匹配的工位取整车生产线
		JointOp = IsNULL(det.JointOp, @MinOp),									--工艺流程没有匹配的工位取整车生产线的最小工序
		Op = IsNULL(det.Op, @MinOp),									--工艺流程没有匹配的工位取整车生产线的最小工序
		OpRef = ISNULL(det.OpRef, @VirtualOpRef),						--工艺流程没有匹配的工位取整车生产线的虚拟工位
		Location = det.Location
		from #tempOrderBom as bom
		left join #tempRoutingDet as det on bom.OpRef = det.OpRef
		
		--按工位映射表更新
		update bom set OpRef = map.OpRef, RefOpRef = map.RefOpRef
		from #tempOrderBom as bom
		inner join CUST_OpRefMap as map on bom.Item = map.Item
		where map.ProdLine = @VanProdLine
		
		--更新物料消耗库位，再从工作中心上找
		update bom set Location = wc.Location
		from #tempOrderBom as bom
		inner join MD_WorkCenter as wc on bom.WorkCenter = wc.Code 
		where bom.Location is null
		
		--更新物料消耗库位，最后取生产线上的原材料库位
		update #tempOrderBom set Location = @LocFrom where Location is null
		-----------------------------↑生成生产单Bom临时表-----------------------------
		
		
		
		-----------------------------↓新增/更新生产单头-----------------------------
		if @IsOrderExists = 0
		begin
			exec USP_GetDocNo_ORD @VanProdLine, 0, 4, 0, 0, 0, @VanProdLineRegion, @VanProdLineRegion, @LocTo, @LocFrom, null, 0, @OrderNo output
		
			insert into ORD_OrderMstr_4 (
			OrderNo,              --生产单号
			Flow,                 --生产线
			TraceCode,            --追溯码，VAN号
			OrderStrategy,        --策略，0
			ExtOrderNo,           --外部订单号，SAP生产单号
			[Type],               --类型，4生产单
			SubType,              --子类型，0正常
			QualityType,          --质量状态，0良品
			StartTime,            --开始时间
			WindowTime,           --窗口时间
			PauseSeq,             --暂停工序，0
			IsQuick,              --是否快速，0
			[Priority],           --优先级，0
			[Status],             --状态，1释放
			PartyFrom,            --区域代码
			PartyTo,              --区域代码
			LocFrom,              --原材料库位
			LocTo,                --成品库位
			IsInspect,            --下线检验，0
			IsAutoRelease,        --自动释放，0
			IsAutoStart,          --自动上线，0
			IsAutoShip,           --自动发货，0
			IsAutoReceive,        --自动收货，0
			IsAutoBill,           --自动账单，0
			IsManualCreateDet,    --手工创建明细，0
			IsListPrice,          --显示价格单，0
			IsPrintOrder,         --打印生产单，0
			IsOrderPrinted,       --生产单已打印，0
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
			OrderTemplate,        --订单模板
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
			FlowDesc,			  --生产线描述	
			ProdLineType,         --生产线类型
			PauseStatus,          --暂停状态，0
			ShipFromContact,      --存放整车物料号，
			ShipFromAddr,         --存放整车物料描述
			ShipFromFax,          --存放整车物料单位
			ShipFromTel			  --存放SAP生产单类型DAUAT
			)
			select 
			@OrderNo,                    --生产单号
			@VanProdLine,                --生产线
			@SapVAN,                     --追溯码，VAN号
			0,                           --策略，0
			@SapOrderNo,                 --外部订单号，SAP生产单号
			4,                           --类型，4生产单
			0,                           --子类型，0正常
			0,                           --质量状态，0良品
			@SapStartTime,               --开始时间
			@SapStartTime,               --窗口时间
			0,                           --暂停工序，0
			0,                           --是否快速，0
			0,                           --优先级，0
			1,                           --状态，0创建、1释放
			@VanProdLineRegion,          --区域代码
			@VanProdLineRegion,          --区域代码
			@LocFrom,                    --原材料库位
			@LocTo,                      --成品库位
			0,							 --下线检验，0
			0,							 --自动释放，0
			0,							 --自动上线，0
			0,							 --自动发货，0
			0,							 --自动收货，0
			0,							 --自动账单，0
			0,							 --手工创建明细，0
			0,						     --显示价格单，0
			0,							 --打印生产单，0
			0,							 --生产单已打印，0
			0,							 --打印ASN，0
			0,							 --打印收货单，0
			0,							 --允许超发，0
			0,							 --允许超收，0
			0,							 --整包装下单，0
			0,							 --整包装发货，0
			0,							 --整包装收货，0
			0,							 --发货扫描条码，0
			0,							 --收货扫描条码，0
			0,							 --创建拣货单，0
			0,							 --拣货单已创建，0
			0,							 --发货先进先出，0
			0,							 --收货先进先出，0
			0,							 --允许按订单发货，0
			0,							 --开口订单，0
			0,							 --ASN一次性收货，0
			0,							 --收货差异处理，0
			@OrderTemplate,              --订单模板
			0,							 --结算方式，0
			0,							 --创建条码选项，0
			0,							 --重新计算价格单选项，0
			@CreateUserId,               --创建用户
			@CreateUserNm,               --创建用户名称
			@DateTimeNow,                --创建日期
			@CreateUserId,               --最后修改用户
			@CreateUserNm,               --最后修改用户名称
			@DateTimeNow,                --最后修改日期
			@CreateUserId,               --释放用户
			@CreateUserNm,               --释放用户名称
			@DateTimeNow,                --释放日期
			1,                           --版本，1
			@FlowDesc,					 --生产线描述
			@ProdLineType,               --生产线类型
			0,                           --暂停状态，0
			@SapProdItem,				 --存放整车物料号，
			@SapProdItemDesc,            --存放整车物料描述
			@SapProdItemUom,             --存放整车物料单位
			@DAUAT						 --SAP生产单类型
		end
		-----------------------------↑新增生产单头-----------------------------
		
		
		
		-----------------------------↓新增生产单顺序-----------------------------
		if @IsOrderExists = 0
		begin
			--新增车序表
			insert into ORD_OrderSeq (
			ProdLine,
			OrderNo,
			TraceCode,
			Seq,
			SubSeq,
			SapSeq,
			CreateUser,
			CreateUserNm,
			CreateDate,
			LastModifyUser,
			LastModifyUserNm,
			LastModifyDate,
			[Version]
			)
			values( 
			@VanProdLine,
			@OrderNo,
			@SapVAN,
			@SapSeq,
			1,
			@SapSeq,
			@CreateUserId,
			@CreateUserNm,
			@DateTimeNow,
			@CreateUserId,
			@CreateUserNm,
			@DateTimeNow,
			1
			)
		end
		-----------------------------↑新增生产单顺序-----------------------------
		
		
		
		-----------------------------↓新增生产单明细-----------------------------
		declare @OrderDetId int
		if @IsOrderExists = 0
		begin
			exec USP_SYS_GetNextId 'ORD_OrderDet', @OrderDetId output
			
			insert into ORD_OrderDet_4 (
			Id,                         --生产单明细标识
			OrderNo,                    --生产单号
			OrderType,                  --类型，4生产单
			OrderSubType,               --子类型，0正常
			Seq,						--行号，1
			ScheduleType,               --计划协议类型，0
			Item,                       --整车物料号
			ItemDesc,                   --整车物料描述
			Uom,                        --单位
			BaseUom,                    --基本单位
			UC,                         --包装，1
			MinUC,                      --最小包装，1
			QualityType,                --质量状态，0
			ReqQty,                     --需求数量，1
			OrderQty,                   --订单数量，1
			ShipQty,                    --发货数量，0
			RecQty,                     --收货数量，0
			RejQty,                     --次品数量，0
			ScrapQty,                   --废品数量，0
			PickQty,                    --拣货数量，0
			UnitQty,                    --单位用量，1
			IsInspect,                  --是否检验，0
			IsProvEst,                  --是否暂估价，0
			IsIncludeTax,               --是否含税价，0
			IsScanHu,                   --是否扫描条码，0
			CreateUser,                 --创建用户
			CreateUserNm,               --创建用户名称
			CreateDate,                 --创建日期
			LastModifyUser,             --最后修改用户
			LastModifyUserNm,           --最后修改用户名称
			LastModifyDate,             --最后修改日期
			[Version],					--版本，1
			IsChangeUC					--是否修改单包装，0
			)
			select 
			@OrderDetId,                --生产单明细标识
			@OrderNo,                   --生产单号
			4,                          --类型，4生产单
			0,                          --子类型，0正常
			1,							--行号，1
			0,                          --计划协议类型，0
			@SapProdItem,               --整车物料号
			@SapProdItemDesc,           --整车物料描述
			@SapProdItemUom,            --单位
			@SapProdItemUom,            --基本单位
			1,                          --包装，1
			1,                          --最小包装，1
			0,                          --质量状态，0
			1,                          --需求数量，1
			1,                          --订单数量，1
			0,                          --发货数量，0
			0,                          --收货数量，0
			0,                          --次品数量，0
			0,                          --废品数量，0
			0,                          --拣货数量，0
			1,                          --单位用量，1
			0,                          --是否检验，0
			0,                          --是否暂估价，0
			0,                          --是否含税价，0
			0,                          --是否扫描条码，0
			@CreateUserId,              --创建用户
			@CreateUserNm,              --创建用户名称
			@DateTimeNow,               --创建日期
			@CreateUserId,              --最后修改用户
			@CreateUserNm,              --最后修改用户名称
			@DateTimeNow,               --最后修改日期
			1,							--版本，1
			0							--是否修改单包装，0
		end
		else
		begin
			select @OrderDetId = Id from ORD_OrderDet_4 WITH(NOLOCK) where OrderNo = @OrderNo
		end
		-----------------------------↑新增生产单明细-----------------------------
		
		
		
		-----------------------------↓新增生产单Bom-----------------------------
		if @IsOrderExists = 0
		begin
			INSERT INTO ORD_OrderBomDet (
			OrderNo,					--生产单号
			OrderType,					--类型，4生产单
			OrderSubType,				--子类型，0正常
			OrderDetId,					--生产单明细标识
			OrderDetSeq,				--生产单明细顺序号
			Seq,						--顺序号
			Item,						--Bom零件号
			ItemDesc,					--Bom零件描述
			RefItemCode,				--旧物料号
			Uom,						--单位
			BaseUom,					--基本单位
			ManufactureParty,			--指定供应商
			JointOp,					--合装工序，对于分装线JointOp代表合装的工序
			Op,							--工序
			OpRef,						--工位
			RefOpRef,					--参考工位
			OrderQty,					--Bom用量
			BFQty,						--反冲合格数量
			BFRejQty,					--反冲不合格数量
			BFScrapQty,					--反冲废品数量
			UnitQty,					--单位用量
			BomUnitQty,					--单个成品用量
			Location,					--反冲库位
			IsPrint,					--是否打印
			BackFlushMethod,            --回冲方式
			FeedMethod,                 --投料方式
			IsAutoFeed,                 --是否自动投料
			IsScanHu,                   --是否关键件
			EstConsumeTime,             --预计消耗时间
			ReserveNo,                  --预留号
			ReserveLine,                --预留行号
			ZOPWZ,						--工艺顺序号
			ZOPID,						--工位ID
			ZOPDS,						--工序描述
			AUFNR,						--生产单号
			CreateUser,					--创建用户
			CreateUserNm,				--创建用户名称
			CreateDate,					--创建日期
			LastModifyUser,				--最后修改用户
			LastModifyUserNm,			--最后修改用户名称
			LastModifyDate,				--最后修改日期
			[Version],					--版本，1
			ICHARG,						--批号
			BWART,						--移动类型
			AssProdLine,				--零件安装的生产线/可能是分装线，没有工位或者工位不存在的取虚拟工位
			IsCreateOrder,				--是否已经创建拉料单
			DISPO,						--MRP控制者
			BESKZ,                      --采购类型
			SOBSL,                      --特殊采购类型
			WorkCenter,					--工作中心
			PLNFL,						--序列
			VORNR,						--操作
			AUFPL						--工艺路线编号
			)
			select
			@OrderNo,					--生产单号
			4,							--类型，4生产单
			0,							--子类型，0正常
			@OrderDetId,				--生产单明细标识
			1,							--生产单明细顺序号
			ROW_NUMBER() over (order by Op, OpRef),--顺序号
			Item,						--Bom零件号
			ItemDesc,					--Bom零件描述
			RefItemCode,				--旧物料号
			Uom,						--单位
			Uom,						--基本单位
			ManufactureParty,			--指定供应商
			JointOp,					--合装工序，对于分装线JointOp代表合装的工序
			Op,							--工序
			OpRef,						--工位
			RefOpRef,					--参考工位
			OrderQty,					--Bom用量
			0,							--反冲合格数量
			0,							--反冲不合格数量
			0,							--反冲废品数量
			1,							--单位用量
			OrderQty,					--单个成品用量
			Location,					--反冲库位
			0,							--是否打印
			0,							--回冲方式
			0,							--投料方式
			0,							--是否自动投料
			IsScanHu,                   --是否关键件
			@DateTimeNow,				--预计消耗时间
			ReserveNo,                  --预留号
			ReserveLine,                --预留行号
			ZOPWZ,						--工艺顺序号
			ZOPID,						--工位ID
			ZOPDS,						--工序描述
			AUFNR,						--生产单号
			@CreateUserId,              --创建用户
			@CreateUserNm,              --创建用户名称
			@DateTimeNow,               --创建日期
			@CreateUserId,              --最后修改用户
			@CreateUserNm,              --最后修改用户名称
			@DateTimeNow,               --最后修改日期
			1,							--版本，1
			ICHARG,						--批号
			BWART,						--移动类型
			AssProdLine,				--零件安装的生产线/可能是分装线
			0,							--是否已经创建拉料单
			DISPO,						--MRP控制者
			BESKZ,                      --采购类型
			SOBSL,                      --特殊采购类型
			WorkCenter,					--工作中心
			PLNFL,						--序列
			VORNR,						--操作
			AUFPL						--工艺路线编号
			from #tempOrderBom
		end
		else
		begin
			----已经计算过的JIT零件要退回BOM用量至工位溢出量中
			--update orb set Qty = Qty + bom.orderQty, [Version] = orb.[Version] + 1, 
			--LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm
			--from ORD_OrderBomDet as bom 
			--inner join #tempOrderBom as tbom on bom.ReserveNo = tbom.ReserveNo and bom.ReserveLine = tbom.ReserveLine and bom.OpRef = tbom.OpRef and bom.OrderQty <> tbom.OrderQty
			--inner join SCM_OpRefBalance as orb on bom.Item = orb.Item and bom.OpRef = orb.OpRef
			--where bom.OrderNo = @OrderNo and bom.IsCreateOrder = 1
			
			----按预留号和预留行号更新Bom数量和工位
			--update bom set OrderQty = tbom.OrderQty, BomUnitQty = tbom.OrderQty, Op = tbom.Op, OpRef = tbom.OpRef, IsCreateOrder = 0, LastModifyDate = @DateTimeNow, 
			--LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, [Version] = bom.[Version] + 1
			--from ORD_OrderBomDet as bom 
			--inner join #tempOrderBom as tbom on bom.ReserveNo = tbom.ReserveNo and bom.ReserveLine = tbom.ReserveLine and bom.OpRef = tbom.OpRef and bom.OrderQty <> tbom.OrderQty
			--where bom.OrderNo = @OrderNo
			
			----新的BOM没有该零件把已经计算过的JIT零件要退回BOM用量至工位溢出量中
			--update orb set Qty = Qty + bom.orderQty, [Version] = orb.[Version] + 1,
			--LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm
			--from ORD_OrderBomDet as bom 
			--inner join SCM_OpRefBalance as orb on bom.Item = orb.Item and bom.OpRef = orb.OpRef
			--left join #tempOrderBom as tbom on bom.ReserveNo = tbom.ReserveNo and bom.ReserveLine = tbom.ReserveLine and bom.OpRef = tbom.OpRef
			--where bom.OrderNo = @OrderNo and tbom.Item is null and bom.IsCreateOrder = 1
			
			----删除新的BOM没有该零件的
			--delete bom from ORD_OrderBomDet as bom 
			--left join #tempOrderBom as tbom on bom.ReserveNo = tbom.ReserveNo and bom.ReserveLine = tbom.ReserveLine and bom.OpRef = tbom.OpRef
			--where bom.OrderNo = @OrderNo and tbom.Item is null
			
			----获取BOm最大序号
			--Declare @OrderBomSeq int
			--select @OrderBomSeq = Max(Seq) from ORD_OrderBomDet where OrderNo = @OrderNo
			--if @OrderBomSeq is null
			--begin
			--	set @OrderBomSeq = 1
			--end
			
			----新增旧的Bom没有该零件的
			--INSERT INTO ORD_OrderBomDet (
			--OrderNo,					--生产单号
			--OrderType,					--类型，4生产单
			--OrderSubType,				--子类型，0正常
			--OrderDetId,					--生产单明细标识
			--OrderDetSeq,				--生产单明细顺序号
			--Seq,						--顺序号
			--Item,						--Bom零件号
			--ItemDesc,					--Bom零件描述
			--RefItemCode,				--旧物料号
			--Uom,						--单位
			--BaseUom,					--基本单位
			--ManufactureParty,			--指定供应商
			--JointOp,					--合装工序，对于分装线JointOp代表合装的工序
			--Op,							--工序
			--OpRef,						--工位
			--OrderQty,					--Bom用量
			--BFQty,						--反冲合格数量
			--BFRejQty,					--反冲不合格数量
			--BFScrapQty,					--反冲废品数量
			--UnitQty,					--单位用量
			--BomUnitQty,					--单个成品用量
			--Location,					--反冲库位
			--IsPrint,					--是否打印
			--BackFlushMethod,            --回冲方式
			--FeedMethod,                 --投料方式
			--IsAutoFeed,                 --是否自动投料
			--IsScanHu,                   --是否关键件
			--EstConsumeTime,             --预计消耗时间
			--ReserveNo,                  --预留号
			--ReserveLine,                --预留行号
			--ZOPWZ,						--工艺顺序号
			--ZOPID,						--工位ID
			--ZOPDS,						--工序描述
			--AUFNR,						--生产单号
			--CreateUser,					--创建用户
			--CreateUserNm,				--创建用户名称
			--CreateDate,					--创建日期
			--LastModifyUser,				--最后修改用户
			--LastModifyUserNm,			--最后修改用户名称
			--LastModifyDate,				--最后修改日期
			--[Version],					--版本，1
			--ICHARG,						--批号
			--BWART,						--移动类型
			--AssProdLine,				--零件安装的生产线/可能是分装线，没有工位或者工位不存在的去虚拟工位
			--IsCreateOrder,				--是否已经创建拉料单
			--DISPO,						--MRP控制者
			--BESKZ,                      --采购类型
			--SOBSL,                      --特殊采购类型
			--WorkCenter,					--工作中心
			--PLNFL,						--序列
			--VORNR,						--操作
			--AUFPL						--工艺路线编号
			--)
			--select
			--@OrderNo,					--生产单号
			--4,							--类型，4生产单
			--0,							--子类型，0正常
			--@OrderDetId,				--生产单明细标识
			--1,							--生产单明细顺序号
			--@OrderBomSeq + ROW_NUMBER() over (order by tbom.Op, tbom.OpRef),		--顺序号
			--tbom.Item,					--Bom零件号
			--tbom.ItemDesc,				--Bom零件描述
			--tbom.RefItemCode,			--旧物料号
			--tbom.Uom,					--单位
			--tbom.Uom,					--基本单位
			--tbom.ManufactureParty,		--指定供应商
			--tbom.JointOp,				--合装工序，对于分装线JointOp代表合装的工序
			--tbom.Op,					--工序
			--tbom.OpRef,					--工位
			--tbom.OrderQty,				--Bom用量
			--0,							--反冲合格数量
			--0,							--反冲不合格数量
			--0,							--反冲废品数量
			--1,							--单位用量
			--tbom.OrderQty,				--单个成品用量
			--tbom.Location,				--反冲库位
			--0,							--是否打印
			--0,							--回冲方式
			--0,							--投料方式
			--0,							--是否自动投料
			--tbom.IsScanHu,				--是否关键件
			--@DateTimeNow,				--预计消耗时间
			--tbom.ReserveNo,				--预留号
			--tbom.ReserveLine,			--预留行号
			--tbom.ZOPWZ,					--工艺顺序号
			--tbom.ZOPID,					--工位ID
			--tbom.ZOPDS,					--工序描述
			--tbom.AUFNR,					--生产单号
			--@CreateUserId,              --创建用户
			--@CreateUserNm,              --创建用户名称
			--@DateTimeNow,               --创建日期
			--@CreateUserId,              --最后修改用户
			--@CreateUserNm,              --最后修改用户名称
			--@DateTimeNow,               --最后修改日期
			--1,							--版本，1
			--tbom.ICHARG,				--批号
			--tbom.BWART,					--移动类型
			--tbom.AssProdLine,			--零件安装的生产线/可能是分装线
			--0,							--是否已经创建拉料单
			--tbom.DISPO,					--MRP控制者
			--tbom.BESKZ,                 --采购类型
			--tbom.SOBSL,                 --特殊采购类型
			--tbom.WorkCenter,			--工作中心
			--tbom.PLNFL,					--序列
			--tbom.VORNR,					--操作
			--tbom.AUFPL					--工艺路线编号
			--from #tempOrderBom as tbom
			--left join ORD_OrderBomDet as bom on bom.ReserveNo = tbom.ReserveNo and bom.ReserveLine = tbom.ReserveLine and bom.OpRef = tbom.OpRef
			--where bom.Id is null
			
			--已经计算过的JIT零件要退回BOM用量至工位溢出量中
			update orb set Qty = Qty + bom.OrderQty, [Version] = orb.[Version] + 1, 
			LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm
			from ORD_OrderBomDet as bom 
			inner join SCM_OpRefBalance as orb on bom.Item = orb.Item and bom.OpRef = orb.OpRef
			where bom.OrderNo = @OrderNo and bom.IsCreateOrder = 1 and bom.IsCreateSeq <> 1
			
			--新增不存在的工位余量
			insert into SCM_OpRefBalance(Item, OpRef, Qty, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, [Version])
			select bom.Item, bom.OpRef, SUM(bom.OrderQty) as Qty, @CreateUserId, @CreateUserNm, @DateTimeNow, @CreateUserId, @CreateUserNm, @DateTimeNow, 1
			from ORD_OrderBomDet as bom WITH(NOLOCK) 
			left join SCM_OpRefBalance as orb WITH(NOLOCK) on orb.Item = bom.Item and orb.OpRef = bom.OpRef
			where bom.OrderNo = @OrderNo and bom.IsCreateOrder = 1 and bom.IsCreateSeq <> 1 and orb.Id is null
			group by bom.Item, bom.OpRef
			
			--删除原生产单Bom
			delete from ORD_OrderBomDet where OrderNo = @OrderNo
			
			--新增生产单Bom
			INSERT INTO ORD_OrderBomDet (
			OrderNo,					--生产单号
			OrderType,					--类型，4生产单
			OrderSubType,				--子类型，0正常
			OrderDetId,					--生产单明细标识
			OrderDetSeq,				--生产单明细顺序号
			Seq,						--顺序号
			Item,						--Bom零件号
			ItemDesc,					--Bom零件描述
			RefItemCode,				--旧物料号
			Uom,						--单位
			BaseUom,					--基本单位
			ManufactureParty,			--指定供应商
			JointOp,					--合装工序，对于分装线JointOp代表合装的工序
			Op,							--工序
			OpRef,						--工位
			RefOpRef,					--参考工位
			OrderQty,					--Bom用量
			BFQty,						--反冲合格数量
			BFRejQty,					--反冲不合格数量
			BFScrapQty,					--反冲废品数量
			UnitQty,					--单位用量
			BomUnitQty,					--单个成品用量
			Location,					--反冲库位
			IsPrint,					--是否打印
			BackFlushMethod,            --回冲方式
			FeedMethod,                 --投料方式
			IsAutoFeed,                 --是否自动投料
			IsScanHu,                   --是否关键件
			EstConsumeTime,             --预计消耗时间
			ReserveNo,                  --预留号
			ReserveLine,                --预留行号
			ZOPWZ,						--工艺顺序号
			ZOPID,						--工位ID
			ZOPDS,						--工序描述
			AUFNR,						--生产单号
			CreateUser,					--创建用户
			CreateUserNm,				--创建用户名称
			CreateDate,					--创建日期
			LastModifyUser,				--最后修改用户
			LastModifyUserNm,			--最后修改用户名称
			LastModifyDate,				--最后修改日期
			[Version],					--版本，1
			ICHARG,						--批号
			BWART,						--移动类型
			AssProdLine,				--零件安装的生产线/可能是分装线，没有工位或者工位不存在的取虚拟工位
			IsCreateOrder,				--是否已经创建拉料单
			DISPO,						--MRP控制者
			BESKZ,                      --采购类型
			SOBSL,                      --特殊采购类型
			WorkCenter,					--工作中心
			PLNFL,						--序列
			VORNR,						--操作
			AUFPL						--工艺路线编号
			)
			select
			@OrderNo,					--生产单号
			4,							--类型，4生产单
			0,							--子类型，0正常
			@OrderDetId,				--生产单明细标识
			1,							--生产单明细顺序号
			ROW_NUMBER() over (order by Op, OpRef),--顺序号
			Item,						--Bom零件号
			ItemDesc,					--Bom零件描述
			RefItemCode,				--旧物料号
			Uom,						--单位
			Uom,						--基本单位
			ManufactureParty,			--指定供应商
			JointOp,					--合装工序，对于分装线JointOp代表合装的工序
			Op,							--工序
			OpRef,						--工位
			RefOpRef,					--参考工位
			OrderQty,					--Bom用量
			0,							--反冲合格数量
			0,							--反冲不合格数量
			0,							--反冲废品数量
			1,							--单位用量
			OrderQty,					--单个成品用量
			Location,					--反冲库位
			0,							--是否打印
			0,							--回冲方式
			0,							--投料方式
			0,							--是否自动投料
			IsScanHu,                   --是否关键件
			@DateTimeNow,				--预计消耗时间
			ReserveNo,                  --预留号
			ReserveLine,                --预留行号
			ZOPWZ,						--工艺顺序号
			ZOPID,						--工位ID
			ZOPDS,						--工序描述
			AUFNR,						--生产单号
			@CreateUserId,              --创建用户
			@CreateUserNm,              --创建用户名称
			@DateTimeNow,               --创建日期
			@CreateUserId,              --最后修改用户
			@CreateUserNm,              --最后修改用户名称
			@DateTimeNow,               --最后修改日期
			1,							--版本，1
			ICHARG,						--批号
			BWART,						--移动类型
			AssProdLine,				--零件安装的生产线/可能是分装线
			0,							--是否已经创建拉料单
			DISPO,						--MRP控制者
			BESKZ,                      --采购类型
			SOBSL,                      --特殊采购类型
			WorkCenter,					--工作中心
			PLNFL,						--序列
			VORNR,						--操作
			AUFPL						--工艺路线编号
			from #tempOrderBom
			
			if not exists(select top 1 1 from ORD_OrderBomDet WITH(NOLOCK) where OrderNo = @OrderNo)
			begin
				update ORD_OrderMstr_4 set [Status] = 3, CompleteDate = @DateTimeNow, CompleteUser = @CreateUserId, CompleteUserNm = @CreateUserNm, [Version] = [Version] + 1
				where OrderNo = @OrderNo and [Version] = @OrderVersion
				
				if @@RowCount = 0
				begin
					set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经更新。'
					insert into Log_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @VanProdLine, @BatchNo, @ErrorMsg)
					RAISERROR(@ErrorMsg, 16, 1)
				end
			end
		end
		-----------------------------↑新增生产单Bom-----------------------------
		
		
		
		-----------------------------↓新增生产单Op-----------------------------
		if @IsOrderExists = 0
		begin
			INSERT INTO ORD_OrderOp (
			OrderNo,					--生产单号
			OrderDetId,					--生产单明细标识
			Op,							--工序
			OpRef,                      --工位, 生成订单工序时不考虑工位
			LeadTime,					--前置期
			TimeUnit,					--前置期单位
			CreateUser,					--创建用户
			CreateUserNm,				--创建用户名称
			CreateDate,					--创建日期
			LastModifyUser,				--最后修改用户
			LastModifyUserNm,			--最后修改用户名称
			LastModifyDate,				--最后修改日期
			[Version],					--版本，1
			WorkCenter,					--工作中心
			IsAutoReport,				--是否自动报工
			ReportQty,					--报工数量
			BackflushQty,				--成品物料反冲数量
			ScrapQty,					--废品数量
			NeedReport,					--是否报工工序
			IsRecFG						--是否收货工序
			)
			select
			@OrderNo,					--生产单号
			@OrderDetId,				--生产单明细标识
			Op,							--工序
			'',							--工位, 生成订单工序时不考虑工位
			0,							--前置期
			1,							--前置期单位
			@CreateUserId,              --创建用户
			@CreateUserNm,              --创建用户名称
			@DateTimeNow,               --创建日期
			@CreateUserId,              --最后修改用户
			@CreateUserNm,              --最后修改用户名称
			@DateTimeNow,               --最后修改日期
			1,							--版本，1
			Max(WorkCenter),			--工作中心
			CASE WHEN SUM(CASE WHEN IsReport = 0 then 0 else 1 End) = 0 then 0 else 1 end,				--是否自动报工
			0,							--报工数量
			0,							--成品物料反冲数量
			0,							--废品数量
			1,							--是否报工工序
			0							--是否收货工序
			from PRD_RoutingDet WITH(NOLOCK)
			where Routing = @Routing
			group by Op
			
			--INSERT INTO CUST_SAPRoutingDet(AUFNR, WERKS, AUFPL, APLZL, PLNTY, PLNNR, PLNAL, PLNFL, VORNR, ARBPL, RUEK, AUTWE) 
			--select AUFNR, WERKS, AUFPL, APLZL, PLNTY, PLNNR, PLNAL, PLNFL, VORNR, ARBPL, RUEK, AUTWE
			--from SAP_ProdRoutingDet where BatchNo = @BatchNo
		end
		-----------------------------↑新增生产单Op-----------------------------
		
		
		
		-----------------------------↓新增关键件扫描-----------------------------
		if @IsOrderExists = 1
		begin
			delete from ORD_OrderItemTrace where OrderNo = @OrderNo
		end
		
		INSERT INTO ORD_OrderItemTrace(
		OrderNo,
		Item,
		ItemDesc,
		RefItemCode,
		Op,
		OpRef,
		Qty,
		ScanQty,
		CreateUser,
		CreateUserNm,
		CreateDate,
		LastModifyUser,
		LastModifyUserNm,
		LastModifyDate,
		Version,
		TraceCode)
		select
		mstr.OrderNo,
		bom.Item,
		bom.ItemDesc,
		bom.RefItemCode,
		bom.Op,
		bom.OpRef,
		SUM(bom.OrderQty),
		0,
		@CreateUserId,
		@CreateUserNm,
		@DateTimeNow,
		@CreateUserId,
		@CreateUserNm,
		@DateTimeNow,
		1,
		mstr.TraceCode
		from ORD_OrderBomDet as bom  WITH(NOLOCK)
		inner join ORD_OrderMstr_4 as mstr WITH(NOLOCK) on bom.OrderNo = mstr.OrderNo
		where mstr.OrderNo = @OrderNo and bom.IsScanHu = 1
		group by mstr.OrderNo, bom.Item, bom.ItemDesc, bom.RefItemCode, bom.Op, bom.OpRef, mstr.TraceCode
		having SUM(bom.OrderQty) > 0
		
		if @IsOrderExists = 1 and exists(select top 1 1 from ORD_OrderItemTraceResult WITH(NOLOCK) where OrderNo = @OrderNo)
		begin
			if exists(select top 1 1 from ORD_OrderItemTraceResult as itr WITH(NOLOCK) left join ORD_OrderItemTrace as it WITH(NOLOCK) on itr.OrderNo = it.OrderNo and itr.Item = it.Item
				where itr.OrderNo = @OrderNo and it.Id is null)
			begin
				select top 1 @ErrorMsg = N'更新后整车生产单' + @OrderNo + N'的Bom不存在和已扫描的关键件' + itr.Item + N'匹配的记录，请删除后再次更新。'
				from ORD_OrderItemTraceResult as itr WITH(NOLOCK) left join ORD_OrderItemTrace as it WITH(NOLOCK) on itr.OrderNo = it.OrderNo and itr.Item = it.Item
				where itr.OrderNo = @OrderNo and it.Id is null
				
				insert into Log_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @VanProdLine, @BatchNo, @ErrorMsg)
				RAISERROR(@ErrorMsg, 16, 1)
			end
			
			if exists(select top 1 1 from ORD_OrderItemTraceResult as itr WITH(NOLOCK) inner join ORD_OrderItemTrace as it WITH(NOLOCK) on itr.OrderNo = it.OrderNo and itr.Item = it.Item
				where itr.OrderNo = @OrderNo group by itr.Id having count(*) > 1)
			begin
				select top 1 @ErrorMsg = N'更新后整车生产单' + @OrderNo + N'的Bom存在多条和已经扫描的关键件' + itr.Item + N'匹配的记录，请删除后再次更新。'
				from ORD_OrderItemTraceResult as itr WITH(NOLOCK) inner join ORD_OrderItemTrace as it WITH(NOLOCK) on itr.OrderNo = it.OrderNo and itr.Item = it.Item
				where itr.OrderNo = @OrderNo group by itr.Id, itr.Item having count(*) > 1
				
				insert into Log_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @VanProdLine, @BatchNo, @ErrorMsg)
				RAISERROR(@ErrorMsg, 16, 1)
			end
			
			--重新更新关键件扫描表的已经扫描数量
			update it set ScanQty = a.ScanQty
			from ORD_OrderItemTrace as it inner join 
			(select it.Id, COUNT(distinct itr.OrderItemTraceId) as ScanQty
			from ORD_OrderItemTraceResult as itr WITH(NOLOCK) inner join ORD_OrderItemTrace as it WITH(NOLOCK) on itr.OrderNo = it.OrderNo and itr.Item = it.Item
			where itr.OrderNo = @OrderNo
			group by it.Id) as a on it.Id = a.Id
		end
		-----------------------------↑新增关键件扫描-----------------------------
		
		
		
		-----------------------------↓新增驾驶室出库-----------------------------
		declare @RowCount int
		declare @CabItem varchar(50)   --驾驶室物料号
		declare @CabItemDesc varchar(100)   --驾驶室物料描述
		declare @CabType tinyint		--自制/外购驾驶室标记
		
		--自制的油漆后驾驶室：MRP控制者IAF不变，采购类型为E，特殊采购类型为空。
		--外购的油漆后驾驶室：MRP控制者L13
		select @CabItem = Item, @CabItemDesc = ItemDesc from #tempOrderBom where DISPO = 'IAF' and BESKZ = 'E' and ISNULL(SOBSL, '') = ''
		set @RowCount = @@rowcount
		if @RowCount = 0
		begin
			select @CabItem = Item, @CabItemDesc = ItemDesc from #tempOrderBom where DISPO = 'L13'
			set @RowCount = @@rowcount
			
			if @RowCount = 1
			begin
				set @CabType = 1
			end
			else if @RowCount > 1
			begin
				set @ErrorMsg = N'找到多个外购驾驶室。'
				insert into Log_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @VanProdLine, @BatchNo, @ErrorMsg)
				RAISERROR(@ErrorMsg, 16, 1)
			end
		end
		else if @RowCount = 1
		begin
			set @CabType = 0
		end
		else if @RowCount > 1
		begin
			set @ErrorMsg = N'找到多个自制驾驶室。'
			insert into Log_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @VanProdLine, @BatchNo, @ErrorMsg)
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if @RowCount = 1
		begin
			if @IsOrderExists = 0
			begin
				insert into CUST_CabOut(
				OrderNo, 
				ProdLine, 
				CabType, 
				CabItem,
				CabItemDesc,
				[Status], 
				CreateUser, 
				CreateUserNm, 
				CreateDate, 
				LastModifyUser, 
				LastModifyUserNm, 
				LastModifyDate)
				values(
				@OrderNo,
				@VanProdLine,
				@CabType,
				@CabItem,
				@CabItemDesc,
				0,
				@CreateUserId,
				@CreateUserNm,
				@DateTimeNow,
				@CreateUserId,
				@CreateUserNm,
				@DateTimeNow
				)
			end
			else if exists(select top 1 1 from CUST_CabOut WITH(NOLOCK) where OrderNo = @OrderNo and [Status] = 0)
			begin
				Update CUST_CabOut set CabType = @CabType, CabItem = @CabItem, CabItemDesc = @CabItemDesc where OrderNo = @OrderNo
			end
			--else
			--begin
			--	Update CUST_CabOut set CabType = @CabType, CabItem = @CabItem where OrderNo = @OrderNo
			--end
		end
		else
		begin
			delete from CUST_CabOut where OrderNo = @OrderNo
		end
		-----------------------------↑新增驾驶室出库-----------------------------
		
		drop table #tempRoutingDet
		drop table #tempOrderBom
	end try 
	begin catch
		set @ErrorMsg = Error_Message() 
		RAISERROR(@ErrorMsg, 16, 1) 
	end catch 
END 
