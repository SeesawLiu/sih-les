SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_StartVanProdOrder') 
     DROP PROCEDURE USP_Busi_StartVanProdOrder
GO

CREATE PROCEDURE [dbo].[USP_Busi_StartVanProdOrder] 
(
	@OrderNo varchar(50),
	@StartUserId int,
	@StartUserNm varchar(50)
) --WITH ENCRYPTION
AS 
BEGIN 
	set nocount on
	declare @DateTimeNow datetime = GetDate()
	declare @ErrorMsg nvarchar(MAX)
	declare @trancount int = @@trancount
	
	begin try
		if @trancount = 0
		begin
            begin tran
        end
       
		Declare @Status tinyint
		Declare @ProdLine varchar(50)
		Declare @Seq bigint
		Declare @SubSeq int
		Declare @PauseStatus tinyint
		Declare @PauseSeq int
		Declare @Version int
		
		select @Status = mstr.[Status], @ProdLine = mstr.Flow, @Seq = seq.Seq, @SubSeq = SubSeq, 
		@PauseStatus = PauseStatus, @PauseSeq = PauseSeq, @Version = mstr.[Version]
		from ORD_OrderMstr_4 as mstr WITH(NOLOCK) 
		inner join ORD_OrderSeq as seq WITH(NOLOCK) on mstr.OrderNo = seq.OrderNo 
		where mstr.OrderNo = @OrderNo
		
		if @Status <> 1
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经上线。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if @PauseStatus = 2
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经暂停不能上线。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if exists(select top 1 1 from ORD_OrderMstr_4 as mstr WITH(NOLOCK) 
					inner join ORD_OrderSeq as seq WITH(NOLOCK) on mstr.OrderNo = seq.OrderNo 
					where mstr.Flow = @ProdLine and Status = 1 and PauseStatus <> 2
					and (seq.Seq < @Seq or (seq.Seq = @Seq and seq.SubSeq < @SubSeq)))
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'不是生产线' + @ProdLine + N'第一张等待上线的生产单。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if exists(select top 1 1 from SCM_FlowMstr WITH(NOLOCK) where Code = @ProdLine and IsPause = 1)
		begin
			set @ErrorMsg = N'整车生产线' + @ProdLine + N'已经暂停，生产单' + @OrderNo + N'不能上线。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		--更新订单状态
		update ORD_OrderMstr_4 set Status = 2, StartDate = @DateTimeNow, StartUser = @StartUserId, StartUserNm = @StartUserNm
		where OrderNo = @OrderNo and [Version] = @Version
		
		if @@rowcount = 0
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经更新，请重试上线。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		--整车移动一个工位
		exec USP_Busi_MoveVanProdOrder @ProdLine, @StartUserId, @StartUserNm
		
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
