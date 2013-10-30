SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_LE_GenJITOrder')
	DROP PROCEDURE USP_LE_GenJITOrder	
GO

CREATE PROCEDURE [dbo].[USP_LE_GenJITOrder]
(
	@Flow varchar(50),
	@BatchNo int,
	@LERunTime datetime,
	@CreateUserId int,
	@CreateUserNm varchar(100)
) --WITH ENCRYPTION
AS
BEGIN 
	set nocount on
	declare @DateTimeNow datetime = GetDate()
	declare @Msg nvarchar(Max)
	declare @trancount int
	
	create table #tempExtraDemandSourceOrderPlan
	(
		Id int,
	)
	
	create table #tempOrderDet
	(
		RowId int identity(1, 1) Primary Key,
		UUID varchar(50),
		FlowDetSnapshotId int,
		Flow varchar(50),
		FlowDetId int,
		Item varchar(50),
		Uom varchar(5),
		UC decimal(18, 8),
		MinUC decimal(18, 8),
		UCDesc varchar(50),
		Container varchar(50),
		ContainerDesc varchar(50),
		ManufactureParty varchar(50),
		LocFrom varchar(50),
		LocTo varchar(50),
		SafeStock decimal(18, 8),
		MaxStock decimal(18, 8),
		MinLotSize decimal(18, 8),
		RoundUpOpt tinyint,
		ReqQty decimal(18, 8),
		OrderQty decimal(18, 8),
		OrderNo varchar(50),
		OrderDetId int,
		OrderDetSeq int,
	)
	
	Create table #tempOrderDetId
	(
		RowId int,
		OrderDetId int,
		OrderDetSeq int
	)
	
	create table #tempCreatedEMOrderDet
	(
		Item varchar(50), 
		RefItemCode varchar(50), 
		ItemDesc1 varchar(100), 
		ManufactureParty varchar(50), 
		Location varchar(50), 
		OrderNo varchar(50), 
		ReqTime datetime, 
		OrderQty decimal(18, 8)
	)
	
	CREATE TABLE #temp_LOG_GenJITOrder(
		Flow varchar(50) NULL,
		OrderNo varchar(50) NULL,
		Lvl tinyint NULL,
		Msg varchar(500) NULL,
		CreateDate datetime NULL default(GETDATE()),
	)
	
	declare @FlowType tinyint = null
	declare @FlowPartyFrom varchar(50) = null
	declare @FlowPartyTo varchar(50) = null
	declare @FlowLocFrom varchar(50) = null
	declare @FlowLocTo varchar(50) = null
	declare @FlowDock varchar(50) = null
	declare @OrderNo varchar(50) = null
	declare @StartTime datetime = null
	declare @WindowTime datetime = null
	declare @EMWindowTime datetime = null
	declare @ReqTimeFrom datetime = null
	declare @ReqTimeTo datetime = null
	declare @OrderDetCount int = null
	declare @BeginOrderDetId bigint = null
	declare @EndOrderDetId bigint = null

	select @FlowType = [Type],
	@FlowPartyFrom = PartyFrom, @FlowPartyTo = PartyTo, @FlowLocFrom = LocFrom, @FlowLocTo = LocTo,
	@FlowDock = Dock, @StartTime = OrderTime, @WindowTime = WindowTime, @EMWindowTime = EMWindowTime,
	@ReqTimeFrom = ReqTimeFrom, @ReqTimeTo = ReqTimeTo
	from LE_FlowMstrSnapShot where Flow = @Flow
	
	set @trancount = @@trancount
	begin try
		if @trancount = 0
		begin
            begin tran
        end
        
        --缓存其它需求源中来源库位和目的库位相同的需求
        truncate table #tempExtraDemandSourceOrderPlan
        insert into #tempExtraDemandSourceOrderPlan(Id) select ip.Id 
		from LE_FlowDetSnapShot as det
		inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.Id = dmd.FlowDetSnapShotId
		inner join LE_OrderPlanSnapshot as ip on det.Item = ip.Item 
				and ((dmd.Location = ip.Location and det.LocTo = ip.RefLocation) or (det.LocTo = ip.Location and dmd.Location = ip.RefLocation))
		where det.Flow = @Flow and det.Strategy = 3

		
		-------------------↓计算紧急需求-----------------------
		--记录日志
		set @Msg = N'计算路线需求生成JIT紧急要货单开始，需求时间小于' + convert(varchar, @ReqTimeFrom, 120)
		insert into #temp_LOG_GenJITOrder(Flow, Lvl, Msg) values(@Flow, 0, @Msg)

		truncate table #tempOrderDet
		insert into #tempOrderDet(UUID, FlowDetSnapshotId, Flow, FlowDetId, Item, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, 
								ManufactureParty, LocFrom, LocTo, 
								ReqQty, SafeStock, MaxStock, MinLotSize, RoundUpOpt)
		select NEWID(), det.Id, det.Flow, det.FlowDetId, det.Item, det.Uom, det.UC, det.MinUC, det.UCDesc, det.Container, det.ContainerDesc,
		det.ManufactureParty, det.LocFrom, det.LocTo,
		--安全库存量 + 待发 - 库存 - 待收
		det.SafeStock + ISNULL(ip.IssQty, 0) - ISNULL(loc.Qty, 0) - ISNULL(op.RctQty, 0) as ReqQty,
		ISNULL(det.SafeStock, 0) as SafeStock, ISNULL(det.MaxStock, 0) as MaxStock, ISNULL(det.MinLotSize, 0) as MinLotSize, ISNULL(det.RoundUpOpt, 0) as RoundUpOpt
		from LE_FlowDetSnapShot as det
		left join LE_LocationDetSnapshot as loc on det.Item = loc.Item and det.LocTo = loc.Location
		left join (select ip.Item, ip.Location, SUM(ip.OrderQty - ip.FinishQty) as IssQty 
					from LE_FlowDetSnapShot as det
					inner join LE_OrderPlanSnapshot as ip on det.Item = ip.Item and det.LocTo = ip.Location
					where det.Flow = @Flow and det.Strategy = 3 and ip.IRType = 0 and ip.ReqTime < @ReqTimeFrom
					and not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = ip.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
					group by ip.Item, ip.Location
					) as ip on det.Item = ip.Item and det.LocTo = ip.Location  --待发
		left join (select op.Item, op.Location, SUM(op.OrderQty - op.FinishQty) as RctQty 
					from LE_FlowDetSnapShot as det
					inner join LE_OrderPlanSnapshot as op on det.Item = op.Item and det.LocTo = op.Location
					where det.Flow = @Flow and det.Strategy = 3 and op.IRType = 1 and op.ReqTime < @ReqTimeFrom
					and not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = op.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
					group by op.Item, op.Location
					) as op on det.Item = op.Item and det.LocTo = op.Location  --待收
		where det.Flow = @Flow and det.Strategy = 3

		truncate table #tempCreatedEMOrderDet
		if exists(select top 1 1 from #tempOrderDet)
		begin
			----更新路线明细字段
			--update tdet set Flow = det.Flow, FlowDetId = det.FlowDetId, Uom = det.Uom, UC = det.UC, ManufactureParty = det.ManufactureParty, LocFrom = det.LocFrom, 
			--SafeStock = ISNULL(det.SafeStock, 0), MaxStock = ISNULL(det.MaxStock, 0), MinLotSize = ISNULL(det.MinLotSize, 0), RoundUpOpt = ISNULL(det.RoundUpOpt, 0)
			--from #tempOrderDet as tdet inner join LE_FlowDetSnapShot as det
			--on tdet.Item = det.Item and tdet.LocTo = det.LocTo
			
			--其它需求源
			update det set ReqQty = det.ReqQty + dmd.ReqQty
			from #tempOrderDet as det
			inner join (select det.Item, det.LocTo, SUM(dmdDet.SafeStock + ISNULL(ip.IssQty, 0) - ISNULL(loc.Qty, 0) - ISNULL(op.RctQty, 0)) as ReqQty
						from LE_FlowDetSnapShot as det
						inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.Id = dmd.FlowDetSnapShotId
						inner join LE_FlowDetSnapShot as dmdDet on dmd.FlowDetSnapShotId = dmdDet.Id
						left join LE_LocationDetSnapshot as loc on det.Item = loc.Item and dmd.Location = loc.Location
						left join (select det.Item, det.LocTo as Location, SUM(ip.OrderQty - ip.FinishQty) as IssQty 
									from LE_FlowDetSnapShot as det
									inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.Id = dmd.FlowDetSnapShotId
									inner join LE_OrderPlanSnapshot as ip on det.Item = ip.Item and dmd.Location = ip.Location
									where det.Flow = @Flow and det.Strategy = 3 and ip.IRType = 0 and ip.ReqTime < @ReqTimeFrom
									and not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = ip.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
									group by det.Item, det.LocTo
									) as ip on det.Item = ip.Item and det.LocTo = ip.Location  --待发
						left join (select det.Item, det.LocTo as Location, SUM(op.OrderQty - op.FinishQty) as RctQty 
									from LE_FlowDetSnapShot as det
									inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.Id = dmd.FlowDetSnapShotId
									inner join LE_OrderPlanSnapshot as op on det.Item = op.Item and dmd.Location = op.Location
									where det.Flow = @Flow and det.Strategy = 3 and op.IRType = 1 and op.ReqTime < @ReqTimeFrom
									and not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = op.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
									group by det.Item, det.LocTo
									) as op on det.Item = op.Item and det.LocTo = op.Location  --待收	
						where det.Flow = @Flow and det.Strategy = 3
						group by det.Item, det.LocTo) as dmd on det.Item = dmd.Item and det.LocTo = dmd.LocTo
	
			--按包装圆整
			update #tempOrderDet set OrderQty = ReqQty where ReqQty > 0 and RoundUpOpt = 0  --不圆整
			update #tempOrderDet set OrderQty = ReqQty where ReqQty > 0 and UC <= 0  --不圆整
			update #tempOrderDet set OrderQty = ceiling(ReqQty / UC) * UC where ReqQty > 0 and RoundUpOpt = 1 and UC > 0  --向上圆整
			update #tempOrderDet set OrderQty = floor(ReqQty / UC) * UC where ReqQty > 0 and RoundUpOpt = 2 and UC > 0  --向下圆整
			
			if exists(select top 1 1 from #tempOrderDet where OrderQty >= MinLotSize and OrderQty > 0)
			begin
				--获取订单号
				exec USP_GetDocNo_ORD @Flow, 3, @FlowType, 0, 0, 1, @FlowPartyFrom, @FlowPartyTo, @FlowLocTo, @FlowLocFrom, @FlowDock, 0, @OrderNo output
				
				--记录订单号
				update #tempOrderDet set OrderNo = @OrderNo where OrderQty >= MinLotSize and OrderQty > 0
				
				--创建订单头
				if (@FlowType = 1)
				begin
					insert into ORD_OrderMstr_1 (
					OrderNo,              --订单号
					Flow,                 --路线
					OrderStrategy,        --策略，
					[Type],               --类型，1 采购
					SubType,              --子类型，0正常
					QualityType,          --质量状态，0良品
					StartTime,            --开始时间
					WindowTime,           --窗口时间
					PauseSeq,             --暂停工序，0
					IsQuick,              --是否快速，0
					[Priority],           --优先级，1
					[Status],             --状态，1释放
					PartyFrom,            --区域代码
					PartyFromNm,            --区域名称
					PartyTo,              --区域代码
					PartyToNm,            --区域名称
					--LocFrom,              --原材料库位
					--LocFromNm,            --原材料库位
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
					IsPrintOrder,         --打印要货单，0
					IsOrderPrinted,       --要货单已打印，0
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
					PauseStatus			  --暂停状态，0
					)
					select 
					@OrderNo,			  --订单号
					mstr.Code,                 --路线
					3,					  --策略
					@FlowType,			  --类型
					0,					  --子类型，0正常
					0,					  --质量状态，0良品
					@DateTimeNow,         --开始时间
					@EMWindowTime,        --窗口时间
					0,					  --暂停工序，0
					0,					  --是否快速，0
					1,					  --优先级，1
					1,					  --状态，1释放
					mstr.PartyFrom,            --区域代码
					pf.Name,            --区域名称
					mstr.PartyTo,              --区域代码
					pt.Name,            --区域名称
					--mstr.LocFrom,              --原材料库位
					--lf.Name,				--原材料库位
					mstr.LocTo,                --成品库位
					lt.Name,                --成品库位
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
					mstr.IsPrintOrder,         --打印要货单，0
					0,					  --要货单已打印，0
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
					0					  --暂停状态，0
					from SCM_FlowMstr as mstr
					inner join MD_Party as pf on mstr.PartyFrom = pf.Code
					inner join MD_Party as pt on mstr.PartyTo = pt.Code
					--left join MD_Location as lf on mstr.LocFrom = lf.Code						
					left join MD_Location as lt on mstr.LocTo = lt.Code						
					where mstr.Code = @Flow
				end
				else if @FlowType = 2
				begin 
					insert into ORD_OrderMstr_2 (
					OrderNo,              --订单号
					Flow,                 --路线
					OrderStrategy,        --策略，
					[Type],               --类型，1 采购
					SubType,              --子类型，0正常
					QualityType,          --质量状态，0良品
					StartTime,            --开始时间
					WindowTime,           --窗口时间
					PauseSeq,             --暂停工序，0
					IsQuick,              --是否快速，0
					[Priority],           --优先级，1
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
					BillAddr,			  --开票地址
					Dock,				  --道口
					IsAutoRelease,        --自动释放，0
					IsAutoStart,          --自动上线，0
					IsAutoShip,           --自动发货，0
					IsAutoReceive,        --自动收货，0
					IsAutoBill,           --自动账单，0
					IsManualCreateDet,    --手工创建明细，0
					IsListPrice,          --显示价格单，0
					IsPrintOrder,         --打印要货单，0
					IsOrderPrinted,       --要货单已打印，0
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
					PauseStatus			  --暂停状态，0
					)
					select
					@OrderNo,			  --订单号
					mstr.Code,                 --路线
					3,					  --策略
					@FlowType,			  --类型
					0,					  --子类型，0正常
					0,					  --质量状态，0良品
					@DateTimeNow,         --开始时间
					@EMWindowTime,        --窗口时间
					0,					  --暂停工序，0
					0,					  --是否快速，0
					1,					  --优先级，1
					1,					  --状态，1释放
					mstr.PartyFrom,            --区域代码
					pf.Name,            --区域代码
					mstr.PartyTo,              --区域代码
					pt.Name,            --区域代码
					mstr.LocFrom,              --原材料库位
					lf.Name,              --原材料库位
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
					mstr.IsPrintOrder,         --打印要货单，0
					0,					  --要货单已打印，0
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
					0					  --暂停状态，0
					from SCM_FlowMstr as mstr
					inner join MD_Party as pf on mstr.PartyFrom = pf.Code
					inner join MD_Party as pt on mstr.PartyTo = pt.Code
					left join MD_Location as lf on mstr.LocFrom = lf.Code						
					left join MD_Location as lt on mstr.LocTo = lt.Code						
					where mstr.Code = @Flow
				end
				else
				begin
					RAISERROR(N'路线类型不正确。', 16, 1)
				end
				
				--查找订单数量
				select @OrderDetCount = COUNT(*) from #tempOrderDet where OrderQty >= MinLotSize and OrderQty > 0
				--锁定OrderDetId字段标识字段
				exec USP_SYS_BatchGetNextId 'ORD_OrderDet', @OrderDetCount, @EndOrderDetId output
				--查找开始标识
				set @BeginOrderDetId = @EndOrderDetId - @OrderDetCount
				
				--缓存订单ID
				truncate table #tempOrderDetId
				insert into #tempOrderDetId(RowId, OrderDetId, OrderDetSeq)
				select RowId, ROW_NUMBER() over (order by RowId) + @BeginOrderDetId as OrderDetId, ROW_NUMBER() over (order by RowId) as OrderDetSeq
				from #tempOrderDet where OrderQty >= MinLotSize and OrderQty > 0
				
				--记录订单明细ID
				update det set OrderDetId = id.OrderDetId, OrderDetSeq = id.OrderDetSeq
				from #tempOrderDet as det inner join #tempOrderDetId as id
				on det.RowId = id.RowId
						
				--创建订单明细
				if @FlowType = 1
				begin
					insert into ORD_OrderDet_1 (
					Id,                         --生产单明细标识
					OrderNo,                    --生产单号
					OrderType,                  --类型，1采购
					OrderSubType,               --子类型，0正常
					Seq,						--行号，1
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
					IsChangeUC,					--是否修改单包装，0
					--LocFrom,					--出库库位
					--LocFromNm,				--出库库位名称
					LocTo,						--入库库位
					LocToNm,					--入库库位名称
					BillAddr,					--开票地址
					ExtraDmdSource,				--记录FlowDetId，如果是0代表系统自动创建的路线明细
					BillTerm					--结算方式
					)
					select 
					det.OrderDetId,				--生产单明细标识
					det.OrderNo,				--生产单号
					@FlowType,					--类型，1采购
					0,							--子类型，0正常
					det.OrderDetSeq,			--行号，
					0,							--计划协议类型，0
					det.Item,					--物料号
					i.RefCode,					--参考物料号
					i.Desc1,					--物料描述
					det.Uom,					--单位
					det.Uom,					--基本单位
					det.UC,						--包装，1
					det.MinUC,                  --最小包装，1
					det.UCDesc,					--包装描述
					det.Container,				--容器代码
					det.ContainerDesc,			--容器描述
					0,							--质量状态，0
					det.ManufactureParty,		--制造商
					det.ReqQty,					--需求数量，1
					det.OrderQty,				--订单数量，1
					0,							--发货数量，0
					0,							--收货数量，0
					0,							--次品数量，0
					0,							--废品数量，0
					0,							--拣货数量，0
					1,							--单位用量，1
					0,							--是否检验，0
					0,							--是否暂估价，0
					0,							--是否含税价，0
					0,							--是否扫描条码，0
					@CreateUserId,				--创建用户
					@CreateUserNm,				--创建用户名称
					@DateTimeNow,				--创建日期
					@CreateUserId,				--最后修改用户
					@CreateUserNm,				--最后修改用户名称
					@DateTimeNow,				--最后修改日期
					1,							--版本，1
					0,							--是否修改单包装，0
					--det.LocFrom,				--出库库位
					--lf.Name,					--出库库位名称
					ISNULL(det.LocTo, mstr.LocTo),	--入库库位
					ISNULL(lt.Name, mstr.LocToNm),--入库库位
					mstr.BillAddr,				--开票地址
					CONVERT(varchar, det.FlowDetId),--记录FlowDetId，如果是0代表系统自动创建的路线明细
					mstr.BillTerm				--结算方式
					from #tempOrderDet as det
					inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
					inner join MD_Item as i on det.Item = i.Code
					--left join MD_Location as lf on det.LocFrom = lf.Code
					left join MD_Location as lt on det.LocTo = lt.Code
					where det.OrderQty >= det.MinLotSize and det.OrderQty > 0
					
					--更新累计交货量
					update q set AccumulateQty = ISNULL(q.AccumulateQty, 0) + det.OrderQty, [Version] = q.[Version] + 1
					from SCM_Quota as q 
					inner join (select det.Item, mstr.PartyFrom, SUM(det.OrderQty) as OrderQty
								from #tempOrderDet as det 
								inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
								where OrderQty >= MinLotSize and OrderQty > 0
								group by det.Item, mstr.PartyFrom) as det on q.Item = det.Item and q.Supplier = det.PartyFrom
				end
				else
				begin					
					insert into ORD_OrderDet_2 (
					Id,                         --生产单明细标识
					OrderNo,                    --生产单号
					OrderType,                  --类型，1采购
					OrderSubType,               --子类型，0正常
					Seq,						--行号，1
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
					IsChangeUC,					--是否修改单包装，0
					LocFrom,					--出库库位
					LocFromNm,					--出库库位名称
					LocTo,						--入库库位
					LocToNm,					--入库库位名称
					--BillAddr,					--开票地址
					ExtraDmdSource				--记录FlowDetId，如果是0代表系统自动创建的路线明细
					)
					select 
					det.OrderDetId,				--生产单明细标识
					det.OrderNo,				--生产单号
					@FlowType,					--类型，1采购
					0,							--子类型，0正常
					det.OrderDetSeq,			--行号，
					0,							--计划协议类型，0
					det.Item,					--物料号
					i.RefCode,					--参考物料号
					i.Desc1,					--物料描述
					det.Uom,					--单位
					det.Uom,					--基本单位
					det.UC,						--包装，1
					det.MinUC,                  --最小包装，1
					det.UCDesc,					--包装描述
					det.Container,				--容器代码
					det.ContainerDesc,			--容器描述
					0,							--质量状态，0
					det.ManufactureParty,		--制造商
					det.ReqQty,					--需求数量，1
					det.OrderQty,				--订单数量，1
					0,							--发货数量，0
					0,							--收货数量，0
					0,							--次品数量，0
					0,							--废品数量，0
					0,							--拣货数量，0
					1,							--单位用量，1
					0,							--是否检验，0
					0,							--是否暂估价，0
					0,							--是否含税价，0
					0,							--是否扫描条码，0
					@CreateUserId,				--创建用户
					@CreateUserNm,				--创建用户名称
					@DateTimeNow,				--创建日期
					@CreateUserId,				--最后修改用户
					@CreateUserNm,				--最后修改用户名称
					@DateTimeNow,				--最后修改日期
					1,							--版本，1
					0,							--是否修改单包装，0
					ISNULL(det.LocFrom, mstr.LocFrom),--出库库位
					ISNULL(lf.Name, mstr.LocFromNm),--出库库位
					ISNULL(det.LocTo, mstr.LocTo),	--入库库位
					ISNULL(lt.Name, mstr.LocToNm),--入库库位
					--mstr.BillAddr,				--开票地址
					CONVERT(varchar, det.FlowDetId)	--记录FlowDetId，如果是0代表系统自动创建的路线明细
					from #tempOrderDet as det
					inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
					inner join MD_Item as i on det.Item = i.Code
					left join MD_Location as lf on det.LocFrom = lf.Code
					left join MD_Location as lt on det.LocTo = lt.Code
					where det.OrderQty >= det.MinLotSize and det.OrderQty > 0
				end
							
				--记录日志
				set @Msg = N'创建JIT紧急要货单' + @OrderNo
				insert into #temp_LOG_GenJITOrder(Flow, OrderNo, Lvl, Msg) values(@Flow, @OrderNo, 0, @Msg)
				
				--缓存已经创建的紧急订单明细
				insert into #tempCreatedEMOrderDet(Item, RefItemCode, ItemDesc1, ManufactureParty, Location, OrderNo, ReqTime, OrderQty)
				select det.Item, i.RefCode, i.Desc1, det.ManufactureParty, det.LocTo, det.OrderNo, @EMWindowTime, det.OrderQty
				from #tempOrderDet as det
				inner join MD_Item as i on det.Item = i.Code
				where det.OrderQty >= det.MinLotSize and det.OrderQty > 0
				
				if @FlowType = 1
				begin  --记录创建计划协议的订单明细
					INSERT INTO SAP_CRSL(EINDT, FRBNR, LIFNR, MATNR, MENGE, SGTXT, WERKS, OrderDetId, [Status], ErrorCount, CreateDate, LastModifyDate)
					select CONVERT(varchar(8), @EMWindowTime, 112), det.OrderNo, mstr.PartyFrom, det.Item, det.OrderQty, det.OrderDetSeq, r.Plant, det.OrderDetId, 0, 0, @DateTimeNow, @DateTimeNow
					from #tempOrderDet as det
					inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
					inner join MD_Region as r on mstr.PartyTo = r.Code
					where det.OrderQty >= det.MinLotSize and det.OrderQty > 0
				end
				else
				begin
					if exists(select top 1 1 from SCM_FlowMstr where Code = @Flow and PartyFrom in ('LOC','SQC'))
					begin
						--记录FIS_CreateProcurementOrderDAT
						INSERT INTO FIS_CreateProcurementOrderDAT(OrderNo, Van, OrderStrategy, StartTime, WindowTime, Priority, 
						Sequence, PartyFrom, PartyTo, Dock, CreateDate, Flow, LineSeq, Item, ManufactureParty, 
						LocationTo, Bin, OrderedQty, IsShipExceed, FileName, IsCreateDat)
						select det.OrderNo, '', mstr.OrderStrategy, mstr.StartTime, mstr.WindowTime, mstr.Priority,
						'', ISNULL(det.LocFrom, mstr.LocFrom), ISNULL(det.LocTo, mstr.LocTo), mstr.Dock, mstr.CreateDate, mstr.Flow, det.OrderDetId, det.Item, det.ManufactureParty, 
						'', '', det.OrderQty, mstr.IsShipExceed, '', 0
						from #tempOrderDet as det
						inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
						where det.OrderQty >= det.MinLotSize and det.OrderQty > 0
					end
					/**else if exists(select top 1 1 from SCM_FlowMstr where Code = @Flow and PartyFrom in ('SQC'))
					begin
						--记录FIS_CreateOrderDAT
						INSERT INTO FIS_CreateOrderDAT(OrderNo, MATNR, LIFNR, ENMNG, CHARG, COLOR, TIME_STAMP, CY_SEQNR,
						TIME_STAMP1, AUFNR, LGORT, UMLGO, LGPBE, REQ_TIME_STAMP, FLG_SORT, PLNBEZ, KTEXT, ZPLISTNO, 
						ErrorCount, IsCreateDat, CreateUserNm)
						select det.OrderNo, det.Item, det.ManufactureParty, det.OrderQty, '', '', REPLACE(REPLACE(REPLACE(CONVERT(varchar(50),mstr.CreateDate,120),':',''),'-',''),' ',''), '',
						null, '', ISNULL(det.LocFrom, mstr.LocFrom), ISNULL(det.LocTo, mstr.LocTo), '', REPLACE(REPLACE(REPLACE(CONVERT(varchar(50),mstr.WindowTime,120),':',''),'-',''),' ',''), 'N', '', '', det.OrderDetId,
						0, 0, @CreateUserNm
						from #tempOrderDet as det
						inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
						where det.OrderQty >= det.MinLotSize and det.OrderQty > 0
					end**/
				end	
			end
			
			--Log: JIT订单计算明细
			insert into LOG_OrderTrace(UUID, Flow, OrderNo, OrderDetSeq, OrderDetId, [Priority], StartTime, WindowTime, EMWindowTime, ReqTimeFrom, ReqTimeTo, 
				Item, RefItemCode, ItemDesc, Uom, UC, ManufactureParty, LocFrom, LocTo, SafeStock, MaxStock, MinLotSize, RoundUpOpt, ReqQty, OrderQty, BatchNo) 
			select UUID, det.Flow, det.OrderNo, det.OrderDetSeq, det.OrderDetId, 1, @StartTime, @WindowTime, @EMWindowTime, @ReqTimeFrom, @ReqTimeTo, 
				det.Item, i.RefCode, i.Desc1, det.Uom, det.UC, det.ManufactureParty, det.LocFrom, det.LocTo, det.SafeStock, det.MaxStock, det.MinLotSize, det.RoundUpOpt, det.ReqQty, det.OrderQty, @BatchNo
			from #tempOrderDet as det inner join MD_Item as i on det.Item = i.Code
			--where det.OrderQty >= det.MinLotSize and det.OrderQty > 0

			insert into LOG_OrderTraceDet(UUID, [Type], Item, RefItemCode, ItemDesc, ManufactureParty, Location, OrderNo, ReqTime, OrderQty, FinishQty, BatchNo)
			--Log: JIT订单计算明细和库存关系
			select det.UUID, 'LOC', loc.Item, i.RefCode, i.Desc1, null, loc.Location, null, null, loc.Qty, 0, @BatchNo
			from #tempOrderDet as det
			inner join LE_LocationDetSnapshot as loc on det.Item = loc.Item and det.LocTo = loc.Location
			inner join MD_Item as i on loc.Item = i.Code
			union all
			--Log: JIT订单计算明细和待发关系
			select det.UUID, 'ISS', ip.Item, i.RefCode, i.Desc1, ip.ManufactureParty, ip.Location, ip.OrderNo, ip.ReqTime, -ip.OrderQty, -ip.FinishQty, @BatchNo
			from #tempOrderDet as det
			inner join LE_OrderPlanSnapshot as ip on det.Item = ip.Item and det.LocTo = ip.Location and ip.IRType = 0 and ip.ReqTime < @ReqTimeFrom  --待发
			inner join MD_Item as i on ip.Item = i.Code
			where not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = ip.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
			union all
			--Log: JIT订单计算明细和待收关系
			select det.UUID, 'RCT', op.Item, i.RefCode, i.Desc1, op.ManufactureParty, op.Location, op.OrderNo, op.ReqTime, op.OrderQty, op.FinishQty, @BatchNo
			from #tempOrderDet as det
			inner join LE_OrderPlanSnapshot as op on det.Item = op.Item and det.LocTo = op.Location and op.IRType = 1 and op.ReqTime < @ReqTimeFrom  --待收
			inner join MD_Item as i on op.Item = i.Code
			where not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = op.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
			union all
			--Log: JIT订单计算明细和库存关系（其它需求源）
			select det.UUID, 'LOC', loc.Item, i.RefCode, i.Desc1, null, loc.Location, null, null, loc.Qty, 0, @BatchNo
			from #tempOrderDet as det
			inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.FlowDetSnapshotId = dmd.FlowDetSnapshotId
			inner join LE_LocationDetSnapshot as loc on det.Item = loc.Item and dmd.Location = loc.Location	
			inner join MD_Item as i on loc.Item = i.Code
			union all
			--Log: JIT订单计算明细和待发关系
			select det.UUID, 'ISS', ip.Item, i.RefCode, i.Desc1, ip.ManufactureParty, ip.Location, ip.OrderNo, ip.ReqTime, -ip.OrderQty, -ip.FinishQty, @BatchNo
			from #tempOrderDet as det
			inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.FlowDetSnapshotId = dmd.FlowDetSnapshotId
			inner join LE_OrderPlanSnapshot as ip on det.Item = ip.Item and dmd.Location = ip.Location and ip.IRType = 0 and ip.ReqTime < @ReqTimeFrom  --待发
			inner join MD_Item as i on ip.Item = i.Code
			where not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = ip.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
			union all
			--Log: JIT订单计算明细和待收关系
			select det.UUID, 'RCT', op.Item, i.RefCode, i.Desc1, op.ManufactureParty, op.Location, op.OrderNo, op.ReqTime, op.OrderQty, op.FinishQty, @BatchNo
			from #tempOrderDet as det
			inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.FlowDetSnapshotId = dmd.FlowDetSnapshotId
			inner join LE_OrderPlanSnapshot as op on det.Item = op.Item and dmd.Location = op.Location and op.IRType = 1 and op.ReqTime < @ReqTimeFrom  --待收
			inner join MD_Item as i on op.Item = i.Code
			where not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = op.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
		end
		
		--记录日志
		set @Msg = N'计算路线需求生成JIT紧急要货单结束'
		insert into #temp_LOG_GenJITOrder(Flow, Lvl, Msg) values(@Flow, 0, @Msg)
		-------------------↑计算紧急需求-----------------------
		
		-------------------↓计算正常需求-----------------------
		if (@StartTime <= @LERunTime)
		begin
			--记录日志
			set @Msg = N'计算路线需求生成JIT要货单开始，发单时间' + convert(varchar, @StartTime, 120) + N'，需求时间范围' + convert(varchar, @ReqTimeFrom, 120) + ' ~ ' + convert(varchar, @ReqTimeTo, 120)
			insert into #temp_LOG_GenJITOrder(Flow, Lvl, Msg) values(@Flow, 0, @Msg)

			truncate table #tempOrderDet
			insert into #tempOrderDet(UUID, FlowDetSnapshotId, Flow, FlowDetId, Item, Uom, UC, MinUC, UCDesc, Container, ContainerDesc,
									ManufactureParty, LocFrom, LocTo, 
									ReqQty, SafeStock, MaxStock, MinLotSize, RoundUpOpt)
			select NEWID(), det.Id, det.Flow, det.FlowDetId, det.Item, det.Uom, det.UC, det.MinUC, det.UCDesc, det.Container, det.ContainerDesc, 
			det.ManufactureParty, det.LocFrom, det.LocTo,
			--安全库存量 + 待发 - 库存 - 待收
			det.SafeStock + ISNULL(ip.IssQty, 0) - ISNULL(loc.Qty, 0) - ISNULL(op.RctQty, 0) as ReqQty,
			ISNULL(det.SafeStock, 0) as SafeStock, ISNULL(det.MaxStock, 0) as MaxStock, ISNULL(det.MinLotSize, 0) as MinLotSize, ISNULL(det.RoundUpOpt, 0) as RoundUpOpt
			from LE_FlowDetSnapShot as det
			left join LE_LocationDetSnapshot as loc on det.Item = loc.Item and det.LocTo = loc.Location
			left join (select ip.Item, ip.Location, SUM(ip.OrderQty - ip.FinishQty) as IssQty 
						from LE_FlowDetSnapShot as det
						inner join LE_OrderPlanSnapshot as ip on det.Item = ip.Item and det.LocTo = ip.Location
						where det.Flow = @Flow and det.Strategy = 3 and ip.IRType = 0 and ip.ReqTime < @ReqTimeTo
						and not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = ip.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
						group by ip.Item, ip.Location
						) as ip on det.Item = ip.Item and det.LocTo = ip.Location  --待发
			left join (select op.Item, op.Location, SUM(op.OrderQty - op.FinishQty) as RctQty 
						from LE_FlowDetSnapShot as det
						inner join LE_OrderPlanSnapshot as op on det.Item = op.Item and det.LocTo = op.Location
						where det.Flow = @Flow and det.Strategy = 3 and op.IRType = 1 and op.ReqTime < @ReqTimeTo
						and not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = op.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
						group by op.Item, op.Location
						) as op on det.Item = op.Item and det.LocTo = op.Location  --待收
			where det.Flow = @Flow and det.Strategy = 3
			
			if exists(select top 1 1 from #tempOrderDet)
			begin
				----更新路线明细字段
				--update tdet set Flow = det.Flow, FlowDetId = det.FlowDetId, Uom = det.Uom, UC = det.UC, ManufactureParty = det.ManufactureParty, LocFrom = det.LocFrom, 
				--SafeStock = det.SafeStock, MaxStock = det.MaxStock, MinLotSize = det.MinLotSize, RoundUpOpt = det.RoundUpOpt
				--from #tempOrderDet as tdet inner join LE_FlowDetSnapShot as det
				--on tdet.Item = det.Item and tdet.LocTo = det.LocTo
				
				--减去紧急订单的待收
				update tdet set ReqQty = tdet.ReqQty - det.OrderQty
				from #tempOrderDet as tdet inner join #tempCreatedEMOrderDet as det
				on tdet.Item = det.Item and tdet.LocTo = det.Location
				
				--其它需求源
				update det set ReqQty = det.ReqQty + dmd.ReqQty
				from #tempOrderDet as det
				inner join (select det.Item, det.LocTo, SUM(dmdDet.SafeStock + ISNULL(ip.IssQty, 0) - ISNULL(loc.Qty, 0) - ISNULL(op.RctQty, 0)) as ReqQty
							from LE_FlowDetSnapShot as det
							inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.Id = dmd.FlowDetSnapshotId
							inner join LE_FlowDetSnapShot as dmdDet on dmd.FlowDetSnapshotId = dmdDet.Id
							left join LE_LocationDetSnapshot as loc on det.Item = loc.Item and dmd.Location = loc.Location
							left join (select det.Item, det.LocTo as Location, SUM(ip.OrderQty - ip.FinishQty) as IssQty 
										from LE_FlowDetSnapShot as det
										inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.Id = dmd.FlowDetSnapshotId
										inner join LE_OrderPlanSnapshot as ip on det.Item = ip.Item and dmd.Location = ip.Location
										where det.Flow = @Flow and det.Strategy = 3 and ip.IRType = 0 and ip.ReqTime < @ReqTimeTo
										and not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = ip.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
										group by det.Item, det.LocTo
										) as ip on det.Item = ip.Item and det.LocTo = ip.Location  --待发
							left join (select det.Item, det.LocTo as Location, SUM(op.OrderQty - op.FinishQty) as RctQty 
										from LE_FlowDetSnapShot as det
										inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.Id = dmd.FlowDetSnapshotId
										inner join LE_OrderPlanSnapshot as op on det.Item = op.Item and dmd.Location = op.Location
										where det.Flow = @Flow and det.Strategy = 3 and op.IRType = 1 and op.ReqTime < @ReqTimeTo
										and not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = op.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
										group by det.Item, det.LocTo
										) as op on det.Item = op.Item and det.LocTo = op.Location  --待收	
							where det.Flow = @Flow and det.Strategy = 3
							group by det.Item, det.LocTo) as dmd on det.Item = dmd.Item and det.LocTo = dmd.LocTo

				--按包装圆整
				update #tempOrderDet set OrderQty = ReqQty where ReqQty > 0 and RoundUpOpt = 0  --不圆整
				update #tempOrderDet set OrderQty = ReqQty where ReqQty > 0 and UC <= 0  --不圆整
				update #tempOrderDet set OrderQty = ceiling(ReqQty / UC) * UC where ReqQty > 0 and RoundUpOpt = 1 and UC > 0  --向上圆整
				update #tempOrderDet set OrderQty = floor(ReqQty / UC) * UC where ReqQty > 0 and RoundUpOpt = 2 and UC > 0  --向下圆整
				
				if exists(select top 1 1 from #tempOrderDet where OrderQty >= MinLotSize and OrderQty > 0)
				begin
					--获取订单号
					exec USP_GetDocNo_ORD @Flow, 3, @FlowType, 0, 0, 0, @FlowPartyFrom, @FlowPartyTo, @FlowLocTo, @FlowLocFrom, @FlowDock, 0, @OrderNo output
					
					--记录订单号
					update #tempOrderDet set OrderNo = @OrderNo where OrderQty >= MinLotSize and OrderQty > 0
					
					--创建订单头
					if (@FlowType = 1)
					begin
						insert into ORD_OrderMstr_1 (
						OrderNo,              --订单号
						Flow,                 --路线
						OrderStrategy,        --策略，
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
						PartyFromNm,		  --区域名称
						PartyTo,              --区域代码
						PartyToNm,            --区域名称
						--LocFrom,              --原材料库位
						--LocFromNm,            --原材料库位
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
						IsPrintOrder,         --打印要货单，0
						IsOrderPrinted,       --要货单已打印，0
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
						PauseStatus			  --暂停状态，0
						)
						select 
						@OrderNo,			  --订单号
						mstr.Code,                 --路线
						3,					  --策略
						@FlowType,			  --类型
						0,					  --子类型，0正常
						0,					  --质量状态，0良品
						@StartTime,			  --开始时间
						@WindowTime,          --窗口时间
						0,					  --暂停工序，0
						0,					  --是否快速，0
						0,					  --优先级，0
						1,					  --状态，1释放
						mstr.PartyFrom,            --区域代码
						pf.Name,            --区域名称
						mstr.PartyTo,              --区域代码
						pt.Name,            --区域名称
						--mstr.LocFrom,              --原材料库位
						--lf.Name,				--原材料库位
						mstr.LocTo,                --成品库位
						lt.Name,                --成品库位
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
						mstr.IsPrintOrder,         --打印要货单，0
						0,					  --要货单已打印，0
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
						0					  --暂停状态，0
						from SCM_FlowMstr as mstr
						inner join MD_Party as pf on mstr.PartyFrom = pf.Code
						inner join MD_Party as pt on mstr.PartyTo = pt.Code
						--left join MD_Location as lf on mstr.LocFrom = lf.Code						
						left join MD_Location as lt on mstr.LocTo = lt.Code						
						where mstr.Code = @Flow
					end
					else if @FlowType = 2
					begin 
						insert into ORD_OrderMstr_2 (
						OrderNo,              --订单号
						Flow,                 --路线
						OrderStrategy,        --策略，
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
						PartyFromNm,		  --区域名称
						PartyTo,              --区域代码
						PartyToNm,            --区域名称
						LocFrom,              --原材料库位
						LocFromNm,            --原材料库位
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
						IsPrintOrder,         --打印要货单，0
						IsOrderPrinted,       --要货单已打印，0
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
						PauseStatus			  --暂停状态，0
						)
						select
						@OrderNo,			  --订单号
						mstr.Code,                 --路线
						3,					  --策略
						@FlowType,			  --类型
						0,					  --子类型，0正常
						0,					  --质量状态，0良品
						@StartTime,			  --开始时间
						@WindowTime,          --窗口时间
						0,					  --暂停工序，0
						0,					  --是否快速，0
						0,					  --优先级，0
						1,					  --状态，1释放
						mstr.PartyFrom,            --区域代码
						pf.Name,            --区域名称
						mstr.PartyTo,              --区域代码
						pt.Name,            --区域名称
						mstr.LocFrom,              --原材料库位
						lf.Name,				--原材料库位
						mstr.LocTo,                --成品库位
						lt.Name,                --成品库位
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
						mstr.IsPrintOrder,         --打印要货单，0
						0,					  --要货单已打印，0
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
						0					  --暂停状态，0
						from SCM_FlowMstr as mstr
						inner join MD_Party as pf on mstr.PartyFrom = pf.Code
						inner join MD_Party as pt on mstr.PartyTo = pt.Code
						left join MD_Location as lf on mstr.LocFrom = lf.Code						
						left join MD_Location as lt on mstr.LocTo = lt.Code						
						where mstr.Code = @Flow
					end
					else
					begin
						RAISERROR(N'路线类型不正确。', 16, 1)
					end
					
					--查找订单数量
					select @OrderDetCount = COUNT(*) from #tempOrderDet where OrderQty >= MinLotSize and OrderQty > 0
					--锁定OrderDetId字段标识字段
					exec USP_SYS_BatchGetNextId 'ORD_OrderDet', @OrderDetCount, @EndOrderDetId output
					--查找开始标识
					set @BeginOrderDetId = @EndOrderDetId - @OrderDetCount
					
					--缓存订单ID
					truncate table #tempOrderDetId
					insert into #tempOrderDetId(RowId, OrderDetId, OrderDetSeq)
					select RowId, ROW_NUMBER() over (order by RowId) + @BeginOrderDetId as OrderDetId, ROW_NUMBER() over (order by RowId) as OrderDetSeq
					from #tempOrderDet where OrderQty >= MinLotSize and OrderQty > 0
					
					--记录订单明细ID
					update det set OrderDetId = id.OrderDetId, OrderDetSeq = id.OrderDetSeq
					from #tempOrderDet as det inner join #tempOrderDetId as id
					on det.RowId = id.RowId						
					
					--创建订单明细
					if @FlowType = 1
					begin
						insert into ORD_OrderDet_1 (
						Id,                         --生产单明细标识
						OrderNo,                    --生产单号
						OrderType,                  --类型，1采购
						OrderSubType,               --子类型，0正常
						Seq,						--行号，1
						ScheduleType,               --计划协议类型，0
						Item,                       --物料号
						RefItemCode,				--参考物料号
						ItemDesc,                   --物料描述
						Uom,                        --单位
						BaseUom,                    --基本单位
						UC,                         --包装，1
						MinUC,						--最小包装，1
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
						IsChangeUC,					--是否修改单包装，0
						--LocFrom,					--出库库位
						--LocFromNm,					--出库库位名称
						LocTo,						--入库库位
						LocToNm,					--入库库位名称
						BillAddr,					--开票地址
						ExtraDmdSource,				--记录FlowDetId，如果是0代表系统自动创建的路线明细
						BillTerm					--结算方式
						)
						select 
						det.OrderDetId,				--生产单明细标识
						det.OrderNo,				--生产单号
						@FlowType,					--类型，1采购
						0,							--子类型，0正常
						det.OrderDetSeq,			--行号，
						0,							--计划协议类型，0
						det.Item,					--物料号
						i.RefCode,					--参考物料号
						i.Desc1,					--物料描述
						det.Uom,					--单位
						det.Uom,					--基本单位
						det.UC,						--包装，1
						det.MinUC,                  --最小包装，1
						det.UCDesc,					--包装描述
						det.Container,				--容器代码
						det.ContainerDesc,			--容器描述
						0,							--质量状态，0
						det.ManufactureParty,		--制造商
						det.ReqQty,					--需求数量，1
						det.OrderQty,				--订单数量，1
						0,							--发货数量，0
						0,							--收货数量，0
						0,							--次品数量，0
						0,							--废品数量，0
						0,							--拣货数量，0
						1,							--单位用量，1
						0,							--是否检验，0
						0,							--是否暂估价，0
						0,							--是否含税价，0
						0,							--是否扫描条码，0
						@CreateUserId,				--创建用户
						@CreateUserNm,				--创建用户名称
						@DateTimeNow,				--创建日期
						@CreateUserId,				--最后修改用户
						@CreateUserNm,				--最后修改用户名称
						@DateTimeNow,				--最后修改日期
						1,							--版本，1
						0,							--是否修改单包装，0
						--det.LocFrom,				--出库库位
						--lf.Name,					--出库库位名称
						ISNULL(det.LocTo, mstr.LocTo),	--入库库位
						ISNULL(lt.Name, mstr.LocToNm),--入库库位
						mstr.BillAddr,				--开票地址
						CONVERT(varchar, det.FlowDetId),--记录FlowDetId，如果是0代表系统自动创建的路线明细
						mstr.BillTerm					--结算方式
						from #tempOrderDet as det
						inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
						inner join MD_Item as i on det.Item = i.Code
						--left join MD_Location as lf on det.LocFrom = lf.Code
						left join MD_Location as lt on det.LocTo = lt.Code
						where det.OrderQty >= det.MinLotSize and det.OrderQty > 0
						
						--更新累计交货量
						update q set AccumulateQty = ISNULL(q.AccumulateQty, 0) + det.OrderQty, [Version] = q.[Version] + 1
						from SCM_Quota as q 
						inner join (select det.Item, mstr.PartyFrom, SUM(det.OrderQty) as OrderQty
									from #tempOrderDet as det 
									inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
									where OrderQty >= MinLotSize and OrderQty > 0
									group by det.Item, mstr.PartyFrom) as det on q.Item = det.Item and q.Supplier = det.PartyFrom
					end
					else
					begin
						insert into ORD_OrderDet_2 (
						Id,                         --生产单明细标识
						OrderNo,                    --生产单号
						OrderType,                  --类型，1采购
						OrderSubType,               --子类型，0正常
						Seq,						--行号，1
						ScheduleType,               --计划协议类型，0
						Item,                       --物料号
						RefItemCode,				--参考物料号
						ItemDesc,                   --物料描述
						Uom,                        --单位
						BaseUom,                    --基本单位
						UC,                         --包装，1
						MinUC,						--最小包装，1
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
						IsChangeUC,					--是否修改单包装，0
						LocFrom,					--出库库位
						LocFromNm,					--出库库位名称
						LocTo,						--入库库位
						LocToNm,					--入库库位名称
						--BillAddr,					--开票地址
						ExtraDmdSource				--记录FlowDetId，如果是0代表系统自动创建的路线明细
						)
						select 
						det.OrderDetId,				--生产单明细标识
						det.OrderNo,				--生产单号
						@FlowType,					--类型，1采购
						0,							--子类型，0正常
						det.OrderDetSeq,			--行号，
						0,							--计划协议类型，0
						det.Item,					--物料号
						i.RefCode,					--参考物料号
						i.Desc1,					--物料描述
						det.Uom,					--单位
						det.Uom,					--基本单位
						det.UC,						--包装，1
						det.MinUC,                  --最小包装，1
						det.UCDesc,					--包装描述
						det.Container,				--容器代码
						det.ContainerDesc,			--容器描述
						0,							--质量状态，0
						det.ManufactureParty,		--制造商
						det.ReqQty,					--需求数量，1
						det.OrderQty,				--订单数量，1
						0,							--发货数量，0
						0,							--收货数量，0
						0,							--次品数量，0
						0,							--废品数量，0
						0,							--拣货数量，0
						1,							--单位用量，1
						0,							--是否检验，0
						0,							--是否暂估价，0
						0,							--是否含税价，0
						0,							--是否扫描条码，0
						@CreateUserId,				--创建用户
						@CreateUserNm,				--创建用户名称
						@DateTimeNow,				--创建日期
						@CreateUserId,				--最后修改用户
						@CreateUserNm,				--最后修改用户名称
						@DateTimeNow,				--最后修改日期
						1,							--版本，1
						0,							--是否修改单包装，0
						ISNULL(det.LocFrom, mstr.LocFrom),--出库库位
						ISNULL(lf.Name, mstr.LocFromNm),--出库库位
						ISNULL(det.LocTo, mstr.LocTo),	--入库库位
						ISNULL(lt.Name, mstr.LocToNm),--入库库位
						--mstr.BillAddr,				--开票地址
						CONVERT(varchar, det.FlowDetId)	--记录FlowDetId，如果是0代表系统自动创建的路线明细
						from #tempOrderDet as det
						inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
						inner join MD_Item as i on det.Item = i.Code
						left join MD_Location as lf on det.LocFrom = lf.Code
						left join MD_Location as lt on det.LocTo = lt.Code
						where det.OrderQty >= det.MinLotSize and det.OrderQty > 0
					end
						
					--记录日志
					set @Msg = N'创建JIT要货单' + @OrderNo
					insert into #temp_LOG_GenJITOrder(Flow, OrderNo, Lvl, Msg) values(@Flow, @OrderNo, 0, @Msg)
					
					if @FlowType = 1
					begin  --记录创建计划协议的订单明细
						INSERT INTO SAP_CRSL(EINDT, FRBNR, LIFNR, MATNR, MENGE, SGTXT, WERKS, OrderDetId, [Status], ErrorCount, CreateDate, LastModifyDate)
						select CONVERT(varchar(8), @WindowTime, 112), det.OrderNo, mstr.PartyFrom, det.Item, det.OrderQty, det.OrderDetSeq, r.Plant, det.OrderDetId, 0, 0, @DateTimeNow, @DateTimeNow
						from #tempOrderDet as det
						inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
						inner join MD_Region as r on mstr.PartyTo = r.Code
						where det.OrderQty >= det.MinLotSize and det.OrderQty > 0
					end
					else
					begin
						if exists(select top 1 1 from SCM_FlowMstr where Code = @Flow and PartyFrom in ('LOC','SQC'))
						begin
							--记录FIS_CreateProcurementOrderDAT
							INSERT INTO FIS_CreateProcurementOrderDAT(OrderNo, Van, OrderStrategy, StartTime, WindowTime, Priority, 
							Sequence, PartyFrom, PartyTo, Dock, CreateDate, Flow, LineSeq, Item, ManufactureParty, 
							LocationTo, Bin, OrderedQty, IsShipExceed, FileName, IsCreateDat)
							select det.OrderNo, '', mstr.OrderStrategy, mstr.StartTime, mstr.WindowTime, mstr.Priority,
							'', det.LocFrom, det.LocTo, mstr.Dock, mstr.CreateDate, mstr.Flow, det.OrderDetId, det.Item, det.ManufactureParty, 
							'', '', det.OrderQty, mstr.IsShipExceed, '', 0
							from #tempOrderDet as det
							inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
							where det.OrderQty >= det.MinLotSize and det.OrderQty > 0
						end
						/**else if exists(select top 1 1 from SCM_FlowMstr where Code = @Flow and PartyFrom in ('SQC'))
						begin
							--记录FIS_CreateOrderDAT
							INSERT INTO FIS_CreateOrderDAT(OrderNo, MATNR, LIFNR, ENMNG, CHARG, COLOR, TIME_STAMP, CY_SEQNR,
							TIME_STAMP1, AUFNR, LGORT, UMLGO, LGPBE, REQ_TIME_STAMP, FLG_SORT, PLNBEZ, KTEXT, ZPLISTNO, 
							ErrorCount, IsCreateDat, CreateUserNm)
							select det.OrderNo, det.Item, det.ManufactureParty, det.OrderQty, '', '', REPLACE(REPLACE(REPLACE(CONVERT(varchar(50),mstr.CreateDate,120),':',''),'-',''),' ',''), '',
							null, '', ISNULL(det.LocFrom, mstr.LocFrom), ISNULL(det.LocTo, mstr.LocTo), '', REPLACE(REPLACE(REPLACE(CONVERT(varchar(50),mstr.WindowTime,120),':',''),'-',''),' ',''), 'N', '', '', det.OrderDetId,
							0, 0, @CreateUserNm
							from #tempOrderDet as det
							inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
							where det.OrderQty >= det.MinLotSize and det.OrderQty > 0
						end**/
					end
				end	
				
				--Log: JIT订单计算明细
				insert into LOG_OrderTrace(UUID, Flow, OrderNo, OrderDetSeq, OrderDetId, [Priority], StartTime, WindowTime, EMWindowTime, ReqTimeFrom, ReqTimeTo, 
					Item, RefItemCode, ItemDesc, Uom, UC, ManufactureParty, LocFrom, LocTo, SafeStock, MaxStock, MinLotSize, RoundUpOpt, ReqQty, OrderQty, BatchNo) 
				select UUID, det.Flow, det.OrderNo, det.OrderDetSeq, det.OrderDetId, 0, @StartTime, @WindowTime, @EMWindowTime, @ReqTimeFrom, @ReqTimeTo, 
					det.Item, i.RefCode, i.Desc1, det.Uom, det.UC, det.ManufactureParty, det.LocFrom, det.LocTo, det.SafeStock, det.MaxStock, det.MinLotSize, det.RoundUpOpt, det.ReqQty, det.OrderQty, @BatchNo
				from #tempOrderDet as det inner join MD_Item as i on det.Item = i.Code
				--where det.OrderQty >= det.MinLotSize and OrderQty > 0

				insert into LOG_OrderTraceDet(UUID, [Type], Item, RefItemCode, ItemDesc, ManufactureParty, Location, OrderNo, ReqTime, OrderQty, FinishQty, BatchNo)
				--Log: JIT订单计算明细和库存关系
				select det.UUID, 'LOC', loc.Item, i.RefCode, i.Desc1, null, loc.Location, null, null, loc.Qty, 0, @BatchNo
				from #tempOrderDet as det
				inner join LE_LocationDetSnapshot as loc on det.Item = loc.Item and det.LocTo = loc.Location
				inner join MD_Item as i on loc.Item = i.Code
				union all
				--Log: JIT订单计算明细和待发关系
				select det.UUID, 'ISS', ip.Item, i.RefCode, i.Desc1, ip.ManufactureParty, ip.Location, ip.OrderNo, ip.ReqTime, -ip.OrderQty, -ip.FinishQty, @BatchNo
				from #tempOrderDet as det
				inner join LE_OrderPlanSnapshot as ip on det.Item = ip.Item and det.LocTo = ip.Location and ip.IRType = 0 and ip.ReqTime < @ReqTimeTo  --待发
				inner join MD_Item as i on ip.Item = i.Code
				where not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = ip.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
				union all
				--Log: JIT订单计算明细和待收关系
				select det.UUID, 'RCT', op.Item, i.RefCode, i.Desc1, op.ManufactureParty, op.Location, op.OrderNo, op.ReqTime, op.OrderQty, op.FinishQty, @BatchNo
				from #tempOrderDet as det
				inner join LE_OrderPlanSnapshot as op on det.Item = op.Item and det.LocTo = op.Location and op.IRType = 1 and op.ReqTime < @ReqTimeTo  --待收
				inner join MD_Item as i on op.Item = i.Code
				where not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = op.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
				union all
				--Log: JIT订单计算明细和库存关系（其它需求源）
				select det.UUID, 'LOC', loc.Item, i.RefCode, i.Desc1, null, loc.Location, null, null, loc.Qty, 0, @BatchNo
				from #tempOrderDet as det
				inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.FlowDetSnapshotId = dmd.FlowDetSnapshotId
				inner join LE_LocationDetSnapshot as loc on det.Item = loc.Item and dmd.Location = loc.Location	
				inner join MD_Item as i on loc.Item = i.Code
				union all
				--Log: JIT订单计算明细和待发关系
				select det.UUID, 'ISS', ip.Item, i.RefCode, i.Desc1, ip.ManufactureParty, ip.Location, ip.OrderNo, ip.ReqTime, -ip.OrderQty, -ip.FinishQty, @BatchNo
				from #tempOrderDet as det
				inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.FlowDetSnapshotId = dmd.FlowDetSnapshotId
				inner join LE_OrderPlanSnapshot as ip on det.Item = ip.Item and dmd.Location = ip.Location and ip.IRType = 0 and ip.ReqTime < @ReqTimeTo  --待发
				inner join MD_Item as i on ip.Item = i.Code
				where not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = ip.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
				union all
				--Log: JIT订单计算明细和待收关系
				select det.UUID, 'RCT', op.Item, i.RefCode, i.Desc1, op.ManufactureParty, op.Location, op.OrderNo, op.ReqTime, op.OrderQty, op.FinishQty, @BatchNo
				from #tempOrderDet as det
				inner join LE_FlowDetExtraDmdSourceSnapshot as dmd on det.FlowDetSnapshotId = dmd.FlowDetSnapshotId
				inner join LE_OrderPlanSnapshot as op on det.Item = op.Item and dmd.Location = op.Location and op.IRType = 1 and op.ReqTime < @ReqTimeTo  --待收
				inner join MD_Item as i on op.Item = i.Code	
				where not exists(select top 1 1 from #tempExtraDemandSourceOrderPlan as edop where edop.Id = op.Id)  --过滤掉其它需求源中来源库位和目的库位相同的需求
				union all
				--Log: JIT订单计算明细和之前待收JIT紧急订单关系
				select det.UUID, 'RCT', eDet.Item, i.RefCode, i.Desc1, eDet.ManufactureParty, eDet.Location, eDet.OrderNo, eDet.ReqTime, eDet.OrderQty, 0 as FinishQty, @BatchNo
				from #tempOrderDet as det
				inner join #tempCreatedEMOrderDet as eDet on det.Item = eDet.Item and det.LocTo = edet.Location
				inner join MD_Item as i on eDet.Item = i.Code
			end
			
			--记录日志
			set @Msg = N'计算路线需求生成JIT要货单结束'
			insert into #temp_LOG_GenJITOrder(Flow, Lvl, Msg) values(@Flow, 0, @Msg)
		end		
		-------------------↑计算正常需求-----------------------
		
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
      
		--记录日志
		set @Msg = N'计算路线需求生成要JIT货单出现异常，异常信息：' +  + Error_Message()
		insert into #temp_LOG_GenJITOrder(Flow, Lvl, Msg) values(@Flow, 2, @Msg)
	end catch
	
	insert into LOG_GenJITOrder(Flow, OrderNo, Lvl, Msg, CreateDate, BatchNo) 
	select Flow, OrderNo, Lvl, Msg, CreateDate, @BatchNo from #temp_LOG_GenJITOrder
	
	drop table #temp_LOG_GenJITOrder
	drop table #tempCreatedEMOrderDet
	drop table #tempOrderDetId
	drop table #tempOrderDet
END 