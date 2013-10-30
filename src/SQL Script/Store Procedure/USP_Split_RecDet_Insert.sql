SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_Split_RecDet_Insert')
	DROP PROCEDURE USP_Split_RecDet_Insert
GO

CREATE PROCEDURE USP_Split_RecDet_Insert
(
	@Version int,
	@RecNo varchar(50),
	@Seq int,
	@OrderNo varchar(50),
	@OrderType tinyint,
	@OrderSubType tinyint,
	@OrderDetSeq int,
	@OrderDetId int,
	@IpNo varchar(50),
	@IpDetId int,
	@IpDetSeq int,
	@IpDetType tinyint,
	@IpGapAdjOpt tinyint,
	@ExtNo varchar(50),
	@ExtSeq varchar(50),
	@Flow varchar(50),
	@Item varchar(50),
	@ItemDesc varchar(100),
	@RefItemCode varchar(50),
	@Uom varchar(5),
	@BaseUom varchar(5),
	@UC decimal(18,8),
	@QualityType tinyint,
	@ManufactureParty varchar(50),
	@RecQty decimal(18,8),
	@ScrapQty decimal(18,8),
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
	@CreateUser int,
	@CreateUserNm varchar(100),
	@CreateDate datetime,
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime
)
AS
BEGIN
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameters nvarchar(4000)
	DECLARE @Id int 
	EXEC USP_SYS_GetNextId 'ORD_RecDet', @Id output 
	
	SET @Statement='INSERT INTO ORD_RecDet_' + CONVERT(varchar, @OrderType) + '(Id,Version,RecNo,Seq,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,IpNo,IpDetId,IpDetSeq,IpDetType,IpGapAdjOpt,ExtNo,ExtSeq,Flow,Item,ItemDesc,RefItemCode,Uom,BaseUom,UC,QualityType,ManufactureParty,RecQty,ScrapQty,UnitQty,LocFrom,LocFromNm,LocTo,LocToNm,IsInspect,BillAddr,PriceList,UnitPrice,Currency,IsProvEst,Tax,IsIncludeTax,BillTerm,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate) VALUES(@Id_1,@Version_1,@RecNo_1,@Seq_1,@OrderNo_1,@OrderType_1,@OrderSubType_1,@OrderDetSeq_1,@OrderDetId_1,@IpNo_1,@IpDetId_1,@IpDetSeq_1,@IpDetType_1,@IpGapAdjOpt_1,@ExtNo_1,@ExtSeq_1,@Flow_1,@Item_1,@ItemDesc_1,@RefItemCode_1,@Uom_1,@BaseUom_1,@UC_1,@QualityType_1,@ManufactureParty_1,@RecQty_1,@ScrapQty_1,@UnitQty_1,@LocFrom_1,@LocFromNm_1,@LocTo_1,@LocToNm_1,@IsInspect_1,@BillAddr_1,@PriceList_1,@UnitPrice_1,@Currency_1,@IsProvEst_1,@Tax_1,@IsIncludeTax_1,@BillTerm_1,@CreateUser_1,@CreateUserNm_1,@CreateDate_1,@LastModifyUser_1,@LastModifyUserNm_1,@LastModifyDate_1)'
	SET @Parameters='@Id_1 int, @Version_1 int, @RecNo_1 varchar(50), @Seq_1 int, @OrderNo_1 varchar(50), @OrderType_1 tinyint, @OrderSubType_1 tinyint, @OrderDetSeq_1 int, @OrderDetId_1 int, @IpNo_1 varchar(50), @IpDetId_1 int, @IpDetSeq_1 int, @IpDetType_1 tinyint, @IpGapAdjOpt_1 tinyint, @ExtNo_1 varchar(50), @ExtSeq_1 varchar(50), @Flow_1 varchar(50), @Item_1 varchar(50), @ItemDesc_1 varchar(100), @RefItemCode_1 varchar(50), @Uom_1 varchar(5), @BaseUom_1 varchar(5), @UC_1 decimal(18,8), @QualityType_1 tinyint, @ManufactureParty_1 varchar(50), @RecQty_1 decimal(18,8), @ScrapQty_1 decimal(18,8), @UnitQty_1 decimal(18,8), @LocFrom_1 varchar(50), @LocFromNm_1 varchar(100), @LocTo_1 varchar(50), @LocToNm_1 varchar(100), @IsInspect_1 bit, @BillAddr_1 varchar(50), @PriceList_1 varchar(50), @UnitPrice_1 decimal(18,8), @Currency_1 varchar(50), @IsProvEst_1 bit, @Tax_1 varchar(50), @IsIncludeTax_1 bit, @BillTerm_1 tinyint, @CreateUser_1 int, @CreateUserNm_1 varchar(100), @CreateDate_1 datetime, @LastModifyUser_1 int, @LastModifyUserNm_1 varchar(100), @LastModifyDate_1 datetime'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@Id_1=@Id,@Version_1=@Version,@RecNo_1=@RecNo,@Seq_1=@Seq,@OrderNo_1=@OrderNo,@OrderType_1=@OrderType,@OrderSubType_1=@OrderSubType,@OrderDetSeq_1=@OrderDetSeq,@OrderDetId_1=@OrderDetId,@IpNo_1=@IpNo,@IpDetId_1=@IpDetId,@IpDetSeq_1=@IpDetSeq,@IpDetType_1=@IpDetType,@IpGapAdjOpt_1=@IpGapAdjOpt,@ExtNo_1=@ExtNo,@ExtSeq_1=@ExtSeq,@Flow_1=@Flow,@Item_1=@Item,@ItemDesc_1=@ItemDesc,@RefItemCode_1=@RefItemCode,@Uom_1=@Uom,@BaseUom_1=@BaseUom,@UC_1=@UC,@QualityType_1=@QualityType,@ManufactureParty_1=@ManufactureParty,@RecQty_1=@RecQty,@ScrapQty_1=@ScrapQty,@UnitQty_1=@UnitQty,@LocFrom_1=@LocFrom,@LocFromNm_1=@LocFromNm,@LocTo_1=@LocTo,@LocToNm_1=@LocToNm,@IsInspect_1=@IsInspect,@BillAddr_1=@BillAddr,@PriceList_1=@PriceList,@UnitPrice_1=@UnitPrice,@Currency_1=@Currency,@IsProvEst_1=@IsProvEst,@Tax_1=@Tax,@IsIncludeTax_1=@IsIncludeTax,@BillTerm_1=@BillTerm,@CreateUser_1=@CreateUser,@CreateUserNm_1=@CreateUserNm,@CreateDate_1=@CreateDate,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,@LastModifyDate_1=@LastModifyDate
	SELECT @Id
END
GO
