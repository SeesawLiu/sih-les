IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Busi_GetMergeInventory')
	DROP PROCEDURE USP_Busi_GetMergeInventory	
GO
CREATE PROCEDURE [dbo].[USP_Busi_GetMergeInventory]
(
	@Location varchar(50),
	@Item varchar(50),
	@Qty decimal(18,8),
	@PlanBill int,
	@QualityType tinyint,
	@IsFreeze bit,
	@IsATP bit,
	@OccupyType tinyint,
	@OccupyRefNo varchar(50),
	@CSSupplier varchar(50)
)
AS
BEGIN
/*******************获取可以合并的库存数据*************************
*******************create info*************************************
Author:		zhangsheng
CreateDate;2012-05-25
*******************Modify Info*************************************
LastModifyDate:
Modify For:
exec [USP_Busi_GetMergeInventory] 'HBF010','5801281111',1,5285,1,0,1,2,NS00000039,null
************steps**************************************************
step1.USP_Busi_GetMergeInventory
******************************************************************/
	SET NOCOUNT ON;
	
	DECLARE @PartSuffix varchar(50)
	SELECT @PartSuffix = PartSuffix FROM MD_Location WHERE Code = @Location
	
	IF ISNULL(@PartSuffix,'')=''
	BEGIN
		SET @PartSuffix='0'
	END 
	
	IF ISNULL(@Location,'')='' OR ISNULL(@Item,'')='' 
	OR @Qty IS NULL	OR @QualityType IS NULL OR @OccupyType IS NULL
	OR (@OccupyType <> 0 AND ISNULL(@OccupyRefNo,'')='')
	OR @IsFreeze IS NULL OR @IsATP IS NULL
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.CSSupplier, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld
				WHERE lld.Location=@Location_1 AND lld.Item=@Item_1 AND HuId is null'
	
	IF @Qty > 0
	BEGIN
		SET @Statement=@Statement + ' AND lld.Qty > 0'
    END
    ELSE
    BEGIN
		SET @Statement=@Statement + ' AND lld.Qty < 0'
    END
    
    IF @PlanBill IS NOT NULL
    BEGIN
		SET @Statement=@Statement + ' AND lld.IsCS=1 and lld.PlanBill=@PlanBill_1'
    END
    ELSE
    BEGIN
		SET @Statement=@Statement + ' AND lld.PlanBill IS NULL'
    END
    
    SET @Statement=@Statement + ' AND lld.QualityType=@QualityType_1 AND lld.IsFreeze=@IsFreeze_1 
                                  AND lld.IsATP=@IsATP_1 AND lld.OccupyType=@OccupyType_1'
	
    IF @OccupyType <> 0
    BEGIN
		SET @Statement=@Statement + ' AND lld.OccupyRefNo=@OccupyRefNo_1'
    END
    
    IF ISNULL(@CSSupplier,'')<>''
    BEGIN
		SET @Statement=@Statement + ' AND lld.CSSupplier=@CSSupplier_1'
    END
 
	SET @Parameter=N'@Location_1 varchar(50),@Item_1 varchar(50),@PlanBill_1 int,@QualityType_1 tinyint,@IsFreeze_1 bit
	,@IsATP_1 bit,@OccupyType_1 tinyint,@OccupyRefNo_1 varchar(50),@CSSupplier_1 varchar(50)'
	
	exec sp_executesql @Statement,@Parameter,
		@Location_1=@Location,@Item_1=@Item,@PlanBill_1=@PlanBill,@QualityType_1=@QualityType,@IsFreeze_1=@IsFreeze,@IsATP_1=@IsATP
		,@OccupyType_1=@OccupyType,@OccupyRefNo_1=@OccupyRefNo,@CSSupplier_1=@CSSupplier

END



GO


