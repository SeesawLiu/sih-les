SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO
IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_GenAnDonOrder') 
     DROP PROCEDURE USP_Busi_GenAnDonOrder 
GO
CREATE PROCEDURE USP_Busi_GenAnDonOrder
(
	@CreateUserId int,
	@CreateUserNm varchar(100)
)
AS
BEGIN
	DECLARE @DateTimeNow datetime
	DECLARE @trancount int
	
	SET @DateTimeNow=GETDATE()
	SELECT ks.* INTO #TempNeedOrderScan FROM KB_KanbanScan ks INNER JOIN SCM_FlowStrategy fs ON ks.Flow=fs.Flow
	WHERE ks.ScanTime<@DateTimeNow AND ks.IsOrdered=0 AND fs.IsOrderNow=0
	
	CREATE TABLE #TempFlow(
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Flow] [varchar](50) NOT NULL,
		[Type] [tinyint] NULL,
		[Strategy] [tinyint] NULL,
		[PartyFrom] [varchar](50) NULL,
		[PartyTo] [varchar](50) NULL,
		[LocFrom] [varchar](50) NULL,
		[LocTo] [varchar](50) NULL,
		[Dock] [varchar](50) NULL,
		[ExtraDmdSource] [varchar](255) NULL,
		[OrderTime] [datetime] NULL,
		[ReqTimeFrom] [datetime] NULL,
		[ReqTimeTo] [datetime] NULL,
		[WindowTime] [datetime] NULL,
		[EMWindowTime] [datetime] NULL
	) 
	
	CREATE TABLE #TempOrderNos
	(
		OrderNo varchar(50)
	)
	
	
	INSERT INTO #TempFlow(Flow, [Type], Strategy, PartyFrom, PartyTo, LocFrom, LocTo, Dock, ExtraDmdSource)
	SELECT DISTINCT fm.Code AS Flow, fm.[Type], fs.Strategy, fm.PartyFrom, fm.PartyTo, fm.LocFrom, fm.LocTo, fm.Dock, fm.ExtraDmdSource
	FROM SCM_FlowMstr AS fm 
	INNER JOIN KB_KanbanScan ks ON fm.Code=ks.Flow
	INNER JOIN SCM_FlowStrategy fs ON fm.Code = fs.Flow
	WHERE ks.ScanTime<@DateTimeNow AND ks.IsOrdered=0 AND fs.IsOrderNow=0
	
	DECLARE @FlowRowId int
	DECLARE @MaxFlowRowId int
	
	
	
	
	SELECT @FlowRowId = MIN(Id), @MaxFlowRowId = MAX(Id) from #TempFlow
	WHILE (@FlowRowId <= @MaxFlowRowId)
	BEGIN
		DECLARE @Flow varchar(50) = null
		DECLARE @FlowType tinyint = null
		DECLARE @FlowStrategy tinyint = null
		DECLARE @PartyFrom varchar(50) = null
		DECLARE @PartyTo varchar(50) = null
		DECLARE @LocFrom varchar(50) = null
		DECLARE @LocTo varchar(50) = null
		DECLARE @ExtraDmdSource varchar(50) = null
		DECLARE @WinTimeType tinyint = null
		DECLARE @WinTimeDiff decimal(18, 8) = null  --秒，进料提前期
		DECLARE @SafeTime decimal(18, 8) = null  --秒，安全期：需求覆盖提前期，@ReqTimeTo会往后增加安全期的时间
		DECLARE @LeadTime decimal(18, 8) = null  --秒，提前期：发单提前期
		DECLARE @EMLeadTime decimal(18, 8) = null  --秒，紧急提前期
		DECLARE @PrevWinTime datetime = null
		DECLARE @WindowTime datetime = null
		DECLARE @WindowTime2 datetime = null
		
		DECLARE @OrderNo varchar(50) = null
		DECLARE @Dock varchar(50) = null
		DECLARE @EMWindowTime datetime = null
		DECLARE @ReqTimeFrom datetime = null
		DECLARE @ReqTimeTo datetime = null
		DECLARE @OrderTime datetime = null
		DECLARE @OrderDetCount int
		DECLARE @EndOrderDetId int
		DECLARE @BeginOrderDetId int
		
		SELECT @Flow = Flow, @FlowType = [Type], @Dock=Dock,
		@PartyFrom = PartyFrom, @PartyTo = PartyTo,
		@LocFrom = LocFrom, @LocTo = LocTo, @ExtraDmdSource = ExtraDmdSource
		FROM #TempFlow WHERE Id = @FlowRowId
		
		SET @trancount = @@TRANCOUNT
		BEGIN TRY
			IF @trancount = 0
			BEGIN
				BEGIN TRAN
			END		
		
			SELECT @FlowStrategy = Strategy, @WinTimeType = WinTimeType, 
			@WinTimeDiff = WinTimeDiff * 60 * 60, --小时转为秒
			@SafeTime = ISNULL(SafeTime, 0) * 60 * 60, --小时转为秒
			@LeadTime = LeadTime * 60 * 60, --小时转为秒
			@EMLeadTime = EMLeadTime * 60 * 60,	   --小时转为秒
			@PrevWinTime = PreWinTime,       --上次窗口时间
			@WindowTime = NextWinTime
			FROM SCM_FlowStrategy WHERE Flow = @Flow
			
			--看板窗口时间只需要当前时间加上提前期即可
			EXEC USP_Busi_AddWorkingDate @DateTimeNow, @LeadTime, null, @PartyTo, @EMWindowTime output
		
			exec USP_GetDocNo_ORD @Flow, 7, @FlowType, 0, 0, 1, @PartyFrom, @PartyTo, @LocTo, @LocFrom, @Dock, 0, @OrderNo output

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
				7,					  --策略
				@FlowType,			  --类型
				0,					  --子类型，0正常
				0,					  --质量状态，0良品
				@DateTimeNow,         --开始时间
				@EMWindowTime,        --窗口时间
				0,					  --暂停工序，0
				0,					  --是否快速，0
				0,					  --优先级，1
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
				7,					  --策略
				@FlowType,			  --类型
				0,					  --子类型，0正常
				0,					  --质量状态，0良品
				@DateTimeNow,         --开始时间
				@EMWindowTime,        --窗口时间
				0,					  --暂停工序，0
				0,					  --是否快速，0
				0,					  --优先级，1
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
			
			SELECT @OrderDetCount = COUNT(*) FROM #TempNeedOrderScan WHERE Flow=@Flow
			
			EXEC USP_SYS_BatchGetNextId 'ORD_OrderDet', @OrderDetCount, @EndOrderDetId output
			--查找开始标识
			SET @BeginOrderDetId = @EndOrderDetId - @OrderDetCount


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
				BillAddr,					--开票地址
				ContainerDesc,				--记录FlowDetId，如果是0代表系统自动创建的路线明细
				BillTerm,					--结算方式
				BinTo						--目的工位
				)
				select 
				@BeginOrderDetId+ROW_NUMBER()OVER(ORDER BY tos.Id),				--生产单明细标识
				@OrderNo,				--生产单号
				@FlowType,					--类型，1采购
				0,							--子类型，0正常
				ROW_NUMBER()OVER(ORDER BY tos.Id),			--行号，
				0,							--计划协议类型，0
				tos.Item,					--物料号
				i.RefCode,					--参考物料号
				i.Desc1,					--物料描述
				i.Uom,					--单位
				i.Uom,					--基本单位
				i.UC,						--包装，1
				1,							--最小包装，1
				0,							--质量状态，0
				fd.ManufactureParty,		--制造商
				tos.ScanQty,					--需求数量，1
				tos.ScanQty,				--订单数量，1
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
				fm.LocFrom,					--出库库位
				lf.Name,					--出库库位名称
				fm.LocTo,					--入库库位
				lt.Name,					--入库库位
				fm.BillAddr,				--开票地址
				CONVERT(varchar, fd.Id),--记录FlowDetId，如果是0代表系统自动创建的路线明细
				fm.BillTerm,				--结算方式
				fd.BinTo						--目的工位
				from #TempNeedOrderScan tos
				INNER JOIN SCM_FlowMstr fm ON tos.Flow=fm.Code
				INNER JOIN SCM_FlowDet fd ON fd.Id=tos.FlowDetId
				INNER JOIN MD_Item as i on tos.Item = i.Code
				LEFT JOIN MD_Location as lf on fm.LocFrom = lf.Code
				LEFT JOIN MD_Location as lt on fm.LocTo = lt.Code
				WHERE tos.Flow=@Flow
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
				BillAddr,					--开票地址
				ContainerDesc,				--记录FlowDetId，如果是0代表系统自动创建的路线明细
				BillTerm,					--结算方式
				BinTo						--目的工位
				)
				select 
				@BeginOrderDetId+ROW_NUMBER()OVER(ORDER BY tos.Id),				--生产单明细标识
				@OrderNo,				--生产单号
				@FlowType,					--类型，1采购
				0,							--子类型，0正常
				ROW_NUMBER()OVER(ORDER BY tos.Id),			--行号，
				0,							--计划协议类型，0
				tos.Item,					--物料号
				i.RefCode,					--参考物料号
				i.Desc1,					--物料描述
				i.Uom,					--单位
				i.Uom,					--基本单位
				i.UC,						--包装，1
				1,							--最小包装，1
				0,							--质量状态，0
				fd.ManufactureParty,		--制造商
				tos.ScanQty,					--需求数量，1
				tos.ScanQty,				--订单数量，1
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
				fm.LocFrom,					--出库库位
				lf.Name,					--出库库位名称
				fm.LocTo,					--入库库位
				lt.Name,					--入库库位
				fm.BillAddr,				--开票地址
				CONVERT(varchar, fd.Id),--记录FlowDetId，如果是0代表系统自动创建的路线明细
				fm.BillTerm,				--结算方式
				fd.BinTo					--目的工位
				from #TempNeedOrderScan tos
				INNER JOIN SCM_FlowMstr fm ON tos.Flow=fm.Code
				INNER JOIN SCM_FlowDet fd ON fd.Id=tos.FlowDetId
				INNER JOIN MD_Item as i on tos.Item = i.Code
				LEFT JOIN MD_Location as lf on fm.LocFrom = lf.Code
				LEFT JOIN MD_Location as lt on fm.LocTo = lt.Code
				WHERE tos.Flow=@Flow
			END
			
			--插入中间数据表
			IF @PartyFrom IN('SQC')
			BEGIN
				--双桥集货库使用旧WMS系统
				INSERT INTO FIS_CreateOrderDAT(
					OrderNo,
					MATNR,
					LIFNR,
					ENMNG,
					CHARG,
					COLOR,
					TIME_STAMP,
					CY_SEQNR,
					TIME_STAMP1,
					AUFNR,
					LGORT,
					UMLGO,
					LGPBE,
					REQ_TIME_STAMP,
					FLG_SORT,
					PLNBEZ,
					KTEXT,
					ZPLISTNO,
					ErrorCount,
					IsCreateDat,
					CreateUserNm)
				SELECT @OrderNo,
					tos.Item,
					tos.Supplier,
					CAST(tos.ScanQty AS decimal(18,3)),
					'',
					'',
					REPLACE(REPLACE(REPLACE(CONVERT(varchar(50),@DateTimeNow,120),':',''),'-',''),' ',''),
					'',
					null,
					'',
					fm.LocFrom,
					fm.LocTo,
					fd.BinTo,
					REPLACE(REPLACE(REPLACE(CONVERT(varchar(50),@EMWindowTime,120),':',''),'-',''),' ',''),
					'N',
					'',
					'',
					@BeginOrderDetId+ROW_NUMBER()OVER(ORDER BY tos.Id),
					0,
					0,
					@CreateUserNm
				FROM #TempNeedOrderScan tos
				INNER JOIN SCM_FlowMstr fm ON tos.Flow=fm.Code
				INNER JOIN SCM_FlowDet fd ON fd.Id=tos.FlowDetId
				INNER JOIN MD_Item as i on tos.Item = i.Code
				WHERE tos.Flow=@Flow
			END IF @PartyFrom IN('LOC')
			BEGIN
				--LOC使用新WMS系统
				INSERT INTO FIS_CreateProcurementOrderDAT(
					OrderNo, 
					Van, 
					OrderStrategy, 
					StartTime, 
					WindowTime, 
					Priority, 
					Sequence, 
					PartyFrom, 
					PartyTo, 
					Dock, 
					CreateDate, 
					Flow, 
					LineSeq, 
					Item, 
					ManufactureParty, 
					LocationTo, 
					Bin, 
					OrderedQty, 
					IsShipExceed, 
					FileName, 
					IsCreateDat)
				SELECT
					@OrderNo,
					'',
					7,
					@DateTimeNow,
					@EMWindowTime,
					0,
					'',
					fm.LocFrom,
					fm.LocTo,
					fm.Dock,
					@DateTimeNow,
					fm.Code,	
					@BeginOrderDetId+ROW_NUMBER()OVER(ORDER BY tos.Id),	
					tos.Item,
					fd.ManufactureParty,
					fd.LocTo,
					fd.BinTo,
					tos.ScanQty,
					fm.IsShipExceed,
					'',
					0
				FROM #TempNeedOrderScan tos
				INNER JOIN SCM_FlowMstr fm ON tos.Flow=fm.Code
				INNER JOIN SCM_FlowDet fd ON fd.Id=tos.FlowDetId
				INNER JOIN MD_Item as i on tos.Item = i.Code
				LEFT JOIN MD_Location as lf on fm.LocFrom = lf.Code
				LEFT JOIN MD_Location as lt on fm.LocTo = lt.Code
				WHERE tos.Flow=@Flow
			END	
								

			INSERT INTO #TempOrderNos(OrderNo)
			SELECT @OrderNo
			
			--IF @OrderTime <= @DateTimeNow
			--BEGIN
			--	--更新路线上次窗口时间
			--	UPDATE SCM_FlowStrategy SET PreWinTime = @WindowTime, NextWinTime = @WindowTime2 WHERE Flow = @Flow
			--END
			
			--更新刷读记录
			UPDATE ks SET ks.IsOrdered=1,ks.OrderNo=@OrderNo,ks.OrderQty=ks.ScanQty,ks.OrderTime=@DateTimeNow,
				ks.OrderUser=@CreateUserId,ks.OrderUserNm=@CreateUserNm
			FROM KB_KanbanScan ks INNER JOIN #TempNeedOrderScan tos ON ks.Id=tos.Id
			WHERE ks.Flow=@Flow
			
			UPDATE kb SET kb.Status=2 FROM KB_KanbanCard kb INNER JOIN #TempNeedOrderScan tos ON kb.CardNo=tos.CardNo
			WHERE kb.Flow=@Flow
			  
			SET @FlowRowId=@FlowRowId+1
			IF @trancount = 0 
			BEGIN  
				COMMIT
			END
		END TRY
		BEGIN CATCH
			IF @trancount = 0
			BEGIN
				ROLLBACK
			END 
			SET @FlowRowId=@FlowRowId+1
			DECLARE @errorMsg varchar(max)
			SET @errorMsg=ERROR_MESSAGE()
			RAISERROR(@errorMsg,16,1)
		END CATCH	
		SELECT * FROM #TempOrderNos		
	END	
END
