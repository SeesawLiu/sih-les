SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_GetOccupyInventory') 
     DROP PROCEDURE USP_Busi_GetOccupyInventory 
GO

CREATE PROCEDURE [dbo].[USP_Busi_GetOccupyInventory] 
( 
 @Location varchar(50), 
 @Item varchar(50), 
 @QualityType tinyint, 
 @OccupyType tinyint, 
 @OccupyReferenceNo varchar(50), 
 @IsConsignment bit 
) 
AS 
BEGIN 
/*******************获取被占用库存数据***************************** 
*******************create info************************************* 
Author:  zhangsheng 
CreateDate;2012-05-25 
*******************Modify Info************************************* 
LastModifyDate: 
Modify For: 
exec [USP_Busi_GetOccupyInventory] 'CS0010','5801306476',0,0,'123', 1 
************steps************************************************** 
step1.GetManufacturePartyConsignmentInventory 
******************************************************************/ 
 SET NOCOUNT ON; 
 DECLARE @PartSuffix varchar(50) 
 SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location 
 IF ISNULL(@PartSuffix,'')='' 
 BEGIN 
  SET @PartSuffix='0' 
  --RAISERROR ('Can''t get any data from table' , 16, 1) WITH NOWAIT 
 END 
 IF ISNULL(@Item,'')='' OR @QualityType IS NULL OR @OccupyType IS NULL OR ISNULL(@OccupyReferenceNo,'')='' 
 BEGIN 
  RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT 
 END  
 DECLARE @Statement nvarchar(4000) 
 DECLARE @Parameter nvarchar(4000) 
 SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId,  
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType,  
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo,  
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser,  
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version, NULL AS Area,NULL AS BinSeq, 
                      NULL AS HuQty,NULL AS UC,NULL AS HuUom,NULL AS BaseUom,NULL AS UnitQty,NULL AS ManufactureParty, 
                      NULL AS ManufactureDate,NULL AS FirstInvDate,NULL AS IsOdd 
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld 
    WHERE lld.Item=@Item_1 AND lld.Location = @Location_1 AND lld.OccupyRefNo=@OccupyReferenceNo_1 AND lld.OccupyType=@OccupyType_1  
	  AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty > 0 AND lld.QualityType=@QualityType_1' 
 SET @Parameter=N'@Location_1 varchar(50),@Item_1 varchar(50),@QualityType_1 tinyint,@OccupyType_1 tinyint,@OccupyReferenceNo_1 varchar(50)' 
 exec sp_executesql @Statement,@Parameter, 
  @Location_1=@Location,@Item_1=@Item,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType,@OccupyReferenceNo_1=@OccupyReferenceNo 
END 
