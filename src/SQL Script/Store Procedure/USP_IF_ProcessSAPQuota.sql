SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_IF_ProcessSAPQuota') 
     DROP PROCEDURE USP_IF_ProcessSAPQuota 
GO

CREATE PROCEDURE [dbo].[USP_IF_ProcessSAPQuota] 
( 
	@BatchNo varchar(50),
	@UserId int,
	@UserName varchar(100)
) 
AS 
BEGIN 
	SET NOCOUNT ON
	
	DECLARE @DateTimeNow datetime = GETDATE()
	
	IF(ISNULL(@BatchNo,'')<>'') 
	BEGIN 
		BEGIN TRY
			create table #tempDuplicateQuota
			(
				LIFNR varchar(50),
				MATNR varchar(50),
				MSG varchar(500)
			)
			
			--更新Quota为0的
			update mq set [Weight] = sq.QUOTE, LastModifyUser = @UserId, LastModifyUserNm = @UserName, LastModifyDate = @DateTimeNow, [Version] = [Version] + 1
			from SCM_Quota as mq inner join SAP_Quota as sq on mq.Item = sq.MATNR and mq.Supplier = sq.LIFNR
			where sq.BatchNo = @BatchNo and sq.QUOTE = 0
			
			--新增配额为0的
			INSERT INTO SCM_Quota(Supplier, SupplierShortCode, SupplierNm, Item, RefItemCode, ItemDesc, StartDate, EndDate, [Weight], Rate, CycleQty, AccumulateQty, AdjQty, IsActive, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, [Version])
			select sq.LIFNR, s.ShortCode, p.Name, sq.MATNR, i.RefCode, i.Desc1, null, null, MAX(sq.QUOTE), null, 0, 0, 0, 1, @UserId, @UserName, @DateTimeNow, @UserId, @UserName, @DateTimeNow, 1
			from SAP_Quota as sq left join SCM_Quota as mq on mq.Item = sq.MATNR and mq.Supplier = sq.LIFNR
			inner join MD_Supplier as s on sq.LIFNR = s.Code
			inner join MD_Party as p on sq.LIFNR = p.Code
			inner join MD_Item as i on sq.MATNR = i.Code
			where sq.BatchNo = @BatchNo and sq.QUOTE = 0 and mq.Id is null
			group by sq.LIFNR, s.ShortCode, p.Name, sq.MATNR, i.RefCode, i.Desc1
			
			--
			insert into #tempDuplicateQuota(LIFNR, MATNR, MSG)
			select LIFNR, MATNR, N'供应商' + LIFNR + N'零件' + MATNR + N'有多个不相同的有效配额值。' as MSG  
			from SAP_Quota where BatchNo = @BatchNo and QUOTE > 0 group by LIFNR, MATNR having MAX(QUOTE) <> MIN(QUOTE)
			
			--更新配额大于0
			update mq set IsActive = 1, [Weight] = sq.QUOTE, LastModifyUser = @UserId, LastModifyUserNm = @UserName, LastModifyDate = @DateTimeNow, [Version] = [Version] + 1
			from SCM_Quota as mq inner join SAP_Quota as sq on mq.Item = sq.MATNR and mq.Supplier = sq.LIFNR
			and not exists(select top 1 1 from #tempDuplicateQuota as d where d.MATNR = sq.MATNR and d.LIFNR = sq.LIFNR)
			where sq.BatchNo = @BatchNo and QUOTE > 0
			
			--新增配额大于0
			INSERT INTO SCM_Quota(Supplier, SupplierShortCode, SupplierNm, Item, RefItemCode, ItemDesc, StartDate, EndDate, [Weight], Rate, CycleQty, AccumulateQty, AdjQty, IsActive, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, [Version])
			select sq.LIFNR, s.ShortCode, p.Name, sq.MATNR, i.RefCode, i.Desc1, null, null, MAX(sq.QUOTE), null, 0, 0, 0, 1, @UserId, @UserName, @DateTimeNow, @UserId, @UserName, @DateTimeNow, 1
			from SAP_Quota as sq left join SCM_Quota as mq on mq.Item = sq.MATNR and mq.Supplier = sq.LIFNR
			inner join MD_Supplier as s on sq.LIFNR = s.Code
			inner join MD_Party as p on sq.LIFNR = p.Code
			inner join MD_Item as i on sq.MATNR = i.Code
			where sq.BatchNo = @BatchNo and QUOTE > 0 and mq.Id is null
			and not exists(select top 1 1 from #tempDuplicateQuota as d where d.MATNR = sq.MATNR and d.LIFNR = sq.LIFNR)
			group by sq.LIFNR, s.ShortCode, p.Name, sq.MATNR, i.RefCode, i.Desc1
			
			--删除配额不存在的
			update mq set IsActive = 0, LastModifyUser = @UserId, LastModifyUserNm = @UserName, LastModifyDate = @DateTimeNow, [Version] = [Version] + 1
			from SCM_Quota as mq left join SAP_Quota as sq on mq.Item = sq.MATNR and mq.Supplier = sq.LIFNR and sq.BatchNo = @BatchNo
			where sq.Id is null
			
			--删除配额为0的
			update SCM_Quota set IsActive = 0, Rate = 0, CycleQty = 0, LastModifyUser = @UserId, LastModifyUserNm = @UserName, LastModifyDate = @DateTimeNow, [Version] = [Version] + 1
			where [Weight] = 0
			
			select MSG from #tempDuplicateQuota
			drop table #tempDuplicateQuota
		END TRY 
		BEGIN CATCH 
			declare @ErrorMsg nvarchar(Max)
			set @ErrorMsg = Error_Message()
			RAISERROR(@ErrorMsg, 16, 1)
		END CATCH				 
	END 
END 

