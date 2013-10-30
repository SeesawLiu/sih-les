﻿CREATE PROCEDURE [dbo].[USP_Busi_PauseVanProdOrder] 
(
	@OrderNo varchar(50),
	@PauseSeq int,
	@CreateUserId int,
	@CreateUserNm varchar(50)
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
		
		declare @Status tinyint
		declare @PauseStatus tinyint
		declare @Version int
		declare @ProdLine varchar(50)
		declare @ProdLineType tinyint
		declare @IsPLPause bit
		declare @CurrentOp int
		
		
		select @Status = [Status], @PauseStatus = PauseStatus, @Version = [Version], @ProdLine = Flow, @ProdLineType = ProdLineType, @CurrentOp = CurtOp
		from ORD_OrderMstr_4 where OrderNo = @OrderNo
		
		select @IsPLPause = IsPause from SCM_FlowMstr where Code = @ProdLine
	
		if @ProdLineType <> 1 and @ProdLineType <> 2 and @ProdLineType <> 3 and @ProdLineType <> 4
		begin
			set @ErrorMsg = N'生产单' + @OrderNo + N'的不是整车生产单。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if @Status = 3 or @Status = 4
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经下线不能暂停。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if @PauseStatus <> 0
		begin
			set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经暂停。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if @IsPLPause = 1
		begin
			set @ErrorMsg = N'整车生产线' + @ProdLine + N'已经暂停，不能暂停整车生产单。'
			RAISERROR(@ErrorMsg, 16, 1)
		end
		
		if @Status = 1
		begin  --整车生产单未上线，直接暂停
			update ORD_OrderMstr_4 set PauseStatus = 2, PauseSeq = 0, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, [Version] = [Version] + 1
			where OrderNo = @OrderNo and [Version] = @Version
			
			if @@ROWCOUNT <> 1
			begin
				set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经更新，请重新暂停。'
				RAISERROR(@ErrorMsg, 16, 1)
			end
			
			--删除工序过点时间
			delete from ORD_OrderOpCPTime where OrderNo = @OrderNo
			
			--删除物料消耗时间
			delete from ORD_OrderBomCPTime where OrderNo = @OrderNo
		end
		else
		begin  --整车生产单已上线，到暂停工序暂停
			if @PauseSeq is null
			begin
				set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经上线，请输入暂停工序。'
				RAISERROR(@ErrorMsg, 16, 1)
			end
			
			if @PauseSeq < @CurrentOp
			begin  --暂停工序小于当前工序，报错
				set @ErrorMsg =  N'整车生产单' + @OrderNo + N'的暂停工序小于当前所在工序。'
				RAISERROR(@ErrorMsg, 16, 1)
			end
			else if @PauseSeq = @CurrentOp
			begin  --暂停工序等于当前工序，直接暂停
				update ORD_OrderMstr_4 set PauseStatus = 2, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, [Version] = [Version] + 1
				where OrderNo = @OrderNo and [Version] = @Version
				
				if @@ROWCOUNT <> 1
				begin
					set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经更新，请重新暂停。'
					RAISERROR(@ErrorMsg, 16, 1)
				end
				
				--删除工序过点时间
				delete from ORD_OrderOpCPTime where OrderNo = @OrderNo
				
				--删除物料消耗时间
				delete from ORD_OrderBomCPTime where OrderNo = @OrderNo
			end
			else  --暂停工序大于当前工序
			begin
				if not exists(select top 1 1 from ORD_OrderOp where OrderNo = @OrderNo and Op = @PauseSeq)
				begin
					set @ErrorMsg = N'输入的暂停工序不存在。'
					RAISERROR(@ErrorMsg, 16, 1)
				end
				
				update ORD_OrderMstr_4 set PauseStatus = 1, PauseSeq = @PauseSeq, LastModifyDate = @DateTimeNow, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, [Version] = [Version] + 1
				where OrderNo = @OrderNo and [Version] = @Version
				
				if @@ROWCOUNT <> 1
				begin
					set @ErrorMsg = N'整车生产单' + @OrderNo + N'已经更新，请重新暂停。'
					RAISERROR(@ErrorMsg, 16, 1)
				end
				
				--重新计算物料消耗时间
				exec USP_Busi_UpdateOrderBomConsumeTime @ProdLine, @CreateUserId, @CreateUserNm
			end
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
