SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[USP_Busi_GetAvailableInventory]
(
	@Location varchar(50),
	@Item varchar(50)
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
exec [USP_Busi_GetAotuPickInventory] 'SQCK01','5801306676',0,0,0,1
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
	
	IF ISNULL(@Item,'')=''
	BEGIN
		RAISERROR ('Backend Query is not correct!' , 16, 1) WITH NOWAIT
	END	
	
	--PRINT @PartSuffix
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameter nvarchar(4000)
	
	SET @Statement=N'SELECT lld.Id, lld.Location, lld.Bin, lld.Item, lld.HuId, 
                      lld.LotNo, lld.Qty, lld.IsCS, lld.PlanBill, lld.QualityType, 
                      lld.IsFreeze, lld.IsATP, lld.OccupyType, lld.OccupyRefNo, 
                      lld.CreateUser, lld.CreateUserNm, lld.CreateDate, lld.LastModifyUser, 
                      lld.LastModifyUserNm, lld.LastModifyDate, lld.Version, lb.Area, lld.CSSupplier,
                      lb.Seq AS BinSeq, hu.Qty AS HuQty, hu.UC, hu.Uom AS HuUom, hu.BaseUom, hu.UnitQty, 
                      hu.ManufactureParty, hu.ManufactureDate, hu.FirstInvDate, pb.Party AS ConsigementParty, hu.IsOdd
                      FROM dbo.INV_LocationLotDet_'+@PartSuffix+' as lld LEFT OUTER JOIN
                      dbo.INV_Hu as hu ON lld.HuId = hu.HuId LEFT OUTER JOIN
                      dbo.BIL_PlanBill as pb ON lld.PlanBill =pb.Id AND lld.IsCS = 1 LEFT OUTER JOIN
                      dbo.MD_LocationBin as lb ON lld.Bin = lb.Code
				WHERE lld.Qty>0 AND lld.Location=@Location_1 AND lld.Item=@Item_1 AND lld.QualityType=0 
					AND lld.OccupyType=0 AND lld.IsFreeze=0 AND lld.IsATP=1'
	SET @Parameter=N'@Location_1 varchar(50),@Item_1 varchar(50)'
	PRINT @Statement
	exec sp_executesql @Statement,@Parameter,
		@Location_1=@Location,@Item_1=@Item
	
END
