CREATE PROCEDURE [dbo].[USP_Busi_CreateVanProdOrder] 
	@BatchNo int,
	@VanProdLine varchar(50),
	@VanProdLineNm varchar(100),
	@CreateUserId int,
	@CreateUserNm varchar(50),
	@OrderNo varchar(50) output
AS 
BEGIN 
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
		
		select @SapProdLine = ZLINE, @SapOrderNo = AUFNR, @SapVAN = CHARG, @SapStartTime = GSTRS, 
		@SapProdItem = MATNR, @SapProdItemDesc = MAKTX, @SapProdItemUom = GMEIN, @SapSeq = Convert(bigint, CY_SEQNR)
		from SAP_ProdOrder where BatchNo = @BatchNo
		-----------------------------↑获取SAP生产单-----------------------------
		
		
		
		-----------------------------↓数据校验-----------------------------
		declare @IsProdLineActive bit  --生产线是否有效
		declare @Routing varchar(50)  --生产线工艺流程
		declare @VanProdLineRegion varchar(50)  --生产线区域
		declare @LocTo varchar(50)     --生产线原材料
		declare @LocFrom varchar(50)   --生产线成品
		declare @ProdLineType tinyint   --生产线类型
		declare @VirtualOpRef varchar(50)   --虚拟工位
		declare @TaktTime int           --节拍时间（秒）
		
		if ISNULL(@VanProdLine, '') = ''
		begin
			set @ErrorMsg = N'没有维护整车生产线' + @SapProdLine + N'映射关系的' + @VanProdLineNm + N'。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if not exists(select top 1 * from SCM_FlowMstr where Code = @VanProdLine)
		begin
			set @ErrorMsg = N'没有找到' + @VanProdLineNm + @VanProdLine + N'。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		--获取生产线信息
		select @VanProdLineRegion = PartyFrom, @LocTo = LocTo, @LocTo = LocTo, @Routing = Routing, 
		@IsProdLineActive = IsActive, @ProdLineType = ProdLineType, @TaktTime = TaktTime, @VirtualOpRef = VirtualOpRef
		from SCM_FlowMstr where Code = @VanProdLine
		
		if @IsProdLineActive = 0
		begin
			set @ErrorMsg = @VanProdLineNm + @VanProdLine + N'没有生效。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if @TaktTime is null
		begin
			set @ErrorMsg = @VanProdLineNm + @VanProdLine + N'没有设置节拍时间。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		else if @TaktTime <= 0
		begin
			set @ErrorMsg = @VanProdLineNm + @VanProdLine + N'的节拍时间不能小于等于0。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if ISNULL(@Routing, '') = ''
		begin
			set @ErrorMsg = N'没有设置' + @VanProdLineNm + @VanProdLine + N'的工艺流程。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if ISNULL(@VirtualOpRef, '') = ''
		begin
			set @ErrorMsg = N'没有设置' + @VanProdLineNm + @VanProdLine + N'的虚拟工位。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
	
		--if exists(select top 1 Op from PRD_RoutingDet where Routing = @Routing and WorkCenter is not null group by Op having COUNT(distinct WorkCenter) > 1)
		--begin
		--	declare @ErrorOp int
		--	select top 1 @ErrorOp = Op from PRD_RoutingDet where Routing = @Routing and WorkCenter is not null group by Op having COUNT(distinct WorkCenter) > 1
		--	set @ErrorMsg = N'工艺流程' + @Routing + N'工序' + convert(varchar, @ErrorOp) + N'的工作中心设置不一致。'
		--	RAISERROR(@ErrorMsg, 16, 1)
		--end
		
		if not exists(select * from PRD_ProdLineWorkCenter where Flow = @VanProdLine)
		begin
			set @ErrorMsg = N'没有找到' + @VanProdLineNm + @VanProdLine + N'的工作中心。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		-----------------------------↑数据校验-----------------------------
		
		
		
		
		-----------------------------↓生成工序临时表-----------------------------
		Create table #tempRoutingDet
		(
			LoopCount int,
			AssProdLine varchar(50),
			Routing varchar(50),
			Op int,
			OpRef varchar(50),
			Location varchar(50),
			WorkCenter varchar(50)
		)
		
		--插入主线工序
		declare @LoopCount int = 1
		insert into #tempRoutingDet(LoopCount, AssProdLine, Routing, Op, OpRef, Location, WorkCenter)
		select @LoopCount, @VanProdLine, Routing, Op, OpRef, Location, WorkCenter 
		from PRD_RoutingDet where Routing = @Routing
		
		--插入分装线工序
		while exists(select top 1 subPL.Code from #tempRoutingDet as tDet
					inner join SCM_FlowBinding as bind on tDet.AssProdLine = bind.MstrFlow
					inner join SCM_FlowMstr as subPL on bind.BindFlow = subPL.Code
					where tDet.LoopCount = @LoopCount)
		begin
			set @LoopCount = @LoopCount + 1
			
			insert into #tempRoutingDet(LoopCount, AssProdLine, Routing, Op, OpRef, Location, WorkCenter)
			select @LoopCount, subPL.Code, subPL.Routing, rDet.Op, rDet.OpRef, rDet.Location, rDet.WorkCenter 
			from #tempRoutingDet as tDet
			inner join SCM_FlowBinding as bind on tDet.AssProdLine = bind.MstrFlow
			inner join SCM_FlowMstr as subPL on bind.BindFlow = subPL.Code
			inner join PRD_RoutingDet as rDet on rDet.Routing = subPL.Routing
			where tDet.LoopCount = @LoopCount - 1
			group by subPL.Code, subPL.Routing, rDet.Op, rDet.OpRef, rDet.Location, rDet.WorkCenter
		end
		-----------------------------↑生成工序临时表-----------------------------
		
		
		
		
		-----------------------------↓生成生产单Bom临时表-----------------------------
		Create table #tempOrderBom
		(
			Item varchar(50),
			ItemDesc varchar(100),
			RefItemCode varchar(50),
			UOM varchar(5),
			ManufactureParty varchar(50),
			AssProdLine varchar(50),
			Op int,
			OpRef varchar(50),
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
			DISPO varchar(50)
		)
		
		insert into #tempOrderBom(Item, ItemDesc, RefItemCode, UOM, ManufactureParty,
		AssProdLine, Op, OpRef, OrderQty, Location,
		ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, AUFNR, ICHARG, BWART,
		IsScanHu, WorkCenter, DISPO)
		select bom.MATERIAL as Item, bom.MAKTX as ItemDesc, bom.BISMT as RefItemCode, bom.MEINS as UOM, case when bom.LIFNR <> '' then bom.LIFNR else null end as ManufactureParty,
		null as ProdLine, null as Op, case when ISNULL(GW, '') = '' then @VirtualOpRef when Len(GW) > 5 then SUBSTRING(GW, 1, 5) else GW end as OpRef, bom.MDMNG as OrderQty, null as Location,
		bom.RSNUM as ReserveNo, bom.RSPOS as ReserveLine, null as ZOPWZ, bom.ZOPID, bom.ZOPDS, bom.AUFNR, bom.ICHARG, bom.BWART,
		CASE WHEN trace.Item is not null THEN 1 ELSE 0 END as IsScanHu, pwc.WorkCenter, bom.DISPO
		from SAP_ProdBomDet as bom
		inner join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo 
		and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  --用工艺路线编号、顺序和操作活动关联
		inner join PRD_ProdLineWorkCenter as pwc on routing.ARBPL = pwc.WorkCenter
		left join CUST_ItemTrace as trace on bom.MATERIAL = trace.Item
		where bom.BatchNo = @BatchNo and pwc.Flow = @VanProdLine
		
		if not exists(select top 1 * from #tempOrderBom)
		begin
			return
		end
		
		--更新工序
		declare @MinOp int  --最小工序，从主线上取
		select @MinOp = MIN(Op) from PRD_RoutingDet where Routing = @Routing
		
		--更新安装生产线、工序、工位和物料消耗库位
		update bom set AssProdLine = IsNULL(det.AssProdLine, @SapProdLine),--工艺流程没有匹配的工位取整车生产线
		Op = IsNULL(det.Op, @MinOp),									--工艺流程没有匹配的工位取整车生产线的最小工序
		OpRef = ISNULL(det.OpRef, @VirtualOpRef),						--工艺流程没有匹配的工位取整车生产线的虚拟工位
		Location = det.Location
		from #tempOrderBom as bom
		left join #tempRoutingDet as det on bom.OpRef = det.OpRef
		
		--更新物料消耗库位，再从工作中心上找
		update bom set Location = wc.Location
		from #tempOrderBom as bom
		inner join MD_WorkCenter as wc on bom.WorkCenter = wc.Code 
		where bom.Location is null
		
		--更新物料消耗库位，最后取生产线上的原材料库位
		update #tempOrderBom set Location = @LocFrom where Location is null
		-----------------------------↑生成生产单Bom临时表-----------------------------
		
		
		
		
		-----------------------------↓新增生产单头-----------------------------
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
		ProdLineType,         --生产线类型
		PauseStatus           --暂停状态，0
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
		@ProdLineType,               --生产线类型
		0                            --暂停状态，0
		-----------------------------↑新增生产单头-----------------------------
		
		
		
		
		-----------------------------↓新增生产单顺序-----------------------------
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
		-----------------------------↑新增生产单顺序-----------------------------
		
		
		
		-----------------------------↓新增生产单明细-----------------------------
		declare @OrderDetId int
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
		-----------------------------↑新增生产单明细-----------------------------
		
		
		
		-----------------------------↓新增生产单Bom-----------------------------
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
		Op,							--工序
		OpRef,						--工位
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
		AssProdLine,				--零件安装的生产线/可能是分装线，没有工位或者工位不存在的去虚拟工位
		IsCreateOrder,				--是否已经创建拉料单
		DISPO						--MRP控制者
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
		Op,							--工序
		OpRef,						--工位
		OrderQty,					--Bom用量
		0,							--反冲合格数量
		0,							--反冲不合格数量
		0,							--反冲废品数量
		OrderQty,					--单位用量
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
		DISPO						--MRP控制者
		from #tempOrderBom
		-----------------------------↑新增生产单Bom-----------------------------
		
		
		
		-----------------------------↓新增生产单Op-----------------------------
		INSERT INTO ORD_OrderOp (
		OrderNo,					--生产单号
		OrderDetId,					--生产单明细标识
		Op,							--工序
		OpRef,                      --工位, 生成订单工序时不考虑工位
		LeadTime,					--前置期
		TimeUnit,					--前置期单位
		IsBackflush,				--是否回冲物料
		CreateUser,					--创建用户
		CreateUserNm,				--创建用户名称
		CreateDate,					--创建日期
		LastModifyUser,				--最后修改用户
		LastModifyUserNm,			--最后修改用户名称
		LastModifyDate,				--最后修改日期
		[Version],					--版本，1
		WorkCenter,					--工作中心
		IsReport,					--是否报工
		IsAutoReport				--是否自动报工
		)
		select
		@OrderNo,					--生产单号
		@OrderDetId,				--生产单明细标识
		Op,							--工序
		'',							--工位, 生成订单工序时不考虑工位
		0,							--前置期
		1,							--前置期单位
		0,							--是否回冲物料
		@CreateUserId,              --创建用户
		@CreateUserNm,              --创建用户名称
		@DateTimeNow,               --创建日期
		@CreateUserId,              --最后修改用户
		@CreateUserNm,              --最后修改用户名称
		@DateTimeNow,               --最后修改日期
		1,							--版本，1
		Max(WorkCenter),			--工作中心
		0,							--是否报工
		CASE WHEN SUM(CASE WHEN IsReport = 0 then 0 else 1 End) = 0 then 0 else 1 end				--是否自动报工
		from PRD_RoutingDet
		where Routing = @Routing
		group by Op
		-----------------------------↑新增生产单Op-----------------------------
		
		
		
		
		-----------------------------↓新增关键件扫描-----------------------------
		INSERT INTO ORD_OrderItemTrace(
		OrderNo,
		OrderBomId,
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
		Version)
		select
		OrderNo,
		Id,
		Item,
		ItemDesc,
		RefItemCode,
		Op,
		OpRef,
		OrderQty,
		0,
		@CreateUserId,
		@CreateUserNm,
		@DateTimeNow,
		@CreateUserId,
		@CreateUserNm,
		@DateTimeNow,
		1
		from ORD_OrderBomDet where OrderNo = @OrderNo and IsScanHu = 1
		-----------------------------↑新增生产单Op-----------------------------
		
		
		
		drop table #tempOrderBom
	end try 
	begin catch
		set @ErrorMsg = Error_Message() 
		RAISERROR(@ErrorMsg, 16, 1) 
	end catch 
END 
