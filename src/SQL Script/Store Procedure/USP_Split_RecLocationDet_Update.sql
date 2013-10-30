SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE name='USP_Split_RecLocationDet_Update') 
	DROP PROCEDURE USP_Split_RecLocationDet_Update
GO

CREATE PROCEDURE USP_Split_RecLocationDet_Update
(
	@RecNo varchar(50),
	@RecDetId int,
	@OrderType tinyint,
	@OrderDetId int,
	@Item varchar(50),
	@HuId varchar(50),
	@LotNo varchar(50),
	@IsCreatePlanBill bit,
	@IsCS bit,
	@PlanBill int,
	@ActBill int,
	@QualityType tinyint,
	@IsFreeze bit,
	@IsATP bit,
	@OccupyType tinyint,
	@OccupyRefNo varchar(50),
	@Qty decimal(18,8),
	@WMSSeq varchar(50),
	@LastModifyUser int,
	@LastModifyUserNm varchar(100),
	@LastModifyDate datetime,
	@Id int
)
AS
BEGIN
	DECLARE @Statement nvarchar(4000)
	DECLARE @Parameters nvarchar(4000)
	SET @Statement='UPDATE ORD_RecLocationDet_' + convert(varchar, @OrderType) + ' SET RecNo=@RecNo_1,RecDetId=@RecDetId_1,OrderType=@OrderType_1,OrderDetId=@OrderDetId_1,Item=@Item_1,HuId=@HuId_1,LotNo=@LotNo_1,IsCreatePlanBill=@IsCreatePlanBill_1,IsCS=@IsCS_1,PlanBill=@PlanBill_1,ActBill=@ActBill_1,QualityType=@QualityType_1,IsFreeze=@IsFreeze_1,IsATP=@IsATP_1,OccupyType=@OccupyType_1,OccupyRefNo=@OccupyRefNo_1,Qty=@Qty_1,WMSSeq=@WMSSeq_1,LastModifyUser=@LastModifyUser_1,LastModifyUserNm=@LastModifyUserNm_1,LastModifyDate=@LastModifyDate_1 WHERE Id=@Id_1'
	SET @Parameters='@RecNo_1 varchar(50), @RecDetId_1 int, @OrderType_1 tinyint, @OrderDetId_1 int, @Item_1 varchar(50), @HuId_1 varchar(50), @LotNo_1 varchar(50), @IsCreatePlanBill_1 bit, @IsCS_1 bit, @PlanBill_1 int, @ActBill_1 int, @QualityType_1 tinyint, @IsFreeze_1 bit, @IsATP_1 bit, @OccupyType_1 tinyint, @OccupyRefNo_1 varchar(50), @Qty_1 decimal(18,8), @WMSSeq_1 varchar(50), @LastModifyUser_1 int, @LastModifyUserNm_1 varchar(100), @LastModifyDate_1 datetime, @Id_1 int'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@RecNo_1=@RecNo,@RecDetId_1=@RecDetId,@OrderType_1=@OrderType,@OrderDetId_1=@OrderDetId,@Item_1=@Item,@HuId_1=@HuId,@LotNo_1=@LotNo,@IsCreatePlanBill_1=@IsCreatePlanBill,@IsCS_1=@IsCS,@PlanBill_1=@PlanBill,@ActBill_1=@ActBill,@QualityType_1=@QualityType,@IsFreeze_1=@IsFreeze,@IsATP_1=@IsATP,@OccupyType_1=@OccupyType,@OccupyRefNo_1=@OccupyRefNo,@Qty_1=@Qty,@WMSSeq_1=@WMSSeq,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,@LastModifyDate_1=@LastModifyDate,@Id_1=@Id
END
GO
