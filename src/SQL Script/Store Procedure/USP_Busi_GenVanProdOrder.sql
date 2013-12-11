SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_GenVanProdOrder') 
     DROP PROCEDURE USP_Busi_GenVanProdOrder
GO

CREATE PROCEDURE [dbo].[USP_Busi_GenVanProdOrder] 
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
		
		
		-----------------------------������׼��-----------------------------
		declare @SapProdLine varchar(50)  --SAP�����ߴ���A/B
		declare @SapOrderNo varchar(50)   --SAP��������������װ���������ţ����ǵ��̵��������ţ�
		declare @SapVAN varchar(50)       --VAN��
		declare @SapVersion varchar(50)   --������װ������װ
		declare @CabProdLine varchar(50)  --LES��ʻ�������ߴ���
		declare @ChassisProdLine varchar(50)  --LES���������ߴ���
		declare @AssemblyProdLine varchar(50)  --LES��װ�����ߴ���
		declare @SpecialProdLine varchar(50)  --LES��װ�����ߴ���
		declare @CheckProdLine varchar(50)  --LES��������ߴ���
		declare @CY_SEQNR bigint  --SAP����
		declare @VHVIN varchar(50)
		declare @ZENGINE varchar(50)

		--������������¼
		select @SapProdLine = ZLINE, @SapOrderNo = AUFNR, @SapVAN = CHARG, @SapVersion = [VERSION], @CY_SEQNR = CY_SEQNR,
		@VHVIN = VHVIN, @ZENGINE = ZENGINE
		from SAP_ProdOrder where BatchNo = @BatchNo
	
		select @CabProdLine = CabProdLine, @ChassisProdLine = ChassisProdLine, 
		@AssemblyProdLine = AssemblyProdLine, @SpecialProdLine = SpecialProdLine,
		@CheckProdLine = CheckProdLine
		from CUST_ProductLineMap where SapProdLine = @SapProdLine and [Type] = 1
		-----------------------------������׼��-----------------------------
		
		
		if @trancount = 0
		begin
            begin tran
        end
        
		--if not exists(select top 1 1 from ORD_OrderSeq where TraceCode = @SapVAN)
		--begin
			-----------------------------�����ɼ�ʻ��������-----------------------------
			declare @CabOrderNo varchar(50)
			exec USP_Busi_CreateOrUpdateVanProdOrder @BatchNo, @CabProdLine, N'��ʻ��������', null, @CreateUserId, @CreateUserNm, @CabOrderNo output
			-----------------------------�����ɼ�ʻ��������-----------------------------
			
			
			
			
			-----------------------------�����ɵ���������-----------------------------
			declare @ChassisOrderNo varchar(50)
			exec USP_Busi_CreateOrUpdateVanProdOrder @BatchNo, @ChassisProdLine, N'����������', null, @CreateUserId, @CreateUserNm, @ChassisOrderNo output
			-----------------------------�����ɵ���������-----------------------------
			
			
			
			if @SapVersion <> '0002'
			begin
			-----------------------------��������װ������-----------------------------
			declare @AssemblyOrderNo varchar(50)
			exec USP_Busi_CreateOrUpdateVanProdOrder @BatchNo, @AssemblyProdLine, N'��װ������', @SpecialProdLine, @CreateUserId, @CreateUserNm, @AssemblyOrderNo output
			-----------------------------��������װ������-----------------------------
			end
			
			else
			
			begin
			-----------------------------��������װ������-----------------------------
			declare @SpecialOrderNo varchar(50)
			exec USP_Busi_CreateOrUpdateVanProdOrder @BatchNo, @SpecialProdLine, N'��װ������', @AssemblyProdLine, @CreateUserId, @CreateUserNm, @SpecialOrderNo output
			-----------------------------��������װ������-----------------------------
			end
			
			
			
			-----------------------------�����ɼ��������-----------------------------
			declare @CheckOrderNo varchar(50)
			exec USP_Busi_CreateOrUpdateVanProdOrder @BatchNo, @CheckProdLine, N'���������', null, @CreateUserId, @CreateUserNm, @CheckOrderNo output
			-----------------------------�����ɼ��������-----------------------------
			
			
			
			update CUST_ProductLineMap set InitVanOrder = @SapOrderNo where SapProdLine = @SapProdLine and [Type] = 1 
			
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
		
		--end
		--else
		--begin
		--	set @ErrorMsg = N'����������' + @SapProdLine + N' SAP��������' + @SapOrderNo + N' Van��' + @SapVAN + N'�Ѿ����롣'
		--	RAISERROR(@ErrorMsg, 16, 1)
		--end
		if exists(select top 1 1 from ORD_OrderSeq where TraceCode = @SapVAN group by ProdLine having COUNT(1) > 1)
		begin
			set @ErrorMsg = N'����������' + @SapProdLine + N' SAP��������' + @SapOrderNo + N' Van��' + @SapVAN + N'�Ѿ����롣'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		--if exists(select ProdLine from ORD_OrderSeq where Seq = @CY_SEQNR and SubSeq = 1 group by ProdLine having COUNT(1) > 1)
		--begin
		--	set @ErrorMsg = N'����������' + @SapProdLine + N' SAP��������' + @SapOrderNo + N' Van��' + @SapVAN + N' SAP����' + CONVERT(varchar, @CY_SEQNR) + N'�ظ���'
		--	RAISERROR(@ErrorMsg, 16, 1)
		--end
		
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
