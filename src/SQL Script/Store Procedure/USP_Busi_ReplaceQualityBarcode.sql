SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Busi_ReplaceQualityBarcode')
	DROP PROCEDURE USP_Busi_ReplaceQualityBarcode	
GO

CREATE PROCEDURE [dbo].[USP_Busi_ReplaceQualityBarcode]
(
	@WithdrawBarcode varchar(50),
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
        
        if @WithdrawBarcode = @Barcode
        begin
			RAISERROR(N'原条码和替换条码不能相同。', 16, 1)
        end
		
		declare @OrderItemTraceResultId int
		declare @OrderItemTraceId int
		declare @TraceCode varchar(50)
		
		select top 1 @OrderItemTraceResultId = Id, @OrderItemTraceId = OrderItemTraceId, @TraceCode = TraceCode 
		from ORD_OrderItemTraceResult where BarCode = @WithdrawBarcode and IsWithdraw = 0
		
		if @OrderItemTraceResultId is null
		begin
			RAISERROR(N'条码没有和整车生产单关联。', 16, 1)
		end
		
		exec USP_Busi_WithdrawQualityBarcode @WithdrawBarcode, @CreateUserId, @CreateUserNm
		
		if @OrderItemTraceId is not null
		begin
			exec USP_Busi_ScanQualityBarcode @TraceCode, @Barcode, null, @OrderItemTraceId, 1, 0, @CreateUserId, @CreateUserNm 
		end
		else
		begin
			exec USP_Busi_ScanQualityBarcode @TraceCode, @Barcode, null, @OrderItemTraceId, 1, 1, @CreateUserId, @CreateUserNm 
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
