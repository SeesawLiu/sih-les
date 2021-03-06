SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE name='USP_Split_IpMstr_Update') 
	DROP PROCEDURE USP_Split_IpMstr_Update
GO

CREATE PROCEDURE USP_Split_IpMstr_Update
(
	@Version int,
	@ExtIpNo varchar(50),
	@GapIpNo varchar(50),
	@SeqNo varchar(50),
	@Flow varchar(50),
	@Type tinyint,
	@OrderType tinyint,
	@OrderSubType tinyint,
	@QualityType tinyint,
	@Status tinyint,
	@DepartTime datetime,
	@ArriveTime datetime,
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
	@IsAutoReceive bit,
	@IsShipScanHu bit,
	@CreateHuOpt tinyint,
	@IsPrintAsn bit,
	@IsAsnPrinted bit,
	@IsPrintRec bit,
	@IsRecExceed bit,
	@IsRecFulfillUC bit,
	@IsRecFifo bit,
	@IsAsnUniqueRec bit,
	@IsRecScanHu bit,
	@RecGapTo tinyint,
	@AsnTemplate varchar(100),
	@RecTemplate varchar(100),
	@HuTemplate varchar(100),
	@EffDate datetime,
	@WMSNo varchar(50),
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime,
	@CloseDate datetime,
	@CloseUser int,
	@CloseUserNm varchar(100),
	@CloseReason varchar(256),
	@IsCheckPartyFromAuth bit,
	@IsCheckPartyToAuth bit,
	@ShipNo varchar(50),
	@Vehicle varchar(50),
	@IpNo varchar(4000),
	@VersionBerfore int
)
AS
BEGIN
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameters nvarchar(4000)
	SET @Statement='UPDATE ORD_IpMstr_' + CONVERT(varchar, @OrderType) + ' SET Version=@Version_1,ExtIpNo=@ExtIpNo_1,GapIpNo=@GapIpNo_1,SeqNo=@SeqNo_1,Flow=@Flow_1,Type=@Type_1,OrderType=@OrderType_1,OrderSubType=@OrderSubType_1,QualityType=@QualityType_1,Status=@Status_1,DepartTime=@DepartTime_1,ArriveTime=@ArriveTime_1,PartyFrom=@PartyFrom_1,PartyFromNm=@PartyFromNm_1,PartyTo=@PartyTo_1,PartyToNm=@PartyToNm_1,ShipFrom=@ShipFrom_1,ShipFromAddr=@ShipFromAddr_1,ShipFromTel=@ShipFromTel_1,ShipFromCell=@ShipFromCell_1,ShipFromFax=@ShipFromFax_1,ShipFromContact=@ShipFromContact_1,ShipTo=@ShipTo_1,ShipToAddr=@ShipToAddr_1,ShipToTel=@ShipToTel_1,ShipToCell=@ShipToCell_1,ShipToFax=@ShipToFax_1,ShipToContact=@ShipToContact_1,Dock=@Dock_1,IsAutoReceive=@IsAutoReceive_1,IsShipScanHu=@IsShipScanHu_1,CreateHuOpt=@CreateHuOpt_1,IsPrintAsn=@IsPrintAsn_1,IsAsnPrinted=@IsAsnPrinted_1,IsPrintRec=@IsPrintRec_1,IsRecExceed=@IsRecExceed_1,IsRecFulfillUC=@IsRecFulfillUC_1,IsRecFifo=@IsRecFifo_1,IsAsnUniqueRec=@IsAsnUniqueRec_1,IsRecScanHu=@IsRecScanHu_1,RecGapTo=@RecGapTo_1,AsnTemplate=@AsnTemplate_1,RecTemplate=@RecTemplate_1,HuTemplate=@HuTemplate_1,EffDate=@EffDate_1,WMSNo=@WMSNo_1,LastModifyUser=@LastModifyUser_1,LastModifyUserNm=@LastModifyUserNm_1,LastModifyDate=@LastModifyDate_1,CloseDate=@CloseDate_1,CloseUser=@CloseUser_1,CloseUserNm=@CloseUserNm_1,CloseReason=@CloseReason_1,IsCheckPartyFromAuth=@IsCheckPartyFromAuth_1,IsCheckPartyToAuth=@IsCheckPartyToAuth_1,ShipNo=@ShipNo_1,Vehicle=@Vehicle_1 WHERE IpNo=@IpNo_1 AND Version=@VersionBerfore_1'
	SET @Parameters='@Version_1 int, @ExtIpNo_1 varchar(50), @GapIpNo_1 varchar(50), @SeqNo_1 varchar(50), @Flow_1 varchar(50), @Type_1 tinyint, @OrderType_1 tinyint, @OrderSubType_1 tinyint, @QualityType_1 tinyint, @Status_1 tinyint, @DepartTime_1 datetime, @ArriveTime_1 datetime, @PartyFrom_1 varchar(50), @PartyFromNm_1 varchar(100), @PartyTo_1 varchar(50), @PartyToNm_1 varchar(100), @ShipFrom_1 varchar(50), @ShipFromAddr_1 varchar(256), @ShipFromTel_1 varchar(50), @ShipFromCell_1 varchar(50), @ShipFromFax_1 varchar(50), @ShipFromContact_1 varchar(50), @ShipTo_1 varchar(50), @ShipToAddr_1 varchar(256), @ShipToTel_1 varchar(50), @ShipToCell_1 varchar(50), @ShipToFax_1 varchar(50), @ShipToContact_1 varchar(50), @Dock_1 varchar(100), @IsAutoReceive_1 bit, @IsShipScanHu_1 bit, @CreateHuOpt_1 tinyint, @IsPrintAsn_1 bit, @IsAsnPrinted_1 bit, @IsPrintRec_1 bit, @IsRecExceed_1 bit, @IsRecFulfillUC_1 bit, @IsRecFifo_1 bit, @IsAsnUniqueRec_1 bit, @IsRecScanHu_1 bit, @RecGapTo_1 tinyint, @AsnTemplate_1 varchar(100), @RecTemplate_1 varchar(100), @HuTemplate_1 varchar(100), @EffDate_1 datetime, @WMSNo_1 varchar(50), @LastModifyUser_1 int, @LastModifyUserNm_1 varchar(100), @LastModifyDate_1 datetime, @CloseDate_1 datetime, @CloseUser_1 int, @CloseUserNm_1 varchar(100), @CloseReason_1 varchar(256), @IsCheckPartyFromAuth_1 bit, @IsCheckPartyToAuth_1 bit, @ShipNo_1 varchar(50), @Vehicle_1 varchar(50), @IpNo_1 varchar(4000), @VersionBerfore_1 int'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@Version_1=@Version,@ExtIpNo_1=@ExtIpNo,@GapIpNo_1=@GapIpNo,@SeqNo_1=@SeqNo,@Flow_1=@Flow,@Type_1=@Type,@OrderType_1=@OrderType,@OrderSubType_1=@OrderSubType,@QualityType_1=@QualityType,@Status_1=@Status,@DepartTime_1=@DepartTime,@ArriveTime_1=@ArriveTime,@PartyFrom_1=@PartyFrom,@PartyFromNm_1=@PartyFromNm,@PartyTo_1=@PartyTo,@PartyToNm_1=@PartyToNm,@ShipFrom_1=@ShipFrom,@ShipFromAddr_1=@ShipFromAddr,@ShipFromTel_1=@ShipFromTel,@ShipFromCell_1=@ShipFromCell,@ShipFromFax_1=@ShipFromFax,@ShipFromContact_1=@ShipFromContact,@ShipTo_1=@ShipTo,@ShipToAddr_1=@ShipToAddr,@ShipToTel_1=@ShipToTel,@ShipToCell_1=@ShipToCell,@ShipToFax_1=@ShipToFax,@ShipToContact_1=@ShipToContact,@Dock_1=@Dock,@IsAutoReceive_1=@IsAutoReceive,@IsShipScanHu_1=@IsShipScanHu,@CreateHuOpt_1=@CreateHuOpt,@IsPrintAsn_1=@IsPrintAsn,@IsAsnPrinted_1=@IsAsnPrinted,@IsPrintRec_1=@IsPrintRec,@IsRecExceed_1=@IsRecExceed,@IsRecFulfillUC_1=@IsRecFulfillUC,@IsRecFifo_1=@IsRecFifo,@IsAsnUniqueRec_1=@IsAsnUniqueRec,@IsRecScanHu_1=@IsRecScanHu,@RecGapTo_1=@RecGapTo,@AsnTemplate_1=@AsnTemplate,@RecTemplate_1=@RecTemplate,@HuTemplate_1=@HuTemplate,@EffDate_1=@EffDate,@WMSNo_1=@WMSNo,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,@LastModifyDate_1=@LastModifyDate,@CloseDate_1=@CloseDate,@CloseUser_1=@CloseUser,@CloseUserNm_1=@CloseUserNm,@CloseReason_1=@CloseReason,@IsCheckPartyFromAuth_1=@IsCheckPartyFromAuth,@IsCheckPartyToAuth_1=@IsCheckPartyToAuth,@ShipNo_1=@ShipNo,@Vehicle_1=@Vehicle,@IpNo_1=@IpNo,@VersionBerfore_1=@VersionBerfore
END
GO
