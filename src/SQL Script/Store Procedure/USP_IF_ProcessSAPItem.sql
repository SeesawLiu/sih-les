SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_IF_ProcessSAPItem') 
     DROP PROCEDURE USP_IF_ProcessSAPItem 
GO

CREATE PROCEDURE [dbo].[USP_IF_ProcessSAPItem] 
( 
	@BatchNo varchar(50),
	@UserId int,
	@UserName varchar(100)
) 
AS 
BEGIN 
	SET NOCOUNT ON
	
	DECLARE @CurrentDate datetime
	SET @CurrentDate=GETDATE()
	
	IF(ISNULL(@BatchNo,'')<>'') 
	BEGIN 
		-----插入单位 
		BEGIN TRY
			INSERT INTO MD_Uom(Code,Desc1,CreateUser,CreateUserNm,CreateDate,LastModifyUser,LastModifyUserNm,LastModifyDate) 
			SELECT DISTINCT Uom,Uom,@UserId,@UserName,@CurrentDate,@UserId,@UserName,@CurrentDate FROM Sap_Item i 
			WHERE BatchNo=@BatchNo AND i.Uom IS NOT NULL AND NOT EXISTS(SELECT 1 FROM MD_Uom u WHERE i.Uom=u.Code) 

			-----更新物料主数据 
			UPDATE mi SET 
				mi.RefCode=ii.RefCode,
				mi.ShortCode = ii.ShortCode,
				mi.Desc1 = ii.Desc1,
				mi.Uom = ii.Uom,
				mi.DISPO = ii.DISPO,
				mi.PLIFZ = ii.PLIFZ,
				mi.BESKZ = ii.BESKZ,
				mi.SOBSL = ii.SOBSL,
				mi.LastModifyDate = @CurrentDate,
				mi.LastModifyUser = @UserId,
				mi.LastModifyUserNm = @UserName
			FROM MD_Item mi inner join SAP_Item ii on mi.Code = ii.Code
			WHERE ii.BatchNo=@BatchNo 
				and (ISNULL(mi.RefCode, '') <> ISNULL(ii.RefCode, '') OR
					ISNULL(mi.ShortCode, '') <> ISNULL(ii.ShortCode, '') OR
					ISNULL(mi.Desc1, '') <> ISNULL(ii.Desc1, '') OR
					ISNULL(mi.Uom, '') <> ISNULL(ii.Uom, '') OR
					ISNULL(mi.DISPO, '') <> ISNULL(ii.DISPO, '') OR
					ISNULL(mi.PLIFZ, '') <> ISNULL(ii.PLIFZ, '') OR
					ISNULL(mi.BESKZ, '') <> ISNULL(ii.BESKZ, '') OR
					ISNULL(mi.SOBSL, '') <> ISNULL(ii.SOBSL, ''))

			INSERT INTO MD_Item
			   (Code
			   ,RefCode
			   ,Uom
			   ,Desc1
			   ,UC
			   ,IsActive
			   ,IsPurchase
			   ,IsSales
			   ,IsManufacture
			   ,IsSubContract
			   ,IsCustomerGoods
			   ,IsVirtual
			   ,IsKit
			   ,IsInvFreeze
			   ,Warranty
			   ,WarnLeadTime
			   ,CreateUser
			   ,CreateUserNm
			   ,CreateDate
			   ,LastModifyUser
			   ,LastModifyUserNm
			   ,LastModifyDate
			   ,ShortCode
			   ,MinUC
			   ,SpecifiedModel
			   ,NotBackFlush
			   ,DISPO
			   ,PLIFZ
			   ,BESKZ
			   ,SOBSL)
			SELECT DISTINCT Code
			   ,RefCode
			   ,Uom
			   ,Desc1
			   ,1
			   ,1
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,0
			   ,@UserId
			   ,@UserName
			   ,@CurrentDate
			   ,@UserId
			   ,@UserName
			   ,@CurrentDate
			   ,ShortCode
			   ,0
			   ,null
			   ,0
			   ,DISPO
			   ,PLIFZ
			   ,BESKZ
			   ,SOBSL		
			FROM SAP_Item ii 
			WHERE ii.BatchNo=@BatchNo and NOT EXISTS(SELECT 1 FROM MD_Item mi WHERE mi.Code=ii.Code)
		END TRY 
		BEGIN CATCH 
			declare @ErrorMsg nvarchar(Max)
			set @ErrorMsg = Error_Message()
			RAISERROR(@ErrorMsg, 16, 1)
		END CATCH				 
	END 
END 

