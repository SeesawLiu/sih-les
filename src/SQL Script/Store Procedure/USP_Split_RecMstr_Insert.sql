SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Split_RecMstr_Insert')
	DROP PROCEDURE USP_Split_RecMstr_Insert
GO	

CREATE PROCEDURE USP_Split_RecMstr_Insert
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
	@CreateUser int,
	@CreateUserNm varchar(100),
	@CreateDate datetime,
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime,
	@IsCheckPartyFromAuth bit,
	@IsCheckPartyToAuth bit,
	@RecNo varchar(4000)
)
AS
BEGIN
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameters nvarchar(4000)
	SET @Statement='INSERT INTO ORD_RecMstr_' + CONVERT(varchar, @OrderType) + '(RecNo,Version,ExtRecNo,IpNo,SeqNo,Flow,Status,Type,OrderType,OrderSubType,QualityType,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,EffDate,IsPrintRec,IsRecPrinted,CreateHuOpt,IsRecScanHu,RecTemplate,WMSNo,CancelReason,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,IsCheckPartyFromAuth,IsCheckPartyToAuth) VALUES(@RecNo_1,@Version_1,@ExtRecNo_1,@IpNo_1,@SeqNo_1,@Flow_1,@Status_1,@Type_1,@OrderType_1,@OrderSubType_1,@QualityType_1,@PartyFrom_1,@PartyFromNm_1,@PartyTo_1,@PartyToNm_1,@ShipFrom_1,@ShipFromAddr_1,@ShipFromTel_1,@ShipFromCell_1,@ShipFromFax_1,@ShipFromContact_1,@ShipTo_1,@ShipToAddr_1,@ShipToTel_1,@ShipToCell_1,@ShipToFax_1,@ShipToContact_1,@Dock_1,@EffDate_1,@IsPrintRec_1,@IsRecPrinted_1,@CreateHuOpt_1,@IsRecScanHu_1,@RecTemplate_1,@WMSNo_1,@CancelReason_1,@CreateUser_1,@CreateUserNm_1,@CreateDate_1,@LastModifyUser_1,@LastModifyUserNm_1,@LastModifyDate_1,@IsCheckPartyFromAuth_1,@IsCheckPartyToAuth_1)'
	SET @Parameters='@RecNo_1 varchar(4000), @Version_1 int, @ExtRecNo_1 varchar(50), @IpNo_1 varchar(50), @SeqNo_1 varchar(50), @Flow_1 varchar(50), @Status_1 tinyint, @Type_1 tinyint, @OrderType_1 tinyint, @OrderSubType_1 tinyint, @QualityType_1 tinyint, @PartyFrom_1 varchar(50), @PartyFromNm_1 varchar(100), @PartyTo_1 varchar(50), @PartyToNm_1 varchar(100), @ShipFrom_1 varchar(50), @ShipFromAddr_1 varchar(256), @ShipFromTel_1 varchar(50), @ShipFromCell_1 varchar(50), @ShipFromFax_1 varchar(50), @ShipFromContact_1 varchar(50), @ShipTo_1 varchar(50), @ShipToAddr_1 varchar(256), @ShipToTel_1 varchar(50), @ShipToCell_1 varchar(50), @ShipToFax_1 varchar(50), @ShipToContact_1 varchar(50), @Dock_1 varchar(100), @EffDate_1 datetime, @IsPrintRec_1 bit, @IsRecPrinted_1 bit, @CreateHuOpt_1 tinyint, @IsRecScanHu_1 bit, @RecTemplate_1 varchar(100), @WMSNo_1 varchar(50), @CancelReason_1 varchar(256), @CreateUser_1 int, @CreateUserNm_1 varchar(100), @CreateDate_1 datetime, @LastModifyUser_1 int, @LastModifyUserNm_1 varchar(100), @LastModifyDate_1 datetime, @IsCheckPartyFromAuth_1 bit, @IsCheckPartyToAuth_1 bit'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@RecNo_1=@RecNo,@Version_1=@Version,@ExtRecNo_1=@ExtRecNo,@IpNo_1=@IpNo,@SeqNo_1=@SeqNo,@Flow_1=@Flow,@Status_1=@Status,@Type_1=@Type,@OrderType_1=@OrderType,@OrderSubType_1=@OrderSubType,@QualityType_1=@QualityType,@PartyFrom_1=@PartyFrom,@PartyFromNm_1=@PartyFromNm,@PartyTo_1=@PartyTo,@PartyToNm_1=@PartyToNm,@ShipFrom_1=@ShipFrom,@ShipFromAddr_1=@ShipFromAddr,@ShipFromTel_1=@ShipFromTel,@ShipFromCell_1=@ShipFromCell,@ShipFromFax_1=@ShipFromFax,@ShipFromContact_1=@ShipFromContact,@ShipTo_1=@ShipTo,@ShipToAddr_1=@ShipToAddr,@ShipToTel_1=@ShipToTel,@ShipToCell_1=@ShipToCell,@ShipToFax_1=@ShipToFax,@ShipToContact_1=@ShipToContact,@Dock_1=@Dock,@EffDate_1=@EffDate,@IsPrintRec_1=@IsPrintRec,@IsRecPrinted_1=@IsRecPrinted,@CreateHuOpt_1=@CreateHuOpt,@IsRecScanHu_1=@IsRecScanHu,@RecTemplate_1=@RecTemplate,@WMSNo_1=@WMSNo,@CancelReason_1=@CancelReason,@CreateUser_1=@CreateUser,@CreateUserNm_1=@CreateUserNm,@CreateDate_1=@CreateDate,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,@LastModifyDate_1=@LastModifyDate,@IsCheckPartyFromAuth_1=@IsCheckPartyFromAuth,@IsCheckPartyToAuth_1=@IsCheckPartyToAuth
END
GO
