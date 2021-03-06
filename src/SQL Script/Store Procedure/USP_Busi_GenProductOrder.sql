SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_GenProductOrder') 
     DROP PROCEDURE USP_Busi_GenProductOrder
GO

CREATE PROCEDURE [dbo].[USP_Busi_GenProductOrder]
(
	@BatchNo int,
	@CreateUserId int,
	@CreateUserNm varchar(50)
) --WITH ENCRYPTION
AS 
BEGIN 
	set nocount on
	declare @DateTimeNow datetime = GetDate()
	declare @Msg nvarchar(MAX)
	declare @trancount int = @@trancount
	   
	begin try
		if @trancount = 0
		begin
            begin tran
        end

		Create table #tempNewOrder
		(
			RowId int identity(1, 1),
			ProdOrderId int,
			AUFNR varchar(50),
			GSTRS datetime,
			DAUAT varchar(50),
			LGORT varchar(50),
			MATNR varchar(50),
			MAKTX varchar(100),
			GMEIN varchar(50),
			GAMNG decimal(18, 8),
			CHARG varchar(50)
		)
		
		Create table #tempExistOrder
		(
			RowId int identity(1, 1),
			ProdOrderId int,
			AUFNR varchar(50),
			OrderNo varchar(50),
			OrderDetId int,
			DAUAT varchar(50)
		)
		
		--缓存新增的生产单
		insert into #tempNewOrder(ProdOrderId, AUFNR, GSTRS, DAUAT, LGORT, MATNR, MAKTX, GMEIN, GAMNG, CHARG) 
		select s.Id, s.AUFNR, s.GSTRS, s.DAUAT, s.LGORT, s.MATNR, s.MAKTX, s.GMEIN, s.GAMNG, s.CHARG 
		from SAP_ProdOrder as s 
		left join ORD_OrderMstr_4 as o on s.AUFNR = o.ExtOrderNo and o.[Status] in (0, 1, 2)
		where BatchNo = @BatchNo and o.OrderNo is null
		
		--缓存更新的生产单
		insert into #tempExistOrder(AUFNR, ProdOrderId, OrderNo, OrderDetId,DAUAT) 
		select s.AUFNR, s.Id, o.OrderNo, d.Id, s.DAUAT from SAP_ProdOrder as s 
		inner join ORD_OrderMstr_4 as o on s.AUFNR = o.ExtOrderNo 
		inner join ORD_OrderDet_4 as d on o.OrderNo = d.OrderNo
		where BatchNo = @BatchNo and o.[Status] in (0, 1, 2)
		
		-----------------------------↓新增生产单-----------------------------
		Declare @NewOrderRowId int
		Declare @MaxNewOrderRowId int
		insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 0, '开始创建生产订单')
		select 	@NewOrderRowId = MIN(RowId), @MaxNewOrderRowId = MAX(RowId) from #tempNewOrder
		while (@NewOrderRowId <= @MaxNewOrderRowId)
		begin
			
			Declare @ProdOrderId int = null   --SAP生产单临时表ID
			Declare @AUFNR varchar(50) = null  --SAP生产单号
			Declare @GSTRS datetime = null  --排产日期
			Declare @DAUAT varchar(50) = null  --生产单类型
			Declare @LGORT varchar(50) = null  --成品入库库位
			Declare @ARBPL varchar(50) = null  --自动收货工序的工作中心
			Declare @MATNR varchar(50) = null  --物料代码
			Declare @MAKTX varchar(100) = null --物料描述
			Declare @GMEIN varchar(50) = null  --订单单位
			Declare @GAMNG varchar(50) = null  --订单数量
			Declare @CHARG varchar(50) = null  --批号
			Declare @ProdLine varchar(50) = null
			Declare @ProdLineRegion varchar(50) = null
			Declare @ProdLineLocFrom varchar(50) = null
			Declare @ProdLineLocTo varchar(50) = null
			Declare @ProdLineType tinyint = null
			Declare @OrderNo varchar(50) = null
			Declare @OrderDetId int = null
			Declare @OrderTemplate varchar(50) = null
			Declare @FlowDesc varchar(100) = null
			Declare @AllowRecExceed int = null
		
			select @ProdOrderId = ProdOrderId, @AUFNR = AUFNR, @GSTRS = GSTRS, @DAUAT = DAUAT, @LGORT = LGORT,
			@MATNR = MATNR, @MAKTX = MAKTX, @GMEIN = GMEIN, @GAMNG = GAMNG, @CHARG = CHARG
			from #tempNewOrder where RowId = @NewOrderRowId
			
			select top 1 @ARBPL = ARBPL from SAP_ProdRoutingDet where BatchNo = @BatchNo and AUFNR = @AUFNR and AUTWE = 'X'
			
			--if ISNULL(@LGORT, '') = ''
			--begin
			--	set @Msg = N'SAP生产单' + @AUFNR + N'的成品入库库位为空，生产单创建失败。'
			--	insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 2, @Msg)
				
			--	set @NewOrderRowId = @NewOrderRowId + 1
			--	continue
			--end
			
			if ISNULL(@DAUAT, '') = ''
			begin
				set @Msg = N'SAP生产单' + @AUFNR + N'的生产单类型为空，生产单创建失败。'
				insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 2, @Msg)
				
				set @NewOrderRowId = @NewOrderRowId + 1
				continue
			end
			
			--if ISNULL(@ARBPL, '') = ''
			--begin
			--	set @Msg = N'SAP生产单' + @AUFNR + N'的自动收货工序的工作中心为空，生产单创建失败。'
			--	insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 2, @Msg)
				
			--	set @NewOrderRowId = @NewOrderRowId + 1
			--	continue
			--end
			
			--获取生产线
			select @ProdLine = ProdLine from CUST_ProductLineMap where SAPProdLine = @DAUAT and [Type] = 0 and IsActive = 1
			
			if ISNULL(@ProdLine, '') = ''
			begin
				select @ProdLine = ProdLine from CUST_ProductLineMap where SAPProdLine = @ARBPL + '_' + @LGORT and [Type] = 0 and IsActive = 1
			end
			
			if ISNULL(@ProdLine, '') = ''
			begin
				set @Msg = N'没有找到SAP生产单' + @AUFNR + N'对应的LES生产线，可能没有维护生产线(生产单类型:' + @DAUAT + N', 工作中心:' + @ARBPL + N')映射关系或映射关系没有生效。'
				insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 2, @Msg)
				
				set @NewOrderRowId = @NewOrderRowId + 1
				continue
			end
			
			select @ProdLineRegion = PartyFrom, @ProdLineLocTo = LocTo, @ProdLineLocFrom = LocFrom, @ProdLineType = ProdLineType, @OrderTemplate = OrderTemplate, @FlowDesc = Desc1,@AllowRecExceed = IsRecExceed
			from SCM_FlowMstr where Code =  @ProdLine and [Type] = 4 and ProdLineType not in (1, 2, 3, 4, 9) and IsActive = 1
			
			if ISNULL(@ProdLineRegion, '') = ''
			begin
				set @Msg = N'SAP生产单' + @AUFNR + N'对应的LES生产线' + @ProdLine + N'不存在或没有生效。'
				insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 2, @Msg)
				
				set @NewOrderRowId = @NewOrderRowId + 1
				continue
			end
			
			if exists(select top 1 1 from SAP_ProdBomDet as bom
				left join SAP_ProdRoutingDet as routing on bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR and routing.BatchNo = @BatchNo and routing.AUFNR = @AUFNR
				where bom.BatchNo = @BatchNo and bom.AUFNR = @AUFNR and routing.BatchNo is null) 
			begin
				insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) 
				select @BatchNo, 2, N'SAP生产单' + @AUFNR + N'的Bom(AUFPL:' + CONVERT(varchar, bom.AUFPL) + N', PLNFL:' + bom.PLNFL + N', VORNR:' + bom.VORNR + N')没有找到对应的SAP工序。' from SAP_ProdBomDet as bom
				left join SAP_ProdRoutingDet as routing on bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR and routing.BatchNo = @BatchNo and routing.AUFNR = @AUFNR
				where bom.BatchNo = @BatchNo and bom.AUFNR = @AUFNR and routing.BatchNo is null
				
				set @NewOrderRowId = @NewOrderRowId + 1
				continue
			end
			
			if exists(select top 1 1 from SAP_ProdOpReport where AUFNR=@AUFNR and  IsCancel = 0) 
			begin
				insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) 
				select top 1 @BatchNo, 2, N'SAP生产单' + @AUFNR + N'已经导入(LES订单号： '+ OrderNo +')。' from SAP_ProdOpReport where AUFNR=@AUFNR and  IsCancel = 0
				
				set @NewOrderRowId = @NewOrderRowId + 1
				continue
			end
			
			if exists(select top 1 1 from SAP_ProdBomDet as bom
						inner join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo 
							--用工艺路线编号、顺序和操作活动关联
							and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  
						--获取扣料库位
						left join CUST_ProdLineLocationMap as loc on bom.LGORT = loc.SapLocation and loc.ProdLine = @ProdLine
						where bom.BatchNo = @BatchNo and bom.AUFNR = @AUFNR
						--由于返工订单可能bom中会存在消耗地点为3000的整车物料，要将其过滤掉
						and (bom.LGORT <> '3000' or (bom.LGORT = '3000' and ISNULL(bom.ICHARG, '') = ''))
						and routing.BatchNo = @BatchNo and routing.AUFNR = @AUFNR and loc.Id is null) 
			begin
				insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) 
				select @BatchNo, 2, N'SAP生产单' + @AUFNR + N'的生产线' + bom.LGORT + N'SAP库位' + @ProdLine + N'没有匹配到对应的LES库位。' from SAP_ProdBomDet as bom
						inner join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo 
							--用工艺路线编号、顺序和操作活动关联
							and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  
						--获取扣料库位
						left join CUST_ProdLineLocationMap as loc on bom.LGORT = loc.SapLocation and loc.ProdLine = @ProdLine
						where bom.BatchNo = @BatchNo and bom.AUFNR = @AUFNR 
						--由于返工订单可能bom中会存在消耗地点为3000的整车物料，要将其过滤掉
				        and (bom.LGORT <> '3000' or (bom.LGORT = '3000' and ISNULL(bom.ICHARG, '') = '')) 
						and routing.BatchNo = @BatchNo and routing.AUFNR = @AUFNR and loc.Id is null
						
				set @NewOrderRowId = @NewOrderRowId + 1
				continue
			end
			
			if not exists(select top 1 1 from SAP_ProdRoutingDet where BatchNo = @BatchNo and AUFNR = @AUFNR and (RUEK = '1' or RUEK = '2'))
			begin
				insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 2, N'SAP生产单' + @AUFNR + N'没有报工工序。')
				
				set @NewOrderRowId = @NewOrderRowId + 1
				continue
			end
			
			--if not exists(select top 1 1 from SAP_ProdRoutingDet where BatchNo = @BatchNo and AUFNR = @AUFNR and AUTWE = 'X')
			--begin
			--	insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 2, N'SAP生产单' + @AUFNR + N'没有收货工序。')
				
			--	set @NewOrderRowId = @NewOrderRowId + 1
			--	continue
			--end
			
			--备品备件生产单直接用SAP成品收货库位对应的LES库位作为入库库位
			if (@DAUAT = 'Z903')
			begin
				if (exists(select top 1 1 from MD_Location where Code = @LGORT and IsActive = 1))
				begin
					select @ProdLineLocTo = Code from MD_Location where Code = @LGORT and IsActive = 1
				end 
				else if (select COUNT(1) from MD_Location where SAPLocation = @LGORT and IsActive = 1) = 1
				begin
					select @ProdLineLocTo = Code from MD_Location where SAPLocation = @LGORT and IsActive = 1
				end
				else
				begin
					set @Msg = N'备品备件生产单' + @AUFNR + N'没有找或找到多条SAP成品入库库位' + @LGORT + N'对应的LES库位，系统自动取LES生产线的成品入库库位' + @ProdLineLocTo + N'。'
					insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 2, @Msg)
				end
			end
			
			--获取生产单号
			exec USP_GetDocNo_ORD @ProdLine, 0, 4, 0, 0, 0, @ProdLineRegion, @ProdLineRegion, @ProdLineLocTo, @ProdLineLocFrom, null, 0, @OrderNo output
			
			set @Msg='开始创建订单'+@OrderNo
			insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 0, @Msg)	
			--新建生产单头
			insert into ORD_OrderMstr_4 (
			OrderNo,              --生产单号
			Flow,                 --生产线
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
			FlowDesc,             --生产线描述
			ProdLineType,         --生产线类型
			PauseStatus,          --暂停状态，0
			ShipFromContact,      --存放物料号，
			ShipFromAddr,         --存放物料描述
			ShipFromFax,          --存放物料单位
			ShipFromTel			  --存放SAP生产单类型DAUAT
			)
			select 
			@OrderNo,				--生产单号
			@ProdLine,				--生产线
			0,						--策略，0
			@AUFNR,                 --外部订单号，SAP生产单号
			4,						--类型，4生产单
			0,						--子类型，0正常
			0,						--质量状态，0良品
			@GSTRS,					--开始时间
			@GSTRS,					--窗口时间
			0,                           --暂停工序，0
			0,                           --是否快速，0
			0,                           --优先级，0
			1,                           --状态，0创建、1释放
			@ProdLineRegion,		     --区域代码
			@ProdLineRegion,			--区域代码
			@ProdLineLocFrom,			--原材料库位
			@ProdLineLocTo,				--成品库位
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
			@AllowRecExceed,			 --允许超收，0
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
			@FlowDesc,                   --生产线描述
			@ProdLineType,               --生产线类型
			0,                           --暂停状态，0
			@MATNR,						 --物料号
			@MAKTX,						 --物料描述
			@GMEIN,						 --单位
			@DAUAT						 --SAP生产单类型
			
			--获取生产单明细Id
			exec USP_SYS_GetNextId 'ORD_OrderDet', @OrderDetId output
			
			--新建生产单明细
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
			LocFrom,					--原材料库位
			LocTo,						--成品库位
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
			ICHARG						--批号
			)
			select 
			@OrderDetId,                --生产单明细标识
			@OrderNo,                   --生产单号
			4,                          --类型，4生产单
			0,                          --子类型，0正常
			1,							--行号，1
			0,                          --计划协议类型，0
			@MATNR,						--物料号
			@MAKTX,						--物料描述
			@GMEIN,						--单位
			@GMEIN,						--基本单位
			1,                          --包装，1
			1,                          --最小包装，1
			0,                          --质量状态，0
			@GAMNG,                     --需求数量，1
			@GAMNG,                     --订单数量，1
			0,                          --发货数量，0
			0,                          --收货数量，0
			0,                          --次品数量，0
			0,                          --废品数量，0
			0,                          --拣货数量，0
			1,                          --单位用量，1
			@ProdLineLocFrom,			--原材料库位
			@ProdLineLocTo,				--成品库位
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
			0,							--是否修改单包装，0
			@CHARG						--批号
			
			if @DAUAT in ('ZP01', 'ZP02')
			begin  --试制车，bom.ICHARG = @AUFNR
				--新建生产单Bom
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
				IsCreateOrder,				--是否已经创建拉料单
				DISPO,						--MRP控制者
				PLNFL,						--序列
				VORNR,						--操作
				AUFPL,						--工艺路线编号
				LGORT						--SAP库位
				)
				select
				@OrderNo,					--生产单号
				4,							--类型，4生产单
				0,							--子类型，0正常
				@OrderDetId,				--生产单明细标识
				1,							--生产单明细顺序号
				ROW_NUMBER() over (order by bom.Id),--顺序号
				bom.MATERIAL,				--Bom零件号
				bom.MAKTX,					--Bom零件描述
				bom.BISMT,					--旧物料号
				bom.MEINS,					--单位
				bom.MEINS,					--基本单位
				Case when bom.LIFNR <> '' then bom.LIFNR else null end,					--指定供应商
				Dense_rank() over (order by routing.AUFPL asc, routing.PLNFL asc, routing.VORNR asc),--工序
				bom.GW,						--工位
				case when bom.RGEKZ = 'X' then (case when bom.BWART = '531' then -bom.MDMNG else bom.MDMNG end) else 0 end,					--Bom用量
				0,							--反冲合格数量
				0,							--反冲不合格数量
				0,							--反冲废品数量
				1,							--单位用量
				case when bom.RGEKZ = 'X' then (convert(decimal(18, 8), (Case when bom.BWART = '531' then -bom.MDMNG else bom.MDMNG end / @GAMNG))) else 0 end,--单个成品用量
				ISNULL(loc.Location, @ProdLineLocFrom),--反冲库位
				0,							--是否打印
				0,							--回冲方式
				0,							--投料方式
				0,							--是否自动投料
				0,							--是否关键件
				@DateTimeNow,				--预计消耗时间
				bom.RSNUM,					--预留号
				bom.RSPOS,					--预留行号
				'',							--工艺顺序号
				bom.ZOPID,					--工位ID
				bom.ZOPDS,					--工序描述
				bom.AUFNR,					--生产单号
				@CreateUserId,              --创建用户
				@CreateUserNm,              --创建用户名称
				@DateTimeNow,               --创建日期
				@CreateUserId,              --最后修改用户
				@CreateUserNm,              --最后修改用户名称
				@DateTimeNow,               --最后修改日期
				1,							--版本，1
				bom.ICHARG,					--批号
				bom.BWART,					--移动类型
				0,							--是否已经创建拉料单
				bom.DISPO,					--MRP控制者
				bom.PLNFL,					--序列
				bom.VORNR,					--操作
				bom.AUFPL,					--工艺路线编号
				bom.LGORT					--SAP库位
				from SAP_ProdBomDet as bom
				inner join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo 
					--用工艺路线编号、顺序和操作活动关联
					and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  
				--获取扣料库位
				left join CUST_ProdLineLocationMap as loc on bom.LGORT = loc.SapLocation and loc.ProdLine = @ProdLine
				where bom.BatchNo = @BatchNo and bom.ICHARG = substring(@AUFNR, 3, 10)
				--由于返工订单可能bom中会存在消耗地点为3000的整车物料，要将其过滤掉
				and (bom.LGORT <> '3000' or (bom.LGORT = '3000' and ISNULL(bom.ICHARG, '') = '')) 
				and routing.BatchNo = @BatchNo and routing.AUFNR = @AUFNR
			end
			else
			begin	--非试制车，bom.AUFNR = @AUFNR
				--新建生产单Bom
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
				IsCreateOrder,				--是否已经创建拉料单
				DISPO,						--MRP控制者
				PLNFL,						--序列
				VORNR,						--操作
				AUFPL,						--工艺路线编号
				LGORT						--SAP库位
				)
				select
				@OrderNo,					--生产单号
				4,							--类型，4生产单
				0,							--子类型，0正常
				@OrderDetId,				--生产单明细标识
				1,							--生产单明细顺序号
				ROW_NUMBER() over (order by bom.Id),--顺序号
				bom.MATERIAL,				--Bom零件号
				bom.MAKTX,					--Bom零件描述
				bom.BISMT,					--旧物料号
				bom.MEINS,					--单位
				bom.MEINS,					--基本单位
				Case when bom.LIFNR <> '' then bom.LIFNR else null end,					--指定供应商
				Dense_rank() over (order by routing.AUFPL asc, routing.PLNFL asc, routing.VORNR asc),--工序
				bom.GW,						--工位
				case when bom.RGEKZ = 'X' then (case when bom.BWART = '531' then -bom.MDMNG else bom.MDMNG end) else 0 end,					--Bom用量
				0,							--反冲合格数量
				0,							--反冲不合格数量
				0,							--反冲废品数量
				1,							--单位用量
				case when bom.RGEKZ = 'X' then (convert(decimal(18, 8), (Case when bom.BWART = '531' then -bom.MDMNG else bom.MDMNG end / @GAMNG))) else 0 end,--单个成品用量
				ISNULL(loc.Location, @ProdLineLocFrom),--反冲库位
				0,							--是否打印
				0,							--回冲方式
				0,							--投料方式
				0,							--是否自动投料
				0,							--是否关键件
				@DateTimeNow,				--预计消耗时间
				bom.RSNUM,					--预留号
				bom.RSPOS,					--预留行号
				'',							--工艺顺序号
				bom.ZOPID,					--工位ID
				bom.ZOPDS,					--工序描述
				bom.AUFNR,					--生产单号
				@CreateUserId,              --创建用户
				@CreateUserNm,              --创建用户名称
				@DateTimeNow,               --创建日期
				@CreateUserId,              --最后修改用户
				@CreateUserNm,              --最后修改用户名称
				@DateTimeNow,               --最后修改日期
				1,							--版本，1
				bom.ICHARG,					--批号
				bom.BWART,					--移动类型
				0,							--是否已经创建拉料单
				bom.DISPO,					--MRP控制者
				bom.PLNFL,					--序列
				bom.VORNR,					--操作
				bom.AUFPL,					--工艺路线编号
				bom.LGORT					--SAP库位
				from SAP_ProdBomDet as bom
				inner join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo 
					--用工艺路线编号、顺序和操作活动关联
					and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  
				--获取扣料库位
				left join CUST_ProdLineLocationMap as loc on bom.LGORT = loc.SapLocation and loc.ProdLine = @ProdLine
				where bom.BatchNo = @BatchNo and bom.AUFNR = @AUFNR
			    --由于返工订单可能bom中会存在消耗地点为3000的整车物料，要将其过滤掉
				and (bom.LGORT <> '3000' or (bom.LGORT = '3000' and ISNULL(bom.ICHARG, '') = '')) 
				and routing.BatchNo = @BatchNo and routing.AUFNR = @AUFNR
			end	
			
			--新增工序
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
			AUFPL,						--工艺路线编号
			PLNFL,						--顺序
			VORNR,						--操作活动
			NeedReport,					--是否报工工序
			IsRecFG						--是否收货工序					
			)
			select
			@OrderNo,					--生产单号
			@OrderDetId,				--生产单明细标识
			Dense_rank() over (order by AUFPL asc, PLNFL asc, VORNR asc),--工序
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
			ARBPL,						--工作中心
			0,							--是否自动报工
			0,							--报工数量
			0,							--成品物料反冲数量
			0,							--废品数量
			AUFPL,						--工艺路线编号
			PLNFL,						--顺序
			VORNR,						--操作活动
			CASE WHEN RUEK = '1' or RUEK = '2' THEN 1 ELSE 0 END,	--是否报工工序
			CASE WHEN AUTWE = 'X' THEN 1 ELSE 0 END	--是否收货工序
			from SAP_ProdRoutingDet
			where BatchNo = @BatchNo and AUFNR = @AUFNR
			
			set @Msg = N'SAP生产单' + @AUFNR + N'创建成功，LES生产线为' + @ProdLine + N'，生产订单号为' + @OrderNo + N'。'
			insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 0, @Msg)
				
			set @NewOrderRowId = @NewOrderRowId + 1
		end
		
		insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 0, '结束创建生产订单')
		-----------------------------↑新增生产单-----------------------------
		
		
		
		
		-----------------------------↓更新生产单-----------------------------
		Declare @ExistOrderRowId int
		Declare @MaxExistOrderRowId int
		insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 0, '开始更新生产订单')
		select 	@ExistOrderRowId = MIN(RowId), @MaxExistOrderRowId = MAX(RowId) from #tempExistOrder
		while (@ExistOrderRowId <= @MaxExistOrderRowId)
		begin
			select @AUFNR = AUFNR,@DAUAT = DAUAT from #tempExistOrder where RowId = @ExistOrderRowId
			select @OrderNo = OrderNo, @ProdLine = Flow, @ProdLineLocFrom = LocFrom from ORD_OrderMstr_4 where ExtOrderNo = @AUFNR
			select top 1 @OrderDetId = Id, @GAMNG = OrderQty from ORD_OrderDet_4 where OrderNo = @OrderNo
			set @Msg='开始更新生产订单'+@OrderNo
			insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 0, @Msg)
			if exists(select top 1 * from ORD_OrderOpReport where OrderNo = @OrderNo and [Status] = 0)
			begin
				set @Msg = N'SAP生产单' + @AUFNR + N'已经报工不能更新。'
				insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 2, @Msg)
				
				set @ExistOrderRowId = @ExistOrderRowId + 1
				continue
			end
			
			if exists(select top 1 1 from SAP_ProdBomDet as bom
				left join SAP_ProdRoutingDet as routing on bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR and routing.BatchNo = @BatchNo and routing.AUFNR = @AUFNR
				where bom.BatchNo = @BatchNo and bom.AUFNR = @AUFNR and routing.BatchNo is null) 
			begin
				insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) 
				select @BatchNo, 2, N'SAP生产单' + @AUFNR + N'的Bom(AUFPL:' + CONVERT(varchar, bom.AUFPL) + N', PLNFL:' + bom.PLNFL + N', VORNR:' + bom.VORNR + N')没有找到对应的SAP工序。' from SAP_ProdBomDet as bom
				left join SAP_ProdRoutingDet as routing on bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR and routing.BatchNo = @BatchNo and routing.AUFNR = @AUFNR
				where bom.BatchNo = @BatchNo and bom.AUFNR = @AUFNR and routing.BatchNo is null
				
				set @ExistOrderRowId = @ExistOrderRowId + 1
				continue
			end
			
			if exists(select top 1 1 from SAP_ProdBomDet as bom
						inner join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo 
							--用工艺路线编号、顺序和操作活动关联
							and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  
						--获取扣料库位
						left join CUST_ProdLineLocationMap as loc on bom.LGORT = loc.SapLocation and loc.ProdLine = @ProdLine
						where bom.BatchNo = @BatchNo and bom.AUFNR = @AUFNR 
						--由于返工订单可能bom中会存在消耗地点为3000的整车物料，要将其过滤掉
				        and (bom.LGORT <> '3000' or (bom.LGORT = '3000' and ISNULL(bom.ICHARG, '') = '')) 
						and routing.BatchNo = @BatchNo and routing.AUFNR = @AUFNR and loc.Id is null) 
			begin
				insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) 
				select @BatchNo, 2, N'SAP生产单' + @AUFNR + N'的生产线' + bom.LGORT + N'SAP库位' + @ProdLine + N'没有匹配到对应的LES库位。' from SAP_ProdBomDet as bom
						inner join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo 
							--用工艺路线编号、顺序和操作活动关联
							and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  
						--获取扣料库位
						left join CUST_ProdLineLocationMap as loc on bom.LGORT = loc.SapLocation and loc.ProdLine = @ProdLine
						where bom.BatchNo = @BatchNo and bom.AUFNR = @AUFNR
					    --由于返工订单可能bom中会存在消耗地点为3000的整车物料，要将其过滤掉
				        and (bom.LGORT <> '3000' or (bom.LGORT = '3000' and ISNULL(bom.ICHARG, '') = ''))  
						and routing.BatchNo = @BatchNo and routing.AUFNR = @AUFNR and loc.Id is null
						
				set @ExistOrderRowId = @ExistOrderRowId + 1
				continue
			end
			
			--删除原生产单Bom
			delete from ORD_OrderBomDet where OrderNo = @OrderNo
			
			if @DAUAT in ('ZP01', 'ZP02')
			begin  --试制车，bom.ICHARG = @AUFNR
				--新建生产单Bom
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
				IsCreateOrder,				--是否已经创建拉料单
				DISPO,						--MRP控制者
				PLNFL,						--序列
				VORNR,						--操作
				AUFPL,						--工艺路线编号
				LGORT						--SAP库位
				)
				select
				@OrderNo,					--生产单号
				4,							--类型，4生产单
				0,							--子类型，0正常
				@OrderDetId,				--生产单明细标识
				1,							--生产单明细顺序号
				ROW_NUMBER() over (order by bom.Id),--顺序号
				bom.MATERIAL,				--Bom零件号
				bom.MAKTX,					--Bom零件描述
				bom.BISMT,					--旧物料号
				bom.MEINS,					--单位
				bom.MEINS,					--基本单位
				Case when bom.LIFNR <> '' then bom.LIFNR else null end,		--指定供应商
				Dense_rank() over (order by routing.AUFPL asc, routing.PLNFL asc, routing.VORNR asc),--工序
				bom.GW,						--工位
				case when bom.RGEKZ = 'X' then (Case when bom.BWART = '531' then -bom.MDMNG else bom.MDMNG end) else 0 end,					--Bom用量
				0,							--反冲合格数量
				0,							--反冲不合格数量
				0,							--反冲废品数量
				1,							--单位用量
				case when bom.RGEKZ = 'X' then (convert(decimal(18, 8), (Case when bom.BWART = '531' then -bom.MDMNG else bom.MDMNG end / @GAMNG))) else 0 end,--单个成品用量
				ISNULL(loc.Location, @ProdLineLocFrom),--反冲库位
				0,							--是否打印
				0,							--回冲方式
				0,							--投料方式
				0,							--是否自动投料
				0,							--是否关键件
				@DateTimeNow,				--预计消耗时间
				bom.RSNUM,					--预留号
				bom.RSPOS,					--预留行号
				'',							--工艺顺序号
				bom.ZOPID,					--工位ID
				bom.ZOPDS,					--工序描述
				bom.AUFNR,					--生产单号
				@CreateUserId,              --创建用户
				@CreateUserNm,              --创建用户名称
				@DateTimeNow,               --创建日期
				@CreateUserId,              --最后修改用户
				@CreateUserNm,              --最后修改用户名称
				@DateTimeNow,               --最后修改日期
				1,							--版本，1
				bom.ICHARG,					--批号
				bom.BWART,					--移动类型
				0,							--是否已经创建拉料单
				bom.DISPO,					--MRP控制者
				bom.PLNFL,					--序列
				bom.VORNR,					--操作
				bom.AUFPL,					--工艺路线编号
				bom.LGORT					--SAP库位
				from SAP_ProdBomDet as bom
				inner join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo 
					--用工艺路线编号、顺序和操作活动关联
					and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  
				--获取扣料库位
				left join CUST_ProdLineLocationMap as loc on bom.LGORT = loc.SapLocation and loc.ProdLine = @ProdLine
				where bom.BatchNo = @BatchNo and bom.ICHARG = substring(@AUFNR, 3, 10)  --ICHARG是10位，@AUFNR是12位，去掉@AUFNR头2为进行比较
			    --由于返工订单可能bom中会存在消耗地点为3000的整车物料，要将其过滤掉
				and (bom.LGORT <> '3000' or (bom.LGORT = '3000' and ISNULL(bom.ICHARG, '') = '')) 
				and routing.BatchNo = @BatchNo and routing.AUFNR = @AUFNR
			end
			else
			begin  --非试制车，bom.AUFNR = @AUFNR
				--新建生产单Bom
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
				IsCreateOrder,				--是否已经创建拉料单
				DISPO,						--MRP控制者
				PLNFL,						--序列
				VORNR,						--操作
				AUFPL,						--工艺路线编号
				LGORT						--SAP库位
				)
				select
				@OrderNo,					--生产单号
				4,							--类型，4生产单
				0,							--子类型，0正常
				@OrderDetId,				--生产单明细标识
				1,							--生产单明细顺序号
				ROW_NUMBER() over (order by bom.Id),--顺序号
				bom.MATERIAL,				--Bom零件号
				bom.MAKTX,					--Bom零件描述
				bom.BISMT,					--旧物料号
				bom.MEINS,					--单位
				bom.MEINS,					--基本单位
				Case when bom.LIFNR <> '' then bom.LIFNR else null end,		--指定供应商
				Dense_rank() over (order by routing.AUFPL asc, routing.PLNFL asc, routing.VORNR asc),--工序
				bom.GW,						--工位
				case when bom.RGEKZ = 'X' then (Case when bom.BWART = '531' then -bom.MDMNG else bom.MDMNG end) else 0 end,					--Bom用量
				0,							--反冲合格数量
				0,							--反冲不合格数量
				0,							--反冲废品数量
				1,							--单位用量
				case when bom.RGEKZ = 'X' then (convert(decimal(18, 8), (Case when bom.BWART = '531' then -bom.MDMNG else bom.MDMNG end / @GAMNG))) else 0 end,--单个成品用量
				ISNULL(loc.Location, @ProdLineLocFrom),--反冲库位
				0,							--是否打印
				0,							--回冲方式
				0,							--投料方式
				0,							--是否自动投料
				0,							--是否关键件
				@DateTimeNow,				--预计消耗时间
				bom.RSNUM,					--预留号
				bom.RSPOS,					--预留行号
				'',							--工艺顺序号
				bom.ZOPID,					--工位ID
				bom.ZOPDS,					--工序描述
				bom.AUFNR,					--生产单号
				@CreateUserId,              --创建用户
				@CreateUserNm,              --创建用户名称
				@DateTimeNow,               --创建日期
				@CreateUserId,              --最后修改用户
				@CreateUserNm,              --最后修改用户名称
				@DateTimeNow,               --最后修改日期
				1,							--版本，1
				bom.ICHARG,					--批号
				bom.BWART,					--移动类型
				0,							--是否已经创建拉料单
				bom.DISPO,					--MRP控制者
				bom.PLNFL,					--序列
				bom.VORNR,					--操作
				bom.AUFPL,					--工艺路线编号
				bom.LGORT					--SAP库位
				from SAP_ProdBomDet as bom
				inner join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo 
					--用工艺路线编号、顺序和操作活动关联
					and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  
				--获取扣料库位
				left join CUST_ProdLineLocationMap as loc on bom.LGORT = loc.SapLocation and loc.ProdLine = @ProdLine
				where bom.BatchNo = @BatchNo and bom.AUFNR = @AUFNR 
			    --由于返工订单可能bom中会存在消耗地点为3000的整车物料，要将其过滤掉
				and (bom.LGORT <> '3000' or (bom.LGORT = '3000' and ISNULL(bom.ICHARG, '') = '')) 
				and routing.BatchNo = @BatchNo and routing.AUFNR = @AUFNR
			end
			
			
			set @Msg = N'SAP生产单' + @AUFNR + N'更新成功，LES生产线为' + @ProdLine + N'，生产订单号为' + @OrderNo + N'。'
			insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 0, @Msg)
			
			set @ExistOrderRowId = @ExistOrderRowId + 1
		end
		insert into LOG_GenProductOrder(BatchNo, Lvl, Msg) values(@BatchNo, 0, '结束更新生产订单')
		-----------------------------↑更新生产单-----------------------------
		
		
		
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
       
		set @Msg = Error_Message() 
		RAISERROR(@Msg, 16, 1) 
	end catch 
END 


