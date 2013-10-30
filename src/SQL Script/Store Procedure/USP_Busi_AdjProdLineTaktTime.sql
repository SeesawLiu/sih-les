SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_AdjProdLineTaktTime') 
     DROP PROCEDURE USP_Busi_AdjProdLineTaktTime
GO

CREATE PROCEDURE [dbo].[USP_Busi_AdjProdLineTaktTime] 
(
	@ProdLine varchar(50),
	@TaktTime int,
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
		
		declare @IsPause bit
		declare @ProdLineType tinyint
		
		select @IsPause = IsPause, @ProdLineType = ProdLineType from SCM_FlowMstr WITH (UPDLOCK,SERIALIZABLE) where Code = @ProdLine
		
		if @ProdLineType <>  1 and @ProdLineType <>  2 and @ProdLineType <>  3 and @ProdLineType <>  4
		begin
			set @ErrorMsg = N'生产线' + @ProdLine + N'不是整车生产线。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if @TaktTime <= 0
		begin
			set @ErrorMsg = N'节拍时间不能小于等于0。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		--调整生产线节拍
		update SCM_FlowMstr set TaktTime = @TaktTime, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm where Code = @ProdLine
		
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
	
	--begin try
	--	if @IsPause = 0
	--	begin
	--		--更新物料消耗时间
	--		exec USP_Busi_UpdateOrderBomConsumeTime @ProdLine, @CreateUserId, @CreateUserNm
	--	end
	--end try
	--begin catch
	--	set @ErrorMsg = Error_Message() 
	--	RAISERROR(@ErrorMsg, 16, 1) 
	--end catch 
END 
