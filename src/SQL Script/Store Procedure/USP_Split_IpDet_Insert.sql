SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Split_IpDet_Insert')
	DROP PROCEDURE USP_Split_IpDet_Insert
GO

CREATE PROCEDURE USP_Split_IpDet_Insert
(
	@Version int,
	@Type tinyint,
	@IpNo varchar(50),
	@Seq int,
	@OrderNo varchar(50),
	@OrderType tinyint,
	@OrderSubType tinyint,
	@OrderDetSeq int,
	@OrderDetId int,
	@ExtNo varchar(50),
	@ExtSeq varchar(50),
	@Flow varchar(50),
	@Item varchar(50),
	@ItemDesc varchar(100),
	@RefItemCode varchar(50),
	@Uom varchar(5),
	@BaseUom varchar(5),
	@UC decimal(18,8),
	@UCDesc varchar(50),
	@StartTime datetime,
	@WindowTime datetime,
	@QualityType tinyint,
	@ManufactureParty varchar(50),
	@Qty decimal(18,8),
	@RecQty decimal(18,8),
	@UnitQty decimal(18,8),
	@LocFrom varchar(50),
	@LocFromNm varchar(100),
	@LocTo varchar(50),
	@LocToNm varchar(100),
	@IsInspect bit,
	@BillAddr varchar(50),
	@PriceList varchar(50),
	@UnitPrice decimal(18,8),
	@Currency varchar(50),
	@IsProvEst bit,
	@Tax varchar(50),
	@IsIncludeTax bit,
	@BillTerm tinyint,
	@IsClose bit,
	@GapRecNo varchar(50),
	@GapIpDetId int,
	@BinTo varchar(50),
	@IsScanHu bit,
	@CreateUser int,
	@CreateUserNm varchar(100),
	@CreateDate datetime,
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime,
	@Container varchar(50),
	@ContainerDesc varchar(50),
	@IsChangeUC bit,
	@BWART varchar(50),
	@PSTYP varchar(50)
)
AS
BEGIN
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameters nvarchar(4000)
	DECLARE @Id int 
	EXEC USP_SYS_GetNextId 'ORD_IpDet', @Id output 
	
	SET @Statement='Set ARITHABORT ON;INSERT INTO ORD_IpDet_' + CONVERT(varchar, @OrderType) + '(Id,Version,Type,IpNo,Seq,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,ExtNo,ExtSeq,Flow,Item,ItemDesc,RefItemCode,Uom,BaseUom,UC,UCDesc,StartTime,WindowTime,QualityType,ManufactureParty,Qty,RecQty,UnitQty,LocFrom,LocFromNm,LocTo,LocToNm,IsInspect,BillAddr,PriceList,UnitPrice,Currency,IsProvEst,Tax,IsIncludeTax,BillTerm,IsClose,GapRecNo,GapIpDetId,BinTo,IsScanHu,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,Container,ContainerDesc,IsChangeUC,BWART,PSTYP) VALUES(@Id_1,@Version_1,@Type_1,@IpNo_1,@Seq_1,@OrderNo_1,@OrderType_1,@OrderSubType_1,@OrderDetSeq_1,@OrderDetId_1,@ExtNo_1,@ExtSeq_1,@Flow_1,@Item_1,@ItemDesc_1,@RefItemCode_1,@Uom_1,@BaseUom_1,@UC_1,@UCDesc_1,@StartTime_1,@WindowTime_1,@QualityType_1,@ManufactureParty_1,@Qty_1,@RecQty_1,@UnitQty_1,@LocFrom_1,@LocFromNm_1,@LocTo_1,@LocToNm_1,@IsInspect_1,@BillAddr_1,@PriceList_1,@UnitPrice_1,@Currency_1,@IsProvEst_1,@Tax_1,@IsIncludeTax_1,@BillTerm_1,@IsClose_1,@GapRecNo_1,@GapIpDetId_1,@BinTo_1,@IsScanHu_1,@CreateUser_1,@CreateUserNm_1,@CreateDate_1,@LastModifyUser_1,@LastModifyUserNm_1,@LastModifyDate_1,@Container_1,@ContainerDesc_1,@IsChangeUC_1,@BWART_1,@PSTYP_1)'
	SET @Parameters='@Id_1 int, @Version_1 int, @Type_1 tinyint, @IpNo_1 varchar(50), @Seq_1 int, @OrderNo_1 varchar(50), @OrderType_1 tinyint, @OrderSubType_1 tinyint, @OrderDetSeq_1 int, @OrderDetId_1 int, @ExtNo_1 varchar(50), @ExtSeq_1 varchar(50), @Flow_1 varchar(50), @Item_1 varchar(50), @ItemDesc_1 varchar(100), @RefItemCode_1 varchar(50), @Uom_1 varchar(5), @BaseUom_1 varchar(5), @UC_1 decimal(18,8), @UCDesc_1 varchar(50), @StartTime_1 datetime, @WindowTime_1 datetime, @QualityType_1 tinyint, @ManufactureParty_1 varchar(50), @Qty_1 decimal(18,8), @RecQty_1 decimal(18,8), @UnitQty_1 decimal(18,8), @LocFrom_1 varchar(50), @LocFromNm_1 varchar(100), @LocTo_1 varchar(50), @LocToNm_1 varchar(100), @IsInspect_1 bit, @BillAddr_1 varchar(50), @PriceList_1 varchar(50), @UnitPrice_1 decimal(18,8), @Currency_1 varchar(50), @IsProvEst_1 bit, @Tax_1 varchar(50), @IsIncludeTax_1 bit, @BillTerm_1 tinyint, @IsClose_1 bit, @GapRecNo_1 varchar(50), @GapIpDetId_1 int, @BinTo_1 varchar(50), @IsScanHu_1 bit, @CreateUser_1 int, @CreateUserNm_1 varchar(100), @CreateDate_1 datetime, @LastModifyUser_1 int, @LastModifyUserNm_1 varchar(100), @LastModifyDate_1 datetime, @Container_1 varchar(50), @ContainerDesc_1 varchar(50), @IsChangeUC_1 bit, @BWART_1 varchar(50), @PSTYP_1 varchar(50)'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@Id_1=@Id,@Version_1=@Version,@Type_1=@Type,@IpNo_1=@IpNo,@Seq_1=@Seq,@OrderNo_1=@OrderNo,@OrderType_1=@OrderType,@OrderSubType_1=@OrderSubType,@OrderDetSeq_1=@OrderDetSeq,@OrderDetId_1=@OrderDetId,@ExtNo_1=@ExtNo,@ExtSeq_1=@ExtSeq,@Flow_1=@Flow,@Item_1=@Item,@ItemDesc_1=@ItemDesc,@RefItemCode_1=@RefItemCode,@Uom_1=@Uom,@BaseUom_1=@BaseUom,@UC_1=@UC,@UCDesc_1=@UCDesc,@StartTime_1=@StartTime,@WindowTime_1=@WindowTime,@QualityType_1=@QualityType,@ManufactureParty_1=@ManufactureParty,@Qty_1=@Qty,@RecQty_1=@RecQty,@UnitQty_1=@UnitQty,@LocFrom_1=@LocFrom,@LocFromNm_1=@LocFromNm,@LocTo_1=@LocTo,@LocToNm_1=@LocToNm,@IsInspect_1=@IsInspect,@BillAddr_1=@BillAddr,@PriceList_1=@PriceList,@UnitPrice_1=@UnitPrice,@Currency_1=@Currency,@IsProvEst_1=@IsProvEst,@Tax_1=@Tax,@IsIncludeTax_1=@IsIncludeTax,@BillTerm_1=@BillTerm,@IsClose_1=@IsClose,@GapRecNo_1=@GapRecNo,@GapIpDetId_1=@GapIpDetId,@BinTo_1=@BinTo,@IsScanHu_1=@IsScanHu,@CreateUser_1=@CreateUser,@CreateUserNm_1=@CreateUserNm,@CreateDate_1=@CreateDate,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,@LastModifyDate_1=@LastModifyDate,@Container_1=@Container,@ContainerDesc_1=@ContainerDesc,@IsChangeUC_1=@IsChangeUC,@BWART_1=@BWART,@PSTYP_1=@PSTYP
	SELECT @Id
END
GO