SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_GetPlusCSInventory') 
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
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version, NULL AS Area,NULL AS BinSeq, 
                      NULL AS HuQty,NULL AS UC,NULL AS HuUom,NULL AS BaseUom,NULL AS UnitQty,NULL AS ManufactureParty, 
                      NULL AS ManufactureDate,NULL AS FirstInvDate,NULL AS IsOdd 
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld 
				WHERE lld.Item=@Item_1 AND lld.Location=@Location_1 AND lld.CSSupplier=@CSSupplier_1 
				    AND lld.HuId is null AND lld.IsFreeze = 0 AND lld.Qty > 0 AND lld.IsCS = 1					  
					AND lld.QualityType=@QualityType_1 AND lld.OccupyType=@OccupyType_1' 
	SET @Parameter=N'@Item_1 varchar(50),@CSSupplier_1 varchar(50),@Location_1 varchar(50),@QualityType_1 tinyint,@OccupyType_1 tinyint' 

	exec sp_executesql @Statement,@Parameter, 
		@Item_1=@Item,@CSSupplier_1=@CSSupplier,@Location_1=@Location,@QualityType_1=@QualityType,@OccupyType_1=@OccupyType 

END 
