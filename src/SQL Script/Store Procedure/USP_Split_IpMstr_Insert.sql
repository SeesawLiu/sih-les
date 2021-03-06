SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE name='USP_Split_IpMstr_Insert') 
	DROP PROCEDURE USP_Split_IpMstr_Insert
GO

CREATE PROCEDURE USP_Split_IpMstr_Insert
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
	@CreateUser int,
	@CreateUserNm varchar(100),
	@CreateDate datetime,
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
	@IpNo varchar(4000)
)
AS
BEGIN
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameters nvarchar(4000)
	SET @Statement='INSERT INTO ORD_IpMstr_' + CONVERT(nvarchar, @OrderType) + '(IpNo,Version,ExtIpNo,GapIpNo,SeqNo,Flow,Type,OrderType,OrderSubType,QualityType,Status,DepartTime,ArriveTime,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,IsAutoReceive,IsShipScanHu,CreateHuOpt,IsPrintAsn,IsAsnPrinted,IsPrintRec,IsRecExceed,IsRecFulfillUC,IsRecFifo,IsAsnUniqueRec,IsRecScanHu,RecGapTo,AsnTemplate,RecTemplate,HuTemplate,EffDate,WMSNo,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,CloseDate,CloseUser,CloseUserNm,CloseReason,IsCheckPartyFromAuth,IsCheckPartyToAuth,ShipNo,Vehicle) VALUES(@IpNo_1,@Version_1,@ExtIpNo_1,@GapIpNo_1,@SeqNo_1,@Flow_1,@Type_1,@OrderType_1,@OrderSubType_1,@QualityType_1,@Status_1,@DepartTime_1,@ArriveTime_1,@PartyFrom_1,@PartyFromNm_1,@PartyTo_1,@PartyToNm_1,@ShipFrom_1,@ShipFromAddr_1,@ShipFromTel_1,@ShipFromCell_1,@ShipFromFax_1,@ShipFromContact_1,@ShipTo_1,@ShipToAddr_1,@ShipToTel_1,@ShipToCell_1,@ShipToFax_1,@ShipToContact_1,@Dock_1,@IsAutoReceive_1,@IsShipScanHu_1,@CreateHuOpt_1,@IsPrintAsn_1,@IsAsnPrinted_1,@IsPrintRec_1,@IsRecExceed_1,@IsRecFulfillUC_1,@IsRecFifo_1,@IsAsnUniqueRec_1,@IsRecScanHu_1,@RecGapTo_1,@AsnTemplate_1,@RecTemplate_1,@HuTemplate_1,@EffDate_1,@WMSNo_1,@CreateUser_1,@CreateUserNm_1,@CreateDate_1,@LastModifyUser_1,@LastModifyUserNm_1,@LastModifyDate_1,@CloseDate_1,@CloseUser_1,@CloseUserNm_1,@CloseReason_1,@IsCheckPartyFromAuth_1,@IsCheckPartyToAuth_1,@ShipNo_1,@Vehicle_1)'
	SET @Parameters='@IpNo_1 varchar(4000), @Version_1 int, @ExtIpNo_1 varchar(50), @GapIpNo_1 varchar(50), @SeqNo_1 varchar(50), @Flow_1 varchar(50), @Type_1 tinyint, @OrderType_1 tinyint, @OrderSubType_1 tinyint, @QualityType_1 tinyint, @Status_1 tinyint, @DepartTime_1 datetime, @ArriveTime_1 datetime, @PartyFrom_1 varchar(50), @PartyFromNm_1 varchar(100), @PartyTo_1 varchar(50), @PartyToNm_1 varchar(100), @ShipFrom_1 varchar(50), @ShipFromAddr_1 varchar(256), @ShipFromTel_1 varchar(50), @ShipFromCell_1 varchar(50), @ShipFromFax_1 varchar(50), @ShipFromContact_1 varchar(50), @ShipTo_1 varchar(50), @ShipToAddr_1 varchar(256), @ShipToTel_1 varchar(50), @ShipToCell_1 varchar(50), @ShipToFax_1 varchar(50), @ShipToContact_1 varchar(50), @Dock_1 varchar(100), @IsAutoReceive_1 bit, @IsShipScanHu_1 bit, @CreateHuOpt_1 tinyint, @IsPrintAsn_1 bit, @IsAsnPrinted_1 bit, @IsPrintRec_1 bit, @IsRecExceed_1 bit, @IsRecFulfillUC_1 bit, @IsRecFifo_1 bit, @IsAsnUniqueRec_1 bit, @IsRecScanHu_1 bit, @RecGapTo_1 tinyint, @AsnTemplate_1 varchar(100), @RecTemplate_1 varchar(100), @HuTemplate_1 varchar(100), @EffDate_1 datetime, @WMSNo_1 varchar(50), @CreateUser_1 int, @CreateUserNm_1 varchar(100), @CreateDate_1 datetime, @LastModifyUser_1 int, @LastModifyUserNm_1 varchar(100), @LastModifyDate_1 datetime, @CloseDate_1 datetime, @CloseUser_1 int, @CloseUserNm_1 varchar(100), @CloseReason_1 varchar(256), @IsCheckPartyFromAuth_1 bit, @IsCheckPartyToAuth_1 bit, @ShipNo_1 varchar(50), @Vehicle_1 varchar(50)'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@IpNo_1=@IpNo,@Version_1=@Version,@ExtIpNo_1=@ExtIpNo,@GapIpNo_1=@GapIpNo,@SeqNo_1=@SeqNo,@Flow_1=@Flow,@Type_1=@Type,@OrderType_1=@OrderType,@OrderSubType_1=@OrderSubType,@QualityType_1=@QualityType,@Status_1=@Status,@DepartTime_1=@DepartTime,@ArriveTime_1=@ArriveTime,@PartyFrom_1=@PartyFrom,@PartyFromNm_1=@PartyFromNm,@PartyTo_1=@PartyTo,@PartyToNm_1=@PartyToNm,@ShipFrom_1=@ShipFrom,@ShipFromAddr_1=@ShipFromAddr,@ShipFromTel_1=@ShipFromTel,@ShipFromCell_1=@ShipFromCell,@ShipFromFax_1=@ShipFromFax,@ShipFromContact_1=@ShipFromContact,@ShipTo_1=@ShipTo,@ShipToAddr_1=@ShipToAddr,@ShipToTel_1=@ShipToTel,@ShipToCell_1=@ShipToCell,@ShipToFax_1=@ShipToFax,@ShipToContact_1=@ShipToContact,@Dock_1=@Dock,@IsAutoReceive_1=@IsAutoReceive,@IsShipScanHu_1=@IsShipScanHu,@CreateHuOpt_1=@CreateHuOpt,@IsPrintAsn_1=@IsPrintAsn,@IsAsnPrinted_1=@IsAsnPrinted,@IsPrintRec_1=@IsPrintRec,@IsRecExceed_1=@IsRecExceed,@IsRecFulfillUC_1=@IsRecFulfillUC,@IsRecFifo_1=@IsRecFifo,@IsAsnUniqueRec_1=@IsAsnUniqueRec,@IsRecScanHu_1=@IsRecScanHu,@RecGapTo_1=@RecGapTo,@AsnTemplate_1=@AsnTemplate,@RecTemplate_1=@RecTemplate,@HuTemplate_1=@HuTemplate,@EffDate_1=@EffDate,@WMSNo_1=@WMSNo,@CreateUser_1=@CreateUser,@CreateUserNm_1=@CreateUserNm,@CreateDate_1=@CreateDate,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,@LastModifyDate_1=@LastModifyDate,@CloseDate_1=@CloseDate,@CloseUser_1=@CloseUser,@CloseUserNm_1=@CloseUserNm,@CloseReason_1=@CloseReason,@IsCheckPartyFromAuth_1=@IsCheckPartyFromAuth,@IsCheckPartyToAuth_1=@IsCheckPartyToAuth,@ShipNo_1=@ShipNo,@Vehicle_1=@Vehicle
END
GO
