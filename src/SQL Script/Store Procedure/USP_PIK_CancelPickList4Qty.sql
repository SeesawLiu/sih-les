SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_PIK_CancelPickList4Qty') 
     DROP PROCEDURE USP_PIK_CancelPickList4Qty
GO

CREATE PROCEDURE [dbo].[USP_PIK_CancelPickList4Qty] 
(
	@PLNo varchar(50),
	@CancelUserId int,
	@CancelUserName varchar(100)
) --WITH ENCRYPTION
AS 
BEGIN 
	set nocount on
	declare @DateTimeNow datetime = GetDate()
	declare @ErrorMsg nvarchar(MAX)
	
	begin try
		declare @trancount int
		set @trancount = @@trancount
		
		if @trancount = 0
		begin
			begin tran
		end
		
		declare @Status tinyint
		declare @Version int
		
		select @Status = [Status], @Version = [Version] from ORD_PickListMstr WITH(NOLOCK) where PLNo = @PLNo
		
		if @Status not in (0, 1)
		begin
			set @ErrorMsg = N'拣货单' + @PLNo + N'已关闭或已取消。' 
			RAISERROR(@ErrorMsg, 16, 1) 
		end
		
		update ord set PickQty = PickQty - pik.PickedQty, LastModifyDate = @DateTimeNow, LastModifyUser = @CancelUserId, LastModifyUserNm = @CancelUserName, [Version] = ord.[Version] + 1
		from ORD_OrderDet_2 as ord inner join (select OrderDetId, sum(Qty) as PickedQty from ORD_PickListDet WITH(NOLOCK)
												where PLNo = @PLNo and IsClose = 0
												group by OrderDetId) as pik on ord.Id = pik.OrderDetId
												
		update ORD_PickListDet set IsClose = 1, [Version] = [Version] + 1
		where PLNo = @PLNo
		
		update ORD_PickListMstr set Status = 3, CancelDate = @DateTimeNow, CancelUser = @CancelUserId, CancelUserNm = @CancelUserName, [Version] = [Version] + 1
		where PLNo = @PLNo and [Version] = @Version
		
		if (@@ROWCOUNT = 0)
		begin
			set @ErrorMsg = N'拣货单' + @PLNo + N'已更新，请重新取消。' 
			RAISERROR(@ErrorMsg, 16, 1) 
		end
		 
		if @trancount = 0 
		begin  
			commit
		end
	end try 
	begin catch
		if @trancount = 0
		begin
			rollback
		end 
        
		set @ErrorMsg = Error_Message() 
		RAISERROR(@ErrorMsg, 16, 1) 
	end catch 
END 
