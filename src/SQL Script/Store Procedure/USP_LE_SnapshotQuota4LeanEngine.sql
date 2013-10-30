SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_LE_SnapshotQuota4LeanEngine')
	DROP PROCEDURE USP_LE_SnapshotQuota4LeanEngine
GO

CREATE PROCEDURE [dbo].[USP_LE_SnapshotQuota4LeanEngine] 
(
	@BatchNo int
)
--WITH ENCRYPTION
AS
BEGIN
	SET NOCOUNT ON
	declare @Msg nvarchar(Max)
	
	set @Msg = N'快照配额开始'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	create table #tempMSI
	(
		Id int,
		Seq int,
		Supplier varchar(50),
		Item varchar(50),
		AccumulateQty decimal(18, 8),
		AdjQty decimal(18, 8),
		[Weight] decimal(18, 8),
		TotalWeight decimal(18, 8),
		Rate decimal(18, 8),
		CycleQty decimal(18, 8),
		DeliveryCount int,
		DeliveryBalance decimal(18, 8)
	)
	
	insert into #tempMSI(Id, Supplier, Item, AccumulateQty, AdjQty, [Weight], TotalWeight, Rate, Seq)
	select Id, Supplier, Item, ISNULL(AccumulateQty, 0), ISNULL(AdjQty, 0), [Weight], 0, 0,
	ROW_Number() over(Partition by Item order by [Weight]) as Seq
	from SCM_Quota where IsActive = 1
	--where Item in (select Item from SCM_Quota where [Weight] <> 0 and IsActive = 1 group by Item having COUNT(1) > 1)
	
	update t set TotalWeight = a.TotalWeight, Rate = [Weight] / case when a.TotalWeight = 0 then null else a.TotalWeight end
	from #tempMSI as t inner join
	(select Item, SUM([Weight]) as TotalWeight from #tempMSI group by Item) as a
	on t.Item = a.Item
	
	--insert into #tempDuplicateQuota(LIFNR, MATNR, MSG)
	--select Supplier, Item, N'供应商' + Supplier + N'零件' + Item + N'为多供应商供货零件，但是配额值为0。' 
	--from #tempMSI where TotalWeight = 0
	
	--delete from #tempMSI where TotalWeight = 0
	
	update t set Rate = 1 - a.Rate 
	from #tempMSI as t inner join 
	(select Item, SUM(Rate) as Rate from #tempMSI where Seq <> 1 group by Item) as a on t.Item = a.Item
	where t.Seq = 1
	
	update t set Rate = t.Rate, CycleQty = c.CycleQty * t.Rate
	from #tempMSI as t
	inner join SCM_QuotaCycleQty as c on t.Item = c.Item
	
	insert into LOG_SnapshotFlowDet4LeanEngine(Lvl, ErrorId, Item, Msg, BatchNo)
	select 1, 19, t.Item, N'零件' + t.Item + N'没有维护供货循环量。' , @BatchNo
	from (select Item from #tempMSI group by Item having COUNT(1) > 1) as t
	inner join SCM_FlowDet as det on t.Item = det.Item
	inner join SCM_FlowMstr as mstr on det.Flow = mstr.Code
	inner join SCM_FlowStrategy as stra on stra.Flow = mstr.Code
	left join  SCM_QuotaCycleQty as c on c.Item = t.Item
	where mstr.[Type] = 1 and mstr.IsActive = 1 --and det.IsActive = 1 
	and ((mstr.IsAutoCreate = 1 --and det.IsAutoCreate = 1
			) or (stra.Strategy = 4))
	and c.Id is null
	group by t.Item

	update #tempMSI set DeliveryCount = (ISNULL(AccumulateQty, 0) + ISNULL(AdjQty, 0)) / CycleQty,
	DeliveryBalance = (ISNULL(AccumulateQty, 0) + ISNULL(AdjQty, 0)) % CycleQty
	
	truncate table LE_QuotaSnapShot
	insert into LE_QuotaSnapShot(Item, Supplier) 
	select a.Item, a.Supplier from
	(select ROW_NUMBER() over(partition by Item order by DeliveryCount asc, DeliveryBalance desc, CycleQty desc, [Weight] desc) as GID, Item, Supplier
	from #tempMSI) as a where a.GID = 1
	
	drop table #tempMSI
	
	set @Msg = N'快照配额结束'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
END
GO

