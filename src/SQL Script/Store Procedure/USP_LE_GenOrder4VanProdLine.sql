SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_LE_GenOrder4VanProdLine')
	DROP PROCEDURE USP_LE_GenOrder4VanProdLine
GO

CREATE PROCEDURE [dbo].[USP_LE_GenOrder4VanProdLine]
(
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
		
	create table #tempOrderBomDet
	(
		RowId int identity(1, 1) Primary Key,
		Flow varchar(50),
		FlowDetId int,
		Item varchar(50),
		Uom varchar(5),
		UC decimal(18, 8),
		MinUC decimal(18, 8),
		UCDesc varchar(50),
		Container varchar(50),
		ContainerDesc varchar(50),
		RoundUpOpt tinyint,
		ManufactureParty varchar(50),
		LocFrom varchar(50),
		LocTo varchar(50),
		OpRef varchar(50),
		RefOpRef varchar(256),
		NetOrderQty decimal(18, 8),
		OpRefQty decimal(18, 8),
		OrderQty decimal(18, 8),
		Balance decimal(18, 8),
		OrderNo varchar(50),
		OrderDetId int,
		OrderDetSeq int,
		IsUrgent bit,
		UUID varchar(50)
	)
	
	create table #tempOrderDet
	(
		RowId int identity(1, 1) Primary Key,
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
		ReqQty decimal(18, 8),
		OrderQty decimal(18, 8),
		OrderNo varchar(50),
		OrderDetId int,
		OrderDetSeq int,
		OpRef varchar(50),
		RefOpRef varchar(50)
	)
	
	Create table #tempOrderDetId
	(
		RowId int,
		OrderDetId int,
		OrderDetSeq int
	)
	
	CREATE TABLE #temp_LOG_GenOrder4VanProdLine(
		Flow varchar(50) NULL,
		OrderNo varchar(50) NULL,
		Lvl tinyint NULL,
		Msg varchar(500) NULL,
		CreateDate datetime NULL default(GETDATE()),
	)
	
	Create table #tempIgnoreWorkCenter(
		WorkCenter varchar(50) NULL
	)
	
	--记录日志
	set @Msg = N'生成整车线边物料要货单开始'
	insert into LOG_GenOrder4VanProdLine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	--过滤掉工作中心是鞍座和变速器的物料
	insert into #tempIgnoreWorkCenter(WorkCenter)
	select distinct WorkCenter from
		(select transPWC.WorkCenter from CUST_ProductLineMap as map
		inner join PRD_ProdLineWorkCenter as transPWC on map.TransFlow = transPWC.Flow
		union all
		select saddlePWC.WorkCenter from CUST_ProductLineMap as map
		inner join PRD_ProdLineWorkCenter as saddlePWC on map.SaddleFlow = saddlePWC.Flow
		) as A

	declare @FlowId int 
	declare @MaxFlowId int
	
	select @FlowId = MIN(Id), @MaxFlowId = MAX(Id) from LE_FlowMstrSnapShot
	
	while (@FlowId <= @MaxFlowId)
	begin
		truncate table #temp_LOG_GenOrder4VanProdLine
		
		declare @Flow varchar(50) = null
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

		select @Flow = Flow, @FlowType = [Type],
		@FlowPartyFrom = PartyFrom, @FlowPartyTo = PartyTo, @FlowLocFrom = LocFrom, @FlowLocTo = LocTo,
		@FlowDock = Dock, @StartTime = OrderTime, @WindowTime = WindowTime, @EMWindowTime = EMWindowTime,
		@ReqTimeFrom = ReqTimeFrom, @ReqTimeTo = ReqTimeTo
		from LE_FlowMstrSnapShot where Id = @FlowId
		
		if exists(select top 1 1 from LE_FlowDetSnapShot where Flow = @Flow and Strategy = 3)
		begin  --只计算JIT
			set @trancount = @@trancount
			begin try
				if @FlowType = 1
				begin
					insert into LOG_SnapshotFlowDet4LeanEngine(Lvl, ErrorId, Item, LocTo, Msg)
					select distinct 1, 26, bom.Item, bom.Location, N'没有找到零件' + bom.Item + N'指定供应商' + bom.ManufactureParty + N'的直送路线'
					from LE_OrderBomCPTimeSnapshot as bom
					inner join LE_FlowDetSnapShot as fdet on bom.Item = fdet.Item and bom.Location = fdet.LocTo
					where fdet.Flow = @Flow and fdet.Strategy = 3 and bom.IsCreateOrder = 0 and bom.ManufactureParty <> @FlowPartyFrom
				end
				
				if @trancount = 0
				begin
					begin tran
				end
				
				-------------------↓计算紧急需求-----------------------
				--记录日志
				set @Msg = N'计算路线紧急需求生成线边物料要货单开始，需求时间小于' + convert(varchar, @ReqTimeFrom, 120)
				insert into #temp_LOG_GenOrder4VanProdLine(Flow, Lvl, Msg) values(@Flow, 0, @Msg)
				
				--计算净用量
				truncate table #tempOrderBomDet
				insert into #tempOrderBomDet(Flow, FlowDetId, Item, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, RoundUpOpt, ManufactureParty, LocFrom, LocTo, OpRef, RefOpRef, NetOrderQty, OpRefQty, OrderQty, UUID)
				select req.Flow, req.FlowDetId, req.Item, req.Uom, req.UC, req.MinUC, req.UCDesc, req.Container, req.ContainerDesc, req.RoundUpOpt, req.ManufactureParty, req.LocFrom, req.LocTo, req.OpRef, req.RefOpRef, req.OrderQty, ISNULL(orb.Qty, 0), req.OrderQty - (CASE WHEN ISNULL(req.ManufactureParty, '') = '' THEN ISNULL(orb.Qty, 0) ELSE 0 END), NEWID()
				from (select fdet.Flow, fdet.FlowDetId, fdet.Item, fdet.Uom, fdet.UC, fdet.MinUC, fdet.UCDesc, fdet.Container, fdet.ContainerDesc, fdet.RoundUpOpt, bom.ManufactureParty, fdet.LocFrom, fdet.LocTo, bom.OpRef, bom.RefOpRef, SUM(bom.OrderQty) as OrderQty
						from LE_OrderBomCPTimeSnapshot as bom
						inner join LE_FlowDetSnapShot as fdet on bom.Item = fdet.Item and bom.Location = fdet.LocTo
						left join #tempIgnoreWorkCenter as wc on bom.WorkCenter = wc.WorkCenter
						where fdet.Flow = @Flow and fdet.Strategy = 3 and bom.CPTime < @ReqTimeFrom and bom.IsCreateOrder = 0 and wc.WorkCenter is null
						group by fdet.Flow, fdet.FlowDetId, fdet.Item, fdet.Uom, fdet.UC, fdet.MinUC, fdet.UCDesc, fdet.Container, fdet.ContainerDesc, fdet.RoundUpOpt, bom.ManufactureParty, fdet.LocFrom, fdet.LocTo, bom.OpRef, bom.RefOpRef) as req
				left join SCM_OpRefBalance as orb on req.Item = orb.Item and req.OpRef = orb.OpRef
				order by req.Item, req.OpRef
				
				--删除直送供应商和指定供应商不相同的
				if @FlowType = 1
				begin
					delete from #tempOrderBomDet where ManufactureParty is not null and ManufactureParty <> @FlowPartyFrom
				end
				
				if exists(select top 1 1 from #tempOrderBomDet)
				begin
					--如果同一个工位+物料有两条以上明细的话
					--只能保留一条的初始工位余量，另外的都要update成0
					--避免工位余量增多
					update bom set OpRefQty = 0
					from #tempOrderBomDet as bom inner join
					--根据零件和工位再按制造商分组排序（null和empty的制造商在前面），把分组号大于1的工位余量全部置为0
					(select ROW_NUMBER() over(Partition by Item, OpRef order by t.ManufactureParty) as PtRowNum, RowId from #tempOrderBomDet) as t 
					on bom.RowId = t.RowId
					where t.PtRowNum > 1
					
					--按包装圆整
					update #tempOrderBomDet set OrderQty = ceiling(OrderQty / UC) * UC, Balance = ceiling(OrderQty / UC) * UC - OrderQty where OrderQty >= 0 and RoundUpOpt = 1 and UC > 0 and ISNULL(ManufactureParty, '') = ''
					update #tempOrderBomDet set OrderQty = 0, Balance = 0 - OrderQty where OrderQty < 0 and ISNULL(ManufactureParty, '') = ''
					update #tempOrderBomDet set OrderQty = ceiling(OrderQty / UC) * UC, Balance = OpRefQty + ceiling(OrderQty / UC) * UC - OrderQty where OrderQty >= 0 and RoundUpOpt = 1 and UC > 0 and ISNULL(ManufactureParty, '') <> ''
					update #tempOrderBomDet set Balance = (case when OrderQty >= 0 then 0 else (OpRefQty - NetOrderQty) end) where Balance is null and ISNULL(ManufactureParty, '') = ''
					update #tempOrderBomDet set Balance = OpRefQty where Balance is null and ISNULL(ManufactureParty, '') <> ''
					
					--更新工位余量
					update orb set Qty = ISNULL(tod.Balance, 0), [Version] = [Version] + 1, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm
					from SCM_OpRefBalance as orb 
						inner join (select Item, OpRef, SUM(Balance) as Balance from #tempOrderBomDet group by Item, OpRef) as tod on orb.Item = tod.Item and orb.OpRef = tod.OpRef
										
					--更新工位余量日志
					insert into LOG_OpRefBalanceChange(Item, OpRef, Qty, [Status], [Version], CreateDate, CreateUserId, CreateUserNm)
					select orb.Item, orb.OpRef, ISNULL(tod.Balance, 0), 1, orb.[Version], @DateTimeNow, @CreateUserId, @CreateUserNm
					from SCM_OpRefBalance as orb 
						inner join (select Item, OpRef, SUM(Balance) as Balance from #tempOrderBomDet group by Item, OpRef) as tod on orb.Item = tod.Item and orb.OpRef = tod.OpRef
					
					--新增不存在的工位余量
					insert into SCM_OpRefBalance (Item, OpRef, Qty, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, [Version])
					select tod.Item, tod.OpRef, ISNULL(tod.Balance, 0), @CreateUserId, @CreateUserNm, @DateTimeNow, @CreateUserId, @CreateUserNm, @DateTimeNow, 1
					from (select Item, OpRef, SUM(Balance) as Balance from #tempOrderBomDet group by Item, OpRef) as tod 
					left join SCM_OpRefBalance as orb on orb.Item = tod.Item and orb.OpRef = tod.OpRef
					where orb.Id is null
					
					--新增工位余量日志
					insert into LOG_OpRefBalanceChange(Item, OpRef, Qty, [Status], [Version], CreateDate, CreateUserId, CreateUserNm)
					select tod.Item, tod.OpRef, ISNULL(tod.Balance, 0), 0, 1, @DateTimeNow, @CreateUserId, @CreateUserNm
					from (select Item, OpRef, SUM(Balance) as Balance from #tempOrderBomDet group by Item, OpRef) as tod 
					left join SCM_OpRefBalance as orb on orb.Item = tod.Item and orb.OpRef = tod.OpRef
					where orb.Id is null
					
					if exists(select top 1 1 from #tempOrderBomDet where OrderQty > 0)
					begin
						--汇总订单明细
						truncate table #tempOrderDet
						insert into #tempOrderDet(Flow, FlowDetId, Item, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, ManufactureParty, LocFrom, LocTo, OpRef, RefOpRef, ReqQty, OrderQty)
						select det.Flow, det.FlowDetId, det.Item, det.Uom, det.UC, det.MinUC, det.UCDesc, det.Container, det.ContainerDesc, det.ManufactureParty, det.LocFrom, det.LocTo, det.OpRef, det.RefOpRef, SUM(det.NetOrderQty), SUM(det.OrderQty)
						from #tempOrderBomDet as det
						group by det.Flow, det.FlowDetId, det.Item, det.Uom, det.UC, det.MinUC, det.UCDesc, det.Container, det.ContainerDesc, det.ManufactureParty, det.LocFrom, det.LocTo, det.OpRef, det.RefOpRef having SUM(det.OrderQty) > 0
						
						--获取订单号
						exec USP_GetDocNo_ORD @Flow, 3, @FlowType, 0, 0, 1, @FlowPartyFrom, @FlowPartyTo, @FlowLocTo, @FlowLocFrom, @FlowDock, 0, @OrderNo output
						--记录订单号
						update #tempOrderDet set OrderNo = @OrderNo
						--记录订单号
						update #tempOrderBomDet set OrderNo = @OrderNo where OrderQty > 0
						
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
							mstr.Code,            --路线
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
							mstr.PartyFrom,			--区域代码
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
							from SCM_FlowMstr  as mstr
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
						select @OrderDetCount = COUNT(*) from #tempOrderDet
						--锁定OrderDetId字段标识字段
						exec USP_SYS_BatchGetNextId 'ORD_OrderDet', @OrderDetCount, @EndOrderDetId output
						--查找开始标识
						set @BeginOrderDetId = @EndOrderDetId - @OrderDetCount
						
						--缓存订单ID
						truncate table #tempOrderDetId
						insert into #tempOrderDetId(RowId, OrderDetId, OrderDetSeq)
						select RowId, ROW_NUMBER() over (order by RowId) + @BeginOrderDetId as OrderDetId, ROW_NUMBER() over (order by RowId) as OrderDetSeq
						from #tempOrderDet
						
						--记录订单明细ID
						update det set OrderDetId = id.OrderDetId, OrderDetSeq = id.OrderDetSeq
						from #tempOrderDet as det inner join #tempOrderDetId as id
						on det.RowId = id.RowId
						
						--记录订单明细和BOM明细的关系
						update bom set OrderDetId = det.OrderDetId, OrderDetSeq = det.RowId
						from #tempOrderDet as det inner join #tempOrderBomDet as bom
						on det.Item = bom.Item and det.LocTo = bom.LocTo
						
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
							WMSSeq,						--JIT计算工位
							BinTo,						--配送工位
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
							det.OpRef,					--JIT计算工位
							ISNULL(det.RefOpRef, det.OpRef),	--配送工位
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
							
							--更新累计交货量
							update q set AccumulateQty = ISNULL(q.AccumulateQty, 0) + det.OrderQty, [Version] = q.[Version] + 1
							from SCM_Quota as q 
							inner join (select det.Item, mstr.PartyFrom, SUM(det.OrderQty) as OrderQty
										from #tempOrderDet as det 
										inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
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
							WMSSeq,						--JIT计算工位
							BinTo,						--配送工位
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
							det.OpRef,					--JIT计算工位
							ISNULL(det.RefOpRef, det.OpRef),--配送工位
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
						end
												
						--记录日志
						set @Msg = N'创建线边物料紧急要货单' + @OrderNo
						insert into #temp_LOG_GenOrder4VanProdLine(Flow, OrderNo, Lvl, Msg) values(@Flow, @OrderNo, 0, @Msg)
						
						if @FlowType = 1
						begin  --记录创建计划协议的订单明细
							INSERT INTO SAP_CRSL(EINDT, FRBNR, LIFNR, MATNR, MENGE, SGTXT, WERKS, OrderDetId, [Status], ErrorCount, CreateDate, LastModifyDate)
							select CONVERT(varchar(8), @EMWindowTime, 112), det.OrderNo, mstr.PartyFrom, det.Item, det.OrderQty, det.OrderDetSeq, r.Plant, det.OrderDetId, 0, 0, @DateTimeNow, @DateTimeNow
							from #tempOrderDet as det
							inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
							inner join MD_Region as r on mstr.PartyTo = r.Code
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
								ISNULL(det.RefOpRef, det.OpRef), '', det.OrderQty, mstr.IsShipExceed, '', 0
								from #tempOrderDet as det
								inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
							end
							/**else if exists(select top 1 1 from SCM_FlowMstr where Code = @Flow and PartyFrom in ('SQC'))
							begin
								--记录FIS_CreateOrderDAT
								INSERT INTO FIS_CreateOrderDAT(OrderNo, MATNR, LIFNR, ENMNG, CHARG, COLOR, TIME_STAMP, CY_SEQNR,
								TIME_STAMP1, AUFNR, LGORT, UMLGO, LGPBE, REQ_TIME_STAMP, FLG_SORT, PLNBEZ, KTEXT, ZPLISTNO, 
								ErrorCount, IsCreateDat, CreateUserNm)
								select det.OrderNo, det.Item, det.ManufactureParty, det.OrderQty, '', '', REPLACE(REPLACE(REPLACE(CONVERT(varchar(50),mstr.CreateDate,120),':',''),'-',''),' ',''), '',
								null, '', ISNULL(det.LocFrom, mstr.LocFrom), ISNULL(det.LocTo, mstr.LocTo), det.OpRef, REPLACE(REPLACE(REPLACE(CONVERT(varchar(50),mstr.WindowTime,120),':',''),'-',''),' ',''), 'N', '', '', det.OrderDetId,
								0, 0, @CreateUserNm
								from #tempOrderDet as det
								inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
							end**/
						end
					end
					
					--Log: JIT订单计算明细
					insert into LOG_VanOrderTrace(UUID, Flow, OrderNo, OrderDetSeq, OrderDetId, [Priority], StartTime, WindowTime, EMWindowTime, ReqTimeFrom, ReqTimeTo, 
						Item, RefItemCode, ItemDesc, Uom, UC, ManufactureParty, Location, OpRef, NetOrderQty, OrgOpRefQty, GrossOrderQty, OpRefQty, BatchNo) 
					select bom.UUID, bom.Flow, bom.OrderNo, bom.OrderDetSeq, bom.OrderDetId, 1, @StartTime, @WindowTime, @EMWindowTime, @ReqTimeFrom, @ReqTimeTo, 
						bom.Item, i.RefCode, i.Desc1, bom.Uom, bom.UC, bom.ManufactureParty, bom.LocTo, bom.OpRef, bom.NetOrderQty, bom.OpRefQty, bom.OrderQty, bom.Balance, @BatchNo
					from #tempOrderBomDet as bom inner join MD_Item as i on bom.Item = i.Code

					--Log: JIT订单计算明细和BOM的关系
					insert into LOG_VanOrderBomTrace(UUID, ProdLine, VanOrderNo, VanOrderBomDetId, Item, RefItemCode, ItemDesc, OpRef, LocFrom, LocTo, OrderQty, CPTime, BatchNo)
					select tod.UUID, bomCP.VanProdLine, bomCP.OrderNo, bomCP.BomId, bomCP.Item, i.RefCode, i.Desc1, bomCP.OpRef, fdet.LocFrom, fdet.LocTo, bomCP.OrderQty, bomCP.CPTime, @BatchNo 
					from LE_OrderBomCPTimeSnapshot as bomCP
					inner join LE_FlowDetSnapShot as fdet on bomCP.Item = fdet.Item and bomCP.Location = fdet.LocTo
					inner join #tempOrderBomDet as tod on tod.Item = bomCP.Item and tod.OpRef = bomCP.OpRef
					inner join MD_Item as i on tod.Item = i.Code
					where fdet.Flow = @Flow and fdet.Strategy = 3 and bomCP.CPTime < @ReqTimeFrom and bomCP.IsCreateOrder = 0
					
					--计算过的JIT需求打上标记
					update bom set IsCreateOrder = 1, [Version] = bom.[Version] + 1, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm
					from ORD_OrderBomDet as bom
					inner join LE_OrderBomCPTimeSnapshot as bomCP on bom.Id = bomCP.BomId
					inner join LE_FlowDetSnapShot as fdet on bomCP.Item = fdet.Item and bomCP.Location = fdet.LocTo
					where fdet.Flow = @Flow and fdet.Strategy = 3 and bomCP.CPTime < @ReqTimeFrom and bomCP.IsCreateOrder = 0
					
					update bomCP set IsCreateOrder = 1
					from LE_OrderBomCPTimeSnapshot as bomCP
					inner join LE_FlowDetSnapShot as fdet on bomCP.Item = fdet.Item and bomCP.Location = fdet.LocTo
					where fdet.Flow = @Flow and fdet.Strategy = 3 and bomCP.CPTime < @ReqTimeFrom and bomCP.IsCreateOrder = 0
				end
				
				set @Msg = N'计算路线紧急需求生成线边物料要货单结束'
				insert into #temp_LOG_GenOrder4VanProdLine(Flow, Lvl, Msg) values(@Flow, 0, @Msg)
				-------------------↑计算紧急需求-----------------------
				
				
				-------------------↓计算正常需求-----------------------
				if (@StartTime <= @LERunTime)
				begin
					--记录日志
					set @Msg = N'计算路线正常需求生成线边物料要货单开始，发单时间' + convert(varchar, @StartTime, 120) + N'需求时间范围' + convert(varchar, @ReqTimeFrom, 120) + ' ~ ' + convert(varchar, @ReqTimeTo, 120)
					insert into #temp_LOG_GenOrder4VanProdLine(Flow, Lvl, Msg) values(@Flow, 0, @Msg)
					
					--计算净用量
					truncate table #tempOrderBomDet
					insert into #tempOrderBomDet(Flow, FlowDetId, Item, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, RoundUpOpt, ManufactureParty, LocFrom, LocTo, OpRef, RefOpRef, NetOrderQty, OpRefQty, OrderQty, UUID)
					select req.Flow, req.FlowDetId, req.Item, req.Uom, req.UC, req.MinUC, req.UCDesc, req.Container, req.ContainerDesc, req.RoundUpOpt, req.ManufactureParty, req.LocFrom, req.LocTo, req.OpRef, req.RefOpRef, req.OrderQty, ISNULL(orb.Qty, 0), req.OrderQty - (CASE WHEN ISNULL(req.ManufactureParty, '') = '' THEN ISNULL(orb.Qty, 0) ELSE 0 END), NEWID()
					from (select fdet.Flow, fdet.FlowDetId, fdet.Item, fdet.Uom, fdet.UC, fdet.MinUC, fdet.UCDesc, fdet.Container, fdet.ContainerDesc, fdet.RoundUpOpt, bom.ManufactureParty, fdet.LocFrom, fdet.LocTo, bom.OpRef, bom.RefOpRef, SUM(bom.OrderQty) as OrderQty
							from LE_OrderBomCPTimeSnapshot as bom
							inner join LE_FlowDetSnapShot as fdet on bom.Item = fdet.Item and bom.Location = fdet.LocTo
							left join #tempIgnoreWorkCenter as wc on bom.WorkCenter = wc.WorkCenter
							where fdet.Flow = @Flow and fdet.Strategy = 3 and bom.CPTime >= @ReqTimeFrom and bom.CPTime < @ReqTimeTo and bom.IsCreateOrder = 0 and wc.WorkCenter is null
							group by fdet.Flow, fdet.FlowDetId, fdet.Item, fdet.Uom, fdet.UC, fdet.MinUC, fdet.UCDesc, fdet.Container, fdet.ContainerDesc, fdet.RoundUpOpt, bom.ManufactureParty, fdet.LocFrom, fdet.LocTo, bom.OpRef, bom.RefOpRef) as req
					left join SCM_OpRefBalance as orb on req.Item = orb.Item and req.OpRef = orb.OpRef
					order by req.Item, req.OpRef
					
					--删除直送供应商和指定供应商不相同的
					if @FlowType = 1
					begin
						delete from #tempOrderBomDet where ManufactureParty is not null and ManufactureParty <> @FlowPartyFrom
					end
					
					if exists(select top 1 1 from #tempOrderBomDet)
					begin
						--如果同一个工位+物料有两条以上明细的话
						--只能保留一条的初始工位余量，另外的都要update成0
						--避免工位余量增多
						update bom set OpRefQty = 0
						from #tempOrderBomDet as bom inner join
						--根据零件和工位再按制造商分组排序（null和empty的制造商在前面），把分组号大于1的工位余量全部置为0
						(select ROW_NUMBER() over(Partition by Item, OpRef order by t.ManufactureParty) as PtRowNum, RowId from #tempOrderBomDet) as t 
						on bom.RowId = t.RowId
						where t.PtRowNum > 1
						
						--按包装圆整
						update #tempOrderBomDet set OrderQty = ceiling(OrderQty / UC) * UC, Balance = ceiling(OrderQty / UC) * UC - OrderQty where OrderQty >= 0 and RoundUpOpt = 1 and UC > 0 and ISNULL(ManufactureParty, '') = ''
						update #tempOrderBomDet set OrderQty = 0, Balance = 0 - OrderQty where OrderQty < 0 and ISNULL(ManufactureParty, '') = ''
						update #tempOrderBomDet set OrderQty = ceiling(OrderQty / UC) * UC, Balance = OpRefQty + ceiling(OrderQty / UC) * UC - OrderQty where OrderQty >= 0 and RoundUpOpt = 1 and UC > 0 and ISNULL(ManufactureParty, '') <> ''
						update #tempOrderBomDet set Balance = (case when OrderQty >= 0 then 0 else (OpRefQty - NetOrderQty) end) where Balance is null and ISNULL(ManufactureParty, '') = ''
						update #tempOrderBomDet set Balance = OpRefQty where Balance is null and ISNULL(ManufactureParty, '') <> ''
						
						--更新工位余量
						update orb set Qty = ISNULL(tod.Balance, 0), [Version] = [Version] + 1, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm
						from SCM_OpRefBalance as orb 
							inner join (select Item, OpRef, SUM(Balance) as Balance from #tempOrderBomDet group by Item, OpRef) as tod on orb.Item = tod.Item and orb.OpRef = tod.OpRef
						
						--更新工位余量日志
						insert into LOG_OpRefBalanceChange(Item, OpRef, Qty, [Status], [Version], CreateDate, CreateUserId, CreateUserNm)
						select tod.Item, tod.OpRef, ISNULL(tod.Balance, 0), 1, orb.[Version], @DateTimeNow, @CreateUserId, @CreateUserNm
						from SCM_OpRefBalance as orb 
							inner join (select Item, OpRef, SUM(Balance) as Balance from #tempOrderBomDet group by Item, OpRef) as tod on orb.Item = tod.Item and orb.OpRef = tod.OpRef
						
						--新增不存在的工位余量
						insert into SCM_OpRefBalance (Item, OpRef, Qty, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, [Version])
						select tod.Item, tod.OpRef, ISNULL(tod.Balance, 0), @CreateUserId, @CreateUserNm, @DateTimeNow, @CreateUserId, @CreateUserNm, @DateTimeNow, 1
						from (select Item, OpRef, SUM(Balance) as Balance from #tempOrderBomDet group by Item, OpRef) as tod 
						left join SCM_OpRefBalance as orb on orb.Item = tod.Item and orb.OpRef = tod.OpRef
						where orb.Id is null
						
						--新增工位余量日志
						insert into LOG_OpRefBalanceChange(Item, OpRef, Qty, [Status], [Version], CreateDate, CreateUserId, CreateUserNm)
						select tod.Item, tod.OpRef, ISNULL(tod.Balance, 0), 0, 1, @DateTimeNow, @CreateUserId, @CreateUserNm
						from (select Item, OpRef, SUM(Balance) as Balance from #tempOrderBomDet group by Item, OpRef) as tod 
						left join SCM_OpRefBalance as orb on orb.Item = tod.Item and orb.OpRef = tod.OpRef
						where orb.Id is null
						
						if exists(select top 1 1 from #tempOrderBomDet where OrderQty > 0)
						begin
							--汇总订单明细
							truncate table #tempOrderDet
							insert into #tempOrderDet(Flow, FlowDetId, Item, Uom, UC, MinUC, UCDesc, Container, ContainerDesc, ManufactureParty, LocFrom, LocTo, OpRef, RefOpRef, ReqQty, OrderQty)
							select det.Flow, det.FlowDetId, det.Item, det.Uom, det.UC, det.MinUC, det.UCDesc, det.Container, det.ContainerDesc, det.ManufactureParty, det.LocFrom, det.LocTo, det.OpRef, det.RefOpRef, SUM(det.NetOrderQty), SUM(det.OrderQty)
							from #tempOrderBomDet as det
							group by det.Flow, det.FlowDetId, det.Item, det.Uom, det.UC, det.MinUC, det.UCDesc, det.Container, det.ContainerDesc, det.ManufactureParty, det.LocFrom, det.LocTo, det.OpRef, det.RefOpRef having SUM(det.OrderQty) > 0
							
							--获取订单号
							exec USP_GetDocNo_ORD @Flow, 3, @FlowType, 0, 0, 0, @FlowPartyFrom, @FlowPartyTo, @FlowLocTo, @FlowLocFrom, @FlowDock, 0, @OrderNo output
							--记录订单号
							update #tempOrderDet set OrderNo = @OrderNo
							--记录订单号
							update #tempOrderBomDet set OrderNo = @OrderNo where OrderQty > 0
							
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
								@StartTime,           --开始时间
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
								@StartTime,           --开始时间
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
							select @OrderDetCount = COUNT(*) from #tempOrderDet
							--锁定OrderDetId字段标识字段
							exec USP_SYS_BatchGetNextId 'ORD_OrderDet', @OrderDetCount, @EndOrderDetId output
							--查找开始标识
							set @BeginOrderDetId = @EndOrderDetId - @OrderDetCount
							
							--缓存订单ID
							truncate table #tempOrderDetId
							insert into #tempOrderDetId(RowId, OrderDetId, OrderDetSeq)
							select RowId, ROW_NUMBER() over (order by RowId) + @BeginOrderDetId as OrderDetId, ROW_NUMBER() over (order by RowId) as OrderDetSeq
							from #tempOrderDet
							
							--记录订单明细ID
							update det set OrderDetId = id.OrderDetId, OrderDetSeq = id.OrderDetSeq
							from #tempOrderDet as det inner join #tempOrderDetId as id
							on det.RowId = id.RowId
							
							--记录订单明细和BOM明细的关系
							update bom set OrderDetId = det.OrderDetId, OrderDetSeq = det.RowId
							from #tempOrderDet as det inner join #tempOrderBomDet as bom
							on det.Item = bom.Item and det.LocTo = bom.LocTo
							
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
								WMSSeq,						--JIT计算工位
								BinTo,						--配送工位
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
								det.OpRef,					--JIT计算工位
								ISNULL(det.RefOpRef, det.OpRef),--配送工位
								--det.LocFrom,				--出库库位
								--lf.Name,					--出库库位名称
								ISNULL(det.LocTo, mstr.LocTo),	--入库库位
								ISNULL(lt.Name, mstr.LocToNm),--入库库位
								mstr.BillAddr,				--开票地址
								CONVERT(varchar, det.FlowDetId),--记录FlowDetId，如果是0代表系统自动创建的路线明细
								mstr.BillTerm
								from #tempOrderDet as det
								inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
								inner join MD_Item as i on det.Item = i.Code
								--left join MD_Location as lf on det.LocFrom = lf.Code
								left join MD_Location as lt on det.LocTo = lt.Code
								
								--更新累计交货量
								update q set AccumulateQty = ISNULL(q.AccumulateQty, 0) + det.OrderQty, [Version] = q.[Version] + 1
								from SCM_Quota as q 
								inner join (select det.Item, mstr.PartyFrom, SUM(det.OrderQty) as OrderQty
											from #tempOrderDet as det 
											inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
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
								WMSSeq,						--JIT计算工位
								BinTo,						--配送工位
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
								det.OpRef,					--JIT计算工位
								ISNULL(det.RefOpRef, det.OpRef),--配送工位
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
							end
							
							--记录日志
							set @Msg = N'创建线边物料要货单' + @OrderNo
							insert into #temp_LOG_GenOrder4VanProdLine(Flow, OrderNo, Lvl, Msg) values(@Flow, @OrderNo, 0, @Msg)
							
							if @FlowType = 1
							begin  --记录创建计划协议的订单明细
								INSERT INTO SAP_CRSL(EINDT, FRBNR, LIFNR, MATNR, MENGE, SGTXT, WERKS, OrderDetId, [Status], ErrorCount, CreateDate, LastModifyDate)
								select CONVERT(varchar(8), @WindowTime, 112), det.OrderNo, mstr.PartyFrom, det.Item, det.OrderQty, det.OrderDetSeq, r.Plant, det.OrderDetId, 0, 0, @DateTimeNow, @DateTimeNow
								from #tempOrderDet as det
								inner join ORD_OrderMstr_1 as mstr on mstr.OrderNo = det.OrderNo
								inner join MD_Region as r on mstr.PartyTo = r.Code
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
									ISNULL(det.RefOpRef, det.OpRef), '', det.OrderQty, mstr.IsShipExceed, '', 0
									from #tempOrderDet as det
									inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
								end
								/**else if exists(select top 1 1 from SCM_FlowMstr where Code = @Flow and PartyFrom in ('SQC'))
								begin
									--记录FIS_CreateOrderDAT
									INSERT INTO FIS_CreateOrderDAT(OrderNo, MATNR, LIFNR, ENMNG, CHARG, COLOR, TIME_STAMP, CY_SEQNR,
									TIME_STAMP1, AUFNR, LGORT, UMLGO, LGPBE, REQ_TIME_STAMP, FLG_SORT, PLNBEZ, KTEXT, ZPLISTNO, 
									ErrorCount, IsCreateDat, CreateUserNm)
									select det.OrderNo, det.Item, det.ManufactureParty, det.OrderQty, '', '', REPLACE(REPLACE(REPLACE(CONVERT(varchar(50),mstr.CreateDate,120),':',''),'-',''),' ',''), '',
									null, '', ISNULL(det.LocFrom, mstr.LocFrom), ISNULL(det.LocTo, mstr.LocTo), det.OpRef, REPLACE(REPLACE(REPLACE(CONVERT(varchar(50),mstr.WindowTime,120),':',''),'-',''),' ',''), 'N', '', '', det.OrderDetId,
									0, 0, @CreateUserNm
									from #tempOrderDet as det
									inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = det.OrderNo
								end**/
							end
						end
						
						--Log: JIT订单计算明细
						insert into LOG_VanOrderTrace(UUID, Flow, OrderNo, OrderDetSeq, OrderDetId, [Priority], StartTime, WindowTime, EMWindowTime, ReqTimeFrom, ReqTimeTo, 
							Item, RefItemCode, ItemDesc, Uom, UC, ManufactureParty, Location, OpRef, NetOrderQty, OrgOpRefQty, GrossOrderQty, OpRefQty, BatchNo) 
						select UUID, bom.Flow, bom.OrderNo, bom.OrderDetSeq, bom.OrderDetId, 0, @StartTime, @WindowTime, @EMWindowTime, @ReqTimeFrom, @ReqTimeTo, 
							bom.Item, i.RefCode, i.Desc1, bom.Uom, bom.UC, bom.ManufactureParty, bom.LocTo, bom.OpRef, bom.NetOrderQty, bom.OpRefQty, bom.OrderQty, bom.Balance, @BatchNo
						from #tempOrderBomDet as bom inner join MD_Item as i on bom.Item = i.Code
						
						--Log: JIT订单计算明细和BOM的关系
						insert into LOG_VanOrderBomTrace(UUID, ProdLine, VanOrderNo, VanOrderBomDetId, Item, RefItemCode, ItemDesc, OpRef, LocFrom, LocTo, OrderQty, CPTime, BatchNo)
						select tod.UUID, bomCP.VanProdLine, bomCP.OrderNo, bomCP.BomId, bomCP.Item, i.RefCode, i.Desc1, bomCP.OpRef, fdet.LocFrom, fdet.LocTo, bomCP.OrderQty, bomCP.CPTime, @BatchNo 
						from LE_OrderBomCPTimeSnapshot as bomCP
						inner join LE_FlowDetSnapShot as fdet on bomCP.Item = fdet.Item and bomCP.Location = fdet.LocTo
						inner join #tempOrderBomDet as tod on tod.Item = bomCP.Item and tod.OpRef = bomCP.OpRef
						inner join MD_Item as i on tod.Item = i.Code
						where fdet.Flow = @Flow and fdet.Strategy = 3 and bomCP.CPTime >= @ReqTimeFrom and bomCP.CPTime < @ReqTimeTo and bomCP.IsCreateOrder = 0
						
						--计算过的JIT需求打上标记
						update bom set IsCreateOrder = 1, [Version] = bom.[Version] + 1, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm
						from ORD_OrderBomDet as bom
						inner join LE_OrderBomCPTimeSnapshot as bomCP on bom.Id = bomCP.BomId
						inner join LE_FlowDetSnapShot as fdet on bomCP.Item = fdet.Item and bomCP.Location = fdet.LocTo
						where fdet.Flow = @Flow and fdet.Strategy = 3 and bomCP.CPTime >= @ReqTimeFrom and bomCP.CPTime < @ReqTimeTo and bomCP.IsCreateOrder = 0
						
						update bomCP set IsCreateOrder = 1
						from LE_OrderBomCPTimeSnapshot as bomCP
						inner join LE_FlowDetSnapShot as fdet on bomCP.Item = fdet.Item and bomCP.Location = fdet.LocTo
						where fdet.Flow = @Flow and fdet.Strategy = 3 and bomCP.CPTime >= @ReqTimeFrom and bomCP.CPTime < @ReqTimeTo and bomCP.IsCreateOrder = 0
					end
					
					--记录日志
					set @Msg = N'计算路线正常需求生成线边物料要货单结束'
					insert into #temp_LOG_GenOrder4VanProdLine(Flow, Lvl, Msg) values(@Flow, 0, @Msg)
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
				set @Msg = N'计算路线需求生成线边物料要货单出现异常，异常信息：' +  + Error_Message()
				insert into #temp_LOG_GenOrder4VanProdLine(Flow, Lvl, Msg) values(@Flow, 1, @Msg)
			end catch	
		end
		
		insert into LOG_GenOrder4VanProdLine(OrderNo, Flow, Lvl, Msg, CreateDate, BatchNo) 
		select OrderNo, Flow, Lvl, Msg, CreateDate, @BatchNo from #temp_LOG_GenOrder4VanProdLine
	
		set @FlowId = @FlowId + 1
	end
	
	--记录日志
	set @Msg = N'生成整车线边物料要货单结束'
	insert into LOG_GenOrder4VanProdLine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	drop table #tempIgnoreWorkCenter
	drop table #temp_LOG_GenOrder4VanProdLine
	drop table #tempOrderDetId
	drop table #tempOrderDet
	drop table #tempOrderBomDet
END 