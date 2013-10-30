SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_UpdateOrderBomConsumeTime') 
     DROP PROCEDURE USP_Busi_UpdateOrderBomConsumeTime
GO

CREATE PROCEDURE [dbo].[USP_Busi_UpdateOrderBomConsumeTime] 
(
	@ProdLine varchar(50),
	@CreateUserId int,
	@CreateUserNm varchar(50)
) --WITH ENCRYPTION
AS 
BEGIN
	set nocount on
	declare @DateTimeNow datetime = GetDate()
	declare @ErrorMsg nvarchar(MAX)
	declare @trancount int
	
	Create table #tempRoutingDet
	(
		AssProdLine varchar(50),    --生产线，如果是分装工艺流程则存储分装生产线
		AssOp int,               --原工序号
		Op int,
		Capacity int,
		JointProdLine varchar(50), --合装生产线，只有分装生成线才有
		JointOp int,             --合装工序，只有分装生成线才有
		LoopCount int,
		LeadTime int          --合装提前期，秒
	)
	
	Create table #tempRoutingOpRef
	(
		AssProdLine varchar(50),    --生产线，如果是分装工艺流程则存储分装生产线
		AssOp int,               --原工序号
		Op int, 
		Capacity int,
		JointProdLine varchar(50), --合装生产线，只有分装生成线才有
		JointOp int,             --合装工序，只有分装生成线才有
		OpRef varchar(50),
		LoopCount int,
		LeadTime int          --合装提前期，秒
	)
	
	create table #tempOrderOpSeq
	(
		Id int identity(1, 1) Primary Key,
		OrderNo  varchar(50), 
		AssProdLine varchar(50), 
		Seq bigint, 
		SubSeq int, 
		VanOp int, 
		AssOp int, 
		Op int, 
		OpTakt int, 
		LoopCount int,
	)
	
	create table #tempOrderOpTakt
	(
		Id int identity(1, 1) Primary Key,
		OrderNo  varchar(50), 
		AssProdLine varchar(50), 
		Seq bigint, 
		SubSeq int, 
		VanOp int, 
		AssOp int, 
		Op int, 
		OpTaktTime int, 
		LoopCount int
	)
	
	create table #tempCPTime
	(
		Id int identity(1, 1) Primary Key,
		OrderNo  varchar(50), 
		AssProdLine varchar(50), 
		Seq bigint, 
		SubSeq int, 
		VanOp int, 
		AssOp int, 
		Op int, 
		OpTaktTime int,
		CPTime datetime
	)
	
	create table #tempCPTime2
	(
		Id int identity(1, 1) Primary Key,
		OrderNo  varchar(50), 
		AssProdLine varchar(50), 
		Seq bigint, 
		SubSeq int, 
		VanOp int, 
		AssOp int, 
		Op int, 
		OpTaktTime int,
		CPTime datetime
	)
	
	--工作日历临时表
	create table #tempWorkingCalendarView
	(
		RowId int Identity(1, 1),
		WorkingDate datetime,
		DateFrom datetime,
		DateTo datetime
	)

	declare @TaktTime int               --节拍时间（秒）
	declare @ProdLineRegion varchar(50) --生产线区域
	declare @Routing varchar(50)		--工艺流程
	declare @LastOnlineVanSeq bigint    --最后一台上线车序号
	declare @LastOnlineVanSubSeq int    --最后一台上线车序号
	declare @LastOnlineVanStartTime datetime --最后一台上线车时间
	declare @FirstReleaseVanSeq bigint  --第一台释放车序号
	declare @FirstReleaseVanSubSeq int    --第一台释放车序号
	declare @LoopCount int              --生产线/分装生产线深度 主线为1，分装线为2，分装线的分装线为3，依次类推
	declare @MaxLoopCount int           --最大深度
	declare @MinOp int					--最小工序
	declare @MinAssOp int				--最小工序
	declare @MinCPTime datetime         --最小过点时间  
	declare @MaxCPTime datetime         --最大过点时间
	declare @CPTimeLength int           --最小过点时间和最大过点时间的长度
	declare @WCDateFrom datetime        --工作日历开始时间
	declare @WCDateTo datetime          --工作日历结束时间
	declare @WCCycleRowId int           --工作日历循环标识
	declare @WCMaxRowId int             --工作日历最大标识
	declare @WCDeepCount int			--分装线循环次数
	
	--获取生产线
	select  @TaktTime = TaktTime, @ProdLineRegion = PartyFrom, @Routing = Routing from SCM_FlowMstr where Code = @ProdLine
	
	--获取工艺流程明细工位	
	set @LoopCount = 1	
	insert into #tempRoutingOpRef(AssProdLine, AssOp, Op, Capacity, JointProdLine, JointOp, OpRef, LoopCount, LeadTime) 
	--select distinct @ProdLine, Op, Dense_rank() over (order by Op), ISNULL(Capacity, 1) as Capacity, null as JointProdLine, null as JointOp, OpRef, @LoopCount, 0 as LeadTime 
	select distinct @ProdLine, Op, Dense_rank() over (order by Op), 1 as Capacity, null as JointProdLine, null as JointOp, OpRef, @LoopCount, 0 as LeadTime 
	from PRD_RoutingDet 
	where Routing = @Routing
	
	--循环查找分装线的工位，分装线的工序要从合装点降序赋值，如合装点是49工序，分装线的最后一站是48工序，最后第二站是47工序，依次类推
	--分装线是通过工艺流程中的合装工位
	while exists(select top 1 1 from #tempRoutingOpRef as tDet
				inner join SCM_FlowBinding as bind on tDet.AssProdLine = bind.MstrFlow
				inner join SCM_FlowMstr as subPL on bind.BindFlow = subPL.Code
				where tDet.LoopCount = @LoopCount)
	begin
		set @LoopCount = @LoopCount + 1
		
		--获取上层最小工序，如果分装生产线的合装点没有和上层生产线工序对应的，取最小的工序
		select @MinOp = MIN(Op), @MinAssOp = MIN(AssOp) from #tempRoutingOpRef where LoopCount = @LoopCount - 1
		
		insert into #tempRoutingOpRef(AssProdLine, AssOp, Op, Capacity, JointProdLine, JointOp, OpRef, LoopCount, LeadTime)
		--select opRef.SubProdLine, opRef.AssOp, ISNULL(prevOpRef.Op, @MinOp) - opRef.Op, ISNULL(opRef.Capacity, 1) as Capacity, opRef.JointProdLine, ISNULL(opRef.JointOp, @MinAssOp), opRef.OpRef, @LoopCount, opRef.LeadTime
		select opRef.SubProdLine, opRef.AssOp, ISNULL(prevOpRef.Op, @MinOp) - opRef.Op, 1 as Capacity, opRef.JointProdLine, ISNULL(prevOpRef.Op, @MinOp), opRef.OpRef, @LoopCount, opRef.LeadTime
		from
		(select subPL.Code as SubProdLine, subPL.Routing, rDet.Op as AssOp, Dense_rank() over (Partition by subPL.Code order by rDet.Op desc) as Op, rDet.Capacity,  
		bind.MstrFlow as JointProdLine, bind.JointOp, rDet.OpRef, ISNULL(bind.LeadTime, 0) * 60 * 60 * -1 as LeadTime
		from #tempRoutingOpRef as opRef
		inner join SCM_FlowBinding as bind on opRef.AssProdLine = bind.MstrFlow
		inner join SCM_FlowMstr as subPL on bind.BindFlow = subPL.Code
		inner join PRD_RoutingDet as rDet on rDet.Routing = subPL.Routing
		where opRef.LoopCount = @LoopCount - 1
		group by subPL.Code, subPL.Routing, bind.MstrFlow, bind.JointOp, rDet.Op, rDet.Capacity, rDet.OpRef, bind.LeadTime) as opRef
		left join (select distinct AssOp, Op from #tempRoutingOpRef where LoopCount = @LoopCount - 1) as prevOpRef on opRef.JointOp = prevOpRef.AssOp
	end
	
	--获取工艺流程明细		
	insert into #tempRoutingDet(AssProdLine, AssOp, Op, Capacity, JointProdLine, JointOp, LoopCount, LeadTime) 
	select distinct AssProdLine, AssOp, Op, Capacity, JointProdLine, JointOp, LoopCount, LeadTime from #tempRoutingOpRef
	--获取最大分装线循环次数
	select @MaxLoopCount = MAX(LoopCount) from #tempRoutingDet
	
	if exists(select top 1 1 from ORD_OrderMstr_4 with(NOLOCK) where Flow = @ProdLine and [Status] <> 1)
	begin  --有已上线车
		--最后台上线车序号和上线时间
		select top 1 @LastOnlineVanSeq = seq.Seq, @LastOnlineVanSubSeq = seq.SubSeq, @LastOnlineVanStartTime = mstr.StartDate 
		from ORD_OrderMstr_4 as mstr with(NOLOCK)
		inner join ORD_OrderSeq as seq with(NOLOCK) on mstr.OrderNo = seq.OrderNo 
		where mstr.Flow = @ProdLine
		and mstr.[Status] in (2, 3, 4) --已上线车辆
		and mstr.PauseStatus <> 2
		order by mstr.StartDate desc
	end
	else
	begin  --没有已上线车
		set @LastOnlineVanSeq = 0
		set @LastOnlineVanSubSeq = 0
		set @LastOnlineVanStartTime = @DateTimeNow
	end

	if exists(select top 1 1 from ORD_OrderMstr_4 with(NOLOCK) where Flow = @ProdLine and [Status] in (0, 1))
	begin  --有未上线车
		--第一台释放车序号
		select top 1 @FirstReleaseVanSeq = seq.Seq, @FirstReleaseVanSubSeq = seq.SubSeq 
		from ORD_OrderMstr_4 as mstr with(NOLOCK)
		inner join ORD_OrderSeq as seq with(NOLOCK) on mstr.OrderNo = seq.OrderNo 
		where mstr.Flow = @ProdLine 
		and mstr.[Status] in (0, 1) --创建和已释放
		and mstr.PauseStatus <> 2
		order by seq.Seq desc, seq.subSeq desc
	end
	else
	begin  --没有未上线车
		set @FirstReleaseVanSeq = 0
		set @FirstReleaseVanSubSeq = 0
	end
	
	--if @LastOnlineVanSeq <> 0 or @FirstReleaseVanSeq <> 0
	if exists(select top 1 1 from ORD_OrderMstr_4 with(NOLOCK) where Flow = @ProdLine and [Status] in (0, 1, 2))
	begin
		Declare @PTick DateTime = GetDate()
		print N'开始查询工单每道工序的顺序。'
		--查询工单每道工序的顺序
		set @LoopCount = 1
		insert into #tempOrderOpSeq(OrderNo, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTakt, LoopCount)
		select baseOp.OrderNo, baseOp.AssProdLine, baseOp.Seq, baseOp.SubSeq, baseOp.AssOp as VanOp, baseOp.AssOp, baseOp.Op, SUM(case when baseOp.Op > op.Op then op.Capacity else 0 end) as OpTakt, @LoopCount
		from
		(select mstr.OrderNo, tRouting.AssProdLine, seq.Seq, seq.SubSeq, tRouting.AssOp, tRouting.Op
		from ORD_OrderSeq as seq with(NOLOCK)
		inner join ORD_OrderMstr_4 as mstr with(NOLOCK) on seq.OrderNo = mstr.OrderNo
		inner join #tempRoutingDet as tRouting on mstr.Flow = tRouting.AssProdLine  --整车生产线的工艺流程
		where mstr.Flow = @ProdLine and mstr.[Status] in (0, 1, 2) 
		and (mstr.PauseStatus = 0 or (mstr.PauseStatus = 1 and tRouting.AssOp <= mstr.PauseSeq))  --取未暂停或计划暂停前的工序
		and tRouting.LoopCount = @LoopCount) as baseOp
		inner join #tempRoutingDet as op on baseOp.AssProdLine = op.AssProdLine
		where op.LoopCount = @LoopCount
		group by baseOp.OrderNo, baseOp.AssProdLine, baseOp.Seq, baseOp.SubSeq, baseOp.AssOp, baseOp.Op
	
		--循环根据合装生产线+合装工序并把分装线的工序插入到临时表中
		--分装线的节拍时间要用合装点的节拍时间减去分装线容量
		--如最后一个工位容量为8,最后第二个工位的容量为5，分装线的节拍为53。
		--那最后一个工位的节拍为45,最后第二个工位的节拍为40
		set @LoopCount = @LoopCount + 1
		while @LoopCount <= @MaxLoopCount
		begin
			insert into #tempOrderOpSeq(OrderNo, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTakt, LoopCount)
			select opSeq.OrderNo, det.AssProdLine, opSeq.Seq, opSeq.SubSeq, opSeq.VanOp, det.AssOp, det.Op, opSeq.OpTakt - det.OpTakt, @LoopCount
			from #tempOrderOpSeq as opSeq
			inner join(select det1.AssProdLine, det1.AssOp, det1.Op, det1.JointProdLine, det1.JointOp, SUM(det2.Capacity) as OpTakt 
						from #tempRoutingDet as det1 
						inner join #tempRoutingDet as det2 on det1.AssProdLine = det2.AssProdLine
						where det1.LoopCount = @LoopCount and det2.LoopCount = @LoopCount and det1.Op <= det2.Op
						group by det1.AssProdLine, det1.AssOp, det1.Op, det1.JointProdLine, det1.JointOp) as det on opSeq.AssProdLine = det.JointProdLine and opSeq.Op = det.JointOp
			where opSeq.LoopCount = @LoopCount - 1
			
			set @LoopCount = @LoopCount + 1
		end
		print N'结束查询工单每道工序的顺序，用时' + Convert(varchar(50), DateDiff(MS, @PTick, GetDate()))
		
		set @PTick = GetDate()
		print N'开始查询工单每道工序的节拍，开始时间' + convert(varchar(20), @PTick)
		--未上线车的过点时间
		insert into #tempOrderOpTakt(OrderNo, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, LoopCount)
		select toos.OrderNo, toos.AssProdLine, toos.Seq, toos.SubSeq, toos.VanOp, toos.AssOp, toos.Op, (toos.OpTakt + COUNT(*)) * @TaktTime, toos.LoopCount
		from #tempOrderOpSeq as toos
		inner join ORD_OrderSeq as seq with(NOLOCK) on toos.Seq > seq.Seq or (toos.Seq = seq.Seq and toos.SubSeq >= seq.SubSeq)
		left join ORD_OrderMstr_4 as mstr with(NOLOCK) on seq.OrderNo = mstr.OrderNo and mstr.Status in (0, 1, 2)
		where seq.ProdLine = @ProdLine and (seq.Seq > @LastOnlineVanSeq or (seq.Seq = @LastOnlineVanSeq and seq.SubSeq > @LastOnlineVanSubSeq))
		group by toos.OrderNo, toos.AssProdLine, toos.Seq, toos.SubSeq, toos.VanOp, toos.AssOp, toos.Op, toos.OpTakt, toos.LoopCount

		--已上线车的过点时间
		if @LastOnlineVanSeq > 0
		begin
			insert into #tempOrderOpTakt(OrderNo, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, LoopCount)
			select toos.OrderNo, toos.AssProdLine, toos.Seq, toos.SubSeq, toos.VanOp, toos.AssOp, toos.Op, (toos.OpTakt - COUNT(*) + 1) * @TaktTime, toos.LoopCount
			from #tempOrderOpSeq as toos
			inner join ORD_OrderSeq as seq with(NOLOCK) on toos.Seq < seq.Seq or (toos.Seq = seq.Seq and toos.SubSeq <= seq.SubSeq)
			left join ORD_OrderMstr_4 as mstr with(NOLOCK) on seq.OrderNo = mstr.OrderNo and mstr.Status in (0, 1, 2)
			where seq.ProdLine = @ProdLine and (seq.Seq < @LastOnlineVanSeq or (seq.Seq = @LastOnlineVanSeq and seq.SubSeq <= @LastOnlineVanSubSeq))
			group by toos.OrderNo, toos.AssProdLine, toos.Seq, toos.SubSeq, toos.VanOp, toos.AssOp, toos.Op, toos.OpTakt, toos.LoopCount
		end
		print N'结束查询工单每道工序的节拍，用时' + Convert(varchar(50), DateDiff(MS, @PTick, GetDate()))

		set @PTick = GetDate()
		print N'开始根据偏移量计算所有生产单的过点时间（节拍数大于等于0），开始时间' + convert(varchar(20), @PTick)
		--根据偏移量计算所有生产单的过点时间（没有按工作时间）
		truncate table #tempCPTime
		insert into #tempCPTime(OrderNo, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, CPTime)
		select toot.OrderNo, toot.AssProdLine, toot.Seq, toot.SubSeq, toot.VanOp, toot.AssOp, toot.Op, toot.OpTaktTime, DATEADD(SECOND, rd.LeadTime, DATEADD(SECOND, toot.OpTaktTime, @LastOnlineVanStartTime))
		from #tempOrderOpTakt as toot
		inner join #tempRoutingDet as rd on toot.AssProdLine = rd.AssProdLine and toot.AssOp = rd.AssOp
		where toot.OpTaktTime >= 0
		print N'结束根据偏移量计算所有生产单的过点时间（节拍数大于等于0），用时' + Convert(varchar(50), DateDiff(MS, @PTick, GetDate()))
	
		set @PTick = GetDate()
		print N'开始循环添加休息时间（节拍数大于等于0），开始时间' + convert(varchar(20), @PTick)
		--循环添加休息时间
		select @MinCPTime = MIN(CPTime), @MaxCPTime = MAX(CPTime) from #tempCPTime
		set @WCDeepCount = 0
		while 1=1
		begin
			if @MinCPTime = @MAXCPTime
			begin  --跳出循环
				break
			end
			
			truncate table #tempWorkingCalendarView
			insert into #tempWorkingCalendarView(WorkingDate, DateFrom, DateTo) exec USP_Busi_GetWorkingCalendarView @ProdLine, @ProdLineRegion, @MinCPTime, @MaxCPTime
			
			set @WCDateFrom = null
			set @WCDateTo = null
			if exists(select top 1 1 from #tempWorkingCalendarView)
			begin  --找到工作时间
				select @WCCycleRowId = MIN(RowId), @WCMaxRowId = MAX(RowId) from #tempWorkingCalendarView
				
				while @WCCycleRowId <= @WCMaxRowId
				begin
					if @WCDateFrom is null
					begin --第一条记录工作日历记录，检查开始日期是否和@MinCPTime之间有休息时间
						select @WCDateFrom = DateFrom from #tempWorkingCalendarView where RowId = @WCCycleRowId
						
						if @WCDateFrom > @MinCPTime
						begin  --所有记录加上开始日期和@MinCPTime之间的休息时间
							update #tempCPTime set CPTime = DateAdd(SECOND, DateDiff(SECOND, @MinCPTime, @WCDateFrom), CPTime) where CPTime >= @MinCPTime
						end
					end
					
					select @WCDateTo = DateTo from #tempWorkingCalendarView where RowId = @WCCycleRowId
					
					if @WCCycleRowId + 1 <= @WCMaxRowId
					begin  --还有后续工作日历
						select @WCDateFrom = DateFrom from #tempWorkingCalendarView where RowId = @WCCycleRowId + 1
						
						if @WCDateFrom > @WCDateTo
						begin  --结束日期和下一条的开始日期之间有休息时间
							update #tempCPTime set CPTime = DateAdd(SECOND, DateDiff(SECOND, @WCDateTo, @WCDateFrom), CPTime) where CPTime > @WCDateTo
						end
					end
					else
					begin  --最后一条工作日历
						if @WCDateTo < @MaxCPTime
						begin  --结束日期是否和@MaxCPTime之间有休息时间
							update #tempCPTime set CPTime = DateAdd(SECOND, DateDiff(SECOND, @WCDateTo, @MaxCPTime), CPTime) where CPTime > @WCDateTo
						end
						
						--重新取@MaxCPTime，继续循环
						set @MinCPTime = @MaxCPTime
						select @MaxCPTime = MAX(CPTime) from #tempCPTime
					end
					
					set @WCCycleRowId = @WCCycleRowId + 1		
				end
			end
			else
			begin  --没有找到工作时间
				--记录初始的最大和最小过点差异量
				set @CPTimeLength = DATEDIFF(SECOND, @MinCPTime, @MaxCPTime)
				declare @DatePlus1 datetime = @MaxCPTime
				
				if @CPTimeLength < 1000 * 60 * 60 * 24
				begin
					while 1 = 1
					begin  --为了避免@CPTimeLength太小，循环999次都没有找到工作时间
						--通过@MaxCPTime + 1天来找之后的工作结束时间
						set @DatePlus1 = DATEADD(DAY, 1, @DatePlus1)
						
						truncate table #tempWorkingCalendarView
						insert into #tempWorkingCalendarView(WorkingDate, DateFrom, DateTo) exec USP_Busi_GetWorkingCalendarView @ProdLine, @ProdLineRegion, @MaxCPTime, @DatePlus1
						
						if exists(select top 1 1 from #tempWorkingCalendarView)
						begin
							select top 1 @MaxCPTime = DateFrom from #tempWorkingCalendarView order by DateFrom asc
							break
						end
						
						set @WCDeepCount = @WCDeepCount + 1
						if @WCDeepCount > 999
						begin
							set @ErrorMsg = N'计算过点时间超过最大循环量，可能生产线' + @ProdLine + '的工作日历没有维护完整。'
							RAISERROR(@ErrorMsg, 16, 1)
							return
						end
					end
				end
				
				set @WCDeepCount = @WCDeepCount + 1
				if @WCDeepCount > 999
				begin
					set @ErrorMsg = N'计算过点时间超过最大循环量，可能生产线' + @ProdLine + '的工作日历没有维护完整。'
					RAISERROR(@ErrorMsg, 16, 1)
					return
				end
				
				--要先把所有过点时间小于最小时间的记录加上最小时间和最大时间的时间差，再增加最小时间和最大时间的范围
				update #tempCPTime set CPTime = DATEADD(SECOND, DATEDIFF(SECOND, @MinCPTime, @MaxCPTime), CPTime) where CPTime >= @MinCPTime
				set @MinCPTime = @MaxCPTime
				set @MaxCPTime = DATEADD(SECOND, @CPTimeLength, @MaxCPTime)
			end
		end
		print N'结束循环添加休息时间（节拍数大于等于0），用时' + Convert(varchar(50), DateDiff(MS, @PTick, GetDate()))

		set @PTick = GetDate()
		print N'开始根据偏移量计算所有生产单的过点时间（节拍数小于0），开始时间' + convert(varchar(20), @PTick)
		--根据偏移量计算所有生产单的过点时间（没有按工作时间）
		truncate table #tempCPTime2
		insert into #tempCPTime2(OrderNo, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, CPTime) 
		select toot.OrderNo, toot.AssProdLine, toot.Seq, toot.SubSeq, toot.VanOp, toot.AssOp, toot.Op, toot.OpTaktTime, DATEADD(SECOND, rd.LeadTime, DATEADD(SECOND, toot.OpTaktTime, @LastOnlineVanStartTime))
		from #tempOrderOpTakt as toot
		inner join #tempRoutingDet as rd on toot.AssProdLine = rd.AssProdLine and toot.AssOp = rd.AssOp
		where toot.OpTaktTime < 0
		print N'结束根据偏移量计算所有生产单的过点时间（节拍数小于0），用时' + Convert(varchar(50), DateDiff(MS, @PTick, GetDate()))
	
		set @PTick = GetDate()
		print N'开始循环添加休息时间（节拍数小于0），开始时间' + convert(varchar(20), @PTick)
		if exists(select top 1 1 from #tempCPTime2)
		begin
			--循环添加休息时间
			select @MinCPTime = MIN(CPTime), @MaxCPTime = MAX(CPTime) from #tempCPTime2
			set @WCDeepCount = 0
			while 1=1
			begin
				if @MinCPTime = @MAXCPTime
				begin  --跳出循环
					break
				end
				
				truncate table #tempWorkingCalendarView
				insert into #tempWorkingCalendarView(WorkingDate, DateFrom, DateTo) exec USP_Busi_GetWorkingCalendarView @ProdLine, @ProdLineRegion, @MinCPTime, @MaxCPTime
				
				set @WCDateFrom = null
				set @WCDateTo = null
				if exists(select top 1 1 from #tempWorkingCalendarView)
				begin  --找到工作时间
					select @WCCycleRowId = MIN(RowId), @WCMaxRowId = MAX(RowId) from #tempWorkingCalendarView
					
					while @WCCycleRowId <= @WCMaxRowId
					begin
						if @WCDateTo is null
						begin --最后一条记录工作日历记录，检查结束日期是否和@MAXCPTime之间有休息时间
							select @WCDateTo = DateTo from #tempWorkingCalendarView where RowId = @WCMaxRowId
							
							if @WCDateTo < @MAXCPTime
							begin  --所有记录减去结束日期和@MAXCPTime之间的休息时间
								update #tempCPTime2 set CPTime = DateAdd(SECOND, DateDiff(SECOND, @MAXCPTime, @WCDateTo), CPTime) where CPTime <= @MAXCPTime
							end
						end
						
						select @WCDateFrom = DateFrom from #tempWorkingCalendarView where RowId = @WCMaxRowId
						
						if @WCCycleRowId <= @WCMaxRowId - 1
						begin  --还有前面的工作日历
							select @WCDateTo = DateTo from #tempWorkingCalendarView where RowId = @WCMaxRowId - 1
							
							if @WCDateFrom > @WCDateTo
							begin  --结束日期和下一条的开始日期之间有休息时间
								update #tempCPTime2 set CPTime = DateAdd(SECOND, DateDiff(SECOND, @WCDateFrom, @WCDateTo), CPTime) where CPTime < @WCDateFrom
							end
						end
						else
						begin  --第一条工作日历
							if @WCDateFrom > @MinCPTime
							begin  --开始日期是否和@MinCPTime之间有休息时间
								update #tempCPTime2 set CPTime = DateAdd(SECOND, DateDiff(SECOND, @WCDateFrom, @MinCPTime), CPTime) where CPTime < @WCDateFrom
							end
							
							--重新取@MaxCPTime，继续循环
							set @MaxCPTime = @MinCPTime
							select @MinCPTime = MIN(CPTime) from #tempCPTime2
						end
						
						set @WCMaxRowId = @WCMaxRowId - 1
					end
				end
				else
				begin  --没有找到工作时间
					--记录初始的最大和最小过点差异量
					set @CPTimeLength = DATEDIFF(SECOND, @MaxCPTime, @MinCPTime)
					declare @DateMinus1 datetime = @MinCPTime
				
					if @CPTimeLength > -1000 * 60 * 60 * 24
					begin
						while 1 = 1
						begin  --为了避免@CPTimeLength太小，循环999次都没有找到工作时间
							--通过@MinCPTime - 1天来找之前的工作结束时间
							set @DateMinus1 = DATEADD(DAY, -1, @DateMinus1)
							
							truncate table #tempWorkingCalendarView
							insert into #tempWorkingCalendarView(WorkingDate, DateFrom, DateTo) exec USP_Busi_GetWorkingCalendarView @ProdLine, @ProdLineRegion, @DateMinus1, @MinCPTime
							
							if exists(select top 1 1 from #tempWorkingCalendarView)
							begin
								select top 1 @MinCPTime = DateTo from #tempWorkingCalendarView order by DateTo desc
								break
							end
							
							set @WCDeepCount = @WCDeepCount + 1
							if @WCDeepCount > 999
							begin
								set @ErrorMsg = N'计算过点时间超过最大循环量，可能生产线' + @ProdLine + '的工作日历没有维护完整。'
								RAISERROR(@ErrorMsg, 16, 1)
								return
							end
						end
					end
					
					set @WCDeepCount = @WCDeepCount + 1
					if @WCDeepCount > 999
					begin
						set @ErrorMsg = N'计算过点时间超过最大循环量，可能生产线' + @ProdLine + '的工作日历没有维护完整。'
						RAISERROR(@ErrorMsg, 16, 1)
						return
					end
					
					--要先把所有过点时间小于最大时间的记录减去上最小时间和最大时间的时间差，再增加最小时间和最大时间的范围
					update #tempCPTime2 set CPTime = DATEADD(SECOND, DATEDIFF(SECOND, @MaxCPTime, @MinCPTime), CPTime) where CPTime <= @MaxCPTime
					set @MaxCPTime = @MinCPTime
					set @MinCPTime = DATEADD(SECOND, @CPTimeLength, @MinCPTime)
				end
			end
		end
		print N'结束循环添加休息时间（节拍数小于0），用时' + Convert(varchar(50), DateDiff(MS, @PTick, GetDate()))

		set @PTick = GetDate()
		print N'开始生成零件过点时间，开始时间' + convert(varchar(20), @PTick)
		set @trancount = @@trancount
		begin try
			if @trancount = 0
			begin
				begin tran
			end
			
			delete from ORD_OrderOpCPTime where VanProdLine = @ProdLine
			--delete from ORD_OrderBomCPTime where VanProdLine = @ProdLine
			
			insert into ORD_OrderOpCPTime(OrderNo, VanProdLine, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, CPTime) 
			select OrderNo, @ProdLine, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, CPTime from #tempCPTime
			
			insert into ORD_OrderOpCPTime(OrderNo, VanProdLine, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, CPTime) 
			select OrderNo, @ProdLine, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, CPTime from #tempCPTime2
			
			--insert into ORD_OrderBomCPTime(OrderNo, VanProdLine, AssProdLine, Seq, SubSeq, VanOp, AssOp, Op, OpTaktTime, CPTime,
			--Item, Uom, OpRef, OrderQty, Location, IsCreateOrder, BomId, DISPO, ManufactureParty, WorkCenter)
			--select tcpt.OrderNo, @ProdLine, tcpt.AssProdLine, tcpt.Seq, tcpt.SubSeq, tcpt.VanOp, tcpt.AssOp, tcpt.Op, tcpt.OpTaktTime, tcpt.CPTime,
			--bom.Item, bom.Uom, bom.OpRef, bom.OrderQty, bom.Location, bom.IsCreateOrder, bom.Id, bom.DISPO, bom.ManufactureParty, bom.WorkCenter
			--from ORD_OrderOpCPTime as tcpt
			--inner join ORD_OrderBomDet as bom on tcpt.OrderNo = bom.OrderNo and tcpt.AssProdLine = bom.AssProdLine and tcpt.AssOp = bom.Op
			--where tcpt.VanProdLine = @ProdLine
			
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
		print N'结束生成零件过点时间，用时' + Convert(varchar(50), DateDiff(MS, @PTick, GetDate()))	
	end
	else
	begin
		delete from ORD_OrderOpCPTime where VanProdLine = @ProdLine
	end
END