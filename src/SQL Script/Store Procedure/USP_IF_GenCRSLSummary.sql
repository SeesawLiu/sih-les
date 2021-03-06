
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_IF_GenCRSLSummary') 
     DROP PROCEDURE USP_IF_GenCRSLSummary
GO

CREATE PROCEDURE [dbo].[USP_IF_GenCRSLSummary]
AS
BEGIN
	DECLARE @CurrentDate datetime=GETDATE()
	DECLARE @Trancount int
	DECLARE @Guid varchar(50)
	EXEC USP_SYS_GetNextSeq 'GenCRSLSummary',@Guid OUTPUT 
	SET @Trancount = @@TRANCOUNT
	BEGIN TRY
		IF @Trancount = 0
		BEGIN
			BEGIN TRAN
		END	
		SELECT * INTO #Temp_CRSL FROM SAP_CRSL WHERE Status=0 AND (BatchNo is null OR BatchNo='')
		
		INSERT INTO SAP_CRSLSummary(BatchNo, EINDT, FRBNR, LIFNR, MATNR, MENGE, 
		SGTXT, WERKS, EBELN, EBELP, MESSAGE, Status, ErrorCount, CreateDate, LastModifyDate)
		SELECT @Guid,EINDT,FRBNR,LIFNR,MATNR,SUM(MENGE) as MENGE,
		ROW_NUMBER()OVER(PARTITION BY FRBNR ORDER BY FRBNR,MATNR) as SGTXT,WERKS,'','','',0,0,@CurrentDate,@CurrentDate FROM #Temp_CRSL 
		GROUP BY EINDT,FRBNR,LIFNR,MATNR,WERKS
		
		UPDATE A SET A.BatchNo=@Guid FROM SAP_CRSL A INNER JOIN #Temp_CRSL B ON A.Id=B.Id 
		
		SELECT * FROM SAP_CRSLSummary WHERE Status in(0,2) AND ErrorCount<=3
				
		IF @Trancount = 0 
		BEGIN  
			COMMIT
		END
	END TRY
	BEGIN CATCH
		IF @Trancount = 0
		BEGIN
			ROLLBACK
		END 

		DECLARE @ErrorMsg varchar(max)
		SET @ErrorMsg=ERROR_MESSAGE()
		RAISERROR(@ErrorMsg,16,1)
	END CATCH			
END
