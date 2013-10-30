SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_SYS_BatchGetNextId_Old') 
     DROP PROCEDURE USP_SYS_BatchGetNextId_Old 
GO

CREATE PROCEDURE [dbo].[USP_SYS_BatchGetNextId_Old] 
(
	@TablePrefix varchar(50), 
	@BatchSize int, 
	@NextId Bigint OUTPUT 
) --WITH ENCRYPTION
AS
BEGIN
	SET NOCOUNT ON 
	declare @trancount int = @@trancount
	
	begin try
		if @trancount = 0
		begin
			begin tran
		end
        
		IF EXISTS (SELECT * FROM SYS_TabIdSeq WITH (UPDLOCK,SERIALIZABLE) WHERE TabNm=@TablePrefix) 
		BEGIN 
			SELECT @NextId=Id+@BatchSize FROM SYS_TabIdSeq WHERE TabNm=@TablePrefix 
			UPDATE SYS_TabIdSeq SET Id=Id+@BatchSize WHERE TabNm=@TablePrefix 
		END 
		ELSE 
		BEGIN 
			INSERT INTO SYS_TabIdSeq(TabNm,Id) 
			VALUES(@TablePrefix,@BatchSize) 
			SET @NextId=@BatchSize 
		END 
	
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
        
		declare @ErrorMsg nvarchar(MAX) = Error_Message() 
		RAISERROR(@ErrorMsg, 16, 1) 
    end catch
END
