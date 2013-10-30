SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_SYS_GetNextId_Old') 
     DROP PROCEDURE USP_SYS_GetNextId_Old
GO 

CREATE PROCEDURE [dbo].[USP_SYS_GetNextId_Old] 
(
	@TablePrefix varchar(50), 
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
		
		IF EXISTS (SELECT * FROM SYS_TabIdSeq WITH (UPDLOCK, SERIALIZABLE) WHERE TabNm=@TablePrefix) 
		BEGIN 
			SELECT @NextId=Id+1 FROM SYS_TabIdSeq WHERE TabNm=@TablePrefix 
			UPDATE SYS_TabIdSeq SET Id=Id+1 WHERE TabNm=@TablePrefix 
		END 
		ELSE 
		BEGIN 
			INSERT INTO SYS_TabIdSeq(TabNm,Id) 
			VALUES(@TablePrefix,1) 
			SET @NextId=1 
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
