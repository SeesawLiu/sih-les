
CREATE PROCEDURE [dbo].[USP_Busi_AdjWorkingCalendar]
	@WorkingCalendarId int,
	@Shift varchar(50),
	@Type tinyint,
	@CreateUserId int,
	@CreateUserNm varchar(100)
AS
BEGIN
	declare @DateTimeNow datetime = GetDate()
	declare @ErrorMsg nvarchar(Max)
	
	begin try
		begin tran
		
		declare @Region varchar(50)
		declare @OldShift varchar(50)
		declare @OldType tinyint
		
		--获取区域、原班次和原类型
		select @Region = Region, @OldShift = Shift, @OldType = [Type] from PRD_WorkingCalendar where Id = @WorkingCalendarId
		
		--更新班次和类型
		update PRD_WorkingCalendar set Shift = @Shift, [Type] = @Type, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, LastModifyDate = @DateTimeNow where Id = @WorkingCalendarId
		
		if @OldShift <> @Shift or @OldType <> @Type
		begin  --班次或类型发生改变
			---------------------------↓更新JIT路线下次开始时间-----------------------------	
			Declare @JITFlowCycleId int
			Declare @MaxJITFlowId int
			Declare @JITFlow varchar(50)
			Declare @JITPreOrderTime datetime
			Declare @JITPreWinTime datetime
			Declare @JITPreWinTime2 datetime
			Declare @JITNextOrderTime datetime
			Declare @JITNextWinTime datetime
			Declare @JITNextWinTime2 datetime
			Declare @JITLeadTime int
			Declare @JITRegion varchar(50)
			
			select identity(int, 1, 1) as RowId, Flow, PreOrderTime, PreWinTime, PreWinTime2, LeadTime into #tempJITFlow 
			from SCM_FlowStrategy WITH (UPDLOCK, SERIALIZABLE) 
			where Flow in 
			(
				select Flow from SCM_FlowStrategy as stra
				inner join SCM_FlowMstr as mstr on stra.Flow = mstr.Code
				where stra.Strategy = 2 and stra.NextOrderTime is not null and stra.NextWinTime is not null
				and mstr.PartyTo = @Region
			)
			
			if exists(select top 1 Flow from #tempJITFlow)
			begin
				select @JITFlowCycleId = MIN(RowId), @MaxJITFlowId = MAX(RowId) from #tempJITFlow
				while @MaxJITFlowId >= @JITFlowCycleId
				begin
					select @JITFlow = Flow, @JITPreOrderTime = PreOrderTime, @JITPreWinTime = PreWinTime, @JITPreWinTime2 = PreWinTime2, @JITLeadTime = LeadTime 
					from #tempJITFlow where RowId = @JITFlowCycleId
					
					--计算下趟JIT普通订单的发单时间
					exec USP_Busi_GetNextWindowTime @JITFlow, @JITPreOrderTime, @JITNextOrderTime output, @JITNextWinTime output
					
					--计算下趟交货时段
					declare @dCount int = 1
					while @dCount <= @JITLeadTime
					begin  --根据交货间隔循环找交货时段
						exec USP_Busi_GetNextWindowTime @JITFlow, @JITNextWinTime, @JITNextWinTime output, @JITNextWinTime2 output
						set @dCount = @dCount + 1
					end
					
					update SCM_FlowStrategy set NextOrderTime = @JITNextOrderTime, LastModifyUser = @CreateUserId, LastModifyUserNm = @CreateUserNm, LastModifyDate = @DateTimeNow where Flow = @JITFlow
					
					set @JITFlowCycleId = @JITFlowCycleId + 1
				end
			end
			
			drop table #tempJITFlow
			-----------------------------↑更新JIT路线下次开始时间-----------------------------
			
			
			
			
			
			-----------------------------↓更新序列路线下次开始时间-----------------------------
			Declare @ProdLineCycleId int
			Declare @MaxProdLineId int
			Declare @ProdLine varchar(50)
			
			select identity(int, 1, 1) as RowId, Code as ProdLine into #tempProdLine from SCM_FlowMstr where PartyFrom = @Region and [Type] = 4
			
			if exists(select top 1 ProdLine from #tempProdLine)
			begin
				select @ProdLineCycleId = MIN(RowId), @MaxProdLineId = MAX(RowId) from #tempProdLine
				while @MaxProdLineId >= @ProdLineCycleId
				begin
					select @ProdLine = ProdLine from #tempProdLine where RowId = @ProdLineCycleId
					exec USP_Busi_AdjSeqGroupNextWinTime @ProdLine, @CreateUserId, @CreateUserNm
					set @ProdLineCycleId = @ProdLineCycleId + 1
				end
			end
			
			drop table #tempProdLine
			-----------------------------↑更新序列路线下次开始时间-----------------------------
		end
		
		commit tran
	end try
	begin catch
		if xact_state()=-1
		begin
			rollback tran
		end
		set @ErrorMsg = Error_Message()
		RAISERROR(@ErrorMsg, 16, 1)
	end catch 
END
