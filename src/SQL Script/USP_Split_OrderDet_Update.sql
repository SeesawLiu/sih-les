IF EXISTS(SELECT * FROM sys.objects WHERE name='USP_Split_OrderDet_UPDATE') 
		DROP PROCEDURE USP_Split_OrderDet_UPDATE
GO
-----CREATE UPDATE SP
CREATE PROCEDURE USP_Split_OrderDet_UPDATE
(
	@Version int,
	@OrderNo varchar(50),
	@OrderType tinyint,
	@OrderSubType tinyint,
	@Seq int,
	@ExtNo varchar(50),
	@ExtSeq varchar(50),
	@StartDate datetime,
	@EndDate datetime,
	@ScheduleType tinyint,
	@Item varchar(50),
	@ItemDesc varchar(100),
	@RefItemCode varchar(50),
	@Uom varchar(5),
	@BaseUom varchar(5),
	@UC decimal(18,8),
	@UCDesc varchar(50),
	@MinUC decimal(18,8),
	@QualityType tinyint,
	@ManufactureParty varchar(50),
	@ReqQty decimal(18,8),
	@OrderQty decimal(18,8),
	@ShipQty decimal(18,8),
	@RecQty decimal(18,8),
	@RejQty decimal(18,8),
	@ScrapQty decimal(18,8),
	@PickQty decimal(18,8),
	@UnitQty decimal(18,8),
	@RecLotSize decimal(18,8),
	@LocFrom varchar(50),
	@LocFromNm varchar(100),
	@LocTo varchar(50),
	@LocToNm varchar(100),
	@IsInspect bit,
	@BillAddr varchar(50),
	@BillAddrDesc varchar(256),
	@PriceList varchar(50),
	@UnitPrice decimal(18,8),
	@IsProvEst bit,
	@Tax varchar(50),
	@IsIncludeTax bit,
	@Currency varchar(50),
	@Bom varchar(50),
	@Routing varchar(50),
	@BillTerm tinyint,
	@IsScanHu bit,
	@ReserveNo varchar(50),
	@ReserveLine varchar(50),
	@ZOPWZ varchar(50),
	@ZOPID varchar(50),
	@ZOPDS varchar(50),
	@BinTo varchar(50),
	@WMSSeq varchar(50),
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime,
	@Container varchar(50),
	@ContainerDesc varchar(50),
	@PickStrategy varchar(50),
	@ExtraDmdSource varchar(256),
	@IsChangeUC bit,
	@AUFNR varchar(50),
	@ICHARG varchar(50),
	@BWART varchar(50),
	@IsCreatePickList bit,
	@Id int,
	@VersionBerfore int
)
AS
BEGIN
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameters nvarchar(4000)
	SET @Statement='UPDATE ORD_OrderDet_' + CONVERT(varchar(1), @OrderType) + ' SET Version=@Version_1,OrderNo=@OrderNo_1,OrderType=@OrderType_1,OrderSubType=@OrderSubType_1,Seq=@Seq_1,ExtNo=@ExtNo_1,ExtSeq=@ExtSeq_1,StartDate=@StartDate_1,EndDate=@EndDate_1,ScheduleType=@ScheduleType_1,Item=@Item_1,ItemDesc=@ItemDesc_1,RefItemCode=@RefItemCode_1,Uom=@Uom_1,BaseUom=@BaseUom_1,UC=@UC_1,UCDesc=@UCDesc_1,MinUC=@MinUC_1,QualityType=@QualityType_1,ManufactureParty=@ManufactureParty_1,ReqQty=@ReqQty_1,OrderQty=@OrderQty_1,ShipQty=@ShipQty_1,RecQty=@RecQty_1,RejQty=@RejQty_1,ScrapQty=@ScrapQty_1,PickQty=@PickQty_1,UnitQty=@UnitQty_1,RecLotSize=@RecLotSize_1,LocFrom=@LocFrom_1,LocFromNm=@LocFromNm_1,LocTo=@LocTo_1,LocToNm=@LocToNm_1,IsInspect=@IsInspect_1,BillAddr=@BillAddr_1,BillAddrDesc=@BillAddrDesc_1,PriceList=@PriceList_1,UnitPrice=@UnitPrice_1,IsProvEst=@IsProvEst_1,Tax=@Tax_1,IsIncludeTax=@IsIncludeTax_1,Currency=@Currency_1,Bom=@Bom_1,Routing=@Routing_1,BillTerm=@BillTerm_1,IsScanHu=@IsScanHu_1,ReserveNo=@ReserveNo_1,ReserveLine=@ReserveLine_1,ZOPWZ=@ZOPWZ_1,ZOPID=@ZOPID_1,ZOPDS=@ZOPDS_1,BinTo=@BinTo_1,WMSSeq=@WMSSeq_1,LastModifyUser=@LastModifyUser_1,LastModifyUserNm=@LastModifyUserNm_1,LastModifyDate=@LastModifyDate_1,Container=@Container_1,ContainerDesc=@ContainerDesc_1,PickStrategy=@PickStrategy_1,ExtraDmdSource=@ExtraDmdSource_1,IsChangeUC=@IsChangeUC_1,AUFNR=@AUFNR_1,ICHARG=@ICHARG_1,BWART=@BWART_1,IsCreatePickList=@IsCreatePickList_1 WHERE Id=@Id_1 AND Version=@VersionBerfore_1'
	SET @Parameters='@Version_1 int, @OrderNo_1 varchar(50), @OrderType_1 tinyint, @OrderSubType_1 tinyint, @Seq_1 int, @ExtNo_1 varchar(50), @ExtSeq_1 varchar(50), @StartDate_1 datetime, @EndDate_1 datetime, @ScheduleType_1 tinyint, @Item_1 varchar(50), @ItemDesc_1 varchar(100), @RefItemCode_1 varchar(50), @Uom_1 varchar(5), @BaseUom_1 varchar(5), @UC_1 decimal(18,8), @UCDesc_1 varchar(50), @MinUC_1 decimal(18,8), @QualityType_1 tinyint, @ManufactureParty_1 varchar(50), @ReqQty_1 decimal(18,8), @OrderQty_1 decimal(18,8), @ShipQty_1 decimal(18,8), @RecQty_1 decimal(18,8), @RejQty_1 decimal(18,8), @ScrapQty_1 decimal(18,8), @PickQty_1 decimal(18,8), @UnitQty_1 decimal(18,8), @RecLotSize_1 decimal(18,8), @LocFrom_1 varchar(50), @LocFromNm_1 varchar(100), @LocTo_1 varchar(50), @LocToNm_1 varchar(100), @IsInspect_1 bit, @BillAddr_1 varchar(50), @BillAddrDesc_1 varchar(256), @PriceList_1 varchar(50), @UnitPrice_1 decimal(18,8), @IsProvEst_1 bit, @Tax_1 varchar(50), @IsIncludeTax_1 bit, @Currency_1 varchar(50), @Bom_1 varchar(50), @Routing_1 varchar(50), @BillTerm_1 tinyint, @IsScanHu_1 bit, @ReserveNo_1 varchar(50), @ReserveLine_1 varchar(50), @ZOPWZ_1 varchar(50), @ZOPID_1 varchar(50), @ZOPDS_1 varchar(50), @BinTo_1 varchar(50), @WMSSeq_1 varchar(50), @LastModifyUser_1 int, @LastModifyUserNm_1 varchar(100), @LastModifyDate_1 datetime, @Container_1 varchar(50), @ContainerDesc_1 varchar(50), @PickStrategy_1 varchar(50), @ExtraDmdSource_1 varchar(256), @IsChangeUC_1 bit, @AUFNR_1 varchar(50), @ICHARG_1 varchar(50), @BWART_1 varchar(50), @IsCreatePickList_1 bit, @Id_1 int, @VersionBerfore_1 int'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@Version_1=@Version,@OrderNo_1=@OrderNo,@OrderType_1=@OrderType,@OrderSubType_1=@OrderSubType,@Seq_1=@Seq,@ExtNo_1=@ExtNo,@ExtSeq_1=@ExtSeq,@StartDate_1=@StartDate,@EndDate_1=@EndDate,@ScheduleType_1=@ScheduleType,@Item_1=@Item,@ItemDesc_1=@ItemDesc,@RefItemCode_1=@RefItemCode,@Uom_1=@Uom,@BaseUom_1=@BaseUom,@UC_1=@UC,@UCDesc_1=@UCDesc,@MinUC_1=@MinUC,@QualityType_1=@QualityType,@ManufactureParty_1=@ManufactureParty,@ReqQty_1=@ReqQty,@OrderQty_1=@OrderQty,@ShipQty_1=@ShipQty,@RecQty_1=@RecQty,@RejQty_1=@RejQty,@ScrapQty_1=@ScrapQty,@PickQty_1=@PickQty,@UnitQty_1=@UnitQty,@RecLotSize_1=@RecLotSize,@LocFrom_1=@LocFrom,@LocFromNm_1=@LocFromNm,@LocTo_1=@LocTo,@LocToNm_1=@LocToNm,@IsInspect_1=@IsInspect,@BillAddr_1=@BillAddr,@BillAddrDesc_1=@BillAddrDesc,@PriceList_1=@PriceList,@UnitPrice_1=@UnitPrice,@IsProvEst_1=@IsProvEst,@Tax_1=@Tax,@IsIncludeTax_1=@IsIncludeTax,@Currency_1=@Currency,@Bom_1=@Bom,@Routing_1=@Routing,@BillTerm_1=@BillTerm,@IsScanHu_1=@IsScanHu,@ReserveNo_1=@ReserveNo,@ReserveLine_1=@ReserveLine,@ZOPWZ_1=@ZOPWZ,@ZOPID_1=@ZOPID,@ZOPDS_1=@ZOPDS,@BinTo_1=@BinTo,@WMSSeq_1=@WMSSeq,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,@LastModifyDate_1=@LastModifyDate,@Container_1=@Container,@ContainerDesc_1=@ContainerDesc,@PickStrategy_1=@PickStrategy,@ExtraDmdSource_1=@ExtraDmdSource,@IsChangeUC_1=@IsChangeUC,@AUFNR_1=@AUFNR,@ICHARG_1=@ICHARG,@BWART_1=@BWART,@IsCreatePickList_1=@IsCreatePickList,@Id_1=@Id,@VersionBerfore_1=@VersionBerfore
END
GO