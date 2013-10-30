SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE name='USP_Split_IpLocationDet_Insert') 
	DROP PROCEDURE USP_Split_IpLocationDet_Insert
GO

CREATE PROCEDURE USP_Split_IpLocationDet_Insert
(
	@Version int,
	@IpNo varchar(50),
	@IpDetId int,
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
	@RecQty decimal(18,8),
	@IsClose bit,
	@WMSSeq varchar(50),
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
	EXEC USP_SYS_GetNextId 'ORD_IpLocationDet', @Id output 
	
	SET @Statement='INSERT INTO ORD_IpLocationDet_' + CONVERT(varchar, @OrderType) + '(Id,Version,IpNo,IpDetId,OrderType,OrderDetId,Item,HuId,LotNo,IsCreatePlanBill,IsCS,PlanBill,ActBill,QualityType,IsFreeze,IsATP,OccupyType,OccupyRefNo,Qty,RecQty,IsClose,WMSSeq,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate) VALUES(@Id_1,@Version_1,@IpNo_1,@IpDetId_1,@OrderType_1,@OrderDetId_1,@Item_1,@HuId_1,@LotNo_1,@IsCreatePlanBill_1,@IsCS_1,@PlanBill_1,@ActBill_1,@QualityType_1,@IsFreeze_1,@IsATP_1,@OccupyType_1,@OccupyRefNo_1,@Qty_1,@RecQty_1,@IsClose_1,@WMSSeq_1,@CreateUser_1,@CreateUserNm_1,@CreateDate_1,@LastModifyUser_1,@LastModifyUserNm_1,@LastModifyDate_1)'
	SET @Parameters='@Id_1 int, @Version_1 int, @IpNo_1 varchar(50), @IpDetId_1 int, @OrderType_1 tinyint, @OrderDetId_1 int, @Item_1 varchar(50), @HuId_1 varchar(50), @LotNo_1 varchar(50), @IsCreatePlanBill_1 bit, @IsCS_1 bit, @PlanBill_1 int, @ActBill_1 int, @QualityType_1 tinyint, @IsFreeze_1 bit, @IsATP_1 bit, @OccupyType_1 tinyint, @OccupyRefNo_1 varchar(50), @Qty_1 decimal(18,8), @RecQty_1 decimal(18,8), @IsClose_1 bit, @WMSSeq_1 varchar(50), @CreateUser_1 int, @CreateUserNm_1 varchar(100), @CreateDate_1 datetime, @LastModifyUser_1 int, @LastModifyUserNm_1 varchar(100), @LastModifyDate_1 datetime'
	EXEC SP_EXECUTESQL @Statement,@Parameters,@Id_1=@Id,@Version_1=@Version,@IpNo_1=@IpNo,@IpDetId_1=@IpDetId,@OrderType_1=@OrderType,@OrderDetId_1=@OrderDetId,@Item_1=@Item,@HuId_1=@HuId,@LotNo_1=@LotNo,@IsCreatePlanBill_1=@IsCreatePlanBill,@IsCS_1=@IsCS,@PlanBill_1=@PlanBill,@ActBill_1=@ActBill,@QualityType_1=@QualityType,@IsFreeze_1=@IsFreeze,@IsATP_1=@IsATP,@OccupyType_1=@OccupyType,@OccupyRefNo_1=@OccupyRefNo,@Qty_1=@Qty,@RecQty_1=@RecQty,@IsClose_1=@IsClose,@WMSSeq_1=@WMSSeq,@CreateUser_1=@CreateUser,@CreateUserNm_1=@CreateUserNm,@CreateDate_1=@CreateDate,@LastModifyUser_1=@LastModifyUser,@LastModifyUserNm_1=@LastModifyUserNm,@LastModifyDate_1=@LastModifyDate
	SELECT @Id
END
GO