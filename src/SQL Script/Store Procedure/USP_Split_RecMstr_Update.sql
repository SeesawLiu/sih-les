SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Split_RecMstr_Update')
	DROP PROCEDURE USP_Split_RecMstr_Update
GO

CREATE PROCEDURE USP_Split_RecMstr_Update
(
	@Version int,
	@ExtRecNo varchar(50),
	@IpNo varchar(50),
	@SeqNo varchar(50),
	@Flow varchar(50),
	@Status tinyint,
	@Type tinyint,
	@OrderType tinyint,
	@OrderSubType tinyint,
	@QualityType tinyint,
	@PartyFrom varchar(50),
	@PartyFromNm varchar(100),
	@PartyTo varchar(50),
	@PartyToNm varchar(100),
	@ShipFrom varchar(50),
	@ShipFromAddr varchar(256),
	@ShipFromTel varchar(50),
	@ShipFromCell varchar(50),
	@ShipFromFax varchar(50),
	@ShipFromContact varchar(50),
	@ShipTo varchar(50),
	@ShipToAddr varchar(256),
	@ShipToTel varchar(50),
	@ShipToCell varchar(50),
	@ShipToFax varchar(50),
	@ShipToContact varchar(50),
	@Dock varchar(100),
	@EffDate datetime,
	@IsPrintRec bit,
	@IsRecPrinted bit,
	@CreateHuOpt tinyint,
	@IsRecScanHu bit,
	@RecTemplate varchar(100),
	@WMSNo varchar(50),
	@CancelReason varchar(256),
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime,
	@IsCheckPartyFromAuth bit,
	@IsCheckPartyToAuth bit,
	@RecNo varchar(4000),
	@VersionBerfore int
)
AS
BEGIN
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameters nvarchar(4000)
	SET @Statement='UPDATE ORD_RecMstr_' + CONVERT(varchar, @OrderType) + ' SET Version=@Version_1,ExtRecNo=@ExtRecNo_1,IpNo=@IpNo_1,SeqNo=@SeqNo_1,Flow=@Flow_1,Status=@Status_1,Type=@Type_1,OrderType=@OrderType_1,OrderSubType=@OrderSubType_1,QualityType=@QualityType_1,PartyFrom=@PartyFrom_1,PartyFromNm=@PartyFromNm_1,PartyTo=@PartyTo_1,PartyToNm=@PartyToNm_1,ShipFrom=@ShipFrom_1,ShipFromAddr=@ShipFromAddr_1,ShipFromTel=@ShipFromTel_1,ShipFromCell=@ShipFromCell_1,ShipFromFax=@ShipFromFax_1,ShipFromContact=@ShipFromContact_1,ShipTo=@ShipTo_1,ShipToAddr=@ShipToAddr_1,ShipToTel=@ShipToTel_1,ShipToCell=@ShipToCell_1,ShipToFax=@ShipToFax_1,ShipToContact=@ShipToContact_1,Dock=@Dock_1,EffDate=@EffDate_1,IsPrintRec=@IsPrintRec_1,IsRecPrinted=@IsRecPrinted_1,CreateHuOpt=@CreateHuOpt_1,IsRecScanHu=@IsRecScanHu_1,RecTemplate=@RecTemplate_1,WMSNo=@WMSNo_1,CancelReason=@CancelReason_1,LastModifyUser=@LastModifyUser_1,LastModifyUserNm=@LastModifyUserNm_1,LastModifyDate=@LastModifyDate_1,IsCheckPartyFromAuth=@IsCheckPartyFromAuth_1,IsCheckPartyToAuth=@IsCheckPartyToAuth_1 WHERE RecNo=@RecNo_1 AND Version=@VersionBerfore_1'
	SET @Parameters='@Version_1 int, @ExtRecNo_1 varchar(50), @IpNo_1 varchar(50), @SeqNo_1 varchar(50), @Flow_1 varchar(50), @Status_1 tinyint, @Type_1 tinyint, @OrderType_1 tinyint, @OrderSubType_1 tinyint, @QualityType_1 tinyint, @PartyFrom_1 varchar(50), @PartyFromNm_1 varchar(100), @PartyTo_1 varchar(50), @PartyToNm_1 varchar(100), @ShipFrom_1 varchar(50), @ShipFromAddr_1 varchar(256), @ShipFromTel_1 varchar(50), @ShipFromCell_1 varchar(50), @ShipFromFax_1 varchar(50), @ShipFromContact_1 varchar(50), @ShipTo_1 varchar(50), @ShipToAddr_1 varchar(256), @ShipToTel_1 varchar(50), @ShipToCell_1 varchar(50), @ShipToFax_1 varchar(50), @ShipToContact_1 varchar(50), @Dock_1 varchar(100), @EffDate_1 datetime, @IsPrintRec_1 bit, @IsRecPrinted_1 bit, @CreateHuOpt_1 tinyint, @IsRecScanHu_1 bit, @RecTemplate_1 varchar(100), @WMSNo_1 varchar(50), @CancelReason_1 varchar(256), @LastModifyUser_1 int, @LastModifyUserNm_1 varchar(100), @LastModifyDate_1 datetime, @IsCheckPartyFromAuth_1 bit, @IsCheckPartyToAuth_1 bit, @RecNo_1 varchar(4000), @VersionBerfore_1 int'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@Version_1=@Version,@ExtRecNo_1=@ExtRecNo,@IpNo_1=@IpNo,@SeqNo_1=@SeqNo,@Flow_1=@Flow,@Status_1=@Status,@Type_1=@Type,@OrderType_1=@OrderType,@OrderSubType_1=@OrderSubType,@QualityType_1=@QualityType,@PartyFrom_1=@PartyFrom,@PartyFromNm_1=@PartyFromNm,@PartyTo_1=@PartyTo,@PartyToNm_1=@PartyToNm,@ShipFrom_1=@ShipFrom,@ShipFromAddr_1=@ShipFromAddr,@ShipFromTel_1=@ShipFromTel,@ShipFromCell_1=@ShipFromCell,@ShipFromFax_1=@ShipFromFax,@ShipFromContact_1=@ShipFromContact,@ShipTo_1=@ShipTo,@ShipToAddr_1=@ShipToAddr,@ShipToTel_1=@ShipToTel,@ShipToCell_1=@ShipToCell,@ShipToFax_1=@ShipToFax,@ShipToContact_1=@ShipToContact,@Dock_1=@Dock,@EffDate_1=@EffDate,@IsPrintRec_1=@IsPrintRec,@IsRecPrinted_1=@IsRecPrinted,@CreateHuOpt_1=@CreateHuOpt,@IsRecScanHu_1=@IsRecScanHu,@RecTemplate_1=@RecTemplate,@WMSNo_1=@WMSNo,@CancelReason_1=@CancelReason,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,@LastModifyDate_1=@LastModifyDate,@IsCheckPartyFromAuth_1=@IsCheckPartyFromAuth,@IsCheckPartyToAuth_1=@IsCheckPartyToAuth,@RecNo_1=@RecNo,@VersionBerfore_1=@VersionBerfore
END
GO
