SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Split_InvTrans_Insert')
	DROP PROCEDURE USP_Split_InvTrans_Insert
GO

CREATE PROCEDURE USP_Split_InvTrans_Insert
(
	@InvTransTable InvTransType Readonly,
	@InvLocTable InvLocType Readonly
)
AS
BEGIN
	

	IF EXISTS(SELECT TOP 1 1 FROM SAP_InvLoc as a
				inner join @InvLocTable as b on a.SourceId = b.SourceId and a.SourceType = b.SourceType and a.BWART = b.BWART
				where b.SourceId > 0)
	BEGIN
		RAISERROR('SourceId÷ÿ∏¥', 16, 1)
		RETURN
	END 	
	
	INSERT INTO SAP_InvLoc(SourceType, SourceId, FRBNR, SGTXT, CreateUser, CreateDate) 
	select SourceType, SourceId, FRBNR, SGTXT, CreateUser, CreateDate from @InvLocTable
	
	INSERT INTO SAP_InvTrans(TCODE, BWART, BLDAT, BUDAT, EBELN, EBELP, VBELN, POSNR, LIFNR, WERKS, LGORT, SOBKZ, MATNR, ERFMG, ERFME, UMLGO, GRUND, KOSTL, XBLNR, RSNUM, RSPOS, FRBNR, SGTXT, OLD, INSMK, XABLN, AUFNR, UMMAT, UMWRK, POSID, CreateDate, LastModifyDate, Status, ErrorCount, BatchNo, CHARG, KZEAR, ErrorId, OrderNo, DetailId, [Version]) 
	select TCODE, BWART, BLDAT, BUDAT, EBELN, EBELP, VBELN, POSNR, LIFNR, WERKS, LGORT, SOBKZ, MATNR, ERFMG, ERFME, UMLGO, GRUND, KOSTL, XBLNR, RSNUM, RSPOS, FRBNR, SGTXT, OLD, INSMK, XABLN, AUFNR, UMMAT, UMWRK, POSID, CreateDate, LastModifyDate, Status, ErrorCount, BatchNo, CHARG, KZEAR, ErrorId, OrderNo, DetailId, 1 from @InvTransTable
END
GO
