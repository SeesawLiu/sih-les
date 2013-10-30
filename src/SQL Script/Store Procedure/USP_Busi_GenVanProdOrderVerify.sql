SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_GenVanProdOrderVerify') 
     DROP PROCEDURE USP_Busi_GenVanProdOrderVerify
GO

CREATE PROCEDURE [dbo].[USP_Busi_GenVanProdOrderVerify] 
(
	@BatchNo int,
	@IsUpdate bit	
) --WITH ENCRYPTION
AS 
BEGIN 
	set nocount on
	declare @ErrorMsg nvarchar(MAX)
	
	Create table #tempWC
	(
		VanProdLine varchar(50),
		VanRouting varchar(50),
		WorkCenter varchar(50),
	)
	
	declare @SapProdLine varchar(50)  --SAP生产线代码A/B
	declare @SapOrderNo varchar(50)   --SAP整车生产单（总装的生产单号，不是底盘的生产单号）
	declare @SapVAN varchar(50)       --VAN号
	
	declare @CabProdLine varchar(50)  --LES驾驶室生产线代码
	declare @CabProdLineRegion varchar(50)
	declare @CabLocFrom varchar(50)
	declare @CabLocTo varchar(50)
	declare @CabRouting varchar(50)
	declare @IsCabProdLineActive bit
	declare @CabProdLineType tinyint
	declare @CabTaktTime int
	declare @CabVirtualOpRef varchar(50)
	
	declare @ChassisProdLine varchar(50)  --LES底盘生产线代码
	declare @ChassisProdLineRegion varchar(50)
	declare @ChassisLocFrom varchar(50)
	declare @ChassisLocTo varchar(50)
	declare @ChassisRouting varchar(50)
	declare @IsChassisProdLineActive bit
	declare @ChassisProdLineType tinyint
	declare @ChassisTaktTime int
	declare @ChassisVirtualOpRef varchar(50)
	
	declare @AssemblyProdLine varchar(50)  --LES总装生产线代码
	declare @AssemblyProdLineRegion varchar(50)
	declare @AssemblyLocFrom varchar(50)
	declare @AssemblyLocTo varchar(50)
	declare @AssemblyRouting varchar(50)
	declare @IsAssemblyProdLineActive bit
	declare @AssemblyProdLineType tinyint
	declare @AssemblyTaktTime int
	declare @AssemblyVirtualOpRef varchar(50)
	
	declare @SpecialProdLine varchar(50)  --LES特装生产线代码
	declare @SpecialProdLineRegion varchar(50)
	declare @SpecialLocFrom varchar(50)
	declare @SpecialLocTo varchar(50)
	declare @SpecialRouting varchar(50)
	declare @IsSpecialProdLineActive bit
	declare @SpecialProdLineType tinyint
	declare @SpecialTaktTime int
	declare @SpecialVirtualOpRef varchar(50)
	
	declare @CheckProdLine varchar(50)  --LES检测生产线代码
	declare @CheckProdLineRegion varchar(50)
	declare @CheckLocFrom varchar(50)
	declare @CheckLocTo varchar(50)
	declare @CheckRouting varchar(50)
	declare @IsCheckProdLineActive bit
	declare @CheckProdLineType tinyint
	declare @CheckTaktTime int
	declare @CheckVirtualOpRef varchar(50)
	
	declare @TransFlow varchar(50)  --LES变速器路线代码
	declare @SaddleFlow varchar(50)  --LES鞍座路线代码
	

	
	--查找生产单记录
	select @SapProdLine = ZLINE, @SapOrderNo = AUFNR, @SapVAN = CHARG from SAP_ProdOrder where BatchNo = @BatchNo
	
	--删除SAP订单日志
	delete from LOG_GenVanProdOrder where AUFNR = @SapOrderNo
	
	if ISNULL(@SapProdLine, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有批次号' + CONVERT(varchar, @BatchNo) + N'的整车生产单记录。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @BatchNo, @ErrorMsg)
		select Msg from LOG_GenVanProdOrder where AUFNR = @SapOrderNo
		return		
	end
	
	--查找生产线映射表，找出驾驶室、底盘、总装、特装生产线，变速器和鞍座路线代码
	if not exists(select top 1 1 from CUST_ProductLineMap where SapProdLine = @SapProdLine)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有维护整车生产线' + @SapProdLine + N'的映射关系。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @BatchNo, @ErrorMsg) 
		select Msg from LOG_GenVanProdOrder where AUFNR = @SapOrderNo
		return		
	end
	else
	begin
		select @CabProdLine = CabProdLine, @ChassisProdLine = ChassisProdLine, 
		@AssemblyProdLine = AssemblyProdLine, @SpecialProdLine = SpecialProdLine,
		@TransFlow = TransFlow, @SaddleFlow = SaddleFlow, @CheckProdLine = CheckProdLine
		from CUST_ProductLineMap where SapProdLine = @SapProdLine and [Type] = 1
	end
	
	if (@IsUpdate = 0)
	begin
		--校验SAP生产单是否导入
		if exists(select top 1 1 from ORD_OrderSeq where TraceCode = @SapVAN)
		begin
			set @ErrorMsg = N'整车生产线' + @SapProdLine + N' SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'已经导入。'
			insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @BatchNo, @ErrorMsg) 
			select Msg from LOG_GenVanProdOrder where AUFNR = @SapOrderNo
			return		
		end
	end
	else
	begin
		--校验SAP生产单是否导入
		if not exists(select top 1 1 from ORD_OrderSeq where TraceCode = @SapVAN)
		begin
			set @ErrorMsg = N'整车生产线' + @SapProdLine + N'没有找到整车生产单' + @SapVAN + N'。'
			insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @BatchNo, @ErrorMsg) 
			select Msg from LOG_GenVanProdOrder where AUFNR = @SapOrderNo
			return		
		end
		
		if exists(select top 1 1 from ORD_OrderMstr_4 as mstr
					inner join ORD_OrderOpReport as rp on mstr.OrderNo = rp.OrderNo
					 where mstr.TraceCode = @SapVAN)
		begin
			select top 1 @ErrorMsg = N'整车生产线' + @SapProdLine + N' SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'已经报工不能更新Bom。' from ORD_OrderMstr_4 where TraceCode = @SapVAN and Status in (3, 4)
			insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @BatchNo, @ErrorMsg) 
			select Msg from LOG_GenVanProdOrder where AUFNR = @SapOrderNo
			return		
		end
	end
	
	
	
	
	-----------------------------↓驾驶室数据校验-----------------------------
	if ISNULL(@CabProdLine, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有维护整车生产线' + @SapProdLine + N'映射关系中的驾驶室生产线。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CabProdLine, @BatchNo, @ErrorMsg)
	end
	
	if not exists(select top 1 1 from SCM_FlowMstr where Code = @CabProdLine)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有找到驾驶室生产线' + @CabProdLine + N'。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CabProdLine, @BatchNo, @ErrorMsg)
	end
	
	--获取生产线信息
	select @CabProdLineRegion = PartyFrom, @CabLocFrom = LocFrom, @CabLocTo = LocTo, @CabRouting = Routing, 
	@IsCabProdLineActive = IsActive, @CabProdLineType = ProdLineType, @CabTaktTime = TaktTime, @CabVirtualOpRef = VirtualOpRef
	from SCM_FlowMstr where Code = @CabProdLine
	
	if @IsCabProdLineActive = 0
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'驾驶室生产线' + @CabProdLine + N'没有生效。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CabProdLine, @BatchNo, @ErrorMsg)
	end
	
	if @CabTaktTime is null
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'驾驶室生产线' + @CabProdLine + N'没有设置节拍时间。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CabProdLine, @BatchNo, @ErrorMsg)
	end
	else if @CabTaktTime <= 0
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'驾驶室生产线' + @CabProdLine + N'的节拍时间不能小于等于0。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CabProdLine, @BatchNo, @ErrorMsg)
	end
	
	if ISNULL(@CabRouting, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有设置驾驶室生产线' + @CabProdLine + N'的工艺流程。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CabProdLine, @BatchNo, @ErrorMsg)
	end
	
	if ISNULL(@CabVirtualOpRef, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有设置驾驶室生产线' + @CabProdLine + N'的虚拟工位。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CabProdLine, @BatchNo, @ErrorMsg)
	end

	if not exists(select top 1 1 from PRD_ProdLineWorkCenter where Flow = @CabProdLine)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有找到驾驶室生产线' + @CabProdLine + N'的工作中心。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CabProdLine, @BatchNo, @ErrorMsg)
	end
	-----------------------------↑驾驶室数据校验-----------------------------
	
	
	
	
	-----------------------------↓底盘数据校验-----------------------------
	if ISNULL(@ChassisProdLine, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有维护整车生产线' + @SapProdLine + N'映射关系中的底盘生产线。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @ChassisProdLine, @BatchNo, @ErrorMsg)
	end
	
	if not exists(select top 1 1 from SCM_FlowMstr where Code = @ChassisProdLine)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有找到底盘生产线' + @ChassisProdLine + N'。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @ChassisProdLine, @BatchNo, @ErrorMsg)
	end
	
	--获取生产线信息
	select @ChassisProdLineRegion = PartyFrom, @ChassisLocFrom = LocFrom, @ChassisLocTo = LocTo, @ChassisRouting = Routing, 
	@IsChassisProdLineActive = IsActive, @ChassisProdLineType = ProdLineType, @ChassisTaktTime = TaktTime, @ChassisVirtualOpRef = VirtualOpRef
	from SCM_FlowMstr where Code = @ChassisProdLine
	
	if @IsChassisProdLineActive = 0
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'底盘生产线' + @ChassisProdLine + N'没有生效。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @ChassisProdLine, @BatchNo, @ErrorMsg)
	end
	
	if @ChassisTaktTime is null
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'底盘生产线' + @ChassisProdLine + N'没有设置节拍时间。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @ChassisProdLine, @BatchNo, @ErrorMsg)
	end
	else if @ChassisTaktTime <= 0
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'底盘生产线' + @ChassisProdLine + N'的节拍时间不能小于等于0。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @ChassisProdLine, @BatchNo, @ErrorMsg)
	end
	
	if ISNULL(@ChassisRouting, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有设置底盘生产线' + @ChassisProdLine + N'的工艺流程。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @ChassisProdLine, @BatchNo, @ErrorMsg)
	end
	
	if ISNULL(@ChassisVirtualOpRef, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有设置底盘生产线' + @ChassisProdLine + N'的虚拟工位。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @ChassisProdLine, @BatchNo, @ErrorMsg)
	end

	if not exists(select top 1 1 from PRD_ProdLineWorkCenter where Flow = @ChassisProdLine)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有找到底盘生产线' + @ChassisProdLine + N'的工作中心。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @ChassisProdLine, @BatchNo, @ErrorMsg)
	end
	-----------------------------↑底盘数据校验-----------------------------
	
	
	
	
	-----------------------------↓总装数据校验-----------------------------
	if ISNULL(@AssemblyProdLine, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有维护整车生产线' + @SapProdLine + N'映射关系中的总装生产线。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @AssemblyProdLine, @BatchNo, @ErrorMsg)
	end
	
	if not exists(select top 1 1 from SCM_FlowMstr where Code = @AssemblyProdLine)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有找到总装生产线' + @AssemblyProdLine + N'。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @AssemblyProdLine, @BatchNo, @ErrorMsg)
	end
	
	--获取生产线信息
	select @AssemblyProdLineRegion = PartyFrom, @AssemblyLocFrom = LocFrom, @AssemblyLocTo = LocTo, @AssemblyRouting = Routing, 
	@IsAssemblyProdLineActive = IsActive, @AssemblyProdLineType = ProdLineType, @AssemblyTaktTime = TaktTime, @AssemblyVirtualOpRef = VirtualOpRef
	from SCM_FlowMstr where Code = @AssemblyProdLine
	
	if @IsAssemblyProdLineActive = 0
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'总装生产线' + @AssemblyProdLine + N'没有生效。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @AssemblyProdLine, @BatchNo, @ErrorMsg)
	end
	
	if @AssemblyTaktTime is null
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'总装生产线' + @AssemblyProdLine + N'没有设置节拍时间。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @AssemblyProdLine, @BatchNo, @ErrorMsg)
	end
	else if @AssemblyTaktTime <= 0
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'总装生产线' + @AssemblyProdLine + N'的节拍时间不能小于等于0。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @AssemblyProdLine, @BatchNo, @ErrorMsg)
	end
	
	if ISNULL(@AssemblyRouting, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有设置总装生产线' + @AssemblyProdLine + N'的工艺流程。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @AssemblyProdLine, @BatchNo, @ErrorMsg)
	end
	
	if ISNULL(@AssemblyVirtualOpRef, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有设置总装生产线' + @AssemblyProdLine + N'的虚拟工位。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @AssemblyProdLine, @BatchNo, @ErrorMsg)
	end

	if not exists(select top 1 1 from PRD_ProdLineWorkCenter where Flow = @AssemblyProdLine)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有找到总装生产线' + @AssemblyProdLine + N'的工作中心。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @AssemblyProdLine, @BatchNo, @ErrorMsg)
	end
	-----------------------------↑总装数据校验-----------------------------
	
	
	
	
	-----------------------------↓特装数据校验-----------------------------
	if ISNULL(@SpecialProdLine, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有维护整车生产线' + @SapProdLine + N'映射关系中的特装生产线。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @SpecialProdLine, @BatchNo, @ErrorMsg)
	end
	
	if not exists(select top 1 1 from SCM_FlowMstr where Code = @SpecialProdLine)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有找到特装生产线' + @SpecialProdLine + N'。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @SpecialProdLine, @BatchNo, @ErrorMsg)
	end
	
	--获取生产线信息
	select @SpecialProdLineRegion = PartyFrom, @SpecialLocFrom = LocFrom, @SpecialLocTo = LocTo, @SpecialRouting = Routing, 
	@IsSpecialProdLineActive = IsActive, @SpecialProdLineType = ProdLineType, @SpecialTaktTime = TaktTime, @SpecialVirtualOpRef = VirtualOpRef
	from SCM_FlowMstr where Code = @SpecialProdLine
	
	if @IsSpecialProdLineActive = 0
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'特装生产线' + @SpecialProdLine + N'没有生效。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @SpecialProdLine, @BatchNo, @ErrorMsg)
	end
	
	if @SpecialTaktTime is null
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'特装生产线' + @SpecialProdLine + N'没有设置节拍时间。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @SpecialProdLine, @BatchNo, @ErrorMsg)
	end
	else if @SpecialTaktTime <= 0
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'特装生产线' + @SpecialProdLine + N'的节拍时间不能小于等于0。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @SpecialProdLine, @BatchNo, @ErrorMsg)
	end
	
	if ISNULL(@SpecialRouting, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有设置特装生产线' + @SpecialProdLine + N'的工艺流程。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @SpecialProdLine, @BatchNo, @ErrorMsg)
	end
	
	if ISNULL(@SpecialVirtualOpRef, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有设置特装生产线' + @SpecialProdLine + N'的虚拟工位。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @SpecialProdLine, @BatchNo, @ErrorMsg)
	end

	if not exists(select top 1 1 from PRD_ProdLineWorkCenter where Flow = @SpecialProdLine)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有找到特装生产线' + @SpecialProdLine + N'的工作中心。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @SpecialProdLine, @BatchNo, @ErrorMsg)
	end
	-----------------------------↑特装数据校验-----------------------------
	
	
	
	
	-----------------------------↓检测数据校验-----------------------------
	if ISNULL(@CheckProdLine, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有维护整车生产线' + @SapProdLine + N'映射关系中的检测生产线。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CheckProdLine, @BatchNo, @ErrorMsg)
	end
	
	if not exists(select top 1 1 from SCM_FlowMstr where Code = @CheckProdLine)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有找到检测生产线' + @CheckProdLine + N'。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CheckProdLine, @BatchNo, @ErrorMsg)
	end
	
	--获取生产线信息
	select @CheckProdLineRegion = PartyFrom, @CheckLocFrom = LocFrom, @CheckLocTo = LocTo, @CheckRouting = Routing, 
	@IsCheckProdLineActive = IsActive, @CheckProdLineType = ProdLineType, @CheckTaktTime = TaktTime, @CheckVirtualOpRef = VirtualOpRef
	from SCM_FlowMstr where Code = @CheckProdLine
	
	if @IsCheckProdLineActive = 0
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'检测生产线' + @CheckProdLine + N'没有生效。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CheckProdLine, @BatchNo, @ErrorMsg)
	end
	
	if @CheckTaktTime is null
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'检测生产线' + @CheckProdLine + N'没有设置节拍时间。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CheckProdLine, @BatchNo, @ErrorMsg)
	end
	else if @CheckTaktTime <= 0
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'检测生产线' + @CheckProdLine + N'的节拍时间不能小于等于0。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CheckProdLine, @BatchNo, @ErrorMsg)
	end
	
	if ISNULL(@CheckRouting, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有设置检测生产线' + @CheckProdLine + N'的工艺流程。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CheckProdLine, @BatchNo, @ErrorMsg)
	end
	
	if ISNULL(@CheckVirtualOpRef, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有设置检测生产线' + @CheckProdLine + N'的虚拟工位。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CheckProdLine, @BatchNo, @ErrorMsg)
	end

	if not exists(select top 1 1 from PRD_ProdLineWorkCenter where Flow = @CheckProdLine)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有找到检测生产线' + @CheckProdLine + N'的工作中心。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, ProdLine, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @CheckProdLine, @BatchNo, @ErrorMsg)
	end
	-----------------------------↑检测数据校验-----------------------------
	
	
	
	
	-----------------------------↓Bom和Workcenter匹配校验-----------------------------
	insert into #tempWC (VanProdLine, VanRouting, WorkCenter)
	select VanProdLine, VanRouting, WorkCenter
	from (
		select plm.CabProdLine as VanProdLine, @CabRouting as VanRouting, cabWC.WorkCenter 
		from CUST_ProductLineMap as plm 
		inner join PRD_ProdLineWorkCenter as cabWC on plm.CabProdLine = cabWC.Flow
		where SapProdLine = @SapProdLine
		union all
		select plm.ChassisProdLine as VanProdLine, @ChassisRouting as VanRouting, chaWC.WorkCenter 
		from CUST_ProductLineMap as plm 
		inner join PRD_ProdLineWorkCenter as chaWC on plm.ChassisProdLine = chaWC.Flow
		where SapProdLine = @SapProdLine
		union all
		select plm.AssemblyProdLine as VanProdLine, @AssemblyRouting as VanRouting, assWC.WorkCenter 
		from CUST_ProductLineMap as plm 
		inner join PRD_ProdLineWorkCenter as assWC on plm.AssemblyProdLine = assWC.Flow
		where SapProdLine = @SapProdLine
		union all
		select plm.SpecialProdLine as VanProdLine, @SpecialRouting as VanRouting, apeWC.WorkCenter 
		from CUST_ProductLineMap as plm 
		inner join PRD_ProdLineWorkCenter as apeWC on plm.SpecialProdLine = apeWC.Flow
		where SapProdLine = @SapProdLine
		union all
		select plm.CheckProdLine as VanProdLine, @CheckRouting as VanRouting, apeWC.WorkCenter 
		from CUST_ProductLineMap as plm 
		inner join PRD_ProdLineWorkCenter as apeWC on plm.CheckProdLine = apeWC.Flow
		where SapProdLine = @SapProdLine
	) as a
	
	--工作中心在多条整车生产线中存在校验
	insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) 
	select distinct @SapOrderNo, @SapProdLine, @BatchNo, N'整车生产线' + @SapProdLine + N' SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'工作中心' + WorkCenter + N'在多条整车生产线中存在。' 
	from #tempWC group by WorkCenter having COUNT(distinct VanProdLine) > 1
	
	--工作中心没有在整车生产线中维护
	insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) 
	select @SapOrderNo, @SapProdLine, @BatchNo, N'整车生产线' + @SapProdLine + N' SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'工作中心' + routing.ARBPL + N'没有在整车生产线中维护。'
	from SAP_ProdBomDet as bom
	inner join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo 
	and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  --用工艺路线编号、顺序和操作活动关联
	left join #tempWC as wc on routing.ARBPL = wc.WorkCenter
	where bom.BatchNo = @BatchNo and wc.WorkCenter is null
	group by routing.ARBPL
	-----------------------------↑Bom和Workcenter匹配校验-----------------------------
	
	
	
	
	-----------------------------↓RoutingDet和报工工序匹配校验-----------------------------
	insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) 
	select @SapOrderNo, @SapProdLine, @BatchNo, N'整车生产线' + @SapProdLine + N' SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN +  + N'报工工序的工作中心' + sapRouting.ARBPL + N'没有在整车工艺流程中维护。'
	from SAP_ProdRoutingDet as sapRouting 
	left join PRD_RoutingDet as routingDet on sapRouting.ARBPL = routingDet.WorkCenter 
	and routingDet.Routing in ((select wc.VanRouting from SAP_ProdBomDet as bom
	inner join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo 
	and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR  --用工艺路线编号、顺序和操作活动关联
	inner join #tempWC as wc on routing.ARBPL = wc.WorkCenter
	where bom.BatchNo = @BatchNo
	group by wc.VanRouting))
	where sapRouting.BatchNo = @BatchNo and sapRouting.RUEK in ('1', '2') and routingDet.Id is null
	
	drop table #tempWC
	-----------------------------↑RoutingDet和报工工序匹配校验-----------------------------
	
	
	
	
	-----------------------------↓整车生产单Bom和工艺流程匹配校验-----------------------------
	insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) 
	select @SapOrderNo, @SapProdLine, @BatchNo, N'整车生产单Bom(AUFPL:' + CONVERT(varchar, bom.AUFPL) + N', PLNFL:' + bom.PLNFL + N', VORNR:' + bom.VORNR + N')没有找到对应的SAP工序。' 
	from SAP_ProdBomDet as bom
	left join SAP_ProdRoutingDet as routing on bom.BatchNo = routing.BatchNo 
	and bom.AUFPL = routing.AUFPL and bom.PLNFL = routing.PLNFL and bom.VORNR = routing.VORNR
	where bom.BatchNo = @BatchNo and routing.BatchNo is null
	-----------------------------↑整车生产单Bom和工艺流程匹配校验-----------------------------
	
	
	
	
	-----------------------------↓变速器数据校验-----------------------------
	--没有维护整车生产线映射关系的变速器移库路线
	if ISNULL(@TransFlow, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有维护整车生产线' + @TransFlow + N'映射关系中的变速器移库路线。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @BatchNo, @ErrorMsg) 
	end
	
	--没有找到变速器移库路线的工作中心
	if not exists(select top 1 1 from PRD_ProdLineWorkCenter where Flow = @TransFlow)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有找到变速器移库路线' + @TransFlow + N'的工作中心。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @BatchNo, @ErrorMsg) 
	end
	-----------------------------↑变速器数据校验-----------------------------
	
	
	
	
	-----------------------------↓鞍座数据校验-----------------------------
	--没有维护整车生产线映射关系的鞍座移库路线
	if ISNULL(@SaddleFlow, '') = ''
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有维护整车生产线' + @SaddleFlow + N'映射关系中的鞍座移库路线。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @BatchNo, @ErrorMsg) 
	end
	
	--没有找到鞍座移库路线的工作中心
	if not exists(select top 1 1 from PRD_ProdLineWorkCenter where Flow = @SaddleFlow)
	begin
		set @ErrorMsg = N'SAP生产单号' + @SapOrderNo + N' Van号' + @SapVAN + N'没有找到鞍座移库路线' + @SaddleFlow + N'的工作中心。'
		insert into LOG_GenVanProdOrder(AUFNR, ZLINE, BatchNo, Msg) values (@SapOrderNo, @SapProdLine, @BatchNo, @ErrorMsg) 
	end
	-----------------------------↑鞍座数据校验-----------------------------
	
	
	select Msg from LOG_GenVanProdOrder where AUFNR = @SapOrderNo
END 
