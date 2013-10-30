
CREATE PROCEDURE [dbo].[USP_Busi_AdjRegionSpecialTime]
	@Region varchar(50),
	@CreateUserId int,
	@CreateUserNm varchar(100)
AS
BEGIN
	declare @DateTimeNow datetime = GetDate()
	declare @ErrorMsg nvarchar(Max)
	
	begin try
		begin tran AdjRegionSpecialTime
		save tran AdjRegionSpecialTime_Point
		
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
		
		commit tran AdjRegionSpecialTime
	end try
	begin catch
		rollback tran AdjRegionSpecialTime_Point
		commit tran AdjRegionSpecialTime
		set @ErrorMsg = Error_Message()
		RAISERROR(@ErrorMsg, 16, 1)
	end catch 
END
