SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_IF_ProcessSAPSupplier') 
     DROP PROCEDURE USP_IF_ProcessSAPSupplier 
GO

CREATE PROCEDURE [dbo].[USP_IF_ProcessSAPSupplier] 
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
	declare @trancount int = @@trancount
	
	IF(ISNULL(@BatchNo,'')<>'') 
	BEGIN 
		BEGIN TRY 
			if @trancount = 0
			begin
				begin tran
			end
				
			SELECT * INTO #TempSupplier FROM SAP_Supplier ss WHERE BatchNo=@BatchNo AND NOT EXISTS(SELECT 1 FROM MD_Supplier s where ss.Code=s.Code) 

			if exists(select top 1 1 from #TempSupplier)
			begin
				INSERT INTO MD_Address(Code, Type, Address, PostCode, TelPhone, MobilePhone, Fax, Email, ContactPsnNm, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate) 
				SELECT DISTINCT Code+'_BA',1,NULL,NULL,NULL,NULL,NULL,NULL,NULL,@UserId,@UserName,@CurrentDate,@UserId,@UserName,@CurrentDate FROM #TempSupplier WHERE Code IS NOT NULL
				UNION ALL 
				SELECT DISTINCT Code+'_SA',0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,@UserId,@UserName,@CurrentDate,@UserId,@UserName,@CurrentDate FROM #TempSupplier WHERE Code IS NOT NULL			 

				-----插入区域 
				INSERT INTO MD_Party(Code, Name, IsActive, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate) 
				SELECT DISTINCT Code, Name, 1, @UserId, @UserName, @CurrentDate, @UserId,@UserName,@CurrentDate FROM #TempSupplier WHERE Code IS NOT NULL

				------插入区域地址 
				INSERT INTO MD_PartyAddr(Party, Address, IsPrimary, Seq, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate) 
				SELECT DISTINCT Code,Code+'_BA',1,1,@UserId,@UserName,@CurrentDate,@UserId,@UserName,@CurrentDate FROM #TempSupplier WHERE Code IS NOT NULL
				UNION ALL 
				SELECT DISTINCT Code,Code+'_SA',0,1,@UserId,@UserName,@CurrentDate,@UserId,@UserName,@CurrentDate FROM #TempSupplier WHERE Code IS NOT NULL
				
				-----插入供应商权限
				INSERT INTO ACC_Permission(Code,Desc1,Category)
				SELECT DISTINCT Code,Name,'Supplier' FROM #TempSupplier ts
					WHERE NOT EXISTS(SELECT 1 FROM ACC_Permission pm WHERE pm.Code=ts.Code)
				
				-----插入供应商
				UPDATE TOP(1) A SET A.Name=B.Name,A.LastModifyDate=@CurrentDate
				FROM MD_Party A LEFT JOIN SAP_Supplier B
				ON A.Code=B.Code 
				WHERE  B.BatchNo=@BatchNo AND B.Code IS NOT NULL
				
				INSERT INTO MD_Supplier(Code, ShortCode) 
				SELECT DISTINCT Code, ShortCode FROM #TempSupplier WHERE Code IS NOT NULL
			end
			
			if exists(select top 1 1 from MD_Supplier as s inner join SAP_Supplier as ss on s.Code = ss.Code and ISNULL(s.ShortCode,'') <> ISNULL(ss.ShortCode,'') and ss.BatchNo=@BatchNo)
			begin
				update s set ShortCode = ss.ShortCode
				from MD_Supplier as s inner join SAP_Supplier as ss on s.Code = ss.Code and ISNULL(s.ShortCode,'') <> ISNULL(ss.ShortCode,'') and ss.BatchNo=@BatchNo
			end
			
			if exists(select top 1 1 from MD_Party as p inner join SAP_Supplier as ss on p.Code = ss.Code and ISNULL(p.Name,'') <> ISNULL(ss.Name,'') and ss.BatchNo=@BatchNo)
			begin
				update p set Name = ss.Name
				from MD_Party as p inner join SAP_Supplier as ss on p.Code = ss.Code and ISNULL(p.Name,'') <> ISNULL(ss.Name,'') and ss.BatchNo=@BatchNo
			end
			
			if exists(select top 1 1 from ORD_OrderMstr_8 as mstr inner join SAP_Supplier as ss on mstr.PartyFrom = ss.Code and ISNULL(mstr.PartyFromNm,'') <> ISNULL(ss.Name,'') and ss.BatchNo=@BatchNo)
			begin
				update mstr set PartyFromNm = ss.Name
				from ORD_OrderMstr_8 as mstr inner join SAP_Supplier as ss on mstr.PartyFrom = ss.Code and ISNULL(mstr.PartyFromNm,'') <> ISNULL(ss.Name,'') and ss.BatchNo=@BatchNo
			end
			
			drop table #TempSupplier
				
			if @trancount = 0 
			begin  
				commit
			end
		END TRY 
		BEGIN CATCH 
			if @trancount = 0
			begin
				rollback
			end 
			
			declare @ErrorMsg nvarchar(Max)
			set @ErrorMsg = Error_Message()
			RAISERROR(@ErrorMsg, 16, 1)
		END CATCH				 
	END 
END 
