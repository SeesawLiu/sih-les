alter table MD_Item add DISPO varchar(100)
go
alter table MD_Item add PLIFZ varchar(100)
go

ALTER PROCEDURE [dbo].[USP_Split_IpMstr_UPDATE]
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
 IF @OrderType=1
 BEGIN
  UPDATE ORD_IpMstr_1 SET Version=@Version,ExtIpNo=@ExtIpNo,GapIpNo=@GapIpNo,SeqNo=@SeqNo,Flow=@Flow,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,Status=@Status,DepartTime=@DepartTime,ArriveTime=@ArriveTime,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,IsAutoReceive=@IsAutoReceive,IsShipScanHu=@IsShipScanHu,CreateHuOpt=@CreateHuOpt,IsPrintAsn=@IsPrintAsn,IsAsnPrinted=@IsAsnPrinted,IsPrintRec=@IsPrintRec,IsRecExceed=@IsRecExceed,IsRecFulfillUC=@IsRecFulfillUC,IsRecFifo=@IsRecFifo,IsAsnUniqueRec=@IsAsnUniqueRec,IsRecScanHu=@IsRecScanHu,RecGapTo=@RecGapTo,AsnTemplate=@AsnTemplate,RecTemplate=@RecTemplate,HuTemplate=@HuTemplate,EffDate=@EffDate,WMSNo=@WMSNo,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,CloseDate=@CloseDate,CloseUser=@CloseUser,CloseUserNm=@CloseUserNm,CloseReason=@CloseReason,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,ShipNo=@ShipNo,Vehicle=@Vehicle,IpNo=@IpNo
  WHERE IpNo=@IpNo AND Version=@VersionBerfore
 END
 ELSE IF @OrderType=2
 BEGIN
  UPDATE ORD_IpMstr_2 SET Version=@Version,ExtIpNo=@ExtIpNo,GapIpNo=@GapIpNo,SeqNo=@SeqNo,Flow=@Flow,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,Status=@Status,DepartTime=@DepartTime,ArriveTime=@ArriveTime,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,IsAutoReceive=@IsAutoReceive,IsShipScanHu=@IsShipScanHu,CreateHuOpt=@CreateHuOpt,IsPrintAsn=@IsPrintAsn,IsAsnPrinted=@IsAsnPrinted,IsPrintRec=@IsPrintRec,IsRecExceed=@IsRecExceed,IsRecFulfillUC=@IsRecFulfillUC,IsRecFifo=@IsRecFifo,IsAsnUniqueRec=@IsAsnUniqueRec,IsRecScanHu=@IsRecScanHu,RecGapTo=@RecGapTo,AsnTemplate=@AsnTemplate,RecTemplate=@RecTemplate,HuTemplate=@HuTemplate,EffDate=@EffDate,WMSNo=@WMSNo,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,CloseDate=@CloseDate,CloseUser=@CloseUser,CloseUserNm=@CloseUserNm,CloseReason=@CloseReason,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,ShipNo=@ShipNo,Vehicle=@Vehicle,IpNo=@IpNo
  WHERE IpNo=@IpNo AND Version=@VersionBerfore
 END
 ELSE IF @OrderType=3
 BEGIN
  UPDATE ORD_IpMstr_3 SET Version=@Version,ExtIpNo=@ExtIpNo,GapIpNo=@GapIpNo,SeqNo=@SeqNo,Flow=@Flow,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,Status=@Status,DepartTime=@DepartTime,ArriveTime=@ArriveTime,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,IsAutoReceive=@IsAutoReceive,IsShipScanHu=@IsShipScanHu,CreateHuOpt=@CreateHuOpt,IsPrintAsn=@IsPrintAsn,IsAsnPrinted=@IsAsnPrinted,IsPrintRec=@IsPrintRec,IsRecExceed=@IsRecExceed,IsRecFulfillUC=@IsRecFulfillUC,IsRecFifo=@IsRecFifo,IsAsnUniqueRec=@IsAsnUniqueRec,IsRecScanHu=@IsRecScanHu,RecGapTo=@RecGapTo,AsnTemplate=@AsnTemplate,RecTemplate=@RecTemplate,HuTemplate=@HuTemplate,EffDate=@EffDate,WMSNo=@WMSNo,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,CloseDate=@CloseDate,CloseUser=@CloseUser,CloseUserNm=@CloseUserNm,CloseReason=@CloseReason,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,ShipNo=@ShipNo,Vehicle=@Vehicle,IpNo=@IpNo
  WHERE IpNo=@IpNo AND Version=@VersionBerfore
 END
 ELSE IF @OrderType=4
 BEGIN
  UPDATE ORD_IpMstr_4 SET Version=@Version,ExtIpNo=@ExtIpNo,GapIpNo=@GapIpNo,SeqNo=@SeqNo,Flow=@Flow,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,Status=@Status,DepartTime=@DepartTime,ArriveTime=@ArriveTime,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,IsAutoReceive=@IsAutoReceive,IsShipScanHu=@IsShipScanHu,CreateHuOpt=@CreateHuOpt,IsPrintAsn=@IsPrintAsn,IsAsnPrinted=@IsAsnPrinted,IsPrintRec=@IsPrintRec,IsRecExceed=@IsRecExceed,IsRecFulfillUC=@IsRecFulfillUC,IsRecFifo=@IsRecFifo,IsAsnUniqueRec=@IsAsnUniqueRec,IsRecScanHu=@IsRecScanHu,RecGapTo=@RecGapTo,AsnTemplate=@AsnTemplate,RecTemplate=@RecTemplate,HuTemplate=@HuTemplate,EffDate=@EffDate,WMSNo=@WMSNo,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,CloseDate=@CloseDate,CloseUser=@CloseUser,CloseUserNm=@CloseUserNm,CloseReason=@CloseReason,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,ShipNo=@ShipNo,Vehicle=@Vehicle,IpNo=@IpNo
  WHERE IpNo=@IpNo AND Version=@VersionBerfore
 END
 ELSE IF @OrderType=5
 BEGIN
  UPDATE ORD_IpMstr_5 SET Version=@Version,ExtIpNo=@ExtIpNo,GapIpNo=@GapIpNo,SeqNo=@SeqNo,Flow=@Flow,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,Status=@Status,DepartTime=@DepartTime,ArriveTime=@ArriveTime,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,IsAutoReceive=@IsAutoReceive,IsShipScanHu=@IsShipScanHu,CreateHuOpt=@CreateHuOpt,IsPrintAsn=@IsPrintAsn,IsAsnPrinted=@IsAsnPrinted,IsPrintRec=@IsPrintRec,IsRecExceed=@IsRecExceed,IsRecFulfillUC=@IsRecFulfillUC,IsRecFifo=@IsRecFifo,IsAsnUniqueRec=@IsAsnUniqueRec,IsRecScanHu=@IsRecScanHu,RecGapTo=@RecGapTo,AsnTemplate=@AsnTemplate,RecTemplate=@RecTemplate,HuTemplate=@HuTemplate,EffDate=@EffDate,WMSNo=@WMSNo,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,CloseDate=@CloseDate,CloseUser=@CloseUser,CloseUserNm=@CloseUserNm,CloseReason=@CloseReason,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,ShipNo=@ShipNo,Vehicle=@Vehicle,IpNo=@IpNo
  WHERE IpNo=@IpNo AND Version=@VersionBerfore
 END
 ELSE IF @OrderType=6
 BEGIN
  UPDATE ORD_IpMstr_6 SET Version=@Version,ExtIpNo=@ExtIpNo,GapIpNo=@GapIpNo,SeqNo=@SeqNo,Flow=@Flow,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,Status=@Status,DepartTime=@DepartTime,ArriveTime=@ArriveTime,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,IsAutoReceive=@IsAutoReceive,IsShipScanHu=@IsShipScanHu,CreateHuOpt=@CreateHuOpt,IsPrintAsn=@IsPrintAsn,IsAsnPrinted=@IsAsnPrinted,IsPrintRec=@IsPrintRec,IsRecExceed=@IsRecExceed,IsRecFulfillUC=@IsRecFulfillUC,IsRecFifo=@IsRecFifo,IsAsnUniqueRec=@IsAsnUniqueRec,IsRecScanHu=@IsRecScanHu,RecGapTo=@RecGapTo,AsnTemplate=@AsnTemplate,RecTemplate=@RecTemplate,HuTemplate=@HuTemplate,EffDate=@EffDate,WMSNo=@WMSNo,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,CloseDate=@CloseDate,CloseUser=@CloseUser,CloseUserNm=@CloseUserNm,CloseReason=@CloseReason,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,ShipNo=@ShipNo,Vehicle=@Vehicle,IpNo=@IpNo
  WHERE IpNo=@IpNo AND Version=@VersionBerfore
 END
 ELSE IF @OrderType=7
 BEGIN
  UPDATE ORD_IpMstr_7 SET Version=@Version,ExtIpNo=@ExtIpNo,GapIpNo=@GapIpNo,SeqNo=@SeqNo,Flow=@Flow,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,Status=@Status,DepartTime=@DepartTime,ArriveTime=@ArriveTime,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,IsAutoReceive=@IsAutoReceive,IsShipScanHu=@IsShipScanHu,CreateHuOpt=@CreateHuOpt,IsPrintAsn=@IsPrintAsn,IsAsnPrinted=@IsAsnPrinted,IsPrintRec=@IsPrintRec,IsRecExceed=@IsRecExceed,IsRecFulfillUC=@IsRecFulfillUC,IsRecFifo=@IsRecFifo,IsAsnUniqueRec=@IsAsnUniqueRec,IsRecScanHu=@IsRecScanHu,RecGapTo=@RecGapTo,AsnTemplate=@AsnTemplate,RecTemplate=@RecTemplate,HuTemplate=@HuTemplate,EffDate=@EffDate,WMSNo=@WMSNo,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,CloseDate=@CloseDate,CloseUser=@CloseUser,CloseUserNm=@CloseUserNm,CloseReason=@CloseReason,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,ShipNo=@ShipNo,Vehicle=@Vehicle,IpNo=@IpNo
  WHERE IpNo=@IpNo AND Version=@VersionBerfore
 END
 ELSE IF @OrderType=8
 BEGIN
  UPDATE ORD_IpMstr_8 SET Version=@Version,ExtIpNo=@ExtIpNo,GapIpNo=@GapIpNo,SeqNo=@SeqNo,Flow=@Flow,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,Status=@Status,DepartTime=@DepartTime,ArriveTime=@ArriveTime,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,IsAutoReceive=@IsAutoReceive,IsShipScanHu=@IsShipScanHu,CreateHuOpt=@CreateHuOpt,IsPrintAsn=@IsPrintAsn,IsAsnPrinted=@IsAsnPrinted,IsPrintRec=@IsPrintRec,IsRecExceed=@IsRecExceed,IsRecFulfillUC=@IsRecFulfillUC,IsRecFifo=@IsRecFifo,IsAsnUniqueRec=@IsAsnUniqueRec,IsRecScanHu=@IsRecScanHu,RecGapTo=@RecGapTo,AsnTemplate=@AsnTemplate,RecTemplate=@RecTemplate,HuTemplate=@HuTemplate,EffDate=@EffDate,WMSNo=@WMSNo,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,CloseDate=@CloseDate,CloseUser=@CloseUser,CloseUserNm=@CloseUserNm,CloseReason=@CloseReason,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,ShipNo=@ShipNo,Vehicle=@Vehicle,IpNo=@IpNo
  WHERE IpNo=@IpNo AND Version=@VersionBerfore
 END
 ELSE 
 BEGIN
  UPDATE ORD_IpMstr_0 SET Version=@Version,ExtIpNo=@ExtIpNo,GapIpNo=@GapIpNo,SeqNo=@SeqNo,Flow=@Flow,Type=@Type,OrderType=0,OrderSubType=@OrderSubType,QualityType=@QualityType,Status=@Status,DepartTime=@DepartTime,ArriveTime=@ArriveTime,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,IsAutoReceive=@IsAutoReceive,IsShipScanHu=@IsShipScanHu,CreateHuOpt=@CreateHuOpt,IsPrintAsn=@IsPrintAsn,IsAsnPrinted=@IsAsnPrinted,IsPrintRec=@IsPrintRec,IsRecExceed=@IsRecExceed,IsRecFulfillUC=@IsRecFulfillUC,IsRecFifo=@IsRecFifo,IsAsnUniqueRec=@IsAsnUniqueRec,IsRecScanHu=@IsRecScanHu,RecGapTo=@RecGapTo,AsnTemplate=@AsnTemplate,RecTemplate=@RecTemplate,HuTemplate=@HuTemplate,EffDate=@EffDate,WMSNo=@WMSNo,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,CloseDate=@CloseDate,CloseUser=@CloseUser,CloseUserNm=@CloseUserNm,CloseReason=@CloseReason,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,ShipNo=@ShipNo,Vehicle=@Vehicle,IpNo=@IpNo
  WHERE IpNo=@IpNo AND Version=@VersionBerfore
 END
       
END


ALTER PROCEDURE [dbo].[USP_Split_IpMstr_Insert]
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
	IF @OrderType=1
	BEGIN
		INSERT INTO ORD_IpMstr_1(Version,ExtIpNo,GapIpNo,SeqNo,Flow,Type,OrderType,OrderSubType,QualityType,Status,DepartTime,ArriveTime,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,IsAutoReceive,IsShipScanHu,CreateHuOpt,IsPrintAsn,IsAsnPrinted,IsPrintRec,IsRecExceed,IsRecFulfillUC,IsRecFifo,IsAsnUniqueRec,IsRecScanHu,RecGapTo,AsnTemplate,RecTemplate,HuTemplate,EffDate,WMSNo,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,CloseDate,CloseUser,CloseUserNm,CloseReason,IsCheckPartyFromAuth,IsCheckPartyToAuth,ShipNo,Vehicle,IpNo)
		VALUES(@Version,@ExtIpNo,@GapIpNo,@SeqNo,@Flow,@Type,@OrderType,@OrderSubType,@QualityType,@Status,@DepartTime,@ArriveTime,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@IsAutoReceive,@IsShipScanHu,@CreateHuOpt,@IsPrintAsn,@IsAsnPrinted,@IsPrintRec,@IsRecExceed,@IsRecFulfillUC,@IsRecFifo,@IsAsnUniqueRec,@IsRecScanHu,@RecGapTo,@AsnTemplate,@RecTemplate,@HuTemplate,@EffDate,@WMSNo,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@CloseDate,@CloseUser,@CloseUserNm,@CloseReason,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@ShipNo,@Vehicle,@IpNo)
		SELECT SCOPE_IDENTITY()
	END	
	ELSE IF @OrderType=2
	BEGIN
		INSERT INTO ORD_IpMstr_2(Version,ExtIpNo,GapIpNo,SeqNo,Flow,Type,OrderType,OrderSubType,QualityType,Status,DepartTime,ArriveTime,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,IsAutoReceive,IsShipScanHu,CreateHuOpt,IsPrintAsn,IsAsnPrinted,IsPrintRec,IsRecExceed,IsRecFulfillUC,IsRecFifo,IsAsnUniqueRec,IsRecScanHu,RecGapTo,AsnTemplate,RecTemplate,HuTemplate,EffDate,WMSNo,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,CloseDate,CloseUser,CloseUserNm,CloseReason,IsCheckPartyFromAuth,IsCheckPartyToAuth,ShipNo,Vehicle,IpNo)
		VALUES(@Version,@ExtIpNo,@GapIpNo,@SeqNo,@Flow,@Type,@OrderType,@OrderSubType,@QualityType,@Status,@DepartTime,@ArriveTime,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@IsAutoReceive,@IsShipScanHu,@CreateHuOpt,@IsPrintAsn,@IsAsnPrinted,@IsPrintRec,@IsRecExceed,@IsRecFulfillUC,@IsRecFifo,@IsAsnUniqueRec,@IsRecScanHu,@RecGapTo,@AsnTemplate,@RecTemplate,@HuTemplate,@EffDate,@WMSNo,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@CloseDate,@CloseUser,@CloseUserNm,@CloseReason,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@ShipNo,@Vehicle,@IpNo)
		SELECT SCOPE_IDENTITY()
	END	
	ELSE IF @OrderType=3
	BEGIN
		INSERT INTO ORD_IpMstr_3(Version,ExtIpNo,GapIpNo,SeqNo,Flow,Type,OrderType,OrderSubType,QualityType,Status,DepartTime,ArriveTime,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,IsAutoReceive,IsShipScanHu,CreateHuOpt,IsPrintAsn,IsAsnPrinted,IsPrintRec,IsRecExceed,IsRecFulfillUC,IsRecFifo,IsAsnUniqueRec,IsRecScanHu,RecGapTo,AsnTemplate,RecTemplate,HuTemplate,EffDate,WMSNo,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,CloseDate,CloseUser,CloseUserNm,CloseReason,IsCheckPartyFromAuth,IsCheckPartyToAuth,ShipNo,Vehicle,IpNo)
		VALUES(@Version,@ExtIpNo,@GapIpNo,@SeqNo,@Flow,@Type,@OrderType,@OrderSubType,@QualityType,@Status,@DepartTime,@ArriveTime,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@IsAutoReceive,@IsShipScanHu,@CreateHuOpt,@IsPrintAsn,@IsAsnPrinted,@IsPrintRec,@IsRecExceed,@IsRecFulfillUC,@IsRecFifo,@IsAsnUniqueRec,@IsRecScanHu,@RecGapTo,@AsnTemplate,@RecTemplate,@HuTemplate,@EffDate,@WMSNo,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@CloseDate,@CloseUser,@CloseUserNm,@CloseReason,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@ShipNo,@Vehicle,@IpNo)
		SELECT SCOPE_IDENTITY()
	END	
	ELSE IF @OrderType=4
	BEGIN
		INSERT INTO ORD_IpMstr_4(Version,ExtIpNo,GapIpNo,SeqNo,Flow,Type,OrderType,OrderSubType,QualityType,Status,DepartTime,ArriveTime,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,IsAutoReceive,IsShipScanHu,CreateHuOpt,IsPrintAsn,IsAsnPrinted,IsPrintRec,IsRecExceed,IsRecFulfillUC,IsRecFifo,IsAsnUniqueRec,IsRecScanHu,RecGapTo,AsnTemplate,RecTemplate,HuTemplate,EffDate,WMSNo,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,CloseDate,CloseUser,CloseUserNm,CloseReason,IsCheckPartyFromAuth,IsCheckPartyToAuth,ShipNo,Vehicle,IpNo)
		VALUES(@Version,@ExtIpNo,@GapIpNo,@SeqNo,@Flow,@Type,@OrderType,@OrderSubType,@QualityType,@Status,@DepartTime,@ArriveTime,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@IsAutoReceive,@IsShipScanHu,@CreateHuOpt,@IsPrintAsn,@IsAsnPrinted,@IsPrintRec,@IsRecExceed,@IsRecFulfillUC,@IsRecFifo,@IsAsnUniqueRec,@IsRecScanHu,@RecGapTo,@AsnTemplate,@RecTemplate,@HuTemplate,@EffDate,@WMSNo,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@CloseDate,@CloseUser,@CloseUserNm,@CloseReason,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@ShipNo,@Vehicle,@IpNo)
		SELECT SCOPE_IDENTITY()
	END	
	ELSE IF @OrderType=5
	BEGIN
		INSERT INTO ORD_IpMstr_5(Version,ExtIpNo,GapIpNo,SeqNo,Flow,Type,OrderType,OrderSubType,QualityType,Status,DepartTime,ArriveTime,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,IsAutoReceive,IsShipScanHu,CreateHuOpt,IsPrintAsn,IsAsnPrinted,IsPrintRec,IsRecExceed,IsRecFulfillUC,IsRecFifo,IsAsnUniqueRec,IsRecScanHu,RecGapTo,AsnTemplate,RecTemplate,HuTemplate,EffDate,WMSNo,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,CloseDate,CloseUser,CloseUserNm,CloseReason,IsCheckPartyFromAuth,IsCheckPartyToAuth,ShipNo,Vehicle,IpNo)
		VALUES(@Version,@ExtIpNo,@GapIpNo,@SeqNo,@Flow,@Type,@OrderType,@OrderSubType,@QualityType,@Status,@DepartTime,@ArriveTime,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@IsAutoReceive,@IsShipScanHu,@CreateHuOpt,@IsPrintAsn,@IsAsnPrinted,@IsPrintRec,@IsRecExceed,@IsRecFulfillUC,@IsRecFifo,@IsAsnUniqueRec,@IsRecScanHu,@RecGapTo,@AsnTemplate,@RecTemplate,@HuTemplate,@EffDate,@WMSNo,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@CloseDate,@CloseUser,@CloseUserNm,@CloseReason,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@ShipNo,@Vehicle,@IpNo)
		SELECT SCOPE_IDENTITY()
	END	
	ELSE IF @OrderType=6
	BEGIN
		INSERT INTO ORD_IpMstr_6(Version,ExtIpNo,GapIpNo,SeqNo,Flow,Type,OrderType,OrderSubType,QualityType,Status,DepartTime,ArriveTime,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,IsAutoReceive,IsShipScanHu,CreateHuOpt,IsPrintAsn,IsAsnPrinted,IsPrintRec,IsRecExceed,IsRecFulfillUC,IsRecFifo,IsAsnUniqueRec,IsRecScanHu,RecGapTo,AsnTemplate,RecTemplate,HuTemplate,EffDate,WMSNo,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,CloseDate,CloseUser,CloseUserNm,CloseReason,IsCheckPartyFromAuth,IsCheckPartyToAuth,ShipNo,Vehicle,IpNo)
		VALUES(@Version,@ExtIpNo,@GapIpNo,@SeqNo,@Flow,@Type,@OrderType,@OrderSubType,@QualityType,@Status,@DepartTime,@ArriveTime,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@IsAutoReceive,@IsShipScanHu,@CreateHuOpt,@IsPrintAsn,@IsAsnPrinted,@IsPrintRec,@IsRecExceed,@IsRecFulfillUC,@IsRecFifo,@IsAsnUniqueRec,@IsRecScanHu,@RecGapTo,@AsnTemplate,@RecTemplate,@HuTemplate,@EffDate,@WMSNo,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@CloseDate,@CloseUser,@CloseUserNm,@CloseReason,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@ShipNo,@Vehicle,@IpNo)
		SELECT SCOPE_IDENTITY()
	END	
	ELSE IF @OrderType=7
	BEGIN
		INSERT INTO ORD_IpMstr_7(Version,ExtIpNo,GapIpNo,SeqNo,Flow,Type,OrderType,OrderSubType,QualityType,Status,DepartTime,ArriveTime,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,IsAutoReceive,IsShipScanHu,CreateHuOpt,IsPrintAsn,IsAsnPrinted,IsPrintRec,IsRecExceed,IsRecFulfillUC,IsRecFifo,IsAsnUniqueRec,IsRecScanHu,RecGapTo,AsnTemplate,RecTemplate,HuTemplate,EffDate,WMSNo,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,CloseDate,CloseUser,CloseUserNm,CloseReason,IsCheckPartyFromAuth,IsCheckPartyToAuth,ShipNo,Vehicle,IpNo)
		VALUES(@Version,@ExtIpNo,@GapIpNo,@SeqNo,@Flow,@Type,@OrderType,@OrderSubType,@QualityType,@Status,@DepartTime,@ArriveTime,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@IsAutoReceive,@IsShipScanHu,@CreateHuOpt,@IsPrintAsn,@IsAsnPrinted,@IsPrintRec,@IsRecExceed,@IsRecFulfillUC,@IsRecFifo,@IsAsnUniqueRec,@IsRecScanHu,@RecGapTo,@AsnTemplate,@RecTemplate,@HuTemplate,@EffDate,@WMSNo,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@CloseDate,@CloseUser,@CloseUserNm,@CloseReason,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@ShipNo,@Vehicle,@IpNo)
		SELECT SCOPE_IDENTITY()
	END	
	ELSE IF @OrderType=8
	BEGIN
		INSERT INTO ORD_IpMstr_8(Version,ExtIpNo,GapIpNo,SeqNo,Flow,Type,OrderType,OrderSubType,QualityType,Status,DepartTime,ArriveTime,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,IsAutoReceive,IsShipScanHu,CreateHuOpt,IsPrintAsn,IsAsnPrinted,IsPrintRec,IsRecExceed,IsRecFulfillUC,IsRecFifo,IsAsnUniqueRec,IsRecScanHu,RecGapTo,AsnTemplate,RecTemplate,HuTemplate,EffDate,WMSNo,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,CloseDate,CloseUser,CloseUserNm,CloseReason,IsCheckPartyFromAuth,IsCheckPartyToAuth,ShipNo,Vehicle,IpNo)
		VALUES(@Version,@ExtIpNo,@GapIpNo,@SeqNo,@Flow,@Type,@OrderType,@OrderSubType,@QualityType,@Status,@DepartTime,@ArriveTime,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@IsAutoReceive,@IsShipScanHu,@CreateHuOpt,@IsPrintAsn,@IsAsnPrinted,@IsPrintRec,@IsRecExceed,@IsRecFulfillUC,@IsRecFifo,@IsAsnUniqueRec,@IsRecScanHu,@RecGapTo,@AsnTemplate,@RecTemplate,@HuTemplate,@EffDate,@WMSNo,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@CloseDate,@CloseUser,@CloseUserNm,@CloseReason,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@ShipNo,@Vehicle,@IpNo)
		SELECT SCOPE_IDENTITY()
	END			
	ELSE
	BEGIN
		INSERT INTO ORD_IpMstr_0(Version,ExtIpNo,GapIpNo,SeqNo,Flow,Type,OrderType,OrderSubType,QualityType,Status,DepartTime,ArriveTime,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,IsAutoReceive,IsShipScanHu,CreateHuOpt,IsPrintAsn,IsAsnPrinted,IsPrintRec,IsRecExceed,IsRecFulfillUC,IsRecFifo,IsAsnUniqueRec,IsRecScanHu,RecGapTo,AsnTemplate,RecTemplate,HuTemplate,EffDate,WMSNo,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,CloseDate,CloseUser,CloseUserNm,CloseReason,IsCheckPartyFromAuth,IsCheckPartyToAuth,ShipNo,Vehicle,IpNo)
		VALUES(@Version,@ExtIpNo,@GapIpNo,@SeqNo,@Flow,@Type,0,@OrderSubType,@QualityType,@Status,@DepartTime,@ArriveTime,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@IsAutoReceive,@IsShipScanHu,@CreateHuOpt,@IsPrintAsn,@IsAsnPrinted,@IsPrintRec,@IsRecExceed,@IsRecFulfillUC,@IsRecFifo,@IsAsnUniqueRec,@IsRecScanHu,@RecGapTo,@AsnTemplate,@RecTemplate,@HuTemplate,@EffDate,@WMSNo,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@CloseDate,@CloseUser,@CloseUserNm,@CloseReason,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@ShipNo,@Vehicle,@IpNo)
		SELECT SCOPE_IDENTITY()
	END							
END

ALTER PROCEDURE [dbo].[USP_Search_IpMstr] 
( 
	@IPNo varchar(50), 
	@Status tinyint,	 
	@OrderTypes varchar(50), 
	@PartyFrom varchar(50), 
	@PartyTo varchar(50), 
	@StartDate datetime, 
	@EndDate datetime, 
	@Dock varchar(50), 
	@Item varchar(50), 
	@OrderNo varchar(50), 
	@WMSNo varchar(50), 
	@ManufactureParty varchar(50), 
	@Flow varchar(50), 
	@IsSupplier bit, 
	@UserId int, 
	@WhereStatement varchar(8000), 
	@SortCloumn varchar(50)=null, 
	@SortRule varchar(50)=null, 
	@PageSize int, 
	@Page int 
) 
AS 
BEGIN 
	SET NOCOUNT ON 
	DECLARE @SpliceTables nvarchar(4000) 
	DECLARE @Type varchar(5) 
	DECLARE @Statement nvarchar(4000) 
	DECLARE @Parameter nvarchar(4000) 
	DECLARE @PagePara nvarchar(4000) 
	DECLARE @SortDesc nvarchar(100) 
	DECLARE @Where nvarchar(4000) 
	DECLARE @PermissionClause nvarchar(1000) 
	SET @Where='WHERE 1=1 '+ISNULL(@WhereStatement,'') 

	IF(ISNULL(@OrderTypes,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 
	IF(ISNULL(@IPNo,'')<>'') 
	BEGIN 
		SET @IPNo='%'+@IPNo+'%' 
		SET @Where=@Where+' AND i.IpNo LIKE @IPNo_1' 
	END	 
	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @OrderNo=@OrderNo+'%' 
		SET @Where=@Where+' AND i.OrderNo LIKE @OrderNo_1' 
	END 
	IF(ISNULL(@WMSNo,'')<>'') 
	BEGIN 
		SET @WMSNo=@WMSNo+'%' 
		SET @Where=@Where+' AND i.WMSNo LIKE @WMSNo_1' 
	END	 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND i.Flow=@Flow_1' 
	END 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND i.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND i.PartyTo=@PartyTo_1 ' 
	END		 
	IF(@Status is not null) 
	BEGIN 
		SET @Where=@Where+' AND i.Status=@Status_1 ' 
	END 
    IF(ISDATE(@EndDate)=1)
	BEGIN
    set @EndDate=dateadd(day,1,@EndDate)
    END
	IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @Where=@Where+' AND i.CreateDate BETWEEN @StartDate_1 And @EndDate_1 ' 
	END	 
	ELSE IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=0) 
	BEGIN 
		SET @Where=@Where+' AND i.CreateDate > @StartDate_1' 
	END 
	ELSE IF(ISDATE(@StartDate)=0 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @Where=@Where+' AND i.CreateDate < @EndDate_1' 
	END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0) 
		BEGIN 
			SET @Where=@Where+' AND i.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @Where=@Where+' AND (i.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR i.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
		END				 
	END 

	IF ISNULL(@SortCloumn,'')='' 
	BEGIN 
		SET @SortDesc=' ORDER BY CreateDate DESC' 
	END 
	ELSE 
	BEGIN 
		IF(ISNULL(@SortRule,'')='') 
		BEGIN 
			SET @SortRule=' ACS' 
		END 
		IF(CHARINDEX('Reference',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'Reference','Ref') 
		END 
		IF(CHARINDEX('Name',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'Name','NM') 
		END 
		IF(CHARINDEX('UnitCount',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'UnitCount','UC') 
		END 
		IF(CHARINDEX('Sequence',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'Sequence','Seq') 
		END 
		IF(CHARINDEX('Receive',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'Receive','Rec') 
		END					 
		SET @SortDesc=' ORDER BY '+@SortCloumn+' '+@SortRule 
		IF(CHARINDEX('Address',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'Address','Addr') 
		END					 
		SET @SortDesc=' ORDER BY '+@SortCloumn+' '+@SortRule		 
	END 

	IF @Page>0 
	BEGIN 
		SET @PagePara='WHERE RowId BETWEEN '+cast(@PageSize*(@Page-1) as varchar(50))+' AND '++cast(@PageSize*(@Page) as varchar(50)) 
	END 

	WHILE(LEN(@OrderTypes)>0) 
	BEGIN 
		IF(LEN(@OrderTypes)=1) 
		BEGIN 
			SET @Type=@OrderTypes 
		END 
		ELSE 
		BEGIN 
			SET @Type=SUBSTRING(@OrderTypes,1,CHARINDEX(',',@OrderTypes)-1) 
		END 
		--IF(CHARINDEX('OrderDetail',@WhereStatement)>0) 
		--BEGIN 
		--	SET @WhereStatement=REPLACE(@WhereStatement,'OrderDetail','ORD_OrderDet'+@Type) 
		--END			 
		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			SET @SpliceTables='SELECT * FROM ORD_IpMstr_'+@Type+' AS i '+REPLACE(@Where,'IPDetail','ORD_IpDet_'+@Type) 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_IpMstr_'+@Type+' AS i '+REPLACE(@Where,'IPDetail','ORD_IpDet_'+@Type) 
		END 
		SET @OrderTypes=SUBSTRING(@OrderTypes,3,LEN(@OrderTypes)) 
	END	 

	SET @Statement=N'SELECT TOP('+CAST(@PageSize AS VARCHAR(10))+') IpNo, ExtIpNo, GapIpNo, SeqNo, Type, OrderType, OrderSubType, QualityType, Status, DepartTime, ArriveTime, PartyFrom, PartyFromNm, PartyTo, PartyToNm, ShipFrom, ShipFromAddr, ShipFromTel, ShipFromCell, ShipFromFax, ShipFromContact, ShipTo, ShipToAddr, ShipToTel, ShipToCell, ShipToFax, ShipToContact, Dock, IsAutoReceive, IsShipScanHu, IsPrintAsn, IsAsnPrinted, IsPrintRec, IsRecExceed, IsRecFulfillUC, IsRecFifo, IsAsnUniqueRec, IsCheckPartyFromAuth, IsCheckPartyToAuth, RecGapTo, AsnTemplate, RecTemplate, HuTemplate, EffDate, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, CloseDate, CloseUser, CloseUserNm, CloseReason, Version, WMSNo, CreateHuOpt, IsRecScanHu, Flow, ShipNo, Vehicle FROM (SELECT *,RowId=ROW_NUMBER()OVER('+@SortDesc+') FROM ('+@SpliceTables+') AS T1 ) AS T2 '+@PagePara 
	PRINT @Statement 
	PRINT LEN(@Statement) 
	SET @Parameter=N'@IPNo_1 varchar(50),@Status_1 tinyint,	@OrderTypes_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@StartDate_1 datetime,@EndDate_1 datetime,@Dock_1 varchar(50),@Item_1 varchar(50),@OrderNo_1 varchar(50),@WMSNo_1 varchar(50),@ManufactureParty_1 varchar(50),@Flow_1 varchar(50),@IsSupplier_1 bit'		 

	EXEC SP_EXECUTESQL @Statement,@Parameter, 
		@IPNo_1=@IPNo,@Status_1=@Status,@OrderTypes_1=@OrderTypes ,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo ,@StartDate_1=@StartDate , 
		@EndDate_1=@EndDate ,@Dock_1=@Dock ,@Item_1=@Item ,@OrderNo_1=@OrderNo,@WMSNo_1=@WMSNo,@ManufactureParty_1=@ManufactureParty , 
		@Flow_1=@Flow,@IsSupplier_1=@IsSupplier 

END 




IF EXISTS(SELECT * FROM SYS.objects WHERE type='V' AND name='VIEW_IpMstr')
	DROP VIEW VIEW_IpMstr
CREATE VIEW [dbo].[VIEW_IpMstr]
AS
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_1
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_2
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_3
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_4
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_5
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_6
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_7
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_8
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_0
GO
