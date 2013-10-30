SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Busi_WithdrawQualityBarcode')
	DROP PROCEDURE USP_Busi_WithdrawQualityBarcode	
GO

CREATE PROCEDURE [dbo].[USP_Busi_WithdrawQualityBarcode]
(
	@Barcode varchar(50),
	@CreateUserId int,
	@CreateUserNm varchar(100)
) --WITH ENCRYPTION
AS
BEGIN
	set nocount on
	declare @DateTimeNow datetime = GetDate()
	declare @ErrorMsg nvarchar(Max)
	declare @trancount int = @@trancount
	
	begin try
		if @trancount = 0
		begin
            begin tran
        end
		
		declare @Id int
		declare @OrderItemTraceId int
	
		if ISNULL(@Barcode, '') = ''
		begin
			RAISERROR(N'条码不能为空。', 16, 1)
		end
		
		select top 1 @Id = Id, @OrderItemTraceId = OrderItemTraceId from ORD_OrderItemTraceResult where BarCode = @Barcode and IsWithdraw = 0
		
		if @Id is null
		begin
			set @ErrorMsg = N'条码' + @Barcode + N'没有和整车生产单关联。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		update ORD_OrderItemTraceResult set IsWithdraw = 1 where Id = @Id
		update ORD_OrderItemTrace set ScanQty = ScanQty - 1 where Id = @OrderItemTraceId
		
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
