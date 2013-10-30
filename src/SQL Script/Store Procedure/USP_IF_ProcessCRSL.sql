



SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_IF_ProcessCRSL') 
     DROP PROCEDURE USP_IF_ProcessCRSL
GO

CREATE PROCEDURE [dbo].[USP_IF_ProcessCRSL]
(
	@ProcessBatchNo varchar(50)
)
AS
BEGIN
	DECLARE @CurrentDate datetime=GETDATE()
	DECLARE @Trancount int
	
	CREATE TABLE #Temp_CRSL(
		Id int PRIMARY KEY,
		EINDT varchar(50) NULL,
		FRBNR varchar(50) NULL,
		LIFNR varchar(50) NULL,
		MATNR varchar(50) NULL,
		MENGE decimal(18, 8) NULL,
		SGTXT varchar(50) NULL,
		WERKS varchar(50) NULL,
		EBELN varchar(50) NULL,
		EBELP varchar(50) NULL,
		ETENR varchar(50) NULL,
		MESSAGE varchar(256) NULL,
		OrderDetId int NULL,
		Status tinyint NULL,
		ErrorCount int NULL,
		CreateDate datetime NULL,
		LastModifyDate datetime NULL,
		PSTYP varchar(6) NULL,
		BatchNo varchar(50) NULL
	)
	
	SET @Trancount = @@TRANCOUNT
	BEGIN TRY
		IF @Trancount = 0
		BEGIN
			BEGIN TRAN
		END	
		
		SELECT * FROM SAP_CRSLSummary WHERE ProcessBatchNo=@ProcessBatchNo
		UPDATE c SET c.EBELN=cs.EBELN,c.EBELP=cs.EBELP,c.Status=cs.Status,c.MESSAGE=cs.MESSAGE,
			c.ErrorCount=cs.ErrorCount,c.LastModifyDate=@CurrentDate,c.PSTYP=CASE WHEN c.Status=1 THEN c.MESSAGE ELSE '' END
		OUTPUT inserted.* INTO #Temp_CRSL
		FROM SAP_CRSL c INNER JOIN SAP_CRSLSummary cs ON c.BatchNo=cs.BatchNo
			AND c.FRBNR=cs.FRBNR AND c.EINDT=cs.EINDT AND c.LIFNR=cs.LIFNR AND c.MATNR=cs.MATNR AND c.WERKS=cs.WERKS
		WHERE cs.ProcessBatchNo=@ProcessBatchNo
		
		UPDATE od SET od.ExtNo=c.EBELN,od.ExtSeq=c.EBELP,
			od.BillAddrDesc=c.MESSAGE,
			od.BillTerm=CASE WHEN c.Status=1 AND c.MESSAGE<>'' THEN 3 ELSE 1 END
		FROM ORD_OrderDet_1 od INNER JOIN #Temp_CRSL c ON od.Id=c.OrderDetId
		
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