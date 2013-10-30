SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Split_OrderDet_Insert')
	DROP PROCEDURE USP_Split_OrderDet_Insert
GO

CREATE PROCEDURE USP_Split_OrderDet_Insert
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
	@CreateUser int,
	@CreateUserNm varchar(100),
	@CreateDate datetime,
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime,
	@Container varchar(4000),
	@ContainerDesc varchar(50),
	@PickStrategy varchar(50),
	@ExtraDmdSource varchar(256),
	@IsChangeUC bit,
	@AUFNR varchar(50),
	@ICHARG varchar(50),
	@BWART varchar(50),
	@IsCreatePickList bit
)
AS
BEGIN
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameters nvarchar(4000)
	DECLARE @Id int 
	IF @OrderType = 8
	BEGIN
		IF EXISTS(SELECT 1 FROM ORD_OrderDet_8 WHERE EBELN_EBELP=@ExtNo+'&'+@ExtSeq)
		BEGIN
			RAISERROR('计划协议号行号重复', 16, 1)
			RETURN
		END
	END
	
	EXEC USP_SYS_GetNextId 'ORD_OrderDet', @Id output 
	
	SET @Statement='Set ARITHABORT ON;INSERT INTO ORD_OrderDet_' + CONVERT(varchar, @OrderType) + '(Id,Version,OrderNo,OrderType,OrderSubType,Seq,ExtNo,ExtSeq,StartDate,EndDate,ScheduleType,Item,ItemDesc,RefItemCode,Uom,BaseUom,UC,UCDesc,MinUC,QualityType,ManufactureParty,ReqQty,OrderQty,ShipQty,RecQty,RejQty,ScrapQty,PickQty,UnitQty,RecLotSize,LocFrom,LocFromNm,LocTo,LocToNm,IsInspect,BillAddr,BillAddrDesc,PriceList,UnitPrice,IsProvEst,Tax,IsIncludeTax,Currency,Bom,Routing,BillTerm,IsScanHu,ReserveNo,ReserveLine,ZOPWZ,ZOPID,ZOPDS,BinTo,WMSSeq,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,Container,ContainerDesc,PickStrategy,ExtraDmdSource,IsChangeUC,AUFNR,ICHARG,BWART,IsCreatePickList) VALUES(@Id_1,@Version_1,@OrderNo_1,@OrderType_1,@OrderSubType_1,@Seq_1,@ExtNo_1,@ExtSeq_1,@StartDate_1,@EndDate_1,@ScheduleType_1,@Item_1,@ItemDesc_1,@RefItemCode_1,@Uom_1,@BaseUom_1,@UC_1,@UCDesc_1,@MinUC_1,@QualityType_1,@ManufactureParty_1,@ReqQty_1,@OrderQty_1,@ShipQty_1,@RecQty_1,@RejQty_1,@ScrapQty_1,@PickQty_1,@UnitQty_1,@RecLotSize_1,@LocFrom_1,@LocFromNm_1,@LocTo_1,@LocToNm_1,@IsInspect_1,@BillAddr_1,@BillAddrDesc_1,@PriceList_1,@UnitPrice_1,@IsProvEst_1,@Tax_1,@IsIncludeTax_1,@Currency_1,@Bom_1,@Routing_1,@BillTerm_1,@IsScanHu_1,@ReserveNo_1,@ReserveLine_1,@ZOPWZ_1,@ZOPID_1,@ZOPDS_1,@BinTo_1,@WMSSeq_1,@CreateUser_1,@CreateUserNm_1,@CreateDate_1,@LastModifyUser_1,@LastModifyUserNm_1,@LastModifyDate_1,@Container_1,@ContainerDesc_1,@PickStrategy_1,@ExtraDmdSource_1,@IsChangeUC_1,@AUFNR_1,@ICHARG_1,@BWART_1,@IsCreatePickList_1)'
	SET @Parameters='@Id_1 int, @Version_1 int, @OrderNo_1 varchar(50), @OrderType_1 tinyint, @OrderSubType_1 tinyint, @Seq_1 int, @ExtNo_1 varchar(50), @ExtSeq_1 varchar(50), @StartDate_1 datetime, @EndDate_1 datetime, @ScheduleType_1 tinyint, @Item_1 varchar(50), @ItemDesc_1 varchar(100), @RefItemCode_1 varchar(50), @Uom_1 varchar(5), @BaseUom_1 varchar(5), @UC_1 decimal(18,8), @UCDesc_1 varchar(50), @MinUC_1 decimal(18,8), @QualityType_1 tinyint, @ManufactureParty_1 varchar(50), @ReqQty_1 decimal(18,8), @OrderQty_1 decimal(18,8), @ShipQty_1 decimal(18,8), @RecQty_1 decimal(18,8), @RejQty_1 decimal(18,8), @ScrapQty_1 decimal(18,8), @PickQty_1 decimal(18,8), @UnitQty_1 decimal(18,8), @RecLotSize_1 decimal(18,8), @LocFrom_1 varchar(50), @LocFromNm_1 varchar(100), @LocTo_1 varchar(50), @LocToNm_1 varchar(100), @IsInspect_1 bit, @BillAddr_1 varchar(50), @BillAddrDesc_1 varchar(256), @PriceList_1 varchar(50), @UnitPrice_1 decimal(18,8), @IsProvEst_1 bit, @Tax_1 varchar(50), @IsIncludeTax_1 bit, @Currency_1 varchar(50), @Bom_1 varchar(50), @Routing_1 varchar(50), @BillTerm_1 tinyint, @IsScanHu_1 bit, @ReserveNo_1 varchar(50), @ReserveLine_1 varchar(50), @ZOPWZ_1 varchar(50), @ZOPID_1 varchar(50), @ZOPDS_1 varchar(50), @BinTo_1 varchar(50), @WMSSeq_1 varchar(50), @CreateUser_1 int, @CreateUserNm_1 varchar(100), @CreateDate_1 datetime, @LastModifyUser_1 int, @LastModifyUserNm_1 varchar(100), @LastModifyDate_1 datetime, @Container_1 varchar(50), @ContainerDesc_1 varchar(50), @PickStrategy_1 varchar(50), @ExtraDmdSource_1 varchar(256), @IsChangeUC_1 bit, @AUFNR_1 varchar(50), @ICHARG_1 varchar(50), @BWART_1 varchar(50), @IsCreatePickList_1 bit'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@Id_1=@Id,@Version_1=@Version,@OrderNo_1=@OrderNo,@OrderType_1=@OrderType,@OrderSubType_1=@OrderSubType,@Seq_1=@Seq,@ExtNo_1=@ExtNo,@ExtSeq_1=@ExtSeq,@StartDate_1=@StartDate,@EndDate_1=@EndDate,@ScheduleType_1=@ScheduleType,@Item_1=@Item,@ItemDesc_1=@ItemDesc,@RefItemCode_1=@RefItemCode,@Uom_1=@Uom,@BaseUom_1=@BaseUom,@UC_1=@UC,@UCDesc_1=@UCDesc,@MinUC_1=@MinUC,@QualityType_1=@QualityType,@ManufactureParty_1=@ManufactureParty,@ReqQty_1=@ReqQty,@OrderQty_1=@OrderQty,@ShipQty_1=@ShipQty,@RecQty_1=@RecQty,@RejQty_1=@RejQty,@ScrapQty_1=@ScrapQty,@PickQty_1=@PickQty,@UnitQty_1=@UnitQty,@RecLotSize_1=@RecLotSize,@LocFrom_1=@LocFrom,@LocFromNm_1=@LocFromNm,@LocTo_1=@LocTo,@LocToNm_1=@LocToNm,@IsInspect_1=@IsInspect,@BillAddr_1=@BillAddr,@BillAddrDesc_1=@BillAddrDesc,@PriceList_1=@PriceList,@UnitPrice_1=@UnitPrice,@IsProvEst_1=@IsProvEst,@Tax_1=@Tax,@IsIncludeTax_1=@IsIncludeTax,@Currency_1=@Currency,@Bom_1=@Bom,@Routing_1=@Routing,@BillTerm_1=@BillTerm,@IsScanHu_1=@IsScanHu,@ReserveNo_1=@ReserveNo,@ReserveLine_1=@ReserveLine,@ZOPWZ_1=@ZOPWZ,@ZOPID_1=@ZOPID,@ZOPDS_1=@ZOPDS,@BinTo_1=@BinTo,@WMSSeq_1=@WMSSeq,@CreateUser_1=@CreateUser,@CreateUserNm_1=@CreateUserNm,@CreateDate_1=@CreateDate,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,@LastModifyDate_1=@LastModifyDate,@Container_1=@Container,@ContainerDesc_1=@ContainerDesc,@PickStrategy_1=@PickStrategy,@ExtraDmdSource_1=@ExtraDmdSource,@IsChangeUC_1=@IsChangeUC,@AUFNR_1=@AUFNR,@ICHARG_1=@ICHARG,@BWART_1=@BWART,@IsCreatePickList_1=@IsCreatePickList
	SELECT @Id
END
GO
