SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_UpdateVanProdOrder') 
     DROP PROCEDURE USP_Busi_UpdateVanProdOrder
GO

CREATE PROCEDURE [dbo].[USP_Busi_UpdateVanProdOrder] 
(
	@BatchNo int,
	@CreateUserId int,
	@CreateUserNm varchar(50)
) --WITH ENCRYPTION
AS 
BEGIN 
	set nocount on
	declare @DateTimeNow datetime = GetDate()
	declare @ErrorMsg nvarchar(MAX)
	declare @trancount int = @@trancount
		
	begin try
		if @trancount = 0
		begin
            begin tran
        end
		
		-----------------------------↓数据准备-----------------------------
		declare @SapProdLine varchar(50)  --SAP生产线代码A/B
		declare @SapOrderNo varchar(50)   --SAP整车生产单（总装的生产单号，不是底盘的生产单号）
		declare @SapVAN varchar(50)       --VAN号
		declare @SapVersion varchar(50)   --区分总装还是特装
		declare @CabProdLine varchar(50)  --LES驾驶室生产线代码
		declare @ChassisProdLine varchar(50)  --LES底盘生产线代码
		declare @AssemblyProdLine varchar(50)  --LES总装生产线代码
		declare @SpecialProdLine varchar(50)  --LES特装生产线代码
		declare @CheckProdLine varchar(50)  --LES检测生产线代码
		declare @VHVIN varchar(50)
		declare @ZENGINE varchar(50)
		
		--查找生产单记录
		select @SapProdLine = ZLINE, @SapOrderNo = AUFNR, @SapVAN = CHARG, @SapVersion=[VERSION],
		@VHVIN = VHVIN, @ZENGINE = ZENGINE
		from SAP_ProdOrder where BatchNo = @BatchNo
		
		select @CabProdLine = CabProdLine, @ChassisProdLine = ChassisProdLine, 
		@AssemblyProdLine = AssemblyProdLine, @SpecialProdLine = SpecialProdLine,
		@CheckProdLine = CheckProdLine
		from CUST_ProductLineMap where SapProdLine = @SapProdLine and [Type] = 1
		-----------------------------↑数据准备-----------------------------
		
		
		
		
		-----------------------------↓生成驾驶室生产单-----------------------------
		declare @CabOrderNo varchar(50)
		exec USP_Busi_CreateOrUpdateVanProdOrder @BatchNo, @CabProdLine, N'驾驶室生产线', null, @CreateUserId, @CreateUserNm, @CabOrderNo output
		-----------------------------↑生成驾驶室生产单-----------------------------
		
		
		
		
		-----------------------------↓生成底盘生产单-----------------------------
		declare @ChassisOrderNo varchar(50)
		exec USP_Busi_CreateOrUpdateVanProdOrder @BatchNo, @ChassisProdLine, N'底盘生产线', null, @CreateUserId, @CreateUserNm, @ChassisOrderNo output
		-----------------------------↑生成底盘生产单-----------------------------
		
		
		
		if @SapVersion <> '0002'
		begin
		-----------------------------↓生成总装生产单-----------------------------
		declare @AssemblyOrderNo varchar(50)
		exec USP_Busi_CreateOrUpdateVanProdOrder @BatchNo, @AssemblyProdLine, N'总装生产线', @SpecialProdLine, @CreateUserId, @CreateUserNm, @AssemblyOrderNo output
		-----------------------------↑生成总装生产单-----------------------------
		end
		
		else
		
		begin
		-----------------------------↓生成特装生产单-----------------------------
		declare @SpecialOrderNo varchar(50)
		exec USP_Busi_CreateOrUpdateVanProdOrder @BatchNo, @SpecialProdLine, N'特装生产线', @AssemblyProdLine, @CreateUserId, @CreateUserNm, @SpecialOrderNo output
		-----------------------------↑生成特装生产单-----------------------------
		end
		
		
		
		-----------------------------↓生成检测生产单-----------------------------
		declare @CheckOrderNo varchar(50)
		exec USP_Busi_CreateOrUpdateVanProdOrder @BatchNo, @CheckProdLine, N'检测生产线', null, @CreateUserId, @CreateUserNm, @CheckOrderNo output
		-----------------------------↑生成检测生产单-----------------------------
		
		if not exists(select top 1 1 from CUST_EngineTrace where TraceCode = @SapVAN)
		begin
			INSERT INTO CUST_EngineTrace(TraceCode, VHVIN, ZENGINE, CreateDate, CreateUser, CreateUserNm, LastModifyDate, LastModifyUser, LastModifyUserNm)
			values(@SapVAN, @VHVIN, @ZENGINE, @DateTimeNow, @CreateUserId, @CreateUserNm,  @DateTimeNow, @CreateUserId, @CreateUserNm)
		end
		else
		begin
			update CUST_EngineTrace set VHVIN = @VHVIN, ZENGINE = @ZENGINE, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm 
			where TraceCode = @SapVAN
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
		
		set @ErrorMsg = Error_Message() 
		RAISERROR(@ErrorMsg, 16, 1) 
	end catch 
END 
