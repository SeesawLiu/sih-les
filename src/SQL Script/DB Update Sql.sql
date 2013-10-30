


/****** Object:  StoredProcedure [dbo].[USP_Busi_GetVoidInventory]    Script Date: 12/15/2012 14:05:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[USP_Busi_GetVoidInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@PlanBill int,
	@QualityType tinyint,
	@OccupyType tinyint
)
AS
BEGIN
/*******************获取指定Planbill库存数据*********************************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetVoidInventory] 'CS0010','5801306476',0,0,1 
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL OR @PlanBill IS NULL
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version,dbo.MD_LocationBin.Area, dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, 
                      dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, 
                      dbo.INV_Hu.IsOdd
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld LEFT OUTER JOIN
                      dbo.INV_Hu ON lld.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      dbo.MD_LocationBin ON lld.Bin = dbo.MD_LocationBin.Code
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1 AND lld.PlanBill=@PlanBill_1 
				  AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty > 0 AND lld.IsCS = 1 
				  AND lld.QualityType=@QualityType_1 AND lld.OccupyType=@OccupyType_1 '
	SET @Parameter=N'@Item_1 varchar(50),@PlanBill_1 int,@QualityType_1 tinyint,@OccupyType_1 tinyint,@Location_1 varchar(50)'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@PlanBill_1=@PlanBill,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType,@Location_1=@Location
END



/****** Object:  StoredProcedure [dbo].[USP_Busi_GetVoidOccupyInventory]    Script Date: 12/15/2012 14:06:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[USP_Busi_GetVoidOccupyInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@PlanBill int,
	@QualityType tinyint,
	@OccupyType tinyint,
	@OccupyReferenceNo varchar(50)	
)
AS
BEGIN
/*******************获取指定PlanBill&指定占用库存数据**************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetVoidOccupyInventory] 'CS0010','5801306476',0,0,1,'dsaf' 
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL OR @PlanBill IS NULL
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version,dbo.MD_LocationBin.Area, dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, 
                      dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, 
                      dbo.INV_Hu.IsOdd
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld LEFT OUTER JOIN
                      dbo.INV_Hu ON lld.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      dbo.MD_LocationBin ON lld.Bin = dbo.MD_LocationBin.Code
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1 AND lld.PlanBill=@PlanBill_1 
				  AND lld.OccupyType=@OccupyType_1 AND lld.OccupyRefNo=@OccupyReferenceNo_1
				  AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty > 0 AND lld.IsCS = 1 
				  AND lld.QualityType=@QualityType_1'
	SET @Parameter=N'@Item_1 varchar(50),@PlanBill_1 int,@QualityType_1 tinyint,@OccupyType_1 tinyint,@OccupyReferenceNo_1 varchar(50),@Location_1 varchar(50)'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@PlanBill_1=@PlanBill,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType,@OccupyReferenceNo_1=@OccupyReferenceNo,@Location_1=@Location
END



/****** Object:  StoredProcedure [dbo].[USP_Busi_GetMinusInventory]    Script Date: 12/13/2012 20:07:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[USP_Busi_GetMinusInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@QualityType tinyint,
	@OccupyType tinyint
)
AS
BEGIN
/*******************获取负库存数据*********************************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetMinusInventory] 'CS0010','5801306476',0,0,1
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version,dbo.MD_LocationBin.Area, dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, 
                      dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, 
                      dbo.INV_Hu.IsOdd
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld LEFT OUTER JOIN
                      dbo.INV_Hu ON lld.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      dbo.MD_LocationBin ON lld.Bin = dbo.MD_LocationBin.Code
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1
				    AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty < 0 AND lld.QualityType=@QualityType_1 
					AND lld.OccupyType=@OccupyType_1 AND lld.IsCS=0 AND lld.CSSupplier is null'
	SET @Parameter=N'@Item_1 varchar(50),@QualityType_1 tinyint,@OccupyType_1 tinyint,@Location_1 varchar(50)'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType,@Location_1=@Location

END

/****** Object:  StoredProcedure [dbo].[USP_Busi_GetMinusCSInventory]    Script Date: 12/13/2012 20:09:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[USP_Busi_GetMinusCSInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@QualityType tinyint,
	@OccupyType tinyint,
	@CSSupplier varchar(50)
)
AS
BEGIN
/*******************获取指定供应商负数库存数据*****************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetMinusCSInventory] 'ZAA02R','-CN6T',0,0,'0000102681'
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @sql varchar(max)
	DECLARE @where varchar(8000)
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL
		OR ISNULL(@CSSupplier,'')=''
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version,dbo.MD_LocationBin.Area, dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, 
                      dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, 
                      dbo.INV_Hu.IsOdd
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld LEFT OUTER JOIN
                      dbo.INV_Hu ON lld.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      dbo.MD_LocationBin ON lld.Bin = dbo.MD_LocationBin.Code
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1 AND lld.CSSupplier=@CSSupplier_1
				  AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty < 0 AND lld.IsCS = 0
			      AND lld.QualityType=@QualityType_1 AND lld.OccupyType=@OccupyType_1'
	SET @Parameter=N'@Item_1 varchar(50),@CSSupplier_1 varchar(50),@Location_1 varchar(50),@QualityType_1 tinyint,@OccupyType_1 tinyint'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@CSSupplier_1=@CSSupplier,@Location_1=@Location,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType
	
END


alter table dbo.SCM_FlowDet add IsActive bit null

alter table dbo.ORD_IpDet add PSTYP varchar(50) null
alter table dbo.ORD_IpDet_0 add PSTYP varchar(50) null
alter table dbo.ORD_IpDet_1 add PSTYP varchar(50) null
alter table dbo.ORD_IpDet_2 add PSTYP varchar(50) null
alter table dbo.ORD_IpDet_3 add PSTYP varchar(50) null
alter table dbo.ORD_IpDet_4 add PSTYP varchar(50) null
alter table dbo.ORD_IpDet_5 add PSTYP varchar(50) null
alter table dbo.ORD_IpDet_6 add PSTYP varchar(50) null
alter table dbo.ORD_IpDet_7 add PSTYP varchar(50) null
alter table dbo.ORD_IpDet_8 add PSTYP varchar(50) null

/****** Object:  StoredProcedure [dbo].[USP_Split_IpDet_UPDATE]    Script Date: 12/10/2012 23:17:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[USP_Split_IpDet_UPDATE]
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
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime,
	@Container varchar(50),
	@ContainerDesc varchar(50),
	@IsChangeUC bit,
	@BWART varchar(50),
	@PSTYP varchar(50),
	@Id int,
	@VersionBerfore int
)
AS
BEGIN
	IF @OrderType=1
	BEGIN
		UPDATE ORD_IpDet_1 SET Version=@Version,Type=@Type,IpNo=@IpNo,Seq=@Seq,OrderNo=@OrderNo,OrderType=@OrderType,OrderSubType=@OrderSubType,OrderDetSeq=@OrderDetSeq,OrderDetId=@OrderDetId,ExtNo=@ExtNo,ExtSeq=@ExtSeq,Flow=@Flow,Item=@Item,ItemDesc=@ItemDesc,RefItemCode=@RefItemCode,Uom=@Uom,BaseUom=@BaseUom,UC=@UC,UCDesc=@UCDesc,StartTime=@StartTime,WindowTime=@WindowTime,QualityType=@QualityType,ManufactureParty=@ManufactureParty,Qty=@Qty,RecQty=@RecQty,UnitQty=@UnitQty,LocFrom=@LocFrom,LocFromNm=@LocFromNm,LocTo=@LocTo,LocToNm=@LocToNm,IsInspect=@IsInspect,BillAddr=@BillAddr,PriceList=@PriceList,UnitPrice=@UnitPrice,Currency=@Currency,IsProvEst=@IsProvEst,Tax=@Tax,IsIncludeTax=@IsIncludeTax,BillTerm=@BillTerm,IsClose=@IsClose,GapRecNo=@GapRecNo,GapIpDetId=@GapIpDetId,BinTo=@BinTo,IsScanHu=@IsScanHu,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,Container=@Container,ContainerDesc=@ContainerDesc,IsChangeUC=@IsChangeUC,BWART=@BWART,PSTYP=@PSTYP
		WHERE Id=@Id AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=2
	BEGIN
		UPDATE ORD_IpDet_2 SET Version=@Version,Type=@Type,IpNo=@IpNo,Seq=@Seq,OrderNo=@OrderNo,OrderType=@OrderType,OrderSubType=@OrderSubType,OrderDetSeq=@OrderDetSeq,OrderDetId=@OrderDetId,ExtNo=@ExtNo,ExtSeq=@ExtSeq,Flow=@Flow,Item=@Item,ItemDesc=@ItemDesc,RefItemCode=@RefItemCode,Uom=@Uom,BaseUom=@BaseUom,UC=@UC,UCDesc=@UCDesc,StartTime=@StartTime,WindowTime=@WindowTime,QualityType=@QualityType,ManufactureParty=@ManufactureParty,Qty=@Qty,RecQty=@RecQty,UnitQty=@UnitQty,LocFrom=@LocFrom,LocFromNm=@LocFromNm,LocTo=@LocTo,LocToNm=@LocToNm,IsInspect=@IsInspect,BillAddr=@BillAddr,PriceList=@PriceList,UnitPrice=@UnitPrice,Currency=@Currency,IsProvEst=@IsProvEst,Tax=@Tax,IsIncludeTax=@IsIncludeTax,BillTerm=@BillTerm,IsClose=@IsClose,GapRecNo=@GapRecNo,GapIpDetId=@GapIpDetId,BinTo=@BinTo,IsScanHu=@IsScanHu,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,Container=@Container,ContainerDesc=@ContainerDesc,IsChangeUC=@IsChangeUC,BWART=@BWART,PSTYP=@PSTYP
		WHERE Id=@Id AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=3
	BEGIN
		UPDATE ORD_IpDet_3 SET Version=@Version,Type=@Type,IpNo=@IpNo,Seq=@Seq,OrderNo=@OrderNo,OrderType=@OrderType,OrderSubType=@OrderSubType,OrderDetSeq=@OrderDetSeq,OrderDetId=@OrderDetId,ExtNo=@ExtNo,ExtSeq=@ExtSeq,Flow=@Flow,Item=@Item,ItemDesc=@ItemDesc,RefItemCode=@RefItemCode,Uom=@Uom,BaseUom=@BaseUom,UC=@UC,UCDesc=@UCDesc,StartTime=@StartTime,WindowTime=@WindowTime,QualityType=@QualityType,ManufactureParty=@ManufactureParty,Qty=@Qty,RecQty=@RecQty,UnitQty=@UnitQty,LocFrom=@LocFrom,LocFromNm=@LocFromNm,LocTo=@LocTo,LocToNm=@LocToNm,IsInspect=@IsInspect,BillAddr=@BillAddr,PriceList=@PriceList,UnitPrice=@UnitPrice,Currency=@Currency,IsProvEst=@IsProvEst,Tax=@Tax,IsIncludeTax=@IsIncludeTax,BillTerm=@BillTerm,IsClose=@IsClose,GapRecNo=@GapRecNo,GapIpDetId=@GapIpDetId,BinTo=@BinTo,IsScanHu=@IsScanHu,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,Container=@Container,ContainerDesc=@ContainerDesc,IsChangeUC=@IsChangeUC,BWART=@BWART,PSTYP=@PSTYP
		WHERE Id=@Id AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=4
	BEGIN
		UPDATE ORD_IpDet_4 SET Version=@Version,Type=@Type,IpNo=@IpNo,Seq=@Seq,OrderNo=@OrderNo,OrderType=@OrderType,OrderSubType=@OrderSubType,OrderDetSeq=@OrderDetSeq,OrderDetId=@OrderDetId,ExtNo=@ExtNo,ExtSeq=@ExtSeq,Flow=@Flow,Item=@Item,ItemDesc=@ItemDesc,RefItemCode=@RefItemCode,Uom=@Uom,BaseUom=@BaseUom,UC=@UC,UCDesc=@UCDesc,StartTime=@StartTime,WindowTime=@WindowTime,QualityType=@QualityType,ManufactureParty=@ManufactureParty,Qty=@Qty,RecQty=@RecQty,UnitQty=@UnitQty,LocFrom=@LocFrom,LocFromNm=@LocFromNm,LocTo=@LocTo,LocToNm=@LocToNm,IsInspect=@IsInspect,BillAddr=@BillAddr,PriceList=@PriceList,UnitPrice=@UnitPrice,Currency=@Currency,IsProvEst=@IsProvEst,Tax=@Tax,IsIncludeTax=@IsIncludeTax,BillTerm=@BillTerm,IsClose=@IsClose,GapRecNo=@GapRecNo,GapIpDetId=@GapIpDetId,BinTo=@BinTo,IsScanHu=@IsScanHu,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,Container=@Container,ContainerDesc=@ContainerDesc,IsChangeUC=@IsChangeUC,BWART=@BWART,PSTYP=@PSTYP
		WHERE Id=@Id AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=5
	BEGIN
		UPDATE ORD_IpDet_5 SET Version=@Version,Type=@Type,IpNo=@IpNo,Seq=@Seq,OrderNo=@OrderNo,OrderType=@OrderType,OrderSubType=@OrderSubType,OrderDetSeq=@OrderDetSeq,OrderDetId=@OrderDetId,ExtNo=@ExtNo,ExtSeq=@ExtSeq,Flow=@Flow,Item=@Item,ItemDesc=@ItemDesc,RefItemCode=@RefItemCode,Uom=@Uom,BaseUom=@BaseUom,UC=@UC,UCDesc=@UCDesc,StartTime=@StartTime,WindowTime=@WindowTime,QualityType=@QualityType,ManufactureParty=@ManufactureParty,Qty=@Qty,RecQty=@RecQty,UnitQty=@UnitQty,LocFrom=@LocFrom,LocFromNm=@LocFromNm,LocTo=@LocTo,LocToNm=@LocToNm,IsInspect=@IsInspect,BillAddr=@BillAddr,PriceList=@PriceList,UnitPrice=@UnitPrice,Currency=@Currency,IsProvEst=@IsProvEst,Tax=@Tax,IsIncludeTax=@IsIncludeTax,BillTerm=@BillTerm,IsClose=@IsClose,GapRecNo=@GapRecNo,GapIpDetId=@GapIpDetId,BinTo=@BinTo,IsScanHu=@IsScanHu,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,Container=@Container,ContainerDesc=@ContainerDesc,IsChangeUC=@IsChangeUC,BWART=@BWART,PSTYP=@PSTYP
		WHERE Id=@Id AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=6
	BEGIN
		UPDATE ORD_IpDet_6 SET Version=@Version,Type=@Type,IpNo=@IpNo,Seq=@Seq,OrderNo=@OrderNo,OrderType=@OrderType,OrderSubType=@OrderSubType,OrderDetSeq=@OrderDetSeq,OrderDetId=@OrderDetId,ExtNo=@ExtNo,ExtSeq=@ExtSeq,Flow=@Flow,Item=@Item,ItemDesc=@ItemDesc,RefItemCode=@RefItemCode,Uom=@Uom,BaseUom=@BaseUom,UC=@UC,UCDesc=@UCDesc,StartTime=@StartTime,WindowTime=@WindowTime,QualityType=@QualityType,ManufactureParty=@ManufactureParty,Qty=@Qty,RecQty=@RecQty,UnitQty=@UnitQty,LocFrom=@LocFrom,LocFromNm=@LocFromNm,LocTo=@LocTo,LocToNm=@LocToNm,IsInspect=@IsInspect,BillAddr=@BillAddr,PriceList=@PriceList,UnitPrice=@UnitPrice,Currency=@Currency,IsProvEst=@IsProvEst,Tax=@Tax,IsIncludeTax=@IsIncludeTax,BillTerm=@BillTerm,IsClose=@IsClose,GapRecNo=@GapRecNo,GapIpDetId=@GapIpDetId,BinTo=@BinTo,IsScanHu=@IsScanHu,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,Container=@Container,ContainerDesc=@ContainerDesc,IsChangeUC=@IsChangeUC,BWART=@BWART,PSTYP=@PSTYP
		WHERE Id=@Id AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=7
	BEGIN
		UPDATE ORD_IpDet_7 SET Version=@Version,Type=@Type,IpNo=@IpNo,Seq=@Seq,OrderNo=@OrderNo,OrderType=@OrderType,OrderSubType=@OrderSubType,OrderDetSeq=@OrderDetSeq,OrderDetId=@OrderDetId,ExtNo=@ExtNo,ExtSeq=@ExtSeq,Flow=@Flow,Item=@Item,ItemDesc=@ItemDesc,RefItemCode=@RefItemCode,Uom=@Uom,BaseUom=@BaseUom,UC=@UC,UCDesc=@UCDesc,StartTime=@StartTime,WindowTime=@WindowTime,QualityType=@QualityType,ManufactureParty=@ManufactureParty,Qty=@Qty,RecQty=@RecQty,UnitQty=@UnitQty,LocFrom=@LocFrom,LocFromNm=@LocFromNm,LocTo=@LocTo,LocToNm=@LocToNm,IsInspect=@IsInspect,BillAddr=@BillAddr,PriceList=@PriceList,UnitPrice=@UnitPrice,Currency=@Currency,IsProvEst=@IsProvEst,Tax=@Tax,IsIncludeTax=@IsIncludeTax,BillTerm=@BillTerm,IsClose=@IsClose,GapRecNo=@GapRecNo,GapIpDetId=@GapIpDetId,BinTo=@BinTo,IsScanHu=@IsScanHu,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,Container=@Container,ContainerDesc=@ContainerDesc,IsChangeUC=@IsChangeUC,BWART=@BWART,PSTYP=@PSTYP
		WHERE Id=@Id AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=8
	BEGIN
		UPDATE ORD_IpDet_8 SET Version=@Version,Type=@Type,IpNo=@IpNo,Seq=@Seq,OrderNo=@OrderNo,OrderType=@OrderType,OrderSubType=@OrderSubType,OrderDetSeq=@OrderDetSeq,OrderDetId=@OrderDetId,ExtNo=@ExtNo,ExtSeq=@ExtSeq,Flow=@Flow,Item=@Item,ItemDesc=@ItemDesc,RefItemCode=@RefItemCode,Uom=@Uom,BaseUom=@BaseUom,UC=@UC,UCDesc=@UCDesc,StartTime=@StartTime,WindowTime=@WindowTime,QualityType=@QualityType,ManufactureParty=@ManufactureParty,Qty=@Qty,RecQty=@RecQty,UnitQty=@UnitQty,LocFrom=@LocFrom,LocFromNm=@LocFromNm,LocTo=@LocTo,LocToNm=@LocToNm,IsInspect=@IsInspect,BillAddr=@BillAddr,PriceList=@PriceList,UnitPrice=@UnitPrice,Currency=@Currency,IsProvEst=@IsProvEst,Tax=@Tax,IsIncludeTax=@IsIncludeTax,BillTerm=@BillTerm,IsClose=@IsClose,GapRecNo=@GapRecNo,GapIpDetId=@GapIpDetId,BinTo=@BinTo,IsScanHu=@IsScanHu,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,Container=@Container,ContainerDesc=@ContainerDesc,IsChangeUC=@IsChangeUC,BWART=@BWART,PSTYP=@PSTYP
		WHERE Id=@Id AND Version=@VersionBerfore
	END
	ELSE
	BEGIN
		UPDATE ORD_IpDet_0 SET Version=@Version,Type=@Type,IpNo=@IpNo,Seq=@Seq,OrderNo=@OrderNo,OrderType=@OrderType,OrderSubType=@OrderSubType,OrderDetSeq=@OrderDetSeq,OrderDetId=@OrderDetId,ExtNo=@ExtNo,ExtSeq=@ExtSeq,Flow=@Flow,Item=@Item,ItemDesc=@ItemDesc,RefItemCode=@RefItemCode,Uom=@Uom,BaseUom=@BaseUom,UC=@UC,UCDesc=@UCDesc,StartTime=@StartTime,WindowTime=@WindowTime,QualityType=@QualityType,ManufactureParty=@ManufactureParty,Qty=@Qty,RecQty=@RecQty,UnitQty=@UnitQty,LocFrom=@LocFrom,LocFromNm=@LocFromNm,LocTo=@LocTo,LocToNm=@LocToNm,IsInspect=@IsInspect,BillAddr=@BillAddr,PriceList=@PriceList,UnitPrice=@UnitPrice,Currency=@Currency,IsProvEst=@IsProvEst,Tax=@Tax,IsIncludeTax=@IsIncludeTax,BillTerm=@BillTerm,IsClose=@IsClose,GapRecNo=@GapRecNo,GapIpDetId=@GapIpDetId,BinTo=@BinTo,IsScanHu=@IsScanHu,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,Container=@Container,ContainerDesc=@ContainerDesc,IsChangeUC=@IsChangeUC,BWART=@BWART,PSTYP=@PSTYP
		WHERE Id=@Id AND Version=@VersionBerfore
	END						
END

//修改view_ipdet 新增bwart字段
ALTER VIEW [dbo].[VIEW_IpDet]
AS
SELECT     Id, IpNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, Qty, RecQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, PriceList, 
                      UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, IsClose, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, 
                      Version, Seq, OrderNo, OrderDetSeq, OrderType, OrderSubType, ManufactureParty, BaseUom, Type, GapRecNo, GapIpDetId, UCDesc, Container, ContainerDesc, 
                      ExtNo, ExtSeq, StartTime, WindowTime, BinTo, IsScanHu, IsChangeUC, Flow,BWART,PSTYP
FROM         dbo.ORD_IpDet_1
UNION ALL
SELECT     Id, IpNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, Qty, RecQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, PriceList, 
                      UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, IsClose, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, 
                      Version, Seq, OrderNo, OrderDetSeq, OrderType, OrderSubType, ManufactureParty, BaseUom, Type, GapRecNo, GapIpDetId, UCDesc, Container, ContainerDesc, 
                      ExtNo, ExtSeq, StartTime, WindowTime, BinTo, IsScanHu, IsChangeUC,Flow,BWART,PSTYP
FROM         dbo.ORD_IpDet_2
UNION ALL
SELECT     Id, IpNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, Qty, RecQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, PriceList, 
                      UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, IsClose, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, 
                      Version, Seq, OrderNo, OrderDetSeq, OrderType, OrderSubType, ManufactureParty, BaseUom, Type, GapRecNo, GapIpDetId, UCDesc, Container, ContainerDesc, 
                      ExtNo, ExtSeq, StartTime, WindowTime, BinTo, IsScanHu, IsChangeUC,Flow,BWART,PSTYP
FROM         dbo.ORD_IpDet_3
UNION ALL
SELECT     Id, IpNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, Qty, RecQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, PriceList, 
                      UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, IsClose, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, 
                      Version, Seq, OrderNo, OrderDetSeq, OrderType, OrderSubType, ManufactureParty, BaseUom, Type, GapRecNo, GapIpDetId, UCDesc, Container, ContainerDesc, 
                      ExtNo, ExtSeq, StartTime, WindowTime, BinTo, IsScanHu, IsChangeUC,Flow,BWART,PSTYP
FROM         dbo.ORD_IpDet_4
UNION ALL
SELECT     Id, IpNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, Qty, RecQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, PriceList, 
                      UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, IsClose, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, 
                      Version, Seq, OrderNo, OrderDetSeq, OrderType, OrderSubType, ManufactureParty, BaseUom, Type, GapRecNo, GapIpDetId, UCDesc, Container, ContainerDesc, 
                      ExtNo, ExtSeq, StartTime, WindowTime, BinTo, IsScanHu, IsChangeUC, Flow,BWART,PSTYP
FROM         dbo.ORD_IpDet_5
UNION ALL
SELECT     Id, IpNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, Qty, RecQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, PriceList, 
                      UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, IsClose, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, 
                      Version, Seq, OrderNo, OrderDetSeq, OrderType, OrderSubType, ManufactureParty, BaseUom, Type, GapRecNo, GapIpDetId, UCDesc, Container, ContainerDesc, 
                      ExtNo, ExtSeq, StartTime, WindowTime, BinTo, IsScanHu, IsChangeUC,Flow,BWART,PSTYP
FROM         dbo.ORD_IpDet_6
UNION ALL
SELECT     Id, IpNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, Qty, RecQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, PriceList, 
                      UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, IsClose, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, 
                      Version, Seq, OrderNo, OrderDetSeq, OrderType, OrderSubType, ManufactureParty, BaseUom, Type, GapRecNo, GapIpDetId, UCDesc, Container, ContainerDesc, 
                      ExtNo, ExtSeq, StartTime, WindowTime, BinTo, IsScanHu, IsChangeUC,Flow,BWART,PSTYP
FROM         dbo.ORD_IpDet_7
UNION ALL
SELECT     Id, IpNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, Qty, RecQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, PriceList, 
                      UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, IsClose, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, 
                      Version, Seq, OrderNo, OrderDetSeq, OrderType, OrderSubType, ManufactureParty, BaseUom, Type, GapRecNo, GapIpDetId, UCDesc, Container, ContainerDesc, 
                      ExtNo, ExtSeq, StartTime, WindowTime, BinTo, IsScanHu, IsChangeUC, Flow,BWART,PSTYP
FROM         dbo.ORD_IpDet_8
UNION ALL
SELECT     Id, IpNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, Qty, RecQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, PriceList, 
                      UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, IsClose, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, 
                      Version, Seq, OrderNo, OrderDetSeq, OrderType, OrderSubType, ManufactureParty, BaseUom, Type, GapRecNo, GapIpDetId, UCDesc, Container, ContainerDesc, 
                      ExtNo, ExtSeq, StartTime, WindowTime, BinTo, IsScanHu, IsChangeUC,Flow,BWART,PSTYP
FROM         dbo.ORD_IpDet_0

//ipdet表增加bwart字段用来记录移动类型
alter table dbo.ORD_IpDet add BWART varchar(50) null
alter table dbo.ORD_IpDet_0 add BWART varchar(50) null
alter table dbo.ORD_IpDet_1 add BWART varchar(50) null
alter table dbo.ORD_IpDet_2 add BWART varchar(50) null
alter table dbo.ORD_IpDet_3 add BWART varchar(50) null
alter table dbo.ORD_IpDet_4 add BWART varchar(50) null
alter table dbo.ORD_IpDet_5 add BWART varchar(50) null
alter table dbo.ORD_IpDet_6 add BWART varchar(50) null
alter table dbo.ORD_IpDet_7 add BWART varchar(50) null
alter table dbo.ORD_IpDet_8 add BWART varchar(50) null


USE [sconit_prd]
GO
/****** Object:  StoredProcedure [dbo].[USP_Split_IpDet_INSERT]    Script Date: 12/06/2012 17:13:05 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[USP_Split_IpDet_INSERT]
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
	DECLARE @Id bigint
	BEGIN TRAN
		IF EXISTS (SELECT * FROM SYS_TabIdSeq WITH (UPDLOCK,SERIALIZABLE) WHERE TabNm='ORD_IpDet')
		BEGIN
			SELECT @Id=Id+1 FROM SYS_TabIdSeq WHERE TabNm='ORD_IpDet'
			UPDATE SYS_TabIdSeq SET Id=Id+1 WHERE TabNm='ORD_IpDet'
		END
		ELSE
		BEGIN
			INSERT INTO SYS_TabIdSeq(TabNm,Id)
			VALUES('ORD_IpDet',1)
			SET @Id=1
		END
	COMMIT TRAN

	IF @OrderType=1
	BEGIN
		INSERT INTO ORD_IpDet_1(Id,Version,Type,IpNo,Seq,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,ExtNo,ExtSeq,Flow,Item,ItemDesc,RefItemCode,Uom,BaseUom,UC,UCDesc,StartTime,WindowTime,QualityType,ManufactureParty,Qty,RecQty,UnitQty,LocFrom,LocFromNm,LocTo,LocToNm,IsInspect,BillAddr,PriceList,UnitPrice,Currency,IsProvEst,Tax,IsIncludeTax,BillTerm,IsClose,GapRecNo,GapIpDetId,BinTo,IsScanHu,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,Container,ContainerDesc,IsChangeUC,BWART,PSTYP)
		VALUES(@Id,@Version,@Type,@IpNo,@Seq,@OrderNo,@OrderType,@OrderSubType,@OrderDetSeq,@OrderDetId,@ExtNo,@ExtSeq,@Flow,@Item,@ItemDesc,@RefItemCode,@Uom,@BaseUom,@UC,@UCDesc,@StartTime,@WindowTime,@QualityType,@ManufactureParty,@Qty,@RecQty,@UnitQty,@LocFrom,@LocFromNm,@LocTo,@LocToNm,@IsInspect,@BillAddr,@PriceList,@UnitPrice,@Currency,@IsProvEst,@Tax,@IsIncludeTax,@BillTerm,@IsClose,@GapRecNo,@GapIpDetId,@BinTo,@IsScanHu,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@Container,@ContainerDesc,@IsChangeUC,@BWART,@PSTYP)
	END	
	ELSE IF @OrderType=2
	BEGIN
		INSERT INTO ORD_IpDet_2(Id,Version,Type,IpNo,Seq,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,ExtNo,ExtSeq,Flow,Item,ItemDesc,RefItemCode,Uom,BaseUom,UC,UCDesc,StartTime,WindowTime,QualityType,ManufactureParty,Qty,RecQty,UnitQty,LocFrom,LocFromNm,LocTo,LocToNm,IsInspect,BillAddr,PriceList,UnitPrice,Currency,IsProvEst,Tax,IsIncludeTax,BillTerm,IsClose,GapRecNo,GapIpDetId,BinTo,IsScanHu,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,Container,ContainerDesc,IsChangeUC,BWART,PSTYP)
		VALUES(@Id,@Version,@Type,@IpNo,@Seq,@OrderNo,@OrderType,@OrderSubType,@OrderDetSeq,@OrderDetId,@ExtNo,@ExtSeq,@Flow,@Item,@ItemDesc,@RefItemCode,@Uom,@BaseUom,@UC,@UCDesc,@StartTime,@WindowTime,@QualityType,@ManufactureParty,@Qty,@RecQty,@UnitQty,@LocFrom,@LocFromNm,@LocTo,@LocToNm,@IsInspect,@BillAddr,@PriceList,@UnitPrice,@Currency,@IsProvEst,@Tax,@IsIncludeTax,@BillTerm,@IsClose,@GapRecNo,@GapIpDetId,@BinTo,@IsScanHu,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@Container,@ContainerDesc,@IsChangeUC,@BWART,@PSTYP)
	END	
	ELSE IF @OrderType=3
	BEGIN
		INSERT INTO ORD_IpDet_3(Id,Version,Type,IpNo,Seq,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,ExtNo,ExtSeq,Flow,Item,ItemDesc,RefItemCode,Uom,BaseUom,UC,UCDesc,StartTime,WindowTime,QualityType,ManufactureParty,Qty,RecQty,UnitQty,LocFrom,LocFromNm,LocTo,LocToNm,IsInspect,BillAddr,PriceList,UnitPrice,Currency,IsProvEst,Tax,IsIncludeTax,BillTerm,IsClose,GapRecNo,GapIpDetId,BinTo,IsScanHu,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,Container,ContainerDesc,IsChangeUC,BWART,PSTYP)
		VALUES(@Id,@Version,@Type,@IpNo,@Seq,@OrderNo,@OrderType,@OrderSubType,@OrderDetSeq,@OrderDetId,@ExtNo,@ExtSeq,@Flow,@Item,@ItemDesc,@RefItemCode,@Uom,@BaseUom,@UC,@UCDesc,@StartTime,@WindowTime,@QualityType,@ManufactureParty,@Qty,@RecQty,@UnitQty,@LocFrom,@LocFromNm,@LocTo,@LocToNm,@IsInspect,@BillAddr,@PriceList,@UnitPrice,@Currency,@IsProvEst,@Tax,@IsIncludeTax,@BillTerm,@IsClose,@GapRecNo,@GapIpDetId,@BinTo,@IsScanHu,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@Container,@ContainerDesc,@IsChangeUC,@BWART,@PSTYP)
	END	
	ELSE IF @OrderType=4
	BEGIN
		INSERT INTO ORD_IpDet_4(Id,Version,Type,IpNo,Seq,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,ExtNo,ExtSeq,Flow,Item,ItemDesc,RefItemCode,Uom,BaseUom,UC,UCDesc,StartTime,WindowTime,QualityType,ManufactureParty,Qty,RecQty,UnitQty,LocFrom,LocFromNm,LocTo,LocToNm,IsInspect,BillAddr,PriceList,UnitPrice,Currency,IsProvEst,Tax,IsIncludeTax,BillTerm,IsClose,GapRecNo,GapIpDetId,BinTo,IsScanHu,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,Container,ContainerDesc,IsChangeUC,BWART,PSTYP)
		VALUES(@Id,@Version,@Type,@IpNo,@Seq,@OrderNo,@OrderType,@OrderSubType,@OrderDetSeq,@OrderDetId,@ExtNo,@ExtSeq,@Flow,@Item,@ItemDesc,@RefItemCode,@Uom,@BaseUom,@UC,@UCDesc,@StartTime,@WindowTime,@QualityType,@ManufactureParty,@Qty,@RecQty,@UnitQty,@LocFrom,@LocFromNm,@LocTo,@LocToNm,@IsInspect,@BillAddr,@PriceList,@UnitPrice,@Currency,@IsProvEst,@Tax,@IsIncludeTax,@BillTerm,@IsClose,@GapRecNo,@GapIpDetId,@BinTo,@IsScanHu,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@Container,@ContainerDesc,@IsChangeUC,@BWART,@PSTYP)
	END	
	ELSE IF @OrderType=5
	BEGIN
		INSERT INTO ORD_IpDet_5(Id,Version,Type,IpNo,Seq,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,ExtNo,ExtSeq,Flow,Item,ItemDesc,RefItemCode,Uom,BaseUom,UC,UCDesc,StartTime,WindowTime,QualityType,ManufactureParty,Qty,RecQty,UnitQty,LocFrom,LocFromNm,LocTo,LocToNm,IsInspect,BillAddr,PriceList,UnitPrice,Currency,IsProvEst,Tax,IsIncludeTax,BillTerm,IsClose,GapRecNo,GapIpDetId,BinTo,IsScanHu,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,Container,ContainerDesc,IsChangeUC,BWART,PSTYP)
		VALUES(@Id,@Version,@Type,@IpNo,@Seq,@OrderNo,@OrderType,@OrderSubType,@OrderDetSeq,@OrderDetId,@ExtNo,@ExtSeq,@Flow,@Item,@ItemDesc,@RefItemCode,@Uom,@BaseUom,@UC,@UCDesc,@StartTime,@WindowTime,@QualityType,@ManufactureParty,@Qty,@RecQty,@UnitQty,@LocFrom,@LocFromNm,@LocTo,@LocToNm,@IsInspect,@BillAddr,@PriceList,@UnitPrice,@Currency,@IsProvEst,@Tax,@IsIncludeTax,@BillTerm,@IsClose,@GapRecNo,@GapIpDetId,@BinTo,@IsScanHu,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@Container,@ContainerDesc,@IsChangeUC,@BWART,@PSTYP)
	END	
	ELSE IF @OrderType=6
	BEGIN
		INSERT INTO ORD_IpDet_6(Id,Version,Type,IpNo,Seq,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,ExtNo,ExtSeq,Flow,Item,ItemDesc,RefItemCode,Uom,BaseUom,UC,UCDesc,StartTime,WindowTime,QualityType,ManufactureParty,Qty,RecQty,UnitQty,LocFrom,LocFromNm,LocTo,LocToNm,IsInspect,BillAddr,PriceList,UnitPrice,Currency,IsProvEst,Tax,IsIncludeTax,BillTerm,IsClose,GapRecNo,GapIpDetId,BinTo,IsScanHu,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,Container,ContainerDesc,IsChangeUC,BWART,PSTYP)
		VALUES(@Id,@Version,@Type,@IpNo,@Seq,@OrderNo,@OrderType,@OrderSubType,@OrderDetSeq,@OrderDetId,@ExtNo,@ExtSeq,@Flow,@Item,@ItemDesc,@RefItemCode,@Uom,@BaseUom,@UC,@UCDesc,@StartTime,@WindowTime,@QualityType,@ManufactureParty,@Qty,@RecQty,@UnitQty,@LocFrom,@LocFromNm,@LocTo,@LocToNm,@IsInspect,@BillAddr,@PriceList,@UnitPrice,@Currency,@IsProvEst,@Tax,@IsIncludeTax,@BillTerm,@IsClose,@GapRecNo,@GapIpDetId,@BinTo,@IsScanHu,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@Container,@ContainerDesc,@IsChangeUC,@BWART,@PSTYP)
	END	
	ELSE IF @OrderType=7
	BEGIN
		INSERT INTO ORD_IpDet_7(Id,Version,Type,IpNo,Seq,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,ExtNo,ExtSeq,Flow,Item,ItemDesc,RefItemCode,Uom,BaseUom,UC,UCDesc,StartTime,WindowTime,QualityType,ManufactureParty,Qty,RecQty,UnitQty,LocFrom,LocFromNm,LocTo,LocToNm,IsInspect,BillAddr,PriceList,UnitPrice,Currency,IsProvEst,Tax,IsIncludeTax,BillTerm,IsClose,GapRecNo,GapIpDetId,BinTo,IsScanHu,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,Container,ContainerDesc,IsChangeUC,BWART,PSTYP)
		VALUES(@Id,@Version,@Type,@IpNo,@Seq,@OrderNo,@OrderType,@OrderSubType,@OrderDetSeq,@OrderDetId,@ExtNo,@ExtSeq,@Flow,@Item,@ItemDesc,@RefItemCode,@Uom,@BaseUom,@UC,@UCDesc,@StartTime,@WindowTime,@QualityType,@ManufactureParty,@Qty,@RecQty,@UnitQty,@LocFrom,@LocFromNm,@LocTo,@LocToNm,@IsInspect,@BillAddr,@PriceList,@UnitPrice,@Currency,@IsProvEst,@Tax,@IsIncludeTax,@BillTerm,@IsClose,@GapRecNo,@GapIpDetId,@BinTo,@IsScanHu,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@Container,@ContainerDesc,@IsChangeUC,@BWART,@PSTYP)
	END
	ELSE IF @OrderType=8
	BEGIN
		INSERT INTO ORD_IpDet_8(Id,Version,Type,IpNo,Seq,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,ExtNo,ExtSeq,Flow,Item,ItemDesc,RefItemCode,Uom,BaseUom,UC,UCDesc,StartTime,WindowTime,QualityType,ManufactureParty,Qty,RecQty,UnitQty,LocFrom,LocFromNm,LocTo,LocToNm,IsInspect,BillAddr,PriceList,UnitPrice,Currency,IsProvEst,Tax,IsIncludeTax,BillTerm,IsClose,GapRecNo,GapIpDetId,BinTo,IsScanHu,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,Container,ContainerDesc,IsChangeUC,BWART,PSTYP)
		VALUES(@Id,@Version,@Type,@IpNo,@Seq,@OrderNo,@OrderType,@OrderSubType,@OrderDetSeq,@OrderDetId,@ExtNo,@ExtSeq,@Flow,@Item,@ItemDesc,@RefItemCode,@Uom,@BaseUom,@UC,@UCDesc,@StartTime,@WindowTime,@QualityType,@ManufactureParty,@Qty,@RecQty,@UnitQty,@LocFrom,@LocFromNm,@LocTo,@LocToNm,@IsInspect,@BillAddr,@PriceList,@UnitPrice,@Currency,@IsProvEst,@Tax,@IsIncludeTax,@BillTerm,@IsClose,@GapRecNo,@GapIpDetId,@BinTo,@IsScanHu,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@Container,@ContainerDesc,@IsChangeUC,@BWART,@PSTYP)
	END				
	ELSE
	BEGIN
		INSERT INTO ORD_IpDet_0(Id,Version,Type,IpNo,Seq,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,ExtNo,ExtSeq,Flow,Item,ItemDesc,RefItemCode,Uom,BaseUom,UC,UCDesc,StartTime,WindowTime,QualityType,ManufactureParty,Qty,RecQty,UnitQty,LocFrom,LocFromNm,LocTo,LocToNm,IsInspect,BillAddr,PriceList,UnitPrice,Currency,IsProvEst,Tax,IsIncludeTax,BillTerm,IsClose,GapRecNo,GapIpDetId,BinTo,IsScanHu,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,Container,ContainerDesc,IsChangeUC,BWART,PSTYP)
		VALUES(@Id,@Version,@Type,@IpNo,@Seq,@OrderNo,@OrderType,@OrderSubType,@OrderDetSeq,@OrderDetId,@ExtNo,@ExtSeq,@Flow,@Item,@ItemDesc,@RefItemCode,@Uom,@BaseUom,@UC,@UCDesc,@StartTime,@WindowTime,@QualityType,@ManufactureParty,@Qty,@RecQty,@UnitQty,@LocFrom,@LocFromNm,@LocTo,@LocToNm,@IsInspect,@BillAddr,@PriceList,@UnitPrice,@Currency,@IsProvEst,@Tax,@IsIncludeTax,@BillTerm,@IsClose,@GapRecNo,@GapIpDetId,@BinTo,@IsScanHu,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@Container,@ContainerDesc,@IsChangeUC,@BWART,@PSTYP)
	END		
	
	SELECT @Id							
END


//生产单冲销
ALTER PROCEDURE [dbo].[USP_Split_RecMstr_INSERT]
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
	@CreateHuOpt bit,
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
	IF @OrderType=1
	BEGIN
		INSERT INTO ORD_RecMstr_1(Version,ExtRecNo,IpNo,SeqNo,Flow,Status,Type,OrderType,OrderSubType,QualityType,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,EffDate,IsPrintRec,IsRecPrinted,CreateHuOpt,IsRecScanHu,RecTemplate,WMSNo,CancelReason,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,IsCheckPartyFromAuth,IsCheckPartyToAuth,RecNo)
		VALUES(@Version,@ExtRecNo,@IpNo,@SeqNo,@Flow,@Status,@Type,@OrderType,@OrderSubType,@QualityType,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@EffDate,@IsPrintRec,@IsRecPrinted,@CreateHuOpt,@IsRecScanHu,@RecTemplate,@WMSNo,@CancelReason,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@RecNo)
	END	
	ELSE IF @OrderType=2
	BEGIN
		INSERT INTO ORD_RecMstr_2(Version,ExtRecNo,IpNo,SeqNo,Flow,Status,Type,OrderType,OrderSubType,QualityType,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,EffDate,IsPrintRec,IsRecPrinted,CreateHuOpt,IsRecScanHu,RecTemplate,WMSNo,CancelReason,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,IsCheckPartyFromAuth,IsCheckPartyToAuth,RecNo)
		VALUES(@Version,@ExtRecNo,@IpNo,@SeqNo,@Flow,@Status,@Type,@OrderType,@OrderSubType,@QualityType,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@EffDate,@IsPrintRec,@IsRecPrinted,@CreateHuOpt,@IsRecScanHu,@RecTemplate,@WMSNo,@CancelReason,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@RecNo)
	END	
	ELSE IF @OrderType=3
	BEGIN
		INSERT INTO ORD_RecMstr_3(Version,ExtRecNo,IpNo,SeqNo,Flow,Status,Type,OrderType,OrderSubType,QualityType,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,EffDate,IsPrintRec,IsRecPrinted,CreateHuOpt,IsRecScanHu,RecTemplate,WMSNo,CancelReason,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,IsCheckPartyFromAuth,IsCheckPartyToAuth,RecNo)
		VALUES(@Version,@ExtRecNo,@IpNo,@SeqNo,@Flow,@Status,@Type,@OrderType,@OrderSubType,@QualityType,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@EffDate,@IsPrintRec,@IsRecPrinted,@CreateHuOpt,@IsRecScanHu,@RecTemplate,@WMSNo,@CancelReason,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@RecNo)
	END	
	ELSE IF @OrderType=4
	BEGIN
		INSERT INTO ORD_RecMstr_4(Version,ExtRecNo,IpNo,SeqNo,Flow,Status,Type,OrderType,OrderSubType,QualityType,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,EffDate,IsPrintRec,IsRecPrinted,CreateHuOpt,IsRecScanHu,RecTemplate,WMSNo,CancelReason,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,IsCheckPartyFromAuth,IsCheckPartyToAuth,RecNo)
		VALUES(@Version,@ExtRecNo,@IpNo,@SeqNo,@Flow,@Status,@Type,@OrderType,@OrderSubType,@QualityType,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@EffDate,@IsPrintRec,@IsRecPrinted,@CreateHuOpt,@IsRecScanHu,@RecTemplate,@WMSNo,@CancelReason,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@RecNo)
	END	
	ELSE IF @OrderType=5
	BEGIN
		INSERT INTO ORD_RecMstr_5(Version,ExtRecNo,IpNo,SeqNo,Flow,Status,Type,OrderType,OrderSubType,QualityType,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,EffDate,IsPrintRec,IsRecPrinted,CreateHuOpt,IsRecScanHu,RecTemplate,WMSNo,CancelReason,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,IsCheckPartyFromAuth,IsCheckPartyToAuth,RecNo)
		VALUES(@Version,@ExtRecNo,@IpNo,@SeqNo,@Flow,@Status,@Type,@OrderType,@OrderSubType,@QualityType,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@EffDate,@IsPrintRec,@IsRecPrinted,@CreateHuOpt,@IsRecScanHu,@RecTemplate,@WMSNo,@CancelReason,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@RecNo)
	END	
	ELSE IF @OrderType=6
	BEGIN
		INSERT INTO ORD_RecMstr_6(Version,ExtRecNo,IpNo,SeqNo,Flow,Status,Type,OrderType,OrderSubType,QualityType,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,EffDate,IsPrintRec,IsRecPrinted,CreateHuOpt,IsRecScanHu,RecTemplate,WMSNo,CancelReason,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,IsCheckPartyFromAuth,IsCheckPartyToAuth,RecNo)
		VALUES(@Version,@ExtRecNo,@IpNo,@SeqNo,@Flow,@Status,@Type,@OrderType,@OrderSubType,@QualityType,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@EffDate,@IsPrintRec,@IsRecPrinted,@CreateHuOpt,@IsRecScanHu,@RecTemplate,@WMSNo,@CancelReason,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@RecNo)
	END	
	ELSE IF @OrderType=7
	BEGIN
		INSERT INTO ORD_RecMstr_7(Version,ExtRecNo,IpNo,SeqNo,Flow,Status,Type,OrderType,OrderSubType,QualityType,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,EffDate,IsPrintRec,IsRecPrinted,CreateHuOpt,IsRecScanHu,RecTemplate,WMSNo,CancelReason,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,IsCheckPartyFromAuth,IsCheckPartyToAuth,RecNo)
		VALUES(@Version,@ExtRecNo,@IpNo,@SeqNo,@Flow,@Status,@Type,@OrderType,@OrderSubType,@QualityType,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@EffDate,@IsPrintRec,@IsRecPrinted,@CreateHuOpt,@IsRecScanHu,@RecTemplate,@WMSNo,@CancelReason,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@RecNo)
	END	
	ELSE IF @OrderType=8
	BEGIN
		INSERT INTO ORD_RecMstr_8(Version,ExtRecNo,IpNo,SeqNo,Flow,Status,Type,OrderType,OrderSubType,QualityType,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,EffDate,IsPrintRec,IsRecPrinted,CreateHuOpt,IsRecScanHu,RecTemplate,WMSNo,CancelReason,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,IsCheckPartyFromAuth,IsCheckPartyToAuth,RecNo)
		VALUES(@Version,@ExtRecNo,@IpNo,@SeqNo,@Flow,@Status,@Type,@OrderType,@OrderSubType,@QualityType,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@EffDate,@IsPrintRec,@IsRecPrinted,@CreateHuOpt,@IsRecScanHu,@RecTemplate,@WMSNo,@CancelReason,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@RecNo)
	END	
	ELSE
	BEGIN
		INSERT INTO ORD_RecMstr_0(Version,ExtRecNo,IpNo,SeqNo,Flow,Status,Type,OrderType,OrderSubType,QualityType,PartyFrom,PartyFromNm,PartyTo,PartyToNm,ShipFrom,ShipFromAddr,ShipFromTel,ShipFromCell,ShipFromFax,ShipFromContact,ShipTo,ShipToAddr,ShipToTel,ShipToCell,ShipToFax,ShipToContact,Dock,EffDate,IsPrintRec,IsRecPrinted,CreateHuOpt,IsRecScanHu,RecTemplate,WMSNo,CancelReason,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate,IsCheckPartyFromAuth,IsCheckPartyToAuth,RecNo)
		VALUES(@Version,@ExtRecNo,@IpNo,@SeqNo,@Flow,@Status,@Type,@OrderType,@OrderSubType,@QualityType,@PartyFrom,@PartyFromNm,@PartyTo,@PartyToNm,@ShipFrom,@ShipFromAddr,@ShipFromTel,@ShipFromCell,@ShipFromFax,@ShipFromContact,@ShipTo,@ShipToAddr,@ShipToTel,@ShipToCell,@ShipToFax,@ShipToContact,@Dock,@EffDate,@IsPrintRec,@IsRecPrinted,@CreateHuOpt,@IsRecScanHu,@RecTemplate,@WMSNo,@CancelReason,@CreateUser,@CreateUserNm,@CreateDate,@LastModifyUser,@LastModifyUserNm,@LastModifyDate,@IsCheckPartyFromAuth,@IsCheckPartyToAuth,@RecNo)
	END		
END



/****** Object:  StoredProcedure [dbo].[USP_Split_RecMstr_UPDATE]    Script Date: 11/29/2012 22:12:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[USP_Split_RecMstr_UPDATE]
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
	@CreateHuOpt bit,
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
	IF @OrderType=1
	BEGIN
		UPDATE ORD_RecMstr_1 SET Version=@Version,ExtRecNo=@ExtRecNo,IpNo=@IpNo,SeqNo=@SeqNo,Flow=@Flow,Status=@Status,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,EffDate=@EffDate,IsPrintRec=@IsPrintRec,IsRecPrinted=@IsRecPrinted,CreateHuOpt=@CreateHuOpt,IsRecScanHu=@IsRecScanHu,RecTemplate=@RecTemplate,WMSNo=@WMSNo,CancelReason = @CancelReason,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,RecNo=@RecNo
		WHERE RecNo=@RecNo AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=2
	BEGIN
		UPDATE ORD_RecMstr_2 SET Version=@Version,ExtRecNo=@ExtRecNo,IpNo=@IpNo,SeqNo=@SeqNo,Flow=@Flow,Status=@Status,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,EffDate=@EffDate,IsPrintRec=@IsPrintRec,IsRecPrinted=@IsRecPrinted,CreateHuOpt=@CreateHuOpt,IsRecScanHu=@IsRecScanHu,RecTemplate=@RecTemplate,WMSNo=@WMSNo,CancelReason = @CancelReason,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,RecNo=@RecNo
		WHERE RecNo=@RecNo AND Version=@VersionBerfore
	END	
	ELSE IF @OrderType=3
	BEGIN
		UPDATE ORD_RecMstr_3 SET Version=@Version,ExtRecNo=@ExtRecNo,IpNo=@IpNo,SeqNo=@SeqNo,Flow=@Flow,Status=@Status,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,EffDate=@EffDate,IsPrintRec=@IsPrintRec,IsRecPrinted=@IsRecPrinted,CreateHuOpt=@CreateHuOpt,IsRecScanHu=@IsRecScanHu,RecTemplate=@RecTemplate,WMSNo=@WMSNo,CancelReason = @CancelReason,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,RecNo=@RecNo
		WHERE RecNo=@RecNo AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=4
	BEGIN
		UPDATE ORD_RecMstr_4 SET Version=@Version,ExtRecNo=@ExtRecNo,IpNo=@IpNo,SeqNo=@SeqNo,Flow=@Flow,Status=@Status,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,EffDate=@EffDate,IsPrintRec=@IsPrintRec,IsRecPrinted=@IsRecPrinted,CreateHuOpt=@CreateHuOpt,IsRecScanHu=@IsRecScanHu,RecTemplate=@RecTemplate,WMSNo=@WMSNo,CancelReason = @CancelReason,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,RecNo=@RecNo
		WHERE RecNo=@RecNo AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=5
	BEGIN
		UPDATE ORD_RecMstr_5 SET Version=@Version,ExtRecNo=@ExtRecNo,IpNo=@IpNo,SeqNo=@SeqNo,Flow=@Flow,Status=@Status,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,EffDate=@EffDate,IsPrintRec=@IsPrintRec,IsRecPrinted=@IsRecPrinted,CreateHuOpt=@CreateHuOpt,IsRecScanHu=@IsRecScanHu,RecTemplate=@RecTemplate,WMSNo=@WMSNo,CancelReason = @CancelReason,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,RecNo=@RecNo
		WHERE RecNo=@RecNo AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=6
	BEGIN
		UPDATE ORD_RecMstr_6 SET Version=@Version,ExtRecNo=@ExtRecNo,IpNo=@IpNo,SeqNo=@SeqNo,Flow=@Flow,Status=@Status,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,EffDate=@EffDate,IsPrintRec=@IsPrintRec,IsRecPrinted=@IsRecPrinted,CreateHuOpt=@CreateHuOpt,IsRecScanHu=@IsRecScanHu,RecTemplate=@RecTemplate,WMSNo=@WMSNo,CancelReason = @CancelReason,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,RecNo=@RecNo
		WHERE RecNo=@RecNo AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=7
	BEGIN
		UPDATE ORD_RecMstr_7 SET Version=@Version,ExtRecNo=@ExtRecNo,IpNo=@IpNo,SeqNo=@SeqNo,Flow=@Flow,Status=@Status,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,EffDate=@EffDate,IsPrintRec=@IsPrintRec,IsRecPrinted=@IsRecPrinted,CreateHuOpt=@CreateHuOpt,IsRecScanHu=@IsRecScanHu,RecTemplate=@RecTemplate,WMSNo=@WMSNo,CancelReason = @CancelReason,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,RecNo=@RecNo
		WHERE RecNo=@RecNo AND Version=@VersionBerfore
	END
	ELSE IF @OrderType=8
	BEGIN
		UPDATE ORD_RecMstr_8 SET Version=@Version,ExtRecNo=@ExtRecNo,IpNo=@IpNo,SeqNo=@SeqNo,Flow=@Flow,Status=@Status,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,EffDate=@EffDate,IsPrintRec=@IsPrintRec,IsRecPrinted=@IsRecPrinted,CreateHuOpt=@CreateHuOpt,IsRecScanHu=@IsRecScanHu,RecTemplate=@RecTemplate,WMSNo=@WMSNo,CancelReason = @CancelReason,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,RecNo=@RecNo
		WHERE RecNo=@RecNo AND Version=@VersionBerfore
	END
	ELSE
	BEGIN
		UPDATE ORD_RecMstr_0 SET Version=@Version,ExtRecNo=@ExtRecNo,IpNo=@IpNo,SeqNo=@SeqNo,Flow=@Flow,Status=@Status,Type=@Type,OrderType=@OrderType,OrderSubType=@OrderSubType,QualityType=@QualityType,PartyFrom=@PartyFrom,PartyFromNm=@PartyFromNm,PartyTo=@PartyTo,PartyToNm=@PartyToNm,ShipFrom=@ShipFrom,ShipFromAddr=@ShipFromAddr,ShipFromTel=@ShipFromTel,ShipFromCell=@ShipFromCell,ShipFromFax=@ShipFromFax,ShipFromContact=@ShipFromContact,ShipTo=@ShipTo,ShipToAddr=@ShipToAddr,ShipToTel=@ShipToTel,ShipToCell=@ShipToCell,ShipToFax=@ShipToFax,ShipToContact=@ShipToContact,Dock=@Dock,EffDate=@EffDate,IsPrintRec=@IsPrintRec,IsRecPrinted=@IsRecPrinted,CreateHuOpt=@CreateHuOpt,IsRecScanHu=@IsRecScanHu,RecTemplate=@RecTemplate,WMSNo=@WMSNo,CancelReason = @CancelReason,LastModifyUser=@LastModifyUser,LastModifyUserNm=@LastModifyUserNm,LastModifyDate=@LastModifyDate,IsCheckPartyFromAuth=@IsCheckPartyFromAuth,IsCheckPartyToAuth=@IsCheckPartyToAuth,RecNo=@RecNo
		WHERE RecNo=@RecNo AND Version=@VersionBerfore
	END	
END




Use [sconit5_Si]
alter table dbo.SI_SAP_ProdOrder add WORKCENTER varchar(50) null

alter table ORD_OrderBackflushDet add FGItem varchar(50) not null
alter table ORD_OrderBackflushDet
add constraint FK_ORD_ORDE_REFERENCE_MD_ITEM4 foreign key (FGItem)
references MD_Item (Code)
go

alter table ORD_OrderBackflushDet add IsVoid bit not null
alter table PRD_PlanBackflush add FGItem varchar(50) not null
alter table PRD_PlanBackflush
add constraint FK_PRD_PLAN_REFERENCE_MD_ITEM2 foreign key (FGItem)
references MD_Item (Code)
go

USE [sconit5]
GO
/****** Object:  StoredProcedure [dbo].[USP_Busi_GetPlusCSInventory]    Script Date: 11/09/2012 11:39:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[USP_Busi_GetPlusCSInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@QualityType tinyint,
	@OccupyType tinyint,
	@CSSupplier varchar(50)
)
AS
BEGIN
/*******************获取正数寄售库存数据*********************************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetPlusCSInventory] 'ZAA02R','-CN6T',0,0,'0000102681'
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @sql varchar(max)
	DECLARE @where varchar(8000)
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL
		OR ISNULL(@CSSupplier,'')=''
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version,dbo.MD_LocationBin.Area, dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, 
                      dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, 
                      dbo.INV_Hu.IsOdd
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld LEFT OUTER JOIN
                      dbo.INV_Hu ON lld.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      dbo.MD_LocationBin ON lld.Bin = dbo.MD_LocationBin.Code
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1 AND lld.CSSupplier=@CSSupplier_1
				    AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty > 0 AND lld.IsCS = 1					 
					AND lld.QualityType=@QualityType_1 AND lld.OccupyType=@OccupyType_1'
	SET @Parameter=N'@Item_1 varchar(50),@CSSupplier_1 varchar(50),@Location_1 varchar(50),@QualityType_1 tinyint,@OccupyType_1 tinyint'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@CSSupplier_1=@CSSupplier,@Location_1=@Location,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType
	
END

USE [sconit5]
GO
/****** Object:  StoredProcedure [dbo].[USP_Busi_GetPlusInventory]    Script Date: 11/09/2012 11:38:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[USP_Busi_GetPlusInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@QualityType tinyint,
	@OccupyType tinyint,
	@IsConsignment bit
)
AS
BEGIN
/*******************获取正数库存数据*********************************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetMinusInventory] 'CS0010','5801306476',0,0,1
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL OR @IsConsignment IS NULL
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version,dbo.MD_LocationBin.Area, dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, 
                      dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, 
                      dbo.INV_Hu.IsOdd
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld LEFT OUTER JOIN
                      dbo.INV_Hu ON lld.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      dbo.MD_LocationBin ON lld.Bin = dbo.MD_LocationBin.Code
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1 
				    AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty > 0 AND lld.QualityType=@QualityType_1 
					AND lld.OccupyType=@OccupyType_1 AND lld.IsCS=@IsCS_1'
	SET @Parameter=N'@Item_1 varchar(50),@QualityType_1 tinyint,@OccupyType_1 tinyint,@IsCS_1 bit,@Location_1 varchar(50)'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType,@IsCS_1=@IsConsignment,@Location_1=@Location
	
END

USE [sconit5]
GO
/****** Object:  StoredProcedure [dbo].[USP_Split_LocationLotDet_INSERT]    Script Date: 11/09/2012 11:37:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[USP_Split_LocationLotDet_INSERT]
(
	@Version int,
	@Location varchar(50),
	@Bin varchar(50),
	@Item varchar(50),
	@LotNo varchar(50),
	@HuId varchar(50),
	@Qty decimal(18,8),
	@IsCS bit,
	@PlanBill int,
	@CSSupplier varchar(50),
	@QualityType tinyint,
	@IsFreeze bit,
	@IsATP bit,
	@OccupyType tinyint,
	@OccupyRefNo varchar(50),
	@CreateUser int,
	@CreateUserNm varchar(100),
	@CreateDate datetime,
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime
)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @Id bigint
	BEGIN TRAN
		IF EXISTS (SELECT * FROM SYS_TabIdSeq WITH (UPDLOCK,SERIALIZABLE) WHERE TabNm='INV_LocationLotDet')
		BEGIN
			SELECT @Id=Id+1 FROM SYS_TabIdSeq WHERE TabNm='INV_LocationLotDet'
			UPDATE SYS_TabIdSeq SET Id=Id+1 WHERE TabNm='INV_LocationLotDet'
		END
		ELSE
		BEGIN
			INSERT INTO SYS_TabIdSeq(TabNm,Id)
			VALUES('INV_LocationLotDet',1)
			SET @Id=1
		END
	COMMIT TRAN
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
		--RAISERROR ('Can''t get any data from table' , 16, 1) WITH NOWAIT
	END 
	
	SET @Statement=N'INSERT INTO INV_LocationLotDet_'+@PartSuffix+'(Id,Version,Location,Bin,Item,LotNo,HuId,Qty,IsCS,PlanBill,CSSupplier,QualityType,IsFreeze,IsATP,OccupyType,OccupyRefNo,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate) VALUES (@Id_1,@Version_1,@Location_1,@Bin_1,@Item_1,@LotNo_1,@HuId_1,@Qty_1,@IsCS_1,@PlanBill_1,@CSSupplier_1,@QualityType_1,@IsFreeze_1,@IsATP_1,@OccupyType_1,@OccupyRefNo_1,@CreateUser_1,@CreateUserNm_1,@CreateDate_1,@LastModifyUser_1,@LastModifyUserNm_1,@LastModifyDate_1)'
	SET @Parameter=N'@Id_1 int,@Version_1 int,@Location_1 varchar(50),@Bin_1 varchar(50),@Item_1 varchar(50),@LotNo_1 varchar(50),@HuId_1 varchar(50),@Qty_1 decimal(18,8),@IsCS_1 bit,@PlanBill_1 int,@CSSupplier_1 varchar(50),@QualityType_1 tinyint,@IsFreeze_1 bit,@IsATP_1 bit,@OccupyType_1 tinyint,@OccupyRefNo_1 varchar(50),@CreateUser_1 int,@CreateUserNm_1 varchar(100),@CreateDate_1 datetime,@LastModifyUser_1 int,@LastModifyUserNm_1 varchar(100),@LastModifyDate_1 datetime'
	
	exec sp_executesql @Statement,@Parameter,
		@Id_1=@Id,@Version_1=@Version,@Location_1=@Location,@Bin_1=@Bin,@Item_1=@Item,@LotNo_1=@LotNo,@HuId_1=@HuId,
		@Qty_1=@Qty,@IsCS_1=@IsCS,@PlanBill_1=@PlanBill,@CSSupplier_1=@CSSupplier,@QualityType_1=@QualityType,@IsFreeze_1=@IsFreeze,@IsATP_1=@IsATP,
		@OccupyType_1=@OccupyType,@OccupyRefNo_1=@OccupyRefNo,@CreateUser_1=@CreateUser,@CreateUserNm_1=@CreateUserNm,
		@CreateDate_1=@CreateDate,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,@LastModifyDate_1=@LastModifyDate	
	SELECT @Id
END


USE [sconit5]
GO
/****** Object:  StoredProcedure [dbo].[USP_Split_LocationLotDet_UPDATE]    Script Date: 11/09/2012 11:27:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[USP_Split_LocationLotDet_UPDATE]
(
	@Version int,
	@Location varchar(50),
	@Bin varchar(50),
	@Item varchar(50),
	@LotNo varchar(50),
	@HuId varchar(50),
	@Qty decimal(18,8),
	@IsCS bit,
	@PlanBill int,
	@CSSupplier varchar(50),
	@QualityType tinyint,
	@IsFreeze bit,
	@IsATP bit,
	@OccupyType tinyint,
	@OccupyRefNo varchar(50),
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime,
	@Id int,
	@VersionBerfore int
)
AS
BEGIN
	--SET NOCOUNT ON;
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	SET @Statement='UPDATE INV_LocationLotDet_'+@PartSuffix+' SET Version=@Version_1,Location=@Location_1,Bin=@Bin_1,Item=@Item_1,LotNo=@LotNo_1,HuId=@HuId_1,Qty=@Qty_1,IsCS=@IsCS_1,PlanBill=@PlanBill_1,CSSupplier=@CSSupplier_1,QualityType=@QualityType_1,IsFreeze=@IsFreeze_1,IsATP=@IsATP_1,OccupyType=@OccupyType_1,OccupyRefNo=@OccupyRefNo_1,LastModifyUser=@LastModifyUser_1,LastModifyUserNm=@LastModifyUserNm_1,LastModifyDate=@LastModifyDate_1 WHERE Id=@Id_1 AND Version=@VersionBerfore_1'
	SET @Parameter=N'@Version_1 int,@Location_1 varchar(50),@Bin_1 varchar(50),@Item_1 varchar(50),@LotNo_1 varchar(50),@HuId_1 varchar(50),@Qty_1 decimal(18,8),@IsCS_1 bit,@PlanBill_1 int,@CSSupplier_1 varchar(50),@QualityType_1 tinyint,@IsFreeze_1 bit,@IsATP_1 bit,@OccupyType_1 tinyint,@OccupyRefNo_1 varchar(50),@LastModifyUser_1 int,@LastModifyUserNm_1 varchar(100),@LastModifyDate_1 datetime,@Id_1 int,@VersionBerfore_1 int'
	PRINT @Statement

	exec sp_executesql @Statement,@Parameter,
		@Version_1=@Version,@Location_1=@Location,@Bin_1=@Bin,@Item_1=@Item,@LotNo_1=@LotNo,@HuId_1=@HuId,@Qty_1=@Qty,
		@IsCS_1=@IsCS,@PlanBill_1=@PlanBill,@CSSupplier_1=@CSSupplier,@QualityType_1=@QualityType,@IsFreeze_1=@IsFreeze,@IsATP_1=@IsATP,@OccupyType_1=@OccupyType,
		@OccupyRefNo_1=@OccupyRefNo,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,
		@LastModifyDate_1=@LastModifyDate,@Id_1=@Id,@VersionBerfore_1=@VersionBerfore
END



USE [sconit5]
GO
/****** Object:  StoredProcedure [dbo].[USP_Report_RealTimeLocationDet]    Script Date: 10/26/2012 16:19:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[USP_Report_RealTimeLocationDet]
(
	@Locations varchar(8000),
	@Items varchar(8000),
	@SortDesc varchar(100),
	@PageSize int,
	@Page int,
	@IsSummaryBySAPLoc bit,
	@SummaryLevel int,
	@IsGroupByManufactureParty bit,
	@IsGroupByLotNo bit
)
AS
BEGIN
/*
exec USP_Report_RealTimeLocationDet '','500368184','',100,1,0,0,1,1
500368184
*/
	SET NOCOUNT ON
	DECLARE @Sql varchar(max)
	DECLARE @Where  varchar(8000)
	DECLARE @PartSuffix varchar(5)
	DECLARE @PagePara varchar(8000)
	DECLARE @TmpForLoop int
	SELECT @Sql='',@TmpForLoop=0,@Where=''
	DECLARE @LocationIds table(Id int identity(1,1),PartSuffix varchar(5))
	CREATE TABLE #TempResult
	(
		rowid int,
		Location varchar(50), 
		Item varchar(50), 
		LotNo varchar(50),
		ManufactureParty varchar(50), 
		Qty decimal(18,8), 
		CsQty decimal(18,8), 
		QualifyQty decimal(18,8), 
		InspectQty decimal(18,8), 
		RejectQty decimal(18,8),  
		ATPQty decimal(18,8), 
		FreezeQty decimal(18,8)
	)
	CREATE TABLE #TempInternal
	(
		Location varchar(50), 
		Item varchar(50), 
		LotNo varchar(50),
		ManufactureParty varchar(50), 
		Qty decimal(18,8), 
		CsQty decimal(18,8), 
		QualifyQty decimal(18,8), 
		InspectQty decimal(18,8), 
		RejectQty decimal(18,8),  
		ATPQty decimal(18,8), 
		FreezeQty decimal(18,8)
	)	
	--如果有输入的库位则只查询输入库位的表，否则全部表拼接查询
	IF ISNULL(@Locations,'')='' 
	BEGIN
		INSERT INTO @LocationIds(PartSuffix)
		SELECT DISTINCT(PartSuffix) FROM MD_Location WHERE PartSuffix IS NOT NULL OR PartSuffix<>''
	END
	ELSE
	BEGIN
		SET @Where=' WHERE det.Location in('''+replace(@Locations,',',''',''')+''')'
	    SET @sql='SELECT DISTINCT PartSuffix FROM MD_Location WHERE Code in ('''+replace(@Locations,',',''',''')+''')'
		--PRINT @sql
		INSERT INTO @LocationIds(PartSuffix)
		EXEC(@sql)
	END
	
	---查询出数据时需要的条件
	-----物料
	IF ISNULL(@Items,'')<>'' 
	BEGIN
		IF ISNULL(@Where,'')=''
		BEGIN
			SET @Where=' WHERE det.Item IN ('''+replace(@Items,',',''',''')+''')'
		END
		ELSE
		BEGIN
			SET @Where=@Where+' AND det.Item IN ('''+replace(@Items,',',''',''')+''')'
		END
	END
	--PRINT @Where
	--select * from @LocationIds
	---排序条件
	IF ISNULL(@SortDesc,'')=''
	BEGIN
		SET @SortDesc=' ORDER BY Location ASC'
	END
		
	---查询出结果时需要的条件
	IF @Page>0
	BEGIN
		SET @PagePara='WHERE rowid BETWEEN '+cast(@PageSize*(@Page-1) as varchar(50))+' AND '++cast(@PageSize*(@Page) as varchar(50))
	END

	DECLARE @MaxId int
	SELECT @MaxId = MAX(Id),@Sql='' FROM @LocationIds
	WHILE @TmpForLoop<@MaxId
	BEGIN
		SET @TmpForLoop=@TmpForLoop+1
		SELECT @PartSuffix=PartSuffix FROM @LocationIds WHERE Id=@TmpForLoop
		PRINT @TmpForLoop
		IF 	@IsGroupByManufactureParty=0 AND @IsGroupByLotNo=0
		BEGIN
			SET @Sql='SELECT det.Location, det.Item,'''' as LotNo,'''' as ManufactureParty,
					SUM(det.Qty) AS Qty, 
					SUM(CASE WHEN det.IsCS = 1 THEN det.Qty ELSE 0 END) AS CSQty, 					
                    SUM(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, 
                    SUM(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                    SUM(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, 
                    SUM(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                    SUM(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
					FROM  inv_locationlotdet_'+@PartSuffix+' AS det '+@Where+' 
					GROUP BY det.Location, det.Item'
		END	
		ELSE IF @IsGroupByManufactureParty=1 AND @IsGroupByLotNo=0
		BEGIN
			SET @Sql='SELECT det.Location, det.Item,'''' AS LotNo, 
					CASE WHEN bill.Party IS NOT NULL THEN bill.Party ELSE hu.ManufactureParty END AS ManufactureParty,
					SUM(det.Qty) AS Qty,
					SUM(CASE WHEN det.IsCS = 1 THEN det.Qty ELSE 0 END) AS CSQty,
                    SUM(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, 
                    SUM(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                    SUM(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, 
                    SUM(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                    SUM(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
					FROM  inv_locationlotdet_'+@PartSuffix+' AS det 
					LEFT JOIN dbo.INV_Hu AS hu ON det.HuId = hu.HuId  
					LEFT  JOIN BIL_PlanBill AS bill ON det.PlanBill=bill.id AND bill.Type=0 '+@Where+' 
					GROUP BY det.Location, det.Item,CASE WHEN bill.Party IS NOT NULL THEN bill.Party ELSE hu.ManufactureParty END '			
		END	
		ELSE IF @IsGroupByManufactureParty=0 AND @IsGroupByLotNo=1
		BEGIN
			SET @Sql='SELECT det.Location, det.Item, det.LotNo,'''' as ManufactureParty, 
					SUM(det.Qty) AS Qty, 
					SUM(CASE WHEN det.IsCS = 1 THEN det.Qty ELSE 0 END) AS CSQty,
                    SUM(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                    SUM(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                    SUM(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
					FROM  inv_locationlotdet_'+@PartSuffix+' AS det LEFT JOIN
					dbo.INV_Hu AS hu ON det.HuId = hu.HuId '+@Where+' 
					GROUP BY det.Location, det.Item, det.LotNo'		
		END			
		ELSE IF @IsGroupByManufactureParty=1 AND @IsGroupByLotNo=1	
		BEGIN
			SET @Sql='SELECT det.Location, det.Item,det.LotNo, 
					CASE WHEN bill.Party IS NOT NULL THEN bill.Party ELSE hu.ManufactureParty END AS ManufactureParty,
					SUM(det.Qty) AS Qty, 
					SUM(CASE WHEN det.IsCS = 1 THEN det.Qty ELSE 0 END) AS CSQty,
                    SUM(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                    SUM(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                    SUM(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
					FROM  inv_locationlotdet_'+@PartSuffix+' AS det 
					LEFT JOIN dbo.INV_Hu AS hu ON det.HuId = hu.HuId  
					LEFT  JOIN BIL_PlanBill AS bill ON det.PlanBill=bill.id AND bill.Type=0 '+@Where+' 
					GROUP BY det.Location, det.Item,det.LotNo,CASE WHEN bill.Party IS NOT NULL THEN bill.Party ELSE hu.ManufactureParty END '
		END	
		
		--exec(@Sql)	
		
		INSERT INTO #TempInternal
		EXEC(@Sql)		
	END
	--SELECT * FROM #TempInternal
	
	---最后的查询结果,包含2个数据集,一个是总数一个是分页数据
	IF @IsSummaryBySAPLoc=1
	BEGIN
		--汇总到SAP库位
		SET @sql = 'select row_number() over('+@SortDesc+') as rowid,* FROM (SELECT l.SAPLocation as Location, t.Item, t.LotNo, t.ManufactureParty,
			SUM(t.Qty) as QTY, SUM(t.CSQty) AS CSQty,SUM(t.QualifyQty) as QualifyQty, SUM(t.InspectQty) as InspectQty, 
			SUM(t.RejectQty) as RejectQty, SUM(t.ATPQty) as ATPQty, SUM(t.FreezeQty) as FreezeQty FROM #TempInternal as t
			INNER JOIN MD_Location l ON t.Location=l.Code
			GROUP BY l.SAPLocation, t.Item, t.LotNo, t.ManufactureParty) as LocTranDet'
		
		insert into #TempResult 
		exec(@sql)	
			
		select count(1) from #TempResult
		exec('select top('+@PageSize+')  Location, Item, LotNo, ManufactureParty, Qty, CSQTY, QualifyQty, InspectQty, RejectQty, ATPQty, FreezeQty from #TempResult '+@PagePara) 		
	END
	ELSE
	BEGIN
		IF @SummaryLevel=0
		BEGIN
			--不汇总
			SET @sql = 'select row_number() over('+@SortDesc+') as rowid,Location, Item, LotNo, ManufactureParty,
			Qty, CSQTY, QualifyQty, InspectQty, RejectQty, ATPQty, FreezeQty from #TempInternal as det'
			
			insert into #TempResult 
			exec(@sql)	
			
			PRINT @sql	
			select count(1) from #TempResult
			exec('select top('+@PageSize+')  Location, Item, LotNo, ManufactureParty, Qty, CSQTY, QualifyQty, InspectQty, RejectQty, ATPQty, FreezeQty from #TempResult '+@PagePara) 
		END
		ELSE IF @SummaryLevel=1
		BEGIN
			--汇总到区域
			SET @sql = 'select row_number() over('+@SortDesc+') as rowid,* FROM (SELECT r.Code as Location, t.Item, t.LotNo, t.ManufactureParty,
				SUM(t.Qty) as QTY,SUM(t.CSQty) AS CSQty, SUM(t.QualifyQty) as QualifyQty, SUM(t.InspectQty) as InspectQty, 
				SUM(t.RejectQty) as RejectQty, SUM(t.ATPQty) as ATPQty, SUM(t.FreezeQty) as FreezeQty FROM #TempInternal as t
				INNER JOIN MD_Location l ON t.Location=l.Code
				INNER JOIN MD_Region r ON r.Code=l.Region
				GROUP BY r.Code, t.Item, t.LotNo, t.ManufactureParty) as LocTranDet'
			
			insert into #TempResult 
			exec(@sql)	
				
			select count(1) from #TempResult
			exec('select top('+@PageSize+')  Location, Item, LotNo, ManufactureParty, Qty, CSQTY, QualifyQty, InspectQty, RejectQty, ATPQty, FreezeQty from #TempResult '+@PagePara) 
		END
		ELSE IF @SummaryLevel=2
		BEGIN
			--汇总到车间
			SET @sql = 'select row_number() over('+@SortDesc+') as rowid,* FROM (SELECT r.Workshop as Location, t.Item, t.LotNo, t.ManufactureParty, 
				SUM(t.Qty) as QTY, SUM(t.CSQty) AS CSQty, SUM(t.QualifyQty) as QualifyQty, SUM(t.InspectQty) as InspectQty, 
				SUM(t.RejectQty) as RejectQty, SUM(t.ATPQty) as ATPQty, SUM(t.FreezeQty) as FreezeQty FROM #TempInternal as t
				INNER JOIN MD_Location l ON t.Location=l.Code
				INNER JOIN MD_Region r ON r.Code=l.Region
				GROUP BY r.Workshop, t.Item, t.LotNo, t.ManufactureParty) as LocTranDet'

			insert into #TempResult 
			exec(@sql)	
				
			select count(1) from #TempResult
			exec('select top('+@PageSize+') Location, Item, LotNo, ManufactureParty, Qty, CSQTY, QualifyQty, InspectQty, RejectQty, ATPQty, FreezeQty from #TempResult '+@PagePara) 
		END
		ELSE IF @SummaryLevel=3
		BEGIN
			--汇总到工厂
			SET @sql = 'select row_number() over('+@SortDesc+') as rowid,* FROM (SELECT r.Plant as Location, t.Item, t.LotNo, t.ManufactureParty, 
				SUM(t.Qty) as QTY, SUM(t.CSQty) AS CSQty, SUM(t.QualifyQty) as QualifyQty, SUM(t.InspectQty) as InspectQty, 
				SUM(t.RejectQty) as RejectQty, SUM(t.ATPQty) as ATPQty, SUM(t.FreezeQty) as FreezeQty FROM #TempInternal as t
				INNER JOIN MD_Location l ON t.Location=l.Code
				INNER JOIN MD_Region r ON r.Code=l.Region
				GROUP BY r.Plant, t.Item, t.LotNo, t.ManufactureParty) as LocTranDet'
			
			insert into #TempResult 
			exec(@sql)	
				
			select count(1) from #TempResult
			exec('select top('+@PageSize+') Location, Item, LotNo, ManufactureParty, Qty, CSQTY, QualifyQty, InspectQty, RejectQty, ATPQty, FreezeQty from #TempResult '+@PagePara) 
		END
	END	
END





ALTER VIEW [dbo].[VIEW_OrderDet]
  
AS

SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, 
                      OrderQty, ShipQty, RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, 
                      PriceList, UnitPrice, IsProvEst, Tax, IsIncludeTax, Bom, Routing, ProdScan, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, 
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, 
                      BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, ContainerDesc, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, TraceCode, 
                      PartyFrom, PartyTo, IsScanHu
FROM         dbo.ORD_OrderDet_1
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, 
                      OrderQty, ShipQty, RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, 
                      PriceList, UnitPrice, IsProvEst, Tax, IsIncludeTax, Bom, Routing, ProdScan, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, 
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, 
                      BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, ContainerDesc, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, TraceCode, 
                      PartyFrom, PartyTo, IsScanHu
FROM         dbo.ORD_OrderDet_2
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, 
                      OrderQty, ShipQty, RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, 
                      PriceList, UnitPrice, IsProvEst, Tax, IsIncludeTax, Bom, Routing, ProdScan, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, 
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, 
                      BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, ContainerDesc, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, TraceCode, 
                      PartyFrom, PartyTo, IsScanHu
FROM         dbo.ORD_OrderDet_3
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, 
                      OrderQty, ShipQty, RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, 
                      PriceList, UnitPrice, IsProvEst, Tax, IsIncludeTax, Bom, Routing, ProdScan, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, 
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, 
                      BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, ContainerDesc, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, TraceCode, 
                      PartyFrom, PartyTo, IsScanHu
FROM         dbo.ORD_OrderDet_4
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, 
                      OrderQty, ShipQty, RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, 
                      PriceList, UnitPrice, IsProvEst, Tax, IsIncludeTax, Bom, Routing, ProdScan, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, 
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, 
                      BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, ContainerDesc, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, TraceCode, 
                      PartyFrom, PartyTo, IsScanHu
FROM         dbo.ORD_OrderDet_5
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, 
                      OrderQty, ShipQty, RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, 
                      PriceList, UnitPrice, IsProvEst, Tax, IsIncludeTax, Bom, Routing, ProdScan, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, 
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, 
                      BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, ContainerDesc, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, TraceCode, 
                      PartyFrom, PartyTo, IsScanHu
FROM         dbo.ORD_OrderDet_6
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, 
                      OrderQty, ShipQty, RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, 
                      PriceList, UnitPrice, IsProvEst, Tax, IsIncludeTax, Bom, Routing, ProdScan, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, 
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, 
                      BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, ContainerDesc, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, TraceCode, 
                      PartyFrom, PartyTo, IsScanHu
FROM         dbo.ORD_OrderDet_7
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, 
                      OrderQty, ShipQty, RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, 
                      PriceList, UnitPrice, IsProvEst, Tax, IsIncludeTax, Bom, Routing, ProdScan, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, 
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, 
                      BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, ContainerDesc, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, TraceCode, 
                      PartyFrom, PartyTo, IsScanHu
FROM         dbo.ORD_OrderDet_8
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, 
                      OrderQty, ShipQty, RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, 
                      PriceList, UnitPrice, IsProvEst, Tax, IsIncludeTax, Bom, Routing, ProdScan, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, 
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, 
                      BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, ContainerDesc, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, TraceCode, 
                      PartyFrom, PartyTo, IsScanHu
FROM         dbo.ORD_OrderDet_0

GO

----Sconit5库修改USP_Busi_UpdateSeq4RestoreVanOrder，参数NewSeq改为bigint类型 2012-9-12 modify by jienyuan
USE [sconit5]
GO
/****** Object:  StoredProcedure [dbo].[USP_Busi_UpdateSeq4RestoreVanOrder]    Script Date: 09/12/2012 13:06:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[USP_Busi_UpdateSeq4RestoreVanOrder]
(
	@TraceCode varchar(50),
	@VanOrder varchar(50),
	@NewSeq bigint,
	@UserId int,
	@UserNm varchar(100)
)
AS
BEGIN
	--查询包含TraceCode的采购单（Kit单/排序单）
	select Flow, OrderNo, CONVERT(varchar(8), StartTime, 112) as StartTime into #temp1 from ORD_OrderMstr_1 where TraceCode = @TraceCode;
	--查询包含TraceCode的移库单（Kit单/排序单）
	select Flow, OrderNo, CONVERT(varchar(8), StartTime, 112) as StartTime into #temp2 from ORD_OrderMstr_2 where TraceCode = @TraceCode;
	--查询包含TraceCode的生产单（驾驶室/底盘）
	select Flow, OrderNo, CONVERT(varchar(8), StartTime, 112) as StartTime into #temp4 from ORD_OrderMstr_4 where TraceCode = @TraceCode and OrderNo <> @VanOrder;
	--查询包含TraceCode的计划协议单（Kit单/排序单）
	select Flow, OrderNo, CONVERT(varchar(8), StartTime, 112) as StartTime into #temp8 from ORD_OrderMstr_8 where TraceCode = @TraceCode;
	
begin tran
	update ORD_OrderMstr_1 set Seq = mstr.Seq + 1, Version = Version + 1, LastModifyDate = GETDATE(), LastModifyUser = @UserId, LastModifyUserNm = @UserNm
	from ORD_OrderMstr_1 as mstr inner join #temp1 as t on mstr.Flow = t.Flow and CONVERT(varchar(8), mstr.StartTime, 112) = t.StartTime
	where mstr.OrderNo <> t.OrderNo and mstr.Seq >= @NewSeq;
	
	update ORD_OrderMstr_2 set Seq = mstr.Seq + 1, Version = Version + 1, LastModifyDate = GETDATE(), LastModifyUser = @UserId, LastModifyUserNm = @UserNm
	from ORD_OrderMstr_2 as mstr inner join #temp2 as t on mstr.Flow = t.Flow and CONVERT(varchar(8), mstr.StartTime, 112) = t.StartTime
	where mstr.OrderNo <> t.OrderNo and mstr.Seq >= @NewSeq;
	
	update ORD_OrderMstr_4 set Seq = mstr.Seq + 1, Version = Version + 1, LastModifyDate = GETDATE(), LastModifyUser = @UserId, LastModifyUserNm = @UserNm
	from ORD_OrderMstr_4 as mstr inner join #temp4 as t on mstr.Flow = t.Flow and CONVERT(varchar(8), mstr.StartTime, 112) = t.StartTime
	where mstr.OrderNo <> t.OrderNo and mstr.Seq >= @NewSeq;
	
	update ORD_OrderMstr_8 set Seq = mstr.Seq + 1, Version = Version + 1, LastModifyDate = GETDATE(), LastModifyUser = @UserId, LastModifyUserNm = @UserNm
	from ORD_OrderMstr_8 as mstr inner join #temp8 as t on mstr.Flow = t.Flow and CONVERT(varchar(8), mstr.StartTime, 112) = t.StartTime
	where mstr.OrderNo <> t.OrderNo and mstr.Seq >= @NewSeq;
commit

END


----Sconit5_SI库删除创建计划协议表，改为在sconit5数据库增加 2012-8-28
USE [sconit5_si]
if exists (select 1
            from  sysobjects
           where  id = object_id('SI_SAP_CRSL')
            and   type = 'U')
   drop table SI_SAP_CRSL
go

USE [sconit5]
if exists (select 1
            from  sysobjects
           where  id = object_id('CUST_CRSL')
            and   type = 'U')
   drop table CUST_CRSL
go

/*==============================================================*/
/* Table: CUST_CRSL                                             */
/*==============================================================*/
create table CUST_CRSL (
   Id                   int                  identity,
   OrderNo              varchar(50)          null,
   Seq                  int                  null,
   Item                 varchar(50)          null,
   EBELN                varchar(50)          null,
   EBELP                varchar(50)          null,
   Qty                  decimal(18,8)        null,
   Status               tinyint              null,
   ErrorCount           int                  null,
   ErrorMessage         varchar(255)         null,
   CreateUser           int                  not null,
   CreateUserNm         varchar(100)         not null,
   CreateDate           datetime             not null,
   LastModifyUser       int                  not null,
   LastModifyUserNm     varchar(100)         not null,
   LastModifyDate       datetime             not null
)
go

alter table CUST_CRSL
   add constraint PK_CUST_CRSL primary key (Id)
go


--Sconit5_SI库增加创建计划协议表 2012-8-24
USE [sconit5_si]
GO

/****** Object:  Table [dbo].[SI_SAP_CRSL]    Script Date: 08/24/2012 17:12:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[SI_SAP_CRSL](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
	[Seq] [int] NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[EBELN] [varchar](50) NULL,
	[EBELP] [varchar](50) NULL,
	[Qty] [decimal](18, 8) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Status] [tinyint] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[ErrorMessage] [varchar](256) NOT NULL,
 CONSTRAINT [PK_SI_SAP_CRSL] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[SI_SAP_CRSL] ADD  CONSTRAINT [DF_SI_SAP_CRSL_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

USE [sconit5]
GO
--供应商新增最后刷新日期 2012-8-22
alter table dbo.MD_Supplier add  LastRefreshDate datetime null;

--明细明细上需要确定包装是否已经确认 8.22
ALTER TABLE SCM_FlowDet ADD IsCheckedPackage bit
--大部分零件需要制定车型8.22
ALTER TABLE MD_Item ADD SpecifiedModel varchar(100)
--库存明细表增加寄售供应商字段 2012-8-23
alter table INV_LocationLotDet add CSSupplier varchar(50)
alter table INV_LocationLotDet_0 add CSSupplier varchar(50)
alter table INV_LocationLotDet_1 add CSSupplier varchar(50)
alter table INV_LocationLotDet_2 add CSSupplier varchar(50)
alter table INV_LocationLotDet_3 add CSSupplier varchar(50)
alter table INV_LocationLotDet_4 add CSSupplier varchar(50)
alter table INV_LocationLotDet_5 add CSSupplier varchar(50)
alter table INV_LocationLotDet_6 add CSSupplier varchar(50)
alter table INV_LocationLotDet_7 add CSSupplier varchar(50)
alter table INV_LocationLotDet_8 add CSSupplier varchar(50)
alter table INV_LocationLotDet_9 add CSSupplier varchar(50)
alter table INV_LocationLotDet_10 add CSSupplier varchar(50)
alter table INV_LocationLotDet_11 add CSSupplier varchar(50)
alter table INV_LocationLotDet_12 add CSSupplier varchar(50)
alter table INV_LocationLotDet_13 add CSSupplier varchar(50)
alter table INV_LocationLotDet_14 add CSSupplier varchar(50)
alter table INV_LocationLotDet_15 add CSSupplier varchar(50)
alter table INV_LocationLotDet_16 add CSSupplier varchar(50)
alter table INV_LocationLotDet_17 add CSSupplier varchar(50)
alter table INV_LocationLotDet_18 add CSSupplier varchar(50)
alter table INV_LocationLotDet_19 add CSSupplier varchar(50)
go
alter table INV_LocationLotDet add constraint FK_INV_LocationLotDet_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_0 add constraint FK_INV_LocationLotDet_0_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_1 add constraint FK_INV_LocationLotDet_1_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_2 add constraint FK_INV_LocationLotDet_2_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_3 add constraint FK_INV_LocationLotDet_3_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_4 add constraint FK_INV_LocationLotDet_4_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_5 add constraint FK_INV_LocationLotDet_5_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_6 add constraint FK_INV_LocationLotDet_6_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_7 add constraint FK_INV_LocationLotDet_7_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_8 add constraint FK_INV_LocationLotDet_8_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_9 add constraint FK_INV_LocationLotDet_9_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_10 add constraint FK_INV_LocationLotDet_10_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_11 add constraint FK_INV_LocationLotDet_11_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_12 add constraint FK_INV_LocationLotDet_12_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_13 add constraint FK_INV_LocationLotDet_13_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_14 add constraint FK_INV_LocationLotDet_14_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_15 add constraint FK_INV_LocationLotDet_15_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_16 add constraint FK_INV_LocationLotDet_16_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_17 add constraint FK_INV_LocationLotDet_17_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_18 add constraint FK_INV_LocationLotDet_18_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
alter table INV_LocationLotDet_19 add constraint FK_INV_LocationLotDet_19_REFERENCE_MD_SUPPLIER foreign key (CSSupplier) references MD_Supplier (Code)
go
ALTER VIEW [dbo].[VIEW_LocationLotDet]
  
AS
SELECT     dbo.INV_LocationLotDet_1.Id, dbo.INV_LocationLotDet_1.Location, dbo.INV_LocationLotDet_1.Bin, dbo.INV_LocationLotDet_1.Item, dbo.INV_LocationLotDet_1.HuId, 
                      dbo.INV_LocationLotDet_1.LotNo, dbo.INV_LocationLotDet_1.Qty, dbo.INV_LocationLotDet_1.IsCS, dbo.INV_LocationLotDet_1.PlanBill, dbo.INV_LocationLotDet_1.QualityType, 
                      dbo.INV_LocationLotDet_1.IsFreeze, dbo.INV_LocationLotDet_1.IsATP, dbo.INV_LocationLotDet_1.OccupyType, dbo.INV_LocationLotDet_1.OccupyRefNo, 
                      dbo.INV_LocationLotDet_1.CreateUser, dbo.INV_LocationLotDet_1.CreateUserNm, dbo.INV_LocationLotDet_1.CreateDate, dbo.INV_LocationLotDet_1.LastModifyUser, 
                      dbo.INV_LocationLotDet_1.LastModifyUserNm, dbo.INV_LocationLotDet_1.LastModifyDate, dbo.INV_LocationLotDet_1.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_1.CSSupplier
FROM         dbo.INV_LocationLotDet_1 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_1.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_1.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_1.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_1.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_1.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_0.Id, dbo.INV_LocationLotDet_0.Location, dbo.INV_LocationLotDet_0.Bin, dbo.INV_LocationLotDet_0.Item, dbo.INV_LocationLotDet_0.HuId, 
                      dbo.INV_LocationLotDet_0.LotNo, dbo.INV_LocationLotDet_0.Qty, dbo.INV_LocationLotDet_0.IsCS, dbo.INV_LocationLotDet_0.PlanBill, dbo.INV_LocationLotDet_0.QualityType, 
                      dbo.INV_LocationLotDet_0.IsFreeze, dbo.INV_LocationLotDet_0.IsATP, dbo.INV_LocationLotDet_0.OccupyType, dbo.INV_LocationLotDet_0.OccupyRefNo, 
                      dbo.INV_LocationLotDet_0.CreateUser, dbo.INV_LocationLotDet_0.CreateUserNm, dbo.INV_LocationLotDet_0.CreateDate, dbo.INV_LocationLotDet_0.LastModifyUser, 
                      dbo.INV_LocationLotDet_0.LastModifyUserNm, dbo.INV_LocationLotDet_0.LastModifyDate, dbo.INV_LocationLotDet_0.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_0.CSSupplier
FROM         dbo.INV_LocationLotDet_0 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_0.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_0.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_0.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_0.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_0.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_2.Id, dbo.INV_LocationLotDet_2.Location, dbo.INV_LocationLotDet_2.Bin, dbo.INV_LocationLotDet_2.Item, dbo.INV_LocationLotDet_2.HuId, 
                      dbo.INV_LocationLotDet_2.LotNo, dbo.INV_LocationLotDet_2.Qty, dbo.INV_LocationLotDet_2.IsCS, dbo.INV_LocationLotDet_2.PlanBill, dbo.INV_LocationLotDet_2.QualityType, 
                      dbo.INV_LocationLotDet_2.IsFreeze, dbo.INV_LocationLotDet_2.IsATP, dbo.INV_LocationLotDet_2.OccupyType, dbo.INV_LocationLotDet_2.OccupyRefNo, 
                      dbo.INV_LocationLotDet_2.CreateUser, dbo.INV_LocationLotDet_2.CreateUserNm, dbo.INV_LocationLotDet_2.CreateDate, dbo.INV_LocationLotDet_2.LastModifyUser, 
                      dbo.INV_LocationLotDet_2.LastModifyUserNm, dbo.INV_LocationLotDet_2.LastModifyDate, dbo.INV_LocationLotDet_2.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_2.CSSupplier
FROM         dbo.INV_LocationLotDet_2 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_2.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_2.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_2.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_2.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_2.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_3.Id, dbo.INV_LocationLotDet_3.Location, dbo.INV_LocationLotDet_3.Bin, dbo.INV_LocationLotDet_3.Item, dbo.INV_LocationLotDet_3.HuId, 
                      dbo.INV_LocationLotDet_3.LotNo, dbo.INV_LocationLotDet_3.Qty, dbo.INV_LocationLotDet_3.IsCS, dbo.INV_LocationLotDet_3.PlanBill, dbo.INV_LocationLotDet_3.QualityType, 
                      dbo.INV_LocationLotDet_3.IsFreeze, dbo.INV_LocationLotDet_3.IsATP, dbo.INV_LocationLotDet_3.OccupyType, dbo.INV_LocationLotDet_3.OccupyRefNo, 
                      dbo.INV_LocationLotDet_3.CreateUser, dbo.INV_LocationLotDet_3.CreateUserNm, dbo.INV_LocationLotDet_3.CreateDate, dbo.INV_LocationLotDet_3.LastModifyUser, 
                      dbo.INV_LocationLotDet_3.LastModifyUserNm, dbo.INV_LocationLotDet_3.LastModifyDate, dbo.INV_LocationLotDet_3.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_3.CSSupplier
FROM         dbo.INV_LocationLotDet_3 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_3.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_3.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_3.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_3.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_3.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_4.Id, dbo.INV_LocationLotDet_4.Location, dbo.INV_LocationLotDet_4.Bin, dbo.INV_LocationLotDet_4.Item, dbo.INV_LocationLotDet_4.HuId, 
                      dbo.INV_LocationLotDet_4.LotNo, dbo.INV_LocationLotDet_4.Qty, dbo.INV_LocationLotDet_4.IsCS, dbo.INV_LocationLotDet_4.PlanBill, dbo.INV_LocationLotDet_4.QualityType, 
                      dbo.INV_LocationLotDet_4.IsFreeze, dbo.INV_LocationLotDet_4.IsATP, dbo.INV_LocationLotDet_4.OccupyType, dbo.INV_LocationLotDet_4.OccupyRefNo, 
                      dbo.INV_LocationLotDet_4.CreateUser, dbo.INV_LocationLotDet_4.CreateUserNm, dbo.INV_LocationLotDet_4.CreateDate, dbo.INV_LocationLotDet_4.LastModifyUser, 
                      dbo.INV_LocationLotDet_4.LastModifyUserNm, dbo.INV_LocationLotDet_4.LastModifyDate, dbo.INV_LocationLotDet_4.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_4.CSSupplier
FROM         dbo.INV_LocationLotDet_4 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_4.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_4.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_4.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_4.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_4.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_5.Id, dbo.INV_LocationLotDet_5.Location, dbo.INV_LocationLotDet_5.Bin, dbo.INV_LocationLotDet_5.Item, dbo.INV_LocationLotDet_5.HuId, 
                      dbo.INV_LocationLotDet_5.LotNo, dbo.INV_LocationLotDet_5.Qty, dbo.INV_LocationLotDet_5.IsCS, dbo.INV_LocationLotDet_5.PlanBill, dbo.INV_LocationLotDet_5.QualityType, 
                      dbo.INV_LocationLotDet_5.IsFreeze, dbo.INV_LocationLotDet_5.IsATP, dbo.INV_LocationLotDet_5.OccupyType, dbo.INV_LocationLotDet_5.OccupyRefNo, 
                      dbo.INV_LocationLotDet_5.CreateUser, dbo.INV_LocationLotDet_5.CreateUserNm, dbo.INV_LocationLotDet_5.CreateDate, dbo.INV_LocationLotDet_5.LastModifyUser, 
                      dbo.INV_LocationLotDet_5.LastModifyUserNm, dbo.INV_LocationLotDet_5.LastModifyDate, dbo.INV_LocationLotDet_5.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_5.CSSupplier
FROM         dbo.INV_LocationLotDet_5 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_5.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_5.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_5.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_5.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_5.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_6.Id, dbo.INV_LocationLotDet_6.Location, dbo.INV_LocationLotDet_6.Bin, dbo.INV_LocationLotDet_6.Item, dbo.INV_LocationLotDet_6.HuId, 
                      dbo.INV_LocationLotDet_6.LotNo, dbo.INV_LocationLotDet_6.Qty, dbo.INV_LocationLotDet_6.IsCS, dbo.INV_LocationLotDet_6.PlanBill, dbo.INV_LocationLotDet_6.QualityType, 
                      dbo.INV_LocationLotDet_6.IsFreeze, dbo.INV_LocationLotDet_6.IsATP, dbo.INV_LocationLotDet_6.OccupyType, dbo.INV_LocationLotDet_6.OccupyRefNo, 
                      dbo.INV_LocationLotDet_6.CreateUser, dbo.INV_LocationLotDet_6.CreateUserNm, dbo.INV_LocationLotDet_6.CreateDate, dbo.INV_LocationLotDet_6.LastModifyUser, 
                      dbo.INV_LocationLotDet_6.LastModifyUserNm, dbo.INV_LocationLotDet_6.LastModifyDate, dbo.INV_LocationLotDet_6.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_6.CSSupplier
FROM         dbo.INV_LocationLotDet_6 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_6.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_6.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_6.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_6.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_6.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_7.Id, dbo.INV_LocationLotDet_7.Location, dbo.INV_LocationLotDet_7.Bin, dbo.INV_LocationLotDet_7.Item, dbo.INV_LocationLotDet_7.HuId, 
                      dbo.INV_LocationLotDet_7.LotNo, dbo.INV_LocationLotDet_7.Qty, dbo.INV_LocationLotDet_7.IsCS, dbo.INV_LocationLotDet_7.PlanBill, dbo.INV_LocationLotDet_7.QualityType, 
                      dbo.INV_LocationLotDet_7.IsFreeze, dbo.INV_LocationLotDet_7.IsATP, dbo.INV_LocationLotDet_7.OccupyType, dbo.INV_LocationLotDet_7.OccupyRefNo, 
                      dbo.INV_LocationLotDet_7.CreateUser, dbo.INV_LocationLotDet_7.CreateUserNm, dbo.INV_LocationLotDet_7.CreateDate, dbo.INV_LocationLotDet_7.LastModifyUser, 
                      dbo.INV_LocationLotDet_7.LastModifyUserNm, dbo.INV_LocationLotDet_7.LastModifyDate, dbo.INV_LocationLotDet_7.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_7.CSSupplier
FROM         dbo.INV_LocationLotDet_7 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_7.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_7.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_7.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_7.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_7.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_8.Id, dbo.INV_LocationLotDet_8.Location, dbo.INV_LocationLotDet_8.Bin, dbo.INV_LocationLotDet_8.Item, dbo.INV_LocationLotDet_8.HuId, 
                      dbo.INV_LocationLotDet_8.LotNo, dbo.INV_LocationLotDet_8.Qty, dbo.INV_LocationLotDet_8.IsCS, dbo.INV_LocationLotDet_8.PlanBill, dbo.INV_LocationLotDet_8.QualityType, 
                      dbo.INV_LocationLotDet_8.IsFreeze, dbo.INV_LocationLotDet_8.IsATP, dbo.INV_LocationLotDet_8.OccupyType, dbo.INV_LocationLotDet_8.OccupyRefNo, 
                      dbo.INV_LocationLotDet_8.CreateUser, dbo.INV_LocationLotDet_8.CreateUserNm, dbo.INV_LocationLotDet_8.CreateDate, dbo.INV_LocationLotDet_8.LastModifyUser, 
                      dbo.INV_LocationLotDet_8.LastModifyUserNm, dbo.INV_LocationLotDet_8.LastModifyDate, dbo.INV_LocationLotDet_8.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_8.CSSupplier
FROM         dbo.INV_LocationLotDet_8 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_8.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_8.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_8.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_8.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_8.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_9.Id, dbo.INV_LocationLotDet_9.Location, dbo.INV_LocationLotDet_9.Bin, dbo.INV_LocationLotDet_9.Item, dbo.INV_LocationLotDet_9.HuId, 
                      dbo.INV_LocationLotDet_9.LotNo, dbo.INV_LocationLotDet_9.Qty, dbo.INV_LocationLotDet_9.IsCS, dbo.INV_LocationLotDet_9.PlanBill, dbo.INV_LocationLotDet_9.QualityType, 
                      dbo.INV_LocationLotDet_9.IsFreeze, dbo.INV_LocationLotDet_9.IsATP, dbo.INV_LocationLotDet_9.OccupyType, dbo.INV_LocationLotDet_9.OccupyRefNo, 
                      dbo.INV_LocationLotDet_9.CreateUser, dbo.INV_LocationLotDet_9.CreateUserNm, dbo.INV_LocationLotDet_9.CreateDate, dbo.INV_LocationLotDet_9.LastModifyUser, 
                      dbo.INV_LocationLotDet_9.LastModifyUserNm, dbo.INV_LocationLotDet_9.LastModifyDate, dbo.INV_LocationLotDet_9.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_9.CSSupplier
FROM         dbo.INV_LocationLotDet_9 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_9.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_9.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_9.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_9.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_9.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_10.Id, dbo.INV_LocationLotDet_10.Location, dbo.INV_LocationLotDet_10.Bin, dbo.INV_LocationLotDet_10.Item, dbo.INV_LocationLotDet_10.HuId, 
                      dbo.INV_LocationLotDet_10.LotNo, dbo.INV_LocationLotDet_10.Qty, dbo.INV_LocationLotDet_10.IsCS, dbo.INV_LocationLotDet_10.PlanBill, dbo.INV_LocationLotDet_10.QualityType, 
                      dbo.INV_LocationLotDet_10.IsFreeze, dbo.INV_LocationLotDet_10.IsATP, dbo.INV_LocationLotDet_10.OccupyType, dbo.INV_LocationLotDet_10.OccupyRefNo, 
                      dbo.INV_LocationLotDet_10.CreateUser, dbo.INV_LocationLotDet_10.CreateUserNm, dbo.INV_LocationLotDet_10.CreateDate, dbo.INV_LocationLotDet_10.LastModifyUser, 
                      dbo.INV_LocationLotDet_10.LastModifyUserNm, dbo.INV_LocationLotDet_10.LastModifyDate, dbo.INV_LocationLotDet_10.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_10.CSSupplier
FROM         dbo.INV_LocationLotDet_10 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_10.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_10.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_10.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_10.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_10.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_11.Id, dbo.INV_LocationLotDet_11.Location, dbo.INV_LocationLotDet_11.Bin, dbo.INV_LocationLotDet_11.Item, dbo.INV_LocationLotDet_11.HuId, 
                      dbo.INV_LocationLotDet_11.LotNo, dbo.INV_LocationLotDet_11.Qty, dbo.INV_LocationLotDet_11.IsCS, dbo.INV_LocationLotDet_11.PlanBill, dbo.INV_LocationLotDet_11.QualityType, 
                      dbo.INV_LocationLotDet_11.IsFreeze, dbo.INV_LocationLotDet_11.IsATP, dbo.INV_LocationLotDet_11.OccupyType, dbo.INV_LocationLotDet_11.OccupyRefNo, 
                      dbo.INV_LocationLotDet_11.CreateUser, dbo.INV_LocationLotDet_11.CreateUserNm, dbo.INV_LocationLotDet_11.CreateDate, dbo.INV_LocationLotDet_11.LastModifyUser, 
                      dbo.INV_LocationLotDet_11.LastModifyUserNm, dbo.INV_LocationLotDet_11.LastModifyDate, dbo.INV_LocationLotDet_11.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_11.CSSupplier
FROM         dbo.INV_LocationLotDet_11 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_11.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_11.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_11.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_11.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_11.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_12.Id, dbo.INV_LocationLotDet_12.Location, dbo.INV_LocationLotDet_12.Bin, dbo.INV_LocationLotDet_12.Item, dbo.INV_LocationLotDet_12.HuId, 
                      dbo.INV_LocationLotDet_12.LotNo, dbo.INV_LocationLotDet_12.Qty, dbo.INV_LocationLotDet_12.IsCS, dbo.INV_LocationLotDet_12.PlanBill, dbo.INV_LocationLotDet_12.QualityType, 
                      dbo.INV_LocationLotDet_12.IsFreeze, dbo.INV_LocationLotDet_12.IsATP, dbo.INV_LocationLotDet_12.OccupyType, dbo.INV_LocationLotDet_12.OccupyRefNo, 
                      dbo.INV_LocationLotDet_12.CreateUser, dbo.INV_LocationLotDet_12.CreateUserNm, dbo.INV_LocationLotDet_12.CreateDate, dbo.INV_LocationLotDet_12.LastModifyUser, 
                      dbo.INV_LocationLotDet_12.LastModifyUserNm, dbo.INV_LocationLotDet_12.LastModifyDate, dbo.INV_LocationLotDet_12.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_12.CSSupplier
FROM         dbo.INV_LocationLotDet_12 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_12.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_12.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_12.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_12.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_12.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_13.Id, dbo.INV_LocationLotDet_13.Location, dbo.INV_LocationLotDet_13.Bin, dbo.INV_LocationLotDet_13.Item, dbo.INV_LocationLotDet_13.HuId, 
                      dbo.INV_LocationLotDet_13.LotNo, dbo.INV_LocationLotDet_13.Qty, dbo.INV_LocationLotDet_13.IsCS, dbo.INV_LocationLotDet_13.PlanBill, dbo.INV_LocationLotDet_13.QualityType, 
                      dbo.INV_LocationLotDet_13.IsFreeze, dbo.INV_LocationLotDet_13.IsATP, dbo.INV_LocationLotDet_13.OccupyType, dbo.INV_LocationLotDet_13.OccupyRefNo, 
                      dbo.INV_LocationLotDet_13.CreateUser, dbo.INV_LocationLotDet_13.CreateUserNm, dbo.INV_LocationLotDet_13.CreateDate, dbo.INV_LocationLotDet_13.LastModifyUser, 
                      dbo.INV_LocationLotDet_13.LastModifyUserNm, dbo.INV_LocationLotDet_13.LastModifyDate, dbo.INV_LocationLotDet_13.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_13.CSSupplier
FROM         dbo.INV_LocationLotDet_13 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_13.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_13.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_13.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_13.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_13.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_14.Id, dbo.INV_LocationLotDet_14.Location, dbo.INV_LocationLotDet_14.Bin, dbo.INV_LocationLotDet_14.Item, dbo.INV_LocationLotDet_14.HuId, 
                      dbo.INV_LocationLotDet_14.LotNo, dbo.INV_LocationLotDet_14.Qty, dbo.INV_LocationLotDet_14.IsCS, dbo.INV_LocationLotDet_14.PlanBill, dbo.INV_LocationLotDet_14.QualityType, 
                      dbo.INV_LocationLotDet_14.IsFreeze, dbo.INV_LocationLotDet_14.IsATP, dbo.INV_LocationLotDet_14.OccupyType, dbo.INV_LocationLotDet_14.OccupyRefNo, 
                      dbo.INV_LocationLotDet_14.CreateUser, dbo.INV_LocationLotDet_14.CreateUserNm, dbo.INV_LocationLotDet_14.CreateDate, dbo.INV_LocationLotDet_14.LastModifyUser, 
                      dbo.INV_LocationLotDet_14.LastModifyUserNm, dbo.INV_LocationLotDet_14.LastModifyDate, dbo.INV_LocationLotDet_14.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_14.CSSupplier
FROM         dbo.INV_LocationLotDet_14 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_14.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_14.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_14.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_14.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_14.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_15.Id, dbo.INV_LocationLotDet_15.Location, dbo.INV_LocationLotDet_15.Bin, dbo.INV_LocationLotDet_15.Item, dbo.INV_LocationLotDet_15.HuId, 
                      dbo.INV_LocationLotDet_15.LotNo, dbo.INV_LocationLotDet_15.Qty, dbo.INV_LocationLotDet_15.IsCS, dbo.INV_LocationLotDet_15.PlanBill, dbo.INV_LocationLotDet_15.QualityType, 
                      dbo.INV_LocationLotDet_15.IsFreeze, dbo.INV_LocationLotDet_15.IsATP, dbo.INV_LocationLotDet_15.OccupyType, dbo.INV_LocationLotDet_15.OccupyRefNo, 
                      dbo.INV_LocationLotDet_15.CreateUser, dbo.INV_LocationLotDet_15.CreateUserNm, dbo.INV_LocationLotDet_15.CreateDate, dbo.INV_LocationLotDet_15.LastModifyUser, 
                      dbo.INV_LocationLotDet_15.LastModifyUserNm, dbo.INV_LocationLotDet_15.LastModifyDate, dbo.INV_LocationLotDet_15.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_15.CSSupplier
FROM         dbo.INV_LocationLotDet_15 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_15.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_15.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_15.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_15.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_15.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_16.Id, dbo.INV_LocationLotDet_16.Location, dbo.INV_LocationLotDet_16.Bin, dbo.INV_LocationLotDet_16.Item, dbo.INV_LocationLotDet_16.HuId, 
                      dbo.INV_LocationLotDet_16.LotNo, dbo.INV_LocationLotDet_16.Qty, dbo.INV_LocationLotDet_16.IsCS, dbo.INV_LocationLotDet_16.PlanBill, dbo.INV_LocationLotDet_16.QualityType, 
                      dbo.INV_LocationLotDet_16.IsFreeze, dbo.INV_LocationLotDet_16.IsATP, dbo.INV_LocationLotDet_16.OccupyType, dbo.INV_LocationLotDet_16.OccupyRefNo, 
                      dbo.INV_LocationLotDet_16.CreateUser, dbo.INV_LocationLotDet_16.CreateUserNm, dbo.INV_LocationLotDet_16.CreateDate, dbo.INV_LocationLotDet_16.LastModifyUser, 
                      dbo.INV_LocationLotDet_16.LastModifyUserNm, dbo.INV_LocationLotDet_16.LastModifyDate, dbo.INV_LocationLotDet_16.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_16.CSSupplier
FROM         dbo.INV_LocationLotDet_16 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_16.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_16.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_16.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_16.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_16.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_17.Id, dbo.INV_LocationLotDet_17.Location, dbo.INV_LocationLotDet_17.Bin, dbo.INV_LocationLotDet_17.Item, dbo.INV_LocationLotDet_17.HuId, 
                      dbo.INV_LocationLotDet_17.LotNo, dbo.INV_LocationLotDet_17.Qty, dbo.INV_LocationLotDet_17.IsCS, dbo.INV_LocationLotDet_17.PlanBill, dbo.INV_LocationLotDet_17.QualityType, 
                      dbo.INV_LocationLotDet_17.IsFreeze, dbo.INV_LocationLotDet_17.IsATP, dbo.INV_LocationLotDet_17.OccupyType, dbo.INV_LocationLotDet_17.OccupyRefNo, 
                      dbo.INV_LocationLotDet_17.CreateUser, dbo.INV_LocationLotDet_17.CreateUserNm, dbo.INV_LocationLotDet_17.CreateDate, dbo.INV_LocationLotDet_17.LastModifyUser, 
                      dbo.INV_LocationLotDet_17.LastModifyUserNm, dbo.INV_LocationLotDet_17.LastModifyDate, dbo.INV_LocationLotDet_17.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_17.CSSupplier
FROM         dbo.INV_LocationLotDet_17 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_17.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_17.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_17.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_17.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_17.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_18.Id, dbo.INV_LocationLotDet_18.Location, dbo.INV_LocationLotDet_18.Bin, dbo.INV_LocationLotDet_18.Item, dbo.INV_LocationLotDet_18.HuId, 
                      dbo.INV_LocationLotDet_18.LotNo, dbo.INV_LocationLotDet_18.Qty, dbo.INV_LocationLotDet_18.IsCS, dbo.INV_LocationLotDet_18.PlanBill, dbo.INV_LocationLotDet_18.QualityType, 
                      dbo.INV_LocationLotDet_18.IsFreeze, dbo.INV_LocationLotDet_18.IsATP, dbo.INV_LocationLotDet_18.OccupyType, dbo.INV_LocationLotDet_18.OccupyRefNo, 
                      dbo.INV_LocationLotDet_18.CreateUser, dbo.INV_LocationLotDet_18.CreateUserNm, dbo.INV_LocationLotDet_18.CreateDate, dbo.INV_LocationLotDet_18.LastModifyUser, 
                      dbo.INV_LocationLotDet_18.LastModifyUserNm, dbo.INV_LocationLotDet_18.LastModifyDate, dbo.INV_LocationLotDet_18.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_18.CSSupplier
FROM         dbo.INV_LocationLotDet_18 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_18.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_18.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_18.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_18.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_18.Qty <> 0)
UNION ALL
SELECT     dbo.INV_LocationLotDet_19.Id, dbo.INV_LocationLotDet_19.Location, dbo.INV_LocationLotDet_19.Bin, dbo.INV_LocationLotDet_19.Item, dbo.INV_LocationLotDet_19.HuId, 
                      dbo.INV_LocationLotDet_19.LotNo, dbo.INV_LocationLotDet_19.Qty, dbo.INV_LocationLotDet_19.IsCS, dbo.INV_LocationLotDet_19.PlanBill, dbo.INV_LocationLotDet_19.QualityType, 
                      dbo.INV_LocationLotDet_19.IsFreeze, dbo.INV_LocationLotDet_19.IsATP, dbo.INV_LocationLotDet_19.OccupyType, dbo.INV_LocationLotDet_19.OccupyRefNo, 
                      dbo.INV_LocationLotDet_19.CreateUser, dbo.INV_LocationLotDet_19.CreateUserNm, dbo.INV_LocationLotDet_19.CreateDate, dbo.INV_LocationLotDet_19.LastModifyUser, 
                      dbo.INV_LocationLotDet_19.LastModifyUserNm, dbo.INV_LocationLotDet_19.LastModifyDate, dbo.INV_LocationLotDet_19.Version, dbo.MD_LocationBin.Area, 
                      dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, 
                      dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, --dbo.BIL_PlanBill.Party AS ConsigementParty, 
                      dbo.INV_Hu.IsOdd, dbo.INV_LocationLotDet_19.CSSupplier
FROM         dbo.INV_LocationLotDet_19 LEFT OUTER JOIN
                      dbo.INV_Hu ON dbo.INV_LocationLotDet_19.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      --dbo.BIL_PlanBill ON dbo.INV_LocationLotDet_19.PlanBill = dbo.BIL_PlanBill.Id AND dbo.INV_LocationLotDet_19.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin ON dbo.INV_LocationLotDet_19.Bin = dbo.MD_LocationBin.Code
WHERE     (dbo.INV_LocationLotDet_19.Qty <> 0)

GO


ALTER PROCEDURE [dbo].[USP_Split_LocationLotDet_INSERT]
(
	@Version int,
	@Location varchar(50),
	@Bin varchar(50),
	@Item varchar(50),
	@LotNo varchar(50),
	@HuId varchar(50),
	@Qty decimal(18,8),
	@IsCS bit,
	@PlanBill int,
	@CSSupplier varchar(50),
	@QualityType tinyint,
	@IsFreeze bit,
	@IsATP bit,
	@OccupyType tinyint,
	@OccupyRefNo varchar(50),
	@CreateUser int,
	@CreateUserNm varchar(100),
	@CreateDate datetime,
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime
)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @Id bigint
	BEGIN TRAN
		IF EXISTS (SELECT * FROM SYS_TabIdSeq WITH (UPDLOCK,SERIALIZABLE) WHERE TabNm='INV_LocationLotDet')
		BEGIN
			SELECT @Id=Id+1 FROM SYS_TabIdSeq WHERE TabNm='INV_LocationLotDet'
			UPDATE SYS_TabIdSeq SET Id=Id+1 WHERE TabNm='INV_LocationLotDet'
		END
		ELSE
		BEGIN
			INSERT INTO SYS_TabIdSeq(TabNm,Id)
			VALUES('INV_LocationLotDet',1)
			SET @Id=1
		END
	COMMIT TRAN
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
		--RAISERROR ('Can''t get any data from table' , 16, 1) WITH NOWAIT
	END 
	
	SET @Statement=N'INSERT INTO INV_LocationLotDet_'+@PartSuffix+'(Id,Version,Location,Bin,Item,LotNo,HuId,Qty,IsCS,PlanBill,CSSupplier,QualityType,IsFreeze,IsATP,OccupyType,OccupyRefNo,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate) VALUES (@Id_1,@Version_1,@Location_1,@Bin_1,@Item_1,@LotNo_1,@HuId_1,@Qty_1,@IsCS_1,@PlanBill_1,@CSSupplier_1,@QualityType_1,@IsFreeze_1,@IsATP_1,@OccupyType_1,@OccupyRefNo_1,@CreateUser_1,@CreateUserNm_1,@CreateDate_1,@LastModifyUser_1,@LastModifyUserNm_1,@LastModifyDate_1)'
	SET @Parameter=N'@Id_1 int,@Version_1 int,@Location_1 varchar(50),@Bin_1 varchar(50),@Item_1 varchar(50),@LotNo_1 varchar(50),@HuId_1 varchar(50),@Qty_1 decimal(18,8),@IsCS_1 bit,@PlanBill_1 int,@CSSupplier_1 varchar(50),@QualityType_1 tinyint,@IsFreeze_1 bit,@IsATP_1 bit,@OccupyType_1 tinyint,@OccupyRefNo_1 varchar(50),@CreateUser_1 int,@CreateUserNm_1 varchar(100),@CreateDate_1 datetime,@LastModifyUser_1 int,@LastModifyUserNm_1 varchar(100),@LastModifyDate_1 datetime'
	
	exec sp_executesql @Statement,@Parameter,
		@Id_1=@Id,@Version_1=@Version,@Location_1=@Location,@Bin_1=@Bin,@Item_1=@Item,@LotNo_1=@LotNo,@HuId_1=@HuId,
		@Qty_1=@Qty,@IsCS_1=@IsCS,@PlanBill_1=@PlanBill,@CSSupplier_1=@CSSupplier,@QualityType_1=@QualityType,@IsFreeze_1=@IsFreeze,@IsATP_1=@IsATP,
		@OccupyType_1=@OccupyType,@OccupyRefNo_1=@OccupyRefNo,@CreateUser_1=@CreateUser,@CreateUserNm_1=@CreateUserNm,
		@CreateDate_1=@CreateDate,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,@LastModifyDate_1=@LastModifyDate	
	SELECT @Id
END

GO
ALTER PROCEDURE [dbo].[USP_Split_LocationLotDet_UPDATE]
(
	@Version int,
	@Location varchar(50),
	@Bin varchar(50),
	@Item varchar(50),
	@LotNo varchar(50),
	@HuId varchar(50),
	@Qty decimal(18,8),
	@IsCS bit,
	@PlanBill int,
	@CSSupplier varchar(50),
	@QualityType tinyint,
	@IsFreeze bit,
	@IsATP bit,
	@OccupyType tinyint,
	@OccupyRefNo varchar(50),
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime,
	@Id int,
	@VersionBerfore int
)
AS
BEGIN
	--SET NOCOUNT ON;
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	SET @Statement='UPDATE INV_LocationLotDet_'+@PartSuffix+' SET Version=@Version_1,Location=@Location_1,Bin=@Bin_1,Item=@Item_1,LotNo=@LotNo_1,HuId=@HuId_1,Qty=@Qty_1,IsCS=@IsCS_1,PlanBill=@PlanBill_1,CSSupplier=@CSSupplier_1,QualityType=@QualityType_1,IsFreeze=@IsFreeze_1,IsATP=@IsATP_1,OccupyType=@OccupyType_1,OccupyRefNo=@OccupyRefNo_1,LastModifyUser=@LastModifyUser_1,LastModifyUserNm=@LastModifyUserNm_1,LastModifyDate=@LastModifyDate_1 WHERE Id=@Id_1 AND Version=@VersionBerfore_1'
	SET @Parameter=N'@Version_1 int,@Location_1 varchar(50),@Bin_1 varchar(50),@Item_1 varchar(50),@LotNo_1 varchar(50),@HuId_1 varchar(50),@Qty_1 decimal(18,8),@IsCS_1 bit,@PlanBill_1 int,@CSSupplier_1 varchar(50),@QualityType_1 tinyint,@IsFreeze_1 bit,@IsATP_1 bit,@OccupyType_1 tinyint,@OccupyRefNo_1 varchar(50),@LastModifyUser_1 int,@LastModifyUserNm_1 varchar(100),@LastModifyDate_1 datetime,@Id_1 int,@VersionBerfore_1 int'
	PRINT @Statement

	exec sp_executesql @Statement,@Parameter,
		@Version_1=@Version,@Location_1=@Location,@Bin_1=@Bin,@Item_1=@Item,@LotNo_1=@LotNo,@HuId_1=@HuId,@Qty_1=@Qty,
		@IsCS_1=@IsCS,@PlanBill_1=@PlanBill,@CSSupplier_1=@CSSupplier,@QualityType_1=@QualityType,@IsFreeze_1=@IsFreeze,@IsATP_1=@IsATP,@OccupyType_1=@OccupyType,
		@OccupyRefNo_1=@OccupyRefNo,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,
		@LastModifyDate_1=@LastModifyDate,@Id_1=@Id,@VersionBerfore_1=@VersionBerfore
END

GO



IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Busi_GetMinusInventory')
	DROP PROCEDURE USP_Busi_GetMinusInventory	
GO
CREATE PROCEDURE [dbo].[USP_Busi_GetMinusInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@QualityType tinyint,
	@OccupyType tinyint
)
AS
BEGIN
/*******************获取负库存数据*********************************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetMinusInventory] 'CS0010','5801306476',0,0,1
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version, NULL AS Area,
                      NULL AS BinSeq, NULL AS HuQty, NULL AS UC, NULL AS HuUom, NULL AS BaseUom, NULL AS UnitQty,
                      NULL AS ManufactureParty, NULL AS ManufactureDate, NULL AS FirstInvDate, NULL AS IsOdd 
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1
				    AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty < 0 AND lld.QualityType=@QualityType_1 
					AND lld.OccupyType=@OccupyType_1 AND lld.IsCS=0 AND lld.CSSupplier is null'
	SET @Parameter=N'@Item_1 varchar(50),@QualityType_1 tinyint,@OccupyType_1 tinyint,@Location_1 varchar(50)'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType,@Location_1=@Location

END

GO



IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Busi_GetMinusCSInventory')
	DROP PROCEDURE USP_Busi_GetMinusCSInventory	
GO
CREATE PROCEDURE [dbo].[USP_Busi_GetMinusCSInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@QualityType tinyint,
	@OccupyType tinyint,
	@CSSupplier varchar(50)
)
AS
BEGIN
/*******************获取指定供应商负数库存数据*****************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetMinusCSInventory] 'ZAA02R','-CN6T',0,0,'0000102681'
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @sql varchar(max)
	DECLARE @where varchar(8000)
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL
		OR ISNULL(@CSSupplier,'')=''
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version, NULL AS Area,
                      NULL AS BinSeq, NULL AS HuQty, NULL AS UC, NULL AS HuUom, NULL AS BaseUom, NULL AS UnitQty,
                      NULL AS ManufactureParty, NULL AS ManufactureDate, NULL AS FirstInvDate, NULL AS IsOdd 
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1 AND lld.CSSupplier=@CSSupplier_1
				  AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty < 0 AND lld.IsCS = 0
			      AND lld.QualityType=@QualityType_1 AND lld.OccupyType=@OccupyType_1'
	SET @Parameter=N'@Item_1 varchar(50),@CSSupplier_1 varchar(50),@Location_1 varchar(50),@QualityType_1 tinyint,@OccupyType_1 tinyint'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@CSSupplier_1=@CSSupplier,@Location_1=@Location,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType
	
END

GO


IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Busi_GetVoidInventory')
	DROP PROCEDURE USP_Busi_GetVoidInventory	
GO
CREATE PROCEDURE [dbo].[USP_Busi_GetVoidInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@PlanBill int,
	@QualityType tinyint,
	@OccupyType tinyint
)
AS
BEGIN
/*******************获取指定Planbill库存数据*********************************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetVoidInventory] 'CS0010','5801306476',0,0,1 
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL OR @PlanBill IS NULL
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version, NULL AS Area,
                      NULL AS BinSeq, NULL AS HuQty, NULL AS UC, NULL AS HuUom, NULL AS BaseUom, NULL AS UnitQty,
                      NULL AS ManufactureParty, NULL AS ManufactureDate, NULL AS FirstInvDate, NULL AS IsOdd 
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1 AND lld.PlanBill=@PlanBill_1 
				  AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty > 0 AND lld.IsCS = 1 
				  AND lld.QualityType=@QualityType_1 AND lld.OccupyType=@OccupyType_1 '
	SET @Parameter=N'@Item_1 varchar(50),@PlanBill_1 int,@QualityType_1 tinyint,@OccupyType_1 tinyint,@Location_1 varchar(50)'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@PlanBill_1=@PlanBill,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType,@Location_1=@Location
END

GO


IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Busi_GetVoidOccupyInventory')
	DROP PROCEDURE USP_Busi_GetVoidOccupyInventory	
GO	
CREATE PROCEDURE [dbo].[USP_Busi_GetVoidOccupyInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@PlanBill int,
	@QualityType tinyint,
	@OccupyType tinyint,
	@OccupyReferenceNo varchar(50)	
)
AS
BEGIN
/*******************获取指定PlanBill&指定占用库存数据**************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetVoidOccupyInventory] 'CS0010','5801306476',0,0,1,'dsaf' 
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL OR @PlanBill IS NULL
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version, NULL AS Area,
                      NULL AS BinSeq, NULL AS HuQty, NULL AS UC, NULL AS HuUom, NULL AS BaseUom, NULL AS UnitQty,
                      NULL AS ManufactureParty, NULL AS ManufactureDate, NULL AS FirstInvDate, NULL AS IsOdd 
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1 AND lld.PlanBill=@PlanBill_1 
				  AND lld.OccupyType=@OccupyType_1 AND lld.OccupyRefNo=@OccupyReferenceNo_1
				  AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty > 0 AND lld.IsCS = 1 
				  AND lld.QualityType=@QualityType_1'
	SET @Parameter=N'@Item_1 varchar(50),@PlanBill_1 int,@QualityType_1 tinyint,@OccupyType_1 tinyint,@OccupyReferenceNo_1 varchar(50),@Location_1 varchar(50)'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@PlanBill_1=@PlanBill,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType,@OccupyReferenceNo_1=@OccupyReferenceNo,@Location_1=@Location
END

GO


IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Busi_GetPlusInventory')
	DROP PROCEDURE USP_Busi_GetPlusInventory	
GO
CREATE PROCEDURE [dbo].[USP_Busi_GetPlusInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@QualityType tinyint,
	@OccupyType tinyint,
	@IsConsignment bit
)
AS
BEGIN
/*******************获取正数库存数据*********************************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetMinusInventory] 'CS0010','5801306476',0,0,1
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL OR @IsConsignment IS NULL
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version, NULL AS Area,
                      NULL AS BinSeq, NULL AS HuQty, NULL AS UC, NULL AS HuUom, NULL AS BaseUom, NULL AS UnitQty,
                      NULL AS ManufactureParty, NULL AS ManufactureDate, NULL AS FirstInvDate, NULL AS IsOdd 
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1 
				    AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty > 0 AND lld.QualityType=@QualityType_1 
					AND lld.OccupyType=@OccupyType_1 AND lld.IsCS=@IsCS_1'
	SET @Parameter=N'@Item_1 varchar(50),@QualityType_1 tinyint,@OccupyType_1 tinyint,@IsCS_1 bit,@Location_1 varchar(50)'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType,@IsCS_1=@IsConsignment,@Location_1=@Location
	
END

GO



IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Busi_GetPlusCSInventory')
	DROP PROCEDURE USP_Busi_GetPlusCSInventory	
GO
CREATE PROCEDURE [dbo].[USP_Busi_GetPlusCSInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@QualityType tinyint,
	@OccupyType tinyint,
	@CSSupplier varchar(50)
)
AS
BEGIN
/*******************获取正数寄售库存数据*********************************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetPlusCSInventory] 'ZAA02R','-CN6T',0,0,'0000102681'
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @sql varchar(max)
	DECLARE @where varchar(8000)
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL
		OR ISNULL(@CSSupplier,'')=''
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version, NULL AS Area,
                      NULL AS BinSeq, NULL AS HuQty, NULL AS UC, NULL AS HuUom, NULL AS BaseUom, NULL AS UnitQty,
                      NULL AS ManufactureParty, NULL AS ManufactureDate, NULL AS FirstInvDate, NULL AS IsOdd 
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1 AND lld.CSSupplier=@CSSupplier_1
				    AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty > 0 AND lld.IsCS = 1					 
					AND lld.QualityType=@QualityType_1 AND lld.OccupyType=@OccupyType_1'
	SET @Parameter=N'@Item_1 varchar(50),@CSSupplier_1 varchar(50),@Location_1 varchar(50),@QualityType_1 tinyint,@OccupyType_1 tinyint'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@CSSupplier_1=@CSSupplier,@Location_1=@Location,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType
	
END


GO


ALTER TABLE MD_Item ADD MinUC decimal(18,8)


--2012-11-08 增加冻结天数
alter table scm_flowdet add  FreezeDays int
update scm_flowdet set freezedays=0 where freezedays is null

--2012-11-12 增加追溯表
create table PRD_MaterialTracer (
   Id                   int                  identity,
   OrderNo              varchar(50)          not null,
   FGItem				varchar(50)          null,
   TraceCode            varchar(50)          null,
   Item                 varchar(50)          not null,
   LotNo                varchar(50)          not null,
   CreateUser           int                  null,
   CreateUserNm         varchar(100)         null,
   CreateDate           datetime             null,
   LastModifyUser       int                  null,
   LastModifyUserNm     varchar(100)         null,
   LastModifyDate       datetime             null
)
go

alter table PRD_MaterialTracer
   add constraint PK_PRD_MaterialTracer primary key (Id)
go

create table ORD_ShipmentMstr
(
  ShipmentNo  varchar(50) primary key,--运单号
  VehicleNo  varchar(30),--车牌号
  WorkShop varchar(30),--工厂
  AddressTo varchar(70),--目的地
  Driver  varchar(10),--驾驶员
  CaseQty  int ,--箱数
  Shipper  varchar(30),--承运商
  Status    tinyint ,--状态
  SubmitDate  datetime,	--释放时间
  SubmitUserNm varchar(100),	--释放用户名
  SubmitUser	 int ,--释放用户
  PassDate	datetime ,--放行时间
  PassPerson	 varchar(20),--放行人
  PassUser	varchar(100),-- 放行用户
  CreateUser		int NOT NULL,
  CreateUserNm	varchar(100) NOT NULL,
CreateDate		datetime NOT NULL,
LastModifyUser	int NOT NULL,
LastModifyUserNm varchar(100) NOT NULL,
LastModifyDate	datetime NOT NULL
)

create table ORD_ShipmentDet
(
Id int identity(1,1) ,
ShipmentNo varchar(50),
IpNo varchar(50)

)

/****** Object:  StoredProcedure [dbo].[USP_Busi_GetMinusInventory]    Script Date: 11/14/2012 11:23:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[USP_Busi_GetMinusInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@QualityType tinyint,
	@OccupyType tinyint
)
AS
BEGIN
/*******************获取负库存数据*********************************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetMinusInventory] 'CS0010','5801306476',0,0,1
************steps**************************************************
step1.GetMinusInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version,dbo.MD_LocationBin.Area, dbo.MD_LocationBin.Seq AS BinSeq, dbo.INV_Hu.Qty AS HuQty, dbo.INV_Hu.UC, dbo.INV_Hu.Uom AS HuUom, 
                      dbo.INV_Hu.BaseUom, dbo.INV_Hu.UnitQty, dbo.INV_Hu.ManufactureParty, dbo.INV_Hu.ManufactureDate, dbo.INV_Hu.FirstInvDate, 
                      dbo.INV_Hu.IsOdd
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld LEFT OUTER JOIN
                      dbo.INV_Hu ON lld.HuId = dbo.INV_Hu.HuId LEFT OUTER JOIN
                      dbo.MD_LocationBin ON lld.Bin = dbo.MD_LocationBin.Code
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1
				    AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty < 0 AND lld.QualityType=@QualityType_1 
					AND lld.OccupyType=@OccupyType_1 AND lld.IsCS=0 AND lld.CSSupplier is null'
	SET @Parameter=N'@Item_1 varchar(50),@QualityType_1 tinyint,@OccupyType_1 tinyint,@Location_1 varchar(50)'
	
	exec sp_executesql @Statement,@Parameter,
		@Item_1=@Item,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType,@Location_1=@Location

END





--begion tiansu 20130102 订单号、发货单号、收货单号模糊查询

/****** Object:  StoredProcedure [dbo].[USP_Search_ProcurementOrder]    Script Date: 2013-01-02 11:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


 ALTER PROCEDURE [dbo].[USP_Search_ProcurementOrder] 
( 
	@OrderNo varchar(50), 
	@Flow varchar(50), 
	@Types varchar(50), 
	@SubType tinyint, 
	@PartyFrom varchar(50), 
	@PartyTo varchar(50), 
	@Status tinyint, 
	@Priority tinyint, 
	@ExtOrderNo varchar(50), 
	@RefOrderNo varchar(50), 
	@TraceCode varchar(50), 
	@CreateUserNm varchar(100), 
	@DateFrom datetime, 
	@DateTo datetime,
	@StartTime datetime, 
	@EndTime datetime,
	@WindowTimeFrom datetime, 
	@WindowTimeTo datetime,   
	@Sequence bigint, 
	@IsReturn bit, 
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
/* 
exec sp_executesql N'exec USP_Search_ProcurementOrder @p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22',N'@p0 nvarchar(4000),@p1 nvarchar(4000),@p2 nvarchar(4000),@p3 nvarchar(4000),@p4 nvarchar(4000),@p5 nvarchar(4000),@p6 smallint,@p7 smallint,@p8 nvarchar(4000),@p9 nvarchar(4000),@p10 nvarchar(4000),@p11 nvarchar(4000),@p12 datetime,@p13 datetime,@p14 bigint,@p15 bit,@p16 bit,@p17 int,@p18 nvarchar(4000),@p19 nvarchar(4000),@p20 nvarchar(4000),@p21 int,@p22 int',@p0=NULL,@p1=NULL,@p2=N'6,1,5,2,7,8',@p3=N'0',@p4=NULL,@p5=NULL,@p6=0,@p7=NULL,@p8=NULL,@p9=NULL,@p10=NULL,@p11=NULL,@p12=NULL,@p13='2012-10-19 00:00:00',@p14=NULL,@p15=0,@p16=0,@p17=2603,@p18=N'and o.OrderStrategy != 4',@p19=NULL,@p20=N'asc',@p21=20,@p22=1 
declare @i tinyint 
set @i=0 
select isnull(@i,'') 
*/ 
	SET NOCOUNT ON 
	DECLARE @SpliceTables nvarchar(4000) 
	DECLARE @Type varchar(5) 
	DECLARE @Statement nvarchar(4000) 
	DECLARE @Parameter nvarchar(4000) 
	DECLARE @PagePara nvarchar(4000) 
	DECLARE @SortDesc nvarchar(100) 
	DECLARE @Where nvarchar(4000) 
	DECLARE @PermissionClause nvarchar(1000) 
	SET @Where='WHERE 1=1 '+@WhereStatement 

	PRINT @Status 
	IF(ISNULL(@Types,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 

	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @OrderNo='%'+@OrderNo+'%' 
		SET @Where=@Where+' AND o.OrderNo LIKE @OrderNo_1' 
	END 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.Flow=@Flow_1' 
	END 
	IF(ISNULL(@SubType,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.SubType=@SubType_1' 
	END	 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.PartyTo=@PartyTo_1 ' 
	END	 

	IF(@Status is not null) 
	BEGIN 
		PRINT @Status 
		SET @Where=@Where+' AND o.Status=@Status_1 ' 
	END		 
	IF(ISNULL(@Priority,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.Priority=@Priority_1 ' 
	END	 
	IF(ISNULL(@ExtOrderNo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.ExtOrderNo LIKE @ExtOrderNo_1 ' 
	END	 
	IF(ISNULL(@RefOrderNo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.RefOrderNo LIKE @RefOrderNo_1 ' 
	END		 
	IF(ISNULL(@TraceCode,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.TraceCode LIKE @TraceCode_1 ' 
	END	 
	IF(ISNULL(@CreateUserNm,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.CreateUserNm=@CreateUserNm_1 ' 
	END	 
	--创建时间
    IF(ISDATE(@DateTo)=1)
	BEGIN
    set @DateTo=dateadd(day,1,@DateTo)
    END
	IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.CreateDate BETWEEN @DateFrom_1 And @DateTo_1 ' 
	END	 
	ELSE IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=0) 
	BEGIN 
		SET @Where=@Where+' AND o.CreateDate > @DateFrom_1' 
	END 
	ELSE IF(ISDATE(@DateFrom)=0 AND ISDATE(@DateTo)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.CreateDate < @DateTo_1' 
	END	 
	--开始时间
	IF(ISDATE(@EndTime)=1)
	BEGIN
    set @EndTime=dateadd(day,1,@EndTime)
    END
	IF(ISDATE(@StartTime)=1 AND ISDATE(@EndTime)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.StartTime BETWEEN @StartTime_1 And @EndTime_1 ' 
	END	 
	ELSE IF(ISDATE(@StartTime)=1 AND ISDATE(@EndTime)=0) 
	BEGIN 
		SET @Where=@Where+' AND o.StartTime > @StartTime_1' 
	END 
	ELSE IF(ISDATE(@StartTime)=0 AND ISDATE(@EndTime)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.StartTime < @EndTime_1' 
	END	 
	
	--窗口时间
	IF(ISDATE(@WindowTimeTo)=1)
	BEGIN
    set @WindowTimeTo=dateadd(day,1,@WindowTimeTo)
    END
	IF(ISDATE(@WindowTimeFrom)=1 AND ISDATE(@WindowTimeTo)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.WindowTime BETWEEN @WindowTimeFrom_1 And @WindowTimeTo_1 ' 
	END	 
	ELSE IF(ISDATE(@WindowTimeFrom)=1 AND ISDATE(@WindowTimeTo)=0) 
	BEGIN 
		SET @Where=@Where+' AND o.WindowTime > @WindowTimeFrom_1' 
	END 
	ELSE IF(ISDATE(@WindowTimeFrom)=0 AND ISDATE(@WindowTimeTo)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.WindowTime < @WindowTimeTo_1' 
	END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0 AND @IsReturn=0) 
		BEGIN 
			SET @Where=@Where+' AND o.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE IF(@IsSupplier=0 AND @IsReturn=1) 
		BEGIN 
			SET @Where=@Where+' AND o.PartyFrom IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @Where=@Where+' AND (o.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR o.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
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
		SET @SortDesc=' ORDER BY '+@SortCloumn+' '+@SortRule 
	END 

	IF @Page>0 
	BEGIN 
		SET @PagePara='WHERE RowId BETWEEN '+cast(@PageSize*(@Page-1) as varchar(50))+' AND '++cast(@PageSize*(@Page) as varchar(50)) 
	END 

	WHILE(LEN(@Types)>0) 
	BEGIN 
		IF(LEN(@Types)=1) 
		BEGIN 
			SET @Type=@Types 
		END 
		ELSE 
		BEGIN 
			SET @Type=SUBSTRING(@Types,1,CHARINDEX(',',@Types)-1) 
		END 
		--IF(CHARINDEX('OrderDetail',@WhereStatement)>0) 
		--BEGIN 
		--	SET @WhereStatement=REPLACE(@WhereStatement,'OrderDetail','ORD_OrderDet'+@Type) 
		--END			 
		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			SET @SpliceTables='SELECT * FROM ORD_OrderMstr_'+@Type+' AS o '+REPLACE(@Where,'OrderDetail','ORD_OrderDet_'+@Type) 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_OrderMstr_'+@Type+' AS o '+REPLACE(@Where,'OrderDetail','ORD_OrderDet_'+@Type) 
		END 
		SET @Types=SUBSTRING(@Types,3,LEN(@Types)) 
	END	 
	SET @Statement=N'SELECT TOP('+CAST(@PageSize AS VARCHAR(10))+') * FROM (SELECT *,RowId=ROW_NUMBER()OVER('+@SortDesc+') FROM ('+@SpliceTables+') AS T1 ) AS T2 '+@PagePara 
	PRINT @Statement 
	PRINT LEN(@Statement) 
	SET @Parameter=N'@OrderNo_1 varchar(50),@Flow_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@Status_1 tinyint,@Priority_1 tinyint,@ExtOrderNo_1 varchar(50),@RefOrderNo_1 varchar(50),@TraceCode_1 varchar(50),@CreateUserNm_1 varchar(100),
	@DateFrom_1 datetime,@DateTo_1 datetime,@StartTime_1 datetime,@EndTime_1 datetime,@WindowTimeFrom_1 datetime,@WindowTimeTo_1 datetime,@SubType_1 tinyint,@Sequence_1 bigint'		 


	EXEC SP_EXECUTESQL @Statement,@Parameter, 
		@OrderNo_1=@OrderNo,@Flow_1=@Flow,@SubType_1=@SubType,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo,@Status_1=@Status, 
		@Priority_1=@Priority,@ExtOrderNo_1=@ExtOrderNo,@RefOrderNo_1=@RefOrderNo,@TraceCode_1=@TraceCode, 
		@CreateUserNm_1=@CreateUserNm,@DateFrom_1=@DateFrom,@DateTo_1=@DateTo,@StartTime_1=@StartTime,@EndTime_1=@EndTime,
		@WindowTimeFrom_1=@WindowTimeFrom,@WindowTimeTo_1=@WindowTimeTo,@Sequence_1=@Sequence	 

END 



/****** Object:  StoredProcedure [dbo].[USP_Search_ProcurementOrderCount]    Script Date: 2013-01-02 11:46:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


ALTER PROCEDURE [dbo].[USP_Search_ProcurementOrderCount] 
( 
	@OrderNo varchar(50), 
	@Flow varchar(50), 
	@Types varchar(50), 
	@SubType tinyint, 
	@PartyFrom varchar(50), 
	@PartyTo varchar(50), 
	@Status tinyint, 
	@Priority tinyint, 
	@ExtOrderNo varchar(50), 
	@RefOrderNo varchar(50), 
	@TraceCode varchar(50), 
	@CreateUserNm varchar(100), 
	@DateFrom datetime, 
	@DateTo datetime,
    @StartTime datetime, 
	@EndTime datetime,
	@WindowTimeFrom datetime, 
	@WindowTimeTo datetime,    
	@Sequence bigint, 
	@IsReturn bit, 
	@IsSupplier bit, 
	@UserId int, 
	@WhereStatement varchar(8000) 
) 
AS 
BEGIN 
	SET NOCOUNT ON 
	DECLARE @SpliceTables nvarchar(4000) 
	DECLARE @Type varchar(5) 
	DECLARE @CountStatement nvarchar(4000) 
	DECLARE @Parameter nvarchar(4000) 
	DECLARE @PagePara nvarchar(4000) 
	DECLARE @SortDesc nvarchar(100) 
	DECLARE @Where nvarchar(4000) 
	DECLARE @PermissionClause nvarchar(1000) 
	SET @Where='WHERE 1=1 '+@WhereStatement 

	PRINT @Where 
	IF(ISNULL(@Types,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 

	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @OrderNo='%'+@OrderNo+'%' 
		SET @Where=@Where+' AND o.OrderNo LIKE @OrderNo_1' 
	END 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.Flow=@Flow_1' 
	END 
	IF(ISNULL(@SubType,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.SubType=@SubType_1' 
	END	 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.PartyTo=@PartyTo_1 ' 
	END		 
	IF(@Status is not null) 
	BEGIN 
		SET @Where=@Where+' AND o.Status=@Status_1 ' 
	END		 
	IF(ISNULL(@Priority,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.Priority=@Priority_1 ' 
	END	 
	IF(ISNULL(@ExtOrderNo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.ExtOrderNo LIKE @ExtOrderNo_1 ' 
	END	 
	IF(ISNULL(@RefOrderNo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.RefOrderNo LIKE @RefOrderNo_1 ' 
	END		 
	IF(ISNULL(@TraceCode,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.TraceCode LIKE @TraceCode_1 ' 
	END	 
	IF(ISNULL(@CreateUserNm,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND o.CreateUserNm=@CreateUserNm_1 ' 
	END	 
--创建时间
    IF(ISDATE(@DateTo)=1)
	BEGIN
    set @DateTo=dateadd(day,1,@DateTo)
    END
	IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.CreateDate BETWEEN @DateFrom_1 And @DateTo_1 ' 
	END	 
	ELSE IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=0) 
	BEGIN 
		SET @Where=@Where+' AND o.CreateDate > @DateFrom_1' 
	END 
	ELSE IF(ISDATE(@DateFrom)=0 AND ISDATE(@DateTo)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.CreateDate < @DateTo_1' 
	END	 
	--开始时间
	IF(ISDATE(@EndTime)=1)
	BEGIN
    set @EndTime=dateadd(day,1,@EndTime)
    END
	IF(ISDATE(@StartTime)=1 AND ISDATE(@EndTime)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.StartTime BETWEEN @StartTime_1 And @EndTime_1 ' 
	END	 
	ELSE IF(ISDATE(@StartTime)=1 AND ISDATE(@EndTime)=0) 
	BEGIN 
		SET @Where=@Where+' AND o.StartTime > @StartTime_1' 
	END 
	ELSE IF(ISDATE(@StartTime)=0 AND ISDATE(@EndTime)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.StartTime < @EndTime_1' 
	END	 
	
	--窗口时间
	IF(ISDATE(@WindowTimeTo)=1)
	BEGIN
    set @WindowTimeTo=dateadd(day,1,@WindowTimeTo)
    END
	IF(ISDATE(@WindowTimeFrom)=1 AND ISDATE(@WindowTimeTo)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.WindowTime BETWEEN @WindowTimeFrom_1 And @WindowTimeTo_1 ' 
	END	 
	ELSE IF(ISDATE(@WindowTimeFrom)=1 AND ISDATE(@WindowTimeTo)=0) 
	BEGIN 
		SET @Where=@Where+' AND o.WindowTime > @WindowTimeFrom_1' 
	END 
	ELSE IF(ISDATE(@WindowTimeFrom)=0 AND ISDATE(@WindowTimeTo)=1) 
	BEGIN 
		SET @Where=@Where+' AND o.WindowTime < @WindowTimeTo_1' 
	END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0 AND @IsReturn=0) 
		BEGIN 
			SET @Where=@Where+' AND o.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE IF(@IsSupplier=0 AND @IsReturn=1) 
		BEGIN 
			SET @Where=@Where+' AND o.PartyFrom IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @Where=@Where+' AND (o.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR o.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
		END				 
	END 

	WHILE(LEN(@Types)>0) 
	BEGIN 
		IF(LEN(@Types)=1) 
		BEGIN 
			SET @Type=@Types 
		END 
		ELSE 
		BEGIN 
			SET @Type=SUBSTRING(@Types,1,CHARINDEX(',',@Types)-1) 
		END 
		--IF(CHARINDEX('OrderDetail',@Where)>0) 
		--BEGIN 
		--	SET @Where=REPLACE(@Where,'OrderDetail','ORD_OrderDet_'+@Type) 
		--END		 
		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			SET @SpliceTables='SELECT * FROM ORD_OrderMstr_'+@Type+' AS o '+REPLACE(@Where,'OrderDetail','ORD_OrderDet_'+@Type) 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_OrderMstr_'+@Type+' AS o '+REPLACE(@Where,'OrderDetail','ORD_OrderDet_'+@Type) 
		END 
		SET @Types=SUBSTRING(@Types,3,LEN(@Types)) 
	END	 

	SET @CountStatement=N'SELECT COUNT(1) FROM ('+@SpliceTables+') AS T1' 
	SET @Parameter=N'@OrderNo_1 varchar(50),@Flow_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@Status_1 tinyint,@Priority_1 tinyint,@ExtOrderNo_1 varchar(50),@RefOrderNo_1 varchar(50),@TraceCode_1 varchar(50),@CreateUserNm_1 varchar(100),@DateFrom_1 datetime,@DateTo_1 datetime,
	@StartTime_1 datetime,@EndTime_1 datetime,@WindowTimeFrom_1 datetime,@WindowTimeTo_1 datetime,@SubType_1 tinyint,@Sequence_1 bigint'		 
	PRINT @CountStatement 

	EXEC SP_EXECUTESQL @CountStatement,@Parameter, 
		@OrderNo_1=@OrderNo,@Flow_1=@Flow,@SubType_1=@SubType,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo,@Status_1=@Status, 
		@Priority_1=@Priority,@ExtOrderNo_1=@ExtOrderNo,@RefOrderNo_1=@RefOrderNo,@TraceCode_1=@TraceCode, 
		@CreateUserNm_1=@CreateUserNm,@DateFrom_1=@DateFrom,@DateTo_1=@DateTo,@StartTime_1=@StartTime,@EndTime_1=@EndTime,
		@WindowTimeFrom_1=@WindowTimeFrom,@WindowTimeTo_1=@WindowTimeTo,@Sequence_1=@Sequence		 

END 




/****** Object:  StoredProcedure [dbo].[USP_Search_ProcurementOrderDet]    Script Date: 2013-01-02 11:49:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER PROCEDURE [dbo].[USP_Search_ProcurementOrderDet] 
( 
	@OrderNo varchar(50), 
	@Flow varchar(50), 
	@Types varchar(50), 
	@SubType tinyint, 
	@PartyFrom varchar(50), 
	@PartyTo varchar(50), 
	@Status tinyint, 
	@Priority tinyint, 
	@ExtOrderNo varchar(50), 
	@RefOrderNo varchar(50), 
	@TraceCode varchar(50), 
	@CreateUserNm varchar(100), 
	@DateFrom datetime, 
	@DateTo datetime, 
    @StartTime datetime, 
	@EndTime datetime,
	@WindowTimeFrom datetime, 
	@WindowTimeTo datetime,   
	@Sequence bigint, 
	@IsSupplier bit, 
	@IsReturn bit, 
	@Item varchar(50), 
	@ManufactureParty varchar(50), 
	@WMSSeq varchar(50), 
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
	DECLARE @DetailWhere nvarchar(4000) 
	DECLARE @MasterWhere nvarchar(4000) 
	DECLARE @PermissionClause nvarchar(1000) 
	SET @DetailWhere='WHERE 1=1 ' 
	SET @MasterWhere='WHERE 1=1 ' 

	IF(ISNULL(@Types,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 

	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @OrderNo='%'+@OrderNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND o.OrderNo LIKE @OrderNo_1' 
	END 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.Flow=@Flow_1' 
	END 
	IF(ISNULL(@SubType,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.SubType=@SubType_1' 
	END	 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.PartyTo=@PartyTo_1 ' 
	END		 
	IF(ISNULL(@Status,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.Status=@Status_1 ' 
	END		 
	IF(ISNULL(@Priority,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.Priority=@Priority_1 ' 
	END	 
	IF(ISNULL(@ExtOrderNo,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.ExtOrderNo LIKE @ExtOrderNo_1 ' 
	END	 
	IF(ISNULL(@RefOrderNo,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.RefOrderNo LIKE @RefOrderNo_1 ' 
	END		 
	IF(ISNULL(@TraceCode,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.TraceCode LIKE @TraceCode_1 ' 
	END	 
	IF(ISNULL(@CreateUserNm,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.CreateUserNm=@CreateUserNm_1 ' 
	END	 
	
	--创建时间
    IF(ISDATE(@DateTo)=1)
	BEGIN
    set @DateTo=dateadd(day,1,@DateTo)
    END
	IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.CreateDate BETWEEN @DateFrom_1 And @DateTo_1 ' 
	END	 
	ELSE IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=0) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.CreateDate > @DateFrom_1' 
	END 
	ELSE IF(ISDATE(@DateFrom)=0 AND ISDATE(@DateTo)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.CreateDate < @DateTo_1' 
	END	 
	--开始时间
	IF(ISDATE(@EndTime)=1)
	BEGIN
    set @EndTime=dateadd(day,1,@EndTime)
    END
	IF(ISDATE(@StartTime)=1 AND ISDATE(@EndTime)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.StartTime BETWEEN @StartTime_1 And @EndTime_1 ' 
	END	 
	ELSE IF(ISDATE(@StartTime)=1 AND ISDATE(@EndTime)=0) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.StartTime > @StartTime_1' 
	END 
	ELSE IF(ISDATE(@StartTime)=0 AND ISDATE(@EndTime)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.StartTime < @EndTime_1' 
	END	 
	
	--窗口时间
	IF(ISDATE(@WindowTimeTo)=1)
	BEGIN
    set @WindowTimeTo=dateadd(day,1,@WindowTimeTo)
    END
	IF(ISDATE(@WindowTimeFrom)=1 AND ISDATE(@WindowTimeTo)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.WindowTime BETWEEN @WindowTimeFrom_1 And @WindowTimeTo_1 ' 
	END	 
	ELSE IF(ISDATE(@WindowTimeFrom)=1 AND ISDATE(@WindowTimeTo)=0) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.WindowTime > @WindowTimeFrom_1' 
	END 
	ELSE IF(ISDATE(@WindowTimeFrom)=0 AND ISDATE(@WindowTimeTo)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.WindowTime < @WindowTimeTo_1' 
	END	 

	IF(ISNULL(@Item,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.Item=@Item_1' 
	END 
	IF(ISNULL(@ManufactureParty,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.ManufactureParty=@ManufactureParty_1' 
	END 
	IF(ISNULL(@WMSSeq,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.WMSSeq=@WMSSeq_1' 
	END		 

	--IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=1) 
	--BEGIN 
	--	SET @DetailWhere=@DetailWhere+' AND d.CreateDate BETWEEN @DateFrom_1 And @DateTo_1 ' 
	--END	 
	--ELSE IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=0) 
	--BEGIN 
	--	SET @DetailWhere=@DetailWhere+' AND d.CreateDate > @DateFrom_1' 
	--END 
	--ELSE IF(ISDATE(@DateFrom)=0 AND ISDATE(@DateTo)=1) 
	--BEGIN 
	--	SET @DetailWhere=@DetailWhere+' AND d.CreateDate < @DateTo_1' 
	--END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0 AND @IsReturn=0) 
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND o.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE IF(@IsSupplier=0 AND @IsReturn=1) 
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND o.PartyFrom IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND (o.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR o.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
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
		SET @SortDesc=' ORDER BY '+@SortCloumn+' '+@SortRule 
	END 

	IF @Page>0 
	BEGIN 
		SET @PagePara='WHERE RowId BETWEEN '+cast(@PageSize*(@Page-1) as varchar(50))+' AND '++cast(@PageSize*(@Page) as varchar(50)) 
	END 

	SET @MasterWhere=REPLACE(@WhereStatement,'Where',@MasterWhere+' AND')	 
	WHILE(LEN(@Types)>0) 
	BEGIN 
		IF(LEN(@Types)=1) 
		BEGIN 
			SET @Type=@Types 
		END 
		ELSE 
		BEGIN 
			SET @Type=SUBSTRING(@Types,1,CHARINDEX(',',@Types)-1) 
		END 
		--IF(CHARINDEX('OrderDetail',@WhereStatement)>0) 
		--BEGIN 
		--	SET @WhereStatement=REPLACE(@WhereStatement,'OrderDetail','ORD_OrderDet'+@Type) 
		--END	 

		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			--SET @MasterWhere=REPLACE(@MasterWhere,'OrderMaster','ORD_OrderMstr_'+@Type+' ') 
			SET @SpliceTables='SELECT * FROM ORD_OrderDet_'+@Type+' AS d '+@DetailWhere+' '+REPLACE(@MasterWhere,'OrderMaster','ORD_OrderMstr_'+@Type+' ') 
			--PRINT @SpliceTables 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_OrderDet_'+@Type+' AS d  '+@DetailWhere+' '+REPLACE(@MasterWhere,'OrderMaster','ORD_OrderMstr_'+@Type+' ') 
		END 
		SET @Types=SUBSTRING(@Types,3,LEN(@Types)) 
	END	 

	SET @Statement=N'SELECT TOP('+CAST(@PageSize AS VARCHAR(10))+') * FROM (SELECT *,RowId=ROW_NUMBER()OVER('+@SortDesc+') FROM ('+@SpliceTables+') AS T1 ) AS T2 '+@PagePara 
	PRINT @Statement 
	PRINT LEN(@Statement) 
	SET @Parameter=N'@OrderNo_1 varchar(50),@Flow_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@Status_1 tinyint,@Priority_1 tinyint,@ExtOrderNo_1 varchar(50),@RefOrderNo_1 varchar(50),@TraceCode_1 varchar(50),@CreateUserNm_1 varchar(100),@DateFrom_1 datetime,@DateTo_1 datetime,
	@StartTime_1 datetime,@EndTime_1 datetime,@WindowTimeFrom_1 datetime,@WindowTimeTo_1 datetime,@SubType_1 tinyint,@Sequence_1 bigint,@Item_1 varchar(50),@ManufactureParty_1 varchar(50),@WMSSeq_1 varchar(50)'		 


	EXEC SP_EXECUTESQL @Statement,@Parameter, 
		@OrderNo_1=@OrderNo,@Flow_1=@Flow,@SubType_1=@SubType,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo,@Status_1=@Status, 
		@Priority_1=@Priority,@ExtOrderNo_1=@ExtOrderNo,@RefOrderNo_1=@RefOrderNo,@TraceCode_1=@TraceCode, 
		@CreateUserNm_1=@CreateUserNm,@DateFrom_1=@DateFrom,@DateTo_1=@DateTo,@StartTime_1=@StartTime,@EndTime_1=@EndTime,
		@WindowTimeFrom_1=@WindowTimeFrom,@WindowTimeTo_1=@WindowTimeTo,@Sequence_1=@Sequence,@Item_1=@Item, 
		@ManufactureParty_1=@ManufactureParty,@WMSSeq_1=@WMSSeq	 
END 




SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
ALTER  PROCEDURE [dbo].[USP_Search_ProcurementOrderDetCount] 
( 
	@OrderNo varchar(50), 
	@Flow varchar(50), 
	@Types varchar(50), 
	@SubType tinyint, 
	@PartyFrom varchar(50), 
	@PartyTo varchar(50), 
	@Status tinyint, 
	@Priority tinyint, 
	@ExtOrderNo varchar(50), 
	@RefOrderNo varchar(50), 
	@TraceCode varchar(50), 
	@CreateUserNm varchar(100), 
	@DateFrom datetime, 
	@DateTo datetime, 
	@StartTime datetime, 
	@EndTime datetime,
	@WindowTimeFrom datetime, 
	@WindowTimeTo datetime,   
	@Sequence bigint, 
	@IsSupplier bit, 
	@IsReturn bit, 
	@Item varchar(50), 
	@ManufactureParty varchar(50), 
	@WMSSeq varchar(50), 
	@UserId int, 
	@WhereStatement varchar(8000) 
) 
AS 
BEGIN 
	SET NOCOUNT ON 
	DECLARE @SpliceTables nvarchar(4000) 
	DECLARE @Type varchar(5) 
	DECLARE @CountStatement nvarchar(4000) 
	DECLARE @Parameter nvarchar(4000) 
	DECLARE @PagePara nvarchar(4000) 
	DECLARE @SortDesc nvarchar(100) 
	DECLARE @DetailWhere nvarchar(4000) 
	DECLARE @MasterWhere nvarchar(4000) 
	DECLARE @PermissionClause nvarchar(1000) 
	SET @DetailWhere='WHERE 1=1 ' 
	SET @MasterWhere='WHERE 1=1 ' 

	IF(ISNULL(@Types,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 

	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @OrderNo='%'+@OrderNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND o.OrderNo LIKE @OrderNo_1' 
	END 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.Flow=@Flow_1' 
	END 
	IF(ISNULL(@SubType,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.SubType=@SubType_1' 
	END	 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.PartyTo=@PartyTo_1 ' 
	END		 
	IF(ISNULL(@Status,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.Status=@Status_1 ' 
	END		 
	IF(ISNULL(@Priority,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.Priority=@Priority_1 ' 
	END	 
	IF(ISNULL(@ExtOrderNo,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.ExtOrderNo LIKE @ExtOrderNo_1 ' 
	END	 
	IF(ISNULL(@RefOrderNo,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.RefOrderNo LIKE @RefOrderNo_1 ' 
	END		 
	IF(ISNULL(@TraceCode,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.TraceCode LIKE @TraceCode_1 ' 
	END	 
	IF(ISNULL(@CreateUserNm,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.CreateUserNm=@CreateUserNm_1 ' 
	END	 
	
	--创建时间
    IF(ISDATE(@DateTo)=1)
	BEGIN
    set @DateTo=dateadd(day,1,@DateTo)
    END
	IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.CreateDate BETWEEN @DateFrom_1 And @DateTo_1 ' 
	END	 
	ELSE IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=0) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.CreateDate > @DateFrom_1' 
	END 
	ELSE IF(ISDATE(@DateFrom)=0 AND ISDATE(@DateTo)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.CreateDate < @DateTo_1' 
	END	 
	--开始时间
	IF(ISDATE(@EndTime)=1)
	BEGIN
    set @EndTime=dateadd(day,1,@EndTime)
    END
	IF(ISDATE(@StartTime)=1 AND ISDATE(@EndTime)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.StartTime BETWEEN @StartTime_1 And @EndTime_1 ' 
	END	 
	ELSE IF(ISDATE(@StartTime)=1 AND ISDATE(@EndTime)=0) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.StartTime > @StartTime_1' 
	END 
	ELSE IF(ISDATE(@StartTime)=0 AND ISDATE(@EndTime)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.StartTime < @EndTime_1' 
	END	 
	
	--窗口时间
	IF(ISDATE(@WindowTimeTo)=1)
	BEGIN
    set @WindowTimeTo=dateadd(day,1,@WindowTimeTo)
    END
	IF(ISDATE(@WindowTimeFrom)=1 AND ISDATE(@WindowTimeTo)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.WindowTime BETWEEN @WindowTimeFrom_1 And @WindowTimeTo_1 ' 
	END	 
	ELSE IF(ISDATE(@WindowTimeFrom)=1 AND ISDATE(@WindowTimeTo)=0) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.WindowTime > @WindowTimeFrom_1' 
	END 
	ELSE IF(ISDATE(@WindowTimeFrom)=0 AND ISDATE(@WindowTimeTo)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND o.WindowTime < @WindowTimeTo_1' 
	END	 

	IF(ISNULL(@Item,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.Item=@Item_1' 
	END 
	IF(ISNULL(@ManufactureParty,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.ManufactureParty=@ManufactureParty_1' 
	END 
	IF(ISNULL(@WMSSeq,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.WMSSeq=@WMSSeq_1' 
	END		 

	--IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=1) 
	--BEGIN 
	--	SET @DetailWhere=@DetailWhere+' AND d.CreateDate BETWEEN @DateFrom_1 And @DateTo_1 ' 
	--END	 
	--ELSE IF(ISDATE(@DateFrom)=1 AND ISDATE(@DateTo)=0) 
	--BEGIN 
	--	SET @DetailWhere=@DetailWhere+' AND d.CreateDate > @DateFrom_1' 
	--END 
	--ELSE IF(ISDATE(@DateFrom)=0 AND ISDATE(@DateTo)=1) 
	--BEGIN 
	--	SET @DetailWhere=@DetailWhere+' AND d.CreateDate < @DateTo_1' 
	--END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0 AND @IsReturn=0) 
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND o.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE IF(@IsSupplier=0 AND @IsReturn=1) 
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND o.PartyFrom IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND (o.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR o.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
		END				 
	END 

	SET @MasterWhere=REPLACE(@WhereStatement,'Where',@MasterWhere+' AND')	 
	WHILE(LEN(@Types)>0) 
	BEGIN 
		IF(LEN(@Types)=1) 
		BEGIN 
			SET @Type=@Types 
		END 
		ELSE 
		BEGIN 
			SET @Type=SUBSTRING(@Types,1,CHARINDEX(',',@Types)-1) 
		END 
		--IF(CHARINDEX('OrderDetail',@WhereStatement)>0) 
		--BEGIN 
		--	SET @WhereStatement=REPLACE(@WhereStatement,'OrderDetail','ORD_OrderDet'+@Type) 
		--END	 

		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			--SET @MasterWhere=REPLACE(@MasterWhere,'OrderMaster','ORD_OrderMstr_'+@Type+' ') 
			SET @SpliceTables='SELECT * FROM ORD_OrderDet_'+@Type+' AS d '+@DetailWhere+' '+REPLACE(@MasterWhere,'OrderMaster','ORD_OrderMstr_'+@Type+' ') 
			--PRINT @SpliceTables 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_OrderDet_'+@Type+' AS d  '+@DetailWhere+' '+REPLACE(@MasterWhere,'OrderMaster','ORD_OrderMstr_'+@Type+' ') 
		END 
		SET @Types=SUBSTRING(@Types,3,LEN(@Types)) 
	END	 
	--PRINT @SpliceTables 

	SET @CountStatement=N'SELECT COUNT(1) FROM ('+@SpliceTables+') AS T1' 
	SET @Parameter=N'@OrderNo_1 varchar(50),@Flow_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@Status_1 tinyint,@Priority_1 tinyint,@ExtOrderNo_1 varchar(50),@RefOrderNo_1 varchar(50),@TraceCode_1 varchar(50),@CreateUserNm_1 varchar(100),@DateFrom_1 datetime,@DateTo_1 datetime,
	@StartTime_1 datetime,@EndTime_1 datetime,@WindowTimeFrom_1 datetime,@WindowTimeTo_1 datetime,@SubType_1 tinyint,@Sequence_1 bigint,@Item_1 varchar(50),@ManufactureParty_1 varchar(50),@WMSSeq_1 varchar(50)'		 
	PRINT @CountStatement 

	EXEC SP_EXECUTESQL @CountStatement,@Parameter, 
		@OrderNo_1=@OrderNo,@Flow_1=@Flow,@SubType_1=@SubType,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo,@Status_1=@Status, 
		@Priority_1=@Priority,@ExtOrderNo_1=@ExtOrderNo,@RefOrderNo_1=@RefOrderNo,@TraceCode_1=@TraceCode, 
		@CreateUserNm_1=@CreateUserNm,@DateFrom_1=@DateFrom,@DateTo_1=@DateTo,@StartTime_1=@StartTime,@EndTime_1=@EndTime,
		@WindowTimeFrom_1=@WindowTimeFrom,@WindowTimeTo_1=@WindowTimeTo,@Sequence_1=@Sequence,@Item_1=@Item, 
		@ManufactureParty_1=@ManufactureParty,@WMSSeq_1=@WMSSeq	 

END 



/****** Object:  StoredProcedure [dbo].[USP_Search_RecMstr]    Script Date: 2013-01-02 11:56:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
ALTER PROCEDURE [dbo].[USP_Search_RecMstr] 
( 
	@ReciptNo varchar(50), 
	@IpNo varchar(50), 
	@Status tinyint,	 
	@OrderTypes varchar(50), 
	@PartyFrom varchar(50), 
	@PartyTo varchar(50), 
	@StartDate datetime, 
	@EndDate datetime, 
	@Dock varchar(50), 
	@Item varchar(50), 
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
		SET @IPNo=@IPNo+'%' 
		SET @Where=@Where+' AND r.IpNo LIKE @IpNo_1' 
	END	 
	IF(ISNULL(@ReciptNo,'')<>'') 
	BEGIN 
		SET @ReciptNo='%'+@ReciptNo+'%' 
		SET @Where=@Where+' AND r.RecNo LIKE @ReciptNo_1' 
	END 
	IF(ISNULL(@WMSNo,'')<>'') 
	BEGIN 
		SET @WMSNo=@WMSNo+'%' 
		SET @Where=@Where+' AND r.WMSNo LIKE @WMSNo_1' 
	END	 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND r.Flow=@Flow_1' 
	END 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND r.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND r.PartyTo=@PartyTo_1 ' 
	END		 
	IF(@Status is not null) 
	BEGIN 
		SET @Where=@Where+' AND r.Status=@Status_1 ' 
	END 
	IF(ISDATE(@EndDate)=1)
	BEGIN
    set @EndDate=dateadd(day,1,@EndDate)
    END
	IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @Where=@Where+' AND r.CreateDate BETWEEN @StartDate_1 And @EndDate_1 ' 
	END	 
	ELSE IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=0) 
	BEGIN 
		SET @Where=@Where+' AND r.CreateDate > @StartDate_1' 
	END 
	ELSE IF(ISDATE(@StartDate)=0 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @Where=@Where+' AND r.CreateDate < @EndDate_1' 
	END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0) 
		BEGIN 
			SET @Where=@Where+' AND r.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @Where=@Where+' AND (r.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR r.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
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

		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			SET @SpliceTables='SELECT * FROM ORD_RecMstr_'+@Type+' AS r '+REPLACE(@Where,'RecDetail','ORD_RecDet_'+@Type) 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_RecMstr_'+@Type+' AS r '+REPLACE(@Where,'RecDetail','ORD_RecDet_'+@Type) 
		END 
		SET @OrderTypes=SUBSTRING(@OrderTypes,3,LEN(@OrderTypes)) 
	END	 

	SET @Statement=N'SELECT TOP('+CAST(@PageSize AS VARCHAR(10))+') * FROM (SELECT *,RowId=ROW_NUMBER()OVER('+@SortDesc+') FROM ('+@SpliceTables+') AS T1 ) AS T2 '+@PagePara 
	PRINT @Statement 
	PRINT LEN(@Statement) 
	SET @Parameter=N'@IpNo_1 varchar(50),@Status_1 tinyint,	@OrderTypes_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@StartDate_1 datetime,@EndDate_1 datetime,@Dock_1 varchar(50),@Item_1 varchar(50),@ReciptNo_1 varchar(50),@WMSNo_1 varchar(50),@ManufactureParty_1 varchar(50),@Flow_1 varchar(50),@IsSupplier_1 bit'		 

	EXEC SP_EXECUTESQL @Statement,@Parameter, 
		@IPNo_1=@IPNo,@Status_1=@Status,@OrderTypes_1=@OrderTypes ,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo ,@StartDate_1=@StartDate , 
		@EndDate_1=@EndDate ,@Dock_1=@Dock ,@Item_1=@Item ,@ReciptNo_1=@ReciptNo,@WMSNo_1=@WMSNo,@ManufactureParty_1=@ManufactureParty , 
		@Flow_1=@Flow,@IsSupplier_1=@IsSupplier 

END 



/****** Object:  StoredProcedure [dbo].[USP_Search_RecMstrCount]    Script Date: 2013-01-02 11:57:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER PROCEDURE [dbo].[USP_Search_RecMstrCount] 
( 
	@ReciptNo varchar(50), 
	@IpNo varchar(50), 
	@Status tinyint,	 
	@OrderTypes varchar(50), 
	@PartyFrom varchar(50), 
	@PartyTo varchar(50), 
	@StartDate datetime, 
	@EndDate datetime, 
	@Dock varchar(50), 
	@Item varchar(50), 
	@WMSNo varchar(50), 
	@ManufactureParty varchar(50), 
	@Flow varchar(50), 
	@IsSupplier bit, 
	@UserId int, 
	@WhereStatement varchar(8000) 
) 
AS 
BEGIN 
	SET NOCOUNT ON 
	DECLARE @SpliceTables nvarchar(4000) 
	DECLARE @Type varchar(5) 
	DECLARE @CountStatement nvarchar(4000) 
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
		SET @IPNo=@IPNo+'%' 
		SET @Where=@Where+' AND r.IpNo LIKE @IpNo_1' 
	END	 
	IF(ISNULL(@ReciptNo,'')<>'') 
	BEGIN 
		SET @ReciptNo='%'+@ReciptNo+'%' 
		SET @Where=@Where+' AND r.RecNo LIKE @ReciptNo_1' 
	END 
	IF(ISNULL(@WMSNo,'')<>'') 
	BEGIN 
		SET @WMSNo=@WMSNo+'%' 
		SET @Where=@Where+' AND r.WMSNo LIKE @WMSNo_1' 
	END	 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND r.Flow=@Flow_1' 
	END 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND r.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND r.PartyTo=@PartyTo_1 ' 
	END		 
	IF(@Status is not null) 
	BEGIN 
		SET @Where=@Where+' AND r.Status=@Status_1 ' 
	END 
	IF(ISDATE(@EndDate)=1)
	BEGIN
    set @EndDate=dateadd(day,1,@EndDate)
    END
	IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @Where=@Where+' AND r.CreateDate BETWEEN @StartDate_1 And @EndDate_1 ' 
	END	 
	ELSE IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=0) 
	BEGIN 
		SET @Where=@Where+' AND r.CreateDate > @StartDate_1' 
	END 
	ELSE IF(ISDATE(@StartDate)=0 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @Where=@Where+' AND r.CreateDate < @EndDate_1' 
	END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0) 
		BEGIN 
			SET @Where=@Where+' AND r.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @Where=@Where+' AND (r.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR r.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
		END				 
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

		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			SET @SpliceTables='SELECT * FROM ORD_RecMstr_'+@Type+' AS r '+REPLACE(@Where,'RecDetail','ORD_RecDet_'+@Type) 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_RecMstr_'+@Type+' AS r '+REPLACE(@Where,'RecDetail','ORD_RecDet_'+@Type) 
		END 
		SET @OrderTypes=SUBSTRING(@OrderTypes,3,LEN(@OrderTypes)) 
	END	 

	SET @CountStatement=N'SELECT COUNT(1) FROM ('+@SpliceTables+') AS T1' 
	SET @Parameter=N'@IpNo_1 varchar(50),@Status_1 tinyint,	@OrderTypes_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@StartDate_1 datetime,@EndDate_1 datetime,@Dock_1 varchar(50),@Item_1 varchar(50),@ReciptNo_1 varchar(50),@WMSNo_1 varchar(50),@ManufactureParty_1 varchar(50),@Flow_1 varchar(50),@IsSupplier_1 bit'		 

	EXEC SP_EXECUTESQL @CountStatement,@Parameter, 
		@IPNo_1=@IPNo,@Status_1=@Status,@OrderTypes_1=@OrderTypes ,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo ,@StartDate_1=@StartDate , 
		@EndDate_1=@EndDate ,@Dock_1=@Dock ,@Item_1=@Item ,@ReciptNo_1=@ReciptNo,@WMSNo_1=@WMSNo,@ManufactureParty_1=@ManufactureParty , 
		@Flow_1=@Flow,@IsSupplier_1=@IsSupplier 

END 






/****** Object:  StoredProcedure [dbo].[USP_Search_RecDet]    Script Date: 2013-01-02 12:03:15 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER  PROCEDURE [dbo].[USP_Search_RecDet] 
( 
	@ReciptNo varchar(50), 
	@IpNo varchar(50), 
	@Status tinyint,	 
	@OrderTypes varchar(50), 
	@PartyFrom varchar(50), 
	@PartyTo varchar(50), 
	@StartDate datetime, 
	@EndDate datetime, 
	@Dock varchar(50), 
	@Item varchar(50), 
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
	DECLARE @PermissionClause nvarchar(1000) 
	DECLARE @DetailWhere nvarchar(4000) 
	DECLARE @MasterWhere nvarchar(4000)	 
	SET @DetailWhere='WHERE 1=1 ' 
	SET @MasterWhere='WHERE 1=1 ' 

	IF(ISNULL(@OrderTypes,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 
	IF(ISNULL(@IPNo,'')<>'') 
	BEGIN 
		SET @IPNo=@IPNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND r.IpNo LIKE @IpNo_1' 
	END	 
	IF(ISNULL(@ReciptNo,'')<>'') 
	BEGIN 
		SET @ReciptNo='%'+@ReciptNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND r.RecNo LIKE @ReciptNo_1' 
	END 
	IF(ISNULL(@WMSNo,'')<>'') 
	BEGIN 
		SET @WMSNo=@WMSNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND r.WMSNo LIKE @WMSNo_1' 
	END	 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.Flow=@Flow_1' 
	END 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.PartyTo=@PartyTo_1 ' 
	END		 
	IF(@Status is not null) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.Status=@Status_1 ' 
	END 
    IF(ISDATE(@EndDate)=1)
	BEGIN
    set @EndDate=dateadd(day,1,@EndDate)
    END
	IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.CreateDate BETWEEN @StartDate_1 And @EndDate_1 ' 
	END	 
	ELSE IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=0) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.CreateDate > @StartDate_1' 
	END 
	ELSE IF(ISDATE(@StartDate)=0 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.CreateDate < @EndDate_1' 
	END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0) 
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND r.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND (r.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR r.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
		END				 
	END 
	IF(ISNULL(@Item,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.Item=@Item_1' 
	END 
	IF(ISNULL(@ManufactureParty,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.ManufactureParty=@ManufactureParty_1' 
	END 
	IF(ISNULL(@Dock,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.Dock=@Dock_1' 
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
	SET @MasterWhere=REPLACE(ISNULL(@WhereStatement,''),'Where',@MasterWhere+' AND')	 
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
		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			SET @SpliceTables='SELECT * FROM ORD_RecDet_'+@Type+' AS d '+@DetailWhere+' '+REPLACE(@MasterWhere,'ReceiptMaster','ORD_RecMstr_'+@Type+' ') 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_RecDet_'+@Type+' AS d  '+@DetailWhere+' '+REPLACE(@MasterWhere,'ReceiptMaster','ORD_RecMstr_'+@Type+' ') 
		END 
		SET @OrderTypes=SUBSTRING(@OrderTypes,3,LEN(@OrderTypes)) 
	END	 
	SET @Statement=N'SELECT TOP('+CAST(@PageSize AS VARCHAR(10))+') * FROM (SELECT *,RowId=ROW_NUMBER()OVER('+@SortDesc+') FROM ('+@SpliceTables+') AS T1 ) AS T2 '+@PagePara 
	PRINT @Statement 
	PRINT LEN(@Statement) 
	SET @Parameter=N'@IpNo_1 varchar(50),@Status_1 tinyint,	@OrderTypes_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@StartDate_1 datetime,@EndDate_1 datetime,@Dock_1 varchar(50),@Item_1 varchar(50),@ReciptNo_1 varchar(50),@WMSNo_1 varchar(50),@ManufactureParty_1 varchar(50),@Flow_1 varchar(50),@IsSupplier_1 bit'		 

	EXEC SP_EXECUTESQL @Statement,@Parameter, 
		@IPNo_1=@IPNo,@Status_1=@Status,@OrderTypes_1=@OrderTypes ,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo ,@StartDate_1=@StartDate , 
		@EndDate_1=@EndDate ,@Dock_1=@Dock ,@Item_1=@Item ,@ReciptNo_1=@ReciptNo,@WMSNo_1=@WMSNo,@ManufactureParty_1=@ManufactureParty , 
		@Flow_1=@Flow,@IsSupplier_1=@IsSupplier 

END 




/****** Object:  StoredProcedure [dbo].[USP_Search_RecDetCount]    Script Date: 2013-01-02 12:05:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER  PROCEDURE [dbo].[USP_Search_RecDetCount] 
( 
	@ReciptNo varchar(50), 
	@IpNo varchar(50), 
	@Status tinyint,	 
	@OrderTypes varchar(50), 
	@PartyFrom varchar(50), 
	@PartyTo varchar(50), 
	@StartDate datetime, 
	@EndDate datetime, 
	@Dock varchar(50), 
	@Item varchar(50), 
	@WMSNo varchar(50), 
	@ManufactureParty varchar(50), 
	@Flow varchar(50), 
	@IsSupplier bit, 
	@UserId int, 
	@WhereStatement varchar(8000) 
	--@SortCloumn varchar(50)=null, 
	--@SortRule varchar(50)=null, 
	--@PageSize int, 
	--@Page int 
) 
AS 
BEGIN 
	SET NOCOUNT ON 
	DECLARE @SpliceTables nvarchar(4000) 
	DECLARE @Type varchar(5) 
	DECLARE @CountStatement nvarchar(4000) 
	DECLARE @Parameter nvarchar(4000) 
	DECLARE @PagePara nvarchar(4000) 
	DECLARE @SortDesc nvarchar(100) 
	DECLARE @PermissionClause nvarchar(1000) 
	DECLARE @DetailWhere nvarchar(4000) 
	DECLARE @MasterWhere nvarchar(4000)	 
	SET @DetailWhere='WHERE 1=1 ' 
	SET @MasterWhere='WHERE 1=1 ' 

	IF(ISNULL(@OrderTypes,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 
	IF(ISNULL(@IPNo,'')<>'') 
	BEGIN 
		SET @IPNo=@IPNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND r.IpNo LIKE @IpNo_1' 
	END	 
	IF(ISNULL(@ReciptNo,'')<>'') 
	BEGIN 
		SET @ReciptNo='%'+@ReciptNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND r.RecNo LIKE @ReciptNo_1' 
	END 
	IF(ISNULL(@WMSNo,'')<>'') 
	BEGIN 
		SET @WMSNo=@WMSNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND r.WMSNo LIKE @WMSNo_1' 
	END	 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.Flow=@Flow_1' 
	END 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.PartyTo=@PartyTo_1 ' 
	END		 
	IF(@Status is not null) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.Status=@Status_1 ' 
	END 
	IF(ISDATE(@EndDate)=1)
	BEGIN
    set @EndDate=dateadd(day,1,@EndDate)
    END
	IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.CreateDate BETWEEN @StartDate_1 And @EndDate_1 ' 
	END	 
	ELSE IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=0) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.CreateDate > @StartDate_1' 
	END 
	ELSE IF(ISDATE(@StartDate)=0 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND r.CreateDate < @EndDate_1' 
	END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0) 
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND r.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND (r.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR r.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
		END				 
	END 
	IF(ISNULL(@Item,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.Item=@Item_1' 
	END 
	IF(ISNULL(@ManufactureParty,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.ManufactureParty=@ManufactureParty_1' 
	END 
	IF(ISNULL(@Dock,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.Dock=@Dock_1' 
	END	 
	SET @MasterWhere=REPLACE(ISNULL(@WhereStatement,''),'Where',@MasterWhere+' AND')	 
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
		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			SET @SpliceTables='SELECT * FROM ORD_RecDet_'+@Type+' AS d '+@DetailWhere+' '+REPLACE(@MasterWhere,'ReceiptMaster','ORD_RecMstr_'+@Type+' ') 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_RecDet_'+@Type+' AS d  '+@DetailWhere+' '+REPLACE(@MasterWhere,'ReceiptMaster','ORD_RecMstr_'+@Type+' ') 
		END 
		SET @OrderTypes=SUBSTRING(@OrderTypes,3,LEN(@OrderTypes)) 
	END	 
	SET @CountStatement=N'SELECT COUNT(1) FROM ('+@SpliceTables+') AS T1  '--+@PagePara 
	SET @Parameter=N'@IpNo_1 varchar(50),@Status_1 tinyint,	@OrderTypes_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@StartDate_1 datetime,@EndDate_1 datetime,@Dock_1 varchar(50),@Item_1 varchar(50),@ReciptNo_1 varchar(50),@WMSNo_1 varchar(50),@ManufactureParty_1 varchar(50),@Flow_1 varchar(50),@IsSupplier_1 bit'		 

	EXEC SP_EXECUTESQL @CountStatement,@Parameter, 
		@IPNo_1=@IPNo,@Status_1=@Status,@OrderTypes_1=@OrderTypes ,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo ,@StartDate_1=@StartDate , 
		@EndDate_1=@EndDate ,@Dock_1=@Dock ,@Item_1=@Item ,@ReciptNo_1=@ReciptNo,@WMSNo_1=@WMSNo,@ManufactureParty_1=@ManufactureParty , 
		@Flow_1=@Flow,@IsSupplier_1=@IsSupplier 

END 




/****** Object:  StoredProcedure [dbo].[USP_Search_IpMstr]    Script Date: 2013-01-02 12:06:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

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

	SET @Statement=N'SELECT TOP('+CAST(@PageSize AS VARCHAR(10))+') IpNo, ExtIpNo, GapIpNo, SeqNo, Type, OrderType, OrderSubType, QualityType, Status, DepartTime, ArriveTime, PartyFrom, PartyFromNm, PartyTo, PartyToNm, ShipFrom, ShipFromAddr, ShipFromTel, ShipFromCell, ShipFromFax, ShipFromContact, ShipTo, ShipToAddr, ShipToTel, ShipToCell, ShipToFax, ShipToContact, Dock, IsAutoReceive, IsShipScanHu, IsPrintAsn, IsAsnPrinted, IsPrintRec, IsRecExceed, IsRecFulfillUC, IsRecFifo, IsAsnUniqueRec, IsCheckPartyFromAuth, IsCheckPartyToAuth, RecGapTo, AsnTemplate, RecTemplate, HuTemplate, EffDate, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, CloseDate, CloseUser, CloseUserNm, CloseReason, Version, WMSNo, CreateHuOpt, IsRecScanHu, Flow FROM (SELECT *,RowId=ROW_NUMBER()OVER('+@SortDesc+') FROM ('+@SpliceTables+') AS T1 ) AS T2 '+@PagePara 
	PRINT @Statement 
	PRINT LEN(@Statement) 
	SET @Parameter=N'@IPNo_1 varchar(50),@Status_1 tinyint,	@OrderTypes_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@StartDate_1 datetime,@EndDate_1 datetime,@Dock_1 varchar(50),@Item_1 varchar(50),@OrderNo_1 varchar(50),@WMSNo_1 varchar(50),@ManufactureParty_1 varchar(50),@Flow_1 varchar(50),@IsSupplier_1 bit'		 

	EXEC SP_EXECUTESQL @Statement,@Parameter, 
		@IPNo_1=@IPNo,@Status_1=@Status,@OrderTypes_1=@OrderTypes ,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo ,@StartDate_1=@StartDate , 
		@EndDate_1=@EndDate ,@Dock_1=@Dock ,@Item_1=@Item ,@OrderNo_1=@OrderNo,@WMSNo_1=@WMSNo,@ManufactureParty_1=@ManufactureParty , 
		@Flow_1=@Flow,@IsSupplier_1=@IsSupplier 

END 





/****** Object:  StoredProcedure [dbo].[USP_Search_IpMstrCount]    Script Date: 2013-01-02 12:33:00 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
ALTER  PROCEDURE [dbo].[USP_Search_IpMstrCount] 
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
	@WhereStatement varchar(8000) 
) 
AS 
BEGIN 
	SET NOCOUNT ON 
	DECLARE @SpliceTables nvarchar(4000) 
	DECLARE @Type varchar(5) 
	DECLARE @CountStatement nvarchar(4000) 
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

	SET @CountStatement=N'SELECT COUNT(1) FROM ('+@SpliceTables+') AS T1' 
	SET @Parameter=N'@IPNo_1 varchar(50),@Status_1 tinyint,	@OrderTypes_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@StartDate_1 datetime,@EndDate_1 datetime,@Dock_1 varchar(50),@Item_1 varchar(50),@OrderNo_1 varchar(50),@WMSNo_1 varchar(50),@ManufactureParty_1 varchar(50),@Flow_1 varchar(50),@IsSupplier_1 bit'		 

	EXEC SP_EXECUTESQL @CountStatement,@Parameter, 
		@IPNo_1=@IPNo,@Status_1=@Status,@OrderTypes_1=@OrderTypes ,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo ,@StartDate_1=@StartDate , 
		@EndDate_1=@EndDate ,@Dock_1=@Dock ,@Item_1=@Item ,@OrderNo_1=@OrderNo,@WMSNo_1=@WMSNo,@ManufactureParty_1=@ManufactureParty , 
		@Flow_1=@Flow,@IsSupplier_1=@IsSupplier 

END 




/****** Object:  StoredProcedure [dbo].[USP_Search_IpDet]    Script Date: 2013-01-02 12:33:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER  PROCEDURE [dbo].[USP_Search_IpDet] 
( 
	@IpNo varchar(50), 
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
	--DECLARE @Where nvarchar(4000) 
	DECLARE @PermissionClause nvarchar(1000) 
	DECLARE @DetailWhere nvarchar(4000) 
	DECLARE @MasterWhere nvarchar(4000)	 
	SET @DetailWhere='WHERE 1=1 ' 
	SET @MasterWhere='WHERE 1=1 ' 

	IF(ISNULL(@OrderTypes,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 
	IF(ISNULL(@IPNo,'')<>'') 
	BEGIN 
		SET @IpNo='%'+@IpNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND i.IpNo LIKE @IPNo_1' 
	END	 
	--IF(ISNULL(@OrderNo,'')<>'') 
	--BEGIN 
	--	SET @OrderNo=@OrderNo+'%' 
	--	SET @MasterWhere=@MasterWhere+' AND i.OrderNo LIKE @OrderNo_1' 
	--END 
	IF(ISNULL(@WMSNo,'')<>'') 
	BEGIN 
		SET @WMSNo=@WMSNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND i.WMSNo LIKE @WMSNo_1' 
	END	 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.Flow=@Flow_1' 
	END 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.PartyTo=@PartyTo_1 ' 
	END		 
	IF(@Status is not null) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.Status=@Status_1 ' 
	END 
	IF(ISDATE(@EndDate)=1)
	BEGIN
    set @EndDate=dateadd(day,1,@EndDate)
    END
	IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.CreateDate BETWEEN @StartDate_1 And @EndDate_1 ' 
	END	 
	ELSE IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=0) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.CreateDate > @StartDate_1' 
	END 
	ELSE IF(ISDATE(@StartDate)=0 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.CreateDate < @EndDate_1' 
	END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0) 
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND i.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND (i.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR i.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
		END				 
	END 

	IF(ISNULL(@Item,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.Item=@Item_1' 
	END 
	IF(ISNULL(@ManufactureParty,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.ManufactureParty=@ManufactureParty_1' 
	END 
	IF(ISNULL(@Dock,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.Dock=@Dock_1' 
	END		
	
	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @OrderNo=@OrderNo+'%' 
		SET @DetailWhere=@DetailWhere+' AND d.OrderNo LIKE @OrderNo_1' 
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

	SET @MasterWhere=REPLACE(ISNULL(@WhereStatement,''),'Where',@MasterWhere+' AND')	 
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

		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			SET @SpliceTables='SELECT * FROM ORD_IpDet_'+@Type+' AS d '+@DetailWhere+' '+REPLACE(@MasterWhere,'IpMaster','ORD_IpMstr_'+@Type+' ') 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_IpDet_'+@Type+' AS d  '+@DetailWhere+' '+REPLACE(@MasterWhere,'IpMaster','ORD_IpMstr_'+@Type+' ') 
		END 
		SET @OrderTypes=SUBSTRING(@OrderTypes,3,LEN(@OrderTypes)) 
	END	 


	SET @Statement=N'SELECT TOP('+CAST(@PageSize AS VARCHAR(10))+') * FROM (SELECT *,RowId=ROW_NUMBER()OVER('+@SortDesc+') FROM ('+@SpliceTables+') AS T1 ) AS T2 '+@PagePara 
	SET @Parameter=N'@IpNo_1 varchar(50),@Status_1 tinyint,	@OrderTypes_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@StartDate_1 datetime,@EndDate_1 datetime,@Dock_1 varchar(50),@Item_1 varchar(50),@OrderNo_1 varchar(50),@WMSNo_1 varchar(50),@ManufactureParty_1 varchar(50),@Flow_1 varchar(50),@IsSupplier_1 bit'		 
	PRINT @Statement 

	EXEC SP_EXECUTESQL @Statement,@Parameter, 
		@IpNo_1=@IpNo,@Status_1=@Status,@OrderTypes_1=@OrderTypes ,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo ,@StartDate_1=@StartDate , 
		@EndDate_1=@EndDate ,@Dock_1=@Dock ,@Item_1=@Item ,@OrderNo_1=@OrderNo,@WMSNo_1=@WMSNo,@ManufactureParty_1=@ManufactureParty , 
		@Flow_1=@Flow,@IsSupplier_1=@IsSupplier 

END 






/****** Object:  StoredProcedure [dbo].[USP_Search_IpDetCount]    Script Date: 2013-01-02 12:32:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

ALTER  PROCEDURE [dbo].[USP_Search_IpDetCount] 
( 
	@IpNo varchar(50), 
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
	@WhereStatement varchar(8000) 
) 
AS 
BEGIN 
	SET NOCOUNT ON 
	DECLARE @SpliceTables nvarchar(4000) 
	DECLARE @Type varchar(5) 
	DECLARE @CountStatement nvarchar(4000) 
	DECLARE @Parameter nvarchar(4000) 
	DECLARE @PagePara nvarchar(4000) 
	DECLARE @SortDesc nvarchar(100) 
	--DECLARE @Where nvarchar(4000) 
	DECLARE @PermissionClause nvarchar(1000) 
	DECLARE @DetailWhere nvarchar(4000) 
	DECLARE @MasterWhere nvarchar(4000)	 
	SET @DetailWhere='WHERE 1=1 ' 
	SET @MasterWhere='WHERE 1=1 ' 

	IF(ISNULL(@OrderTypes,'')='') 
	BEGIN 
		RAISERROR ('Please choose order types to search!' , 16, 1) WITH NOWAIT 
	END 
	IF(ISNULL(@IPNo,'')<>'') 
	BEGIN 
		SET @IpNo='%'+@IpNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND i.IpNo LIKE @IpNo_1' 
	END	 
	--IF(ISNULL(@OrderNo,'')<>'') 
	--BEGIN 
	--	SET @OrderNo=@OrderNo+'%' 
	--	SET @MasterWhere=@MasterWhere+' AND i.OrderNo LIKE @OrderNo_1' 
	--END 
	IF(ISNULL(@WMSNo,'')<>'') 
	BEGIN 
		SET @WMSNo=@WMSNo+'%' 
		SET @MasterWhere=@MasterWhere+' AND i.WMSNo LIKE @WMSNo_1' 
	END	 
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.Flow=@Flow_1' 
	END 
	IF(ISNULL(@PartyFrom,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.PartyFrom=@PartyFrom_1' 
	END 
	IF(ISNULL(@PartyTo,'')<>'') 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.PartyTo=@PartyTo_1 ' 
	END		 
	IF(@Status is not null) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.Status=@Status_1 ' 
	END 
    IF(ISDATE(@EndDate)=1)
	BEGIN
    set @EndDate=dateadd(day,1,@EndDate)
    END
	IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.CreateDate BETWEEN @StartDate_1 And @EndDate_1 ' 
	END	 
	ELSE IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=0) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.CreateDate > @StartDate_1' 
	END 
	ELSE IF(ISDATE(@StartDate)=0 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @MasterWhere=@MasterWhere+' AND i.CreateDate < @EndDate_1' 
	END	 

	DECLARE @UserCode varchar(50) 
	SELECT @UserCode=Code FROM ACC_User WHERE Id=@UserId 
	IF(UPPER(@UserCode)<>'SU') 
	BEGIN 
		SELECT PermissionCode INTO #Temp FROM VIEW_UserPermission WHERE UserId=@UserId 
		IF(@IsSupplier=0) 
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND i.PartyTo IN (SELECT PermissionCode FROM #Temp) ' 
		END 
		ELSE  
		BEGIN 
			SET @MasterWhere=@MasterWhere+' AND (i.PartyFrom IN (SELECT PermissionCode FROM #Temp) OR i.PartyTo IN (SELECT PermissionCode FROM #Temp))  ' 
		END				 
	END 

	IF(ISNULL(@Item,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.Item=@Item_1' 
	END 
	IF(ISNULL(@ManufactureParty,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.ManufactureParty=@ManufactureParty_1' 
	END 
	IF(ISNULL(@Dock,'')<>'') 
	BEGIN 
		SET @DetailWhere=@DetailWhere+' AND d.Dock=@Dock_1' 
	END	
	
	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @OrderNo=@OrderNo+'%' 
		SET @DetailWhere=@DetailWhere+' AND d.OrderNo LIKE @OrderNo_1' 
	END 		 

	SET @MasterWhere=REPLACE(ISNULL(@WhereStatement,''),'Where',@MasterWhere+' AND')	 
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
		IF(ISNULL(@SpliceTables,'')='') 
		BEGIN 
			SET @SpliceTables='SELECT * FROM ORD_IpDet_'+@Type+' AS d '+@DetailWhere+' '+REPLACE(@MasterWhere,'IpMaster','ORD_IpMstr_'+@Type+' ') 
		END 
		ELSE 
		BEGIN 
			SET @SpliceTables=@SpliceTables+' UNION ALL SELECT * FROM ORD_IpDet_'+@Type+' AS d  '+@DetailWhere+' '+REPLACE(@MasterWhere,'IpMaster','ORD_IpMstr_'+@Type+' ') 
		END 
		SET @OrderTypes=SUBSTRING(@OrderTypes,3,LEN(@OrderTypes)) 
	END	 

	SET @CountStatement=N'SELECT COUNT(1) FROM ('+@SpliceTables+') AS T1' 
	PRINT @CountStatement 
	SET @Parameter=N'@IpNo_1 varchar(50),@Status_1 tinyint,	@OrderTypes_1 varchar(50),@PartyFrom_1 varchar(50),@PartyTo_1 varchar(50),@StartDate_1 datetime,@EndDate_1 datetime,@Dock_1 varchar(50),@Item_1 varchar(50),@OrderNo_1 varchar(50),@WMSNo_1 varchar(50),@ManufactureParty_1 varchar(50),@Flow_1 varchar(50),@IsSupplier_1 bit'		 

	EXEC SP_EXECUTESQL @CountStatement,@Parameter, 
		@IpNo_1=@IpNo,@Status_1=@Status,@OrderTypes_1=@OrderTypes ,@PartyFrom_1=@PartyFrom,@PartyTo_1=@PartyTo ,@StartDate_1=@StartDate , 
		@EndDate_1=@EndDate ,@Dock_1=@Dock ,@Item_1=@Item ,@OrderNo_1=@OrderNo,@WMSNo_1=@WMSNo,@ManufactureParty_1=@ManufactureParty , 
		@Flow_1=@Flow,@IsSupplier_1=@IsSupplier 

END 




--end tiansu 20130102 订单号模糊查询