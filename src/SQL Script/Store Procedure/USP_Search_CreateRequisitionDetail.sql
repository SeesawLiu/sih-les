
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
--exec USP_Search_CreateRequisitionDetail '','','','','',null,null,'','',50,1,0
create PROCEDURE [dbo].[USP_Search_CreateRequisitionDetail]
(
	@OrderNo varchar(100),
	@LocFrom varchar(100),
	@Item varchar(100),
	@ExtNo varchar(100),
	@ExtSeq varchar(100),
	@StartDate datetime, 
	@EndDate datetime, 
	@SortCloumn varchar(50)=null, 
	@SortRule varchar(50)=null, 
	@PageSize int, 
	@Page int ,
	@RowCount int output
) 
AS
BEGIN
	set nocount on
	DECLARE @WhereStatement nvarchar(4000) 
	DECLARE @SortDesc nvarchar(100) 
	DECLARE @Parameter nvarchar(4000) 
	DECLARE @PagePara nvarchar(4000)
	set @WhereStatement=' and 1=1 '
	
	
	create table #tempFlowDet
	(
		FlowDetRowId int identity(1, 1) Primary Key,
		Flow varchar(50),		
		Item varchar(50), 
		ItemDesc varchar(100),
		RefItemCode varchar(50),
		Uom varchar(5),
		UC decimal(18, 8),
		MinUC decimal(18, 8),
		UCDesc varchar(50),
		Container varchar(50),
		ContainerDesc varchar(50),
		MrpTotal decimal(18, 8),
		MrpTotalAdj decimal(18, 8),
		MrpWeight int,
		LocFrom varchar(50),
		LocTo varchar(50)
	)
	
	create table #tempMultiSupplierGroup
	(
		RowId int identity(1, 1),
		Item varchar(50)
	)
	
	create table #tempMultiSupplierItem
	(
		RowId int identity(1, 1),
		FlowDetRowId int,
		Flow varchar(50),
		Item varchar(50), 
		MSGRowId int,
		MrpTotal decimal(18, 8),
		MrpTotalAdj decimal(18, 8),
		MrpWeight int,
		DeliveryCount int,
		DeliveryBalance int,
	)
	
	create table #tempSortedMultiSupplierItem
	(
		GID int, 
		FlowDetRowId int,
		Flow varchar(50)
	)
	
	create table #tempSeqOrderTrace
	(
		TraceCode varchar(50), 
		ProdLine varchar(50), 
		SeqGroup varchar(50), 
		DeliveryCount varchar(50)
	)
	
	create table #tempDistributionDet ---需要拉动的明细表
	(
		DetId int, 
		OrderNo varchar(50),
		ExtNo varchar(50), 
		ExtSeq varchar(50), 
		Item varchar(50),
		ItemDesc varchar(100),
		RefItemCode varchar(50),
		Uom varchar(50),
		UC decimal(18,8),
		LocFrom varchar(50),
		LocTo varchar(50),
		OrderQty decimal(18,8),
		RecQty decimal(18,8),
		CreateDate datetime,
		Flow varchar(50),
		FlowDesc varchar(50),
		PartyFrom varchar(50),
		PartyTo varchar(50),
		Container varchar(50),
		ContainerDesc varchar(50),
		FlowDetId int,
	)
	
	create table #tempDuplicateDet ---有重复的
	(
		DetId int, 
		Item varchar(50),
		Flow varchar(50),
		FlowDetId int,
	)
	
	-----------------------------------
	
	-----------------------------------
	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @WhereStatement=@WhereStatement+' AND det.OrderNo = @OrderNo_1' 
	END 
	IF(ISNULL(@OrderNo,'')<>'') 
	BEGIN 
		SET @WhereStatement=@WhereStatement+' AND det.OrderNo = @OrderNo_1' 
	END 
	IF(ISNULL(@LocFrom,'')<>'') 
	BEGIN 
		SET @WhereStatement=@WhereStatement+' AND det.LocFrom = @LocFrom_1' 
	END 
	IF(ISNULL(@Item,'')<>'') 
	BEGIN 
		SET @WhereStatement=@WhereStatement+' AND det.Item = @Item_1' 
	END 
	IF(ISNULL(@ExtNo,'')<>'') 
	BEGIN 
		SET @WhereStatement=@WhereStatement+' AND det.ExtNo = @ExtNo_1' 
	END 
	IF(ISNULL(@ExtSeq,'')<>'') 
	BEGIN 
		SET @WhereStatement=@WhereStatement+' AND det.ExtSeq = @ExtSeq_1' 
	END 		 
	IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @WhereStatement=@WhereStatement+' AND det.CreateDate BETWEEN @StartDate_1 And @EndDate_1 ' 
	END	 
	ELSE IF(ISDATE(@StartDate)=1 AND ISDATE(@EndDate)=0) 
	BEGIN 
		SET @WhereStatement=@WhereStatement+' AND det.CreateDate >= @StartDate_1' 
	END 
	ELSE IF(ISDATE(@StartDate)=0 AND ISDATE(@EndDate)=1) 
	BEGIN 
		SET @WhereStatement=@WhereStatement+' AND det.CreateDate <= @EndDate_1' 
	END	
	IF ISNULL(@SortCloumn,'')='' 
	BEGIN 
		SET @SortDesc=' ORDER BY CreateDate DESC' 
	END 
	ELSE 
	BEGIN 
		IF(ISNULL(@SortRule,'')='') 
		BEGIN 
			SET @SortRule=' ACS' 
		END 
		SET @SortDesc=' ORDER BY '+@SortCloumn+' '+@SortRule		 
	END 
	
	set @WhereStatement='insert into #tempDistributionDet(DetId,OrderNo,ExtNo,ExtSeq,Item,ItemDesc,RefItemCode,Uom,UC,LocFrom,LocTo,OrderQty,RecQty,CreateDate,Flow,FlowDesc,PartyFrom,PartyTo,Container,ContainerDesc,FlowDetId)
	select det.Id,det.OrderNo,det.ExtNo,det.ExtSeq,det.Item,det.ItemDesc,det.RefItemCode,det.Uom,det.UC,det.LocFrom,det.LocTo,det.OrderQty,det.RecQty,det.CreateDate,fm.Code,fm.Desc1,fm.PartyFrom,fm.PartyTo,fd.Container,fd.ContainerDesc,fd.Id
from ORD_OrderDet_3 as det
inner join ORD_OrderMstr_3 as om on det.OrderNo=om.OrderNo
inner join SCM_FlowDet as fd on det.Item=fd.Item
inner join  SCM_FlowMstr as fm on fm.Code=fd.Flow and fm.LocTo=det.LocFrom
inner join SCM_FlowStrategy as fs on fs.Flow=fm.Code
left join LOG_DistributionRequisition as ld on ld.OrderDetId=det.Id
left join LOG_DistributionRequisition as ld2 on ld2.ExtNo=det.ExtNo and ld2.ExtSeq=det.ExtSeq
where fm.Type=1 and fs.Strategy in(3,4) and ld.Id is null and ld2.Id is null and om.Status in(2,1) and det.OrderQty>det.RecQty '+@WhereStatement

	SET @Parameter=N'@OrderNo_1 varchar(50),@LocFrom_1 varchar(50),@Item_1 varchar(50),@ExtNo_1 varchar(50),@ExtSeq_1 varchar(50),@StartDate_1 datetime,@EndDate_1 datetime'
	
	EXEC SP_EXECUTESQL @WhereStatement,@Parameter, 
		@OrderNo_1=@OrderNo,@LocFrom_1=@LocFrom,@Item_1=@Item ,@ExtNo_1=@ExtNo,@ExtSeq_1=@ExtSeq ,@StartDate_1=@StartDate , 
		@EndDate_1=@EndDate
		
	------------------
	insert into #tempDuplicateDet (DetId,Item,Flow,FlowDetId)
	select DetId,Item,Flow,FlowDetId from #tempDistributionDet as td where exists (select 1  from #tempDistributionDet as td2 where td.DetId=td2.DetId and td.Flow<>td2.Flow ) order by DetId

	------------------
	
	if exists ( select 1 from #tempDuplicateDet)
	begin
		-------------------↓获取所有排序路线明细-----------------------
		truncate table #tempFlowDet
		insert into #tempFlowDet(Flow, Item, ItemDesc, RefItemCode, Uom, UC, MinUC,
		UCDesc, Container, ContainerDesc, MrpTotal, MrpTotalAdj, MrpWeight, LocFrom, LocTo)
		select fdet.Flow as Flow, fdet.Item, i.Desc1 as ItemDesc, i.RefCode as RefItemCode, fdet.Uom, fdet.UC, fdet.MinUC,
		fdet.UCDesc, fdet.Container, fdet.ContainerDesc, fdet.MrpTotal, fdet.MrpTotalAdj, fdet.MrpWeight, mstr.LocFrom, mstr.LocTo
		from SCM_FlowDet as fdet
		inner join SCM_FlowMstr as mstr on fdet.Flow = mstr.Code
		inner join SCM_FlowStrategy as stra on stra.Flow = fdet.Flow
		inner join MD_Item as i on fdet.Item = i.Code
		where mstr.Type=1 and stra.Strategy in (3,4) 
		-------------------↑获取所有排序路线明细-----------------------
		
				
		-------------------↓获取引用路线明细-----------------------
		insert into #tempFlowDet(Flow, Item, ItemDesc, RefItemCode, Uom, UC, MinUC,
		UCDesc, Container, ContainerDesc, MrpTotal, MrpTotalAdj, MrpWeight, LocFrom, LocTo)
		select mstr.Code as Flow, rDet.Item, i.Desc1 as ItemDesc, i.RefCode as RefItemCode, rDet.Uom, rDet.UC, rDet.MinUC, 
		rDet.UCDesc, rDet.Container, rDet.ContainerDesc, rDet.MrpTotal, rDet.MrpTotalAdj, rDet.MrpWeight, mstr.LocFrom, mstr.LocTo
		from SCM_FlowMstr as mstr
		inner join SCM_FlowStrategy as stra on stra.Flow = mstr.Code
		inner join SCM_FlowDet as rDet on mstr.RefFlow = rDet.Flow
		inner join MD_Item as i on rDet.Item = i.Code
		where mstr.Type=1 and stra.Strategy in (3,4) 
		and not Exists(select top 1 1 from #tempFlowDet as tDet where tdet.Item = rdet.Item)
		-------------------↑获取引用路线明细-----------------------
		
		
		-------------------↓先用配额选择多供应商供货的路线-----------------------
		
			delete det
			from #tempFlowDet as det 
			inner join SCM_FlowMstr as mstr on det.Flow = mstr.Code
			inner join (select det.FlowDetRowId, det.Item, lq.Supplier , det.LocTo
						from #tempFlowDet as det 
						inner join SCM_FlowMstr as mstr on det.Flow = mstr.Code
						inner join LE_QuotaSnapShot as lq on det.Item = lq.Item and mstr.PartyFrom = lq.Supplier
						where mstr.[Type] = 1) as cDet --当前供货的采购路线明细
						on det.Item = cDet.Item and det.FlowDetRowId <> cDet.FlowDetRowId and mstr.PartyFrom <> cDet.Supplier --and det.LocTo = cDet.LocTo 
			--where mstr.[Type] = 1   --只过滤掉采购的路线
		-------------------↑先用配额选择多供应商供货的路线-----------------------
		
		
		-------------------↓按零件分组-----------------------
		truncate table #tempMultiSupplierGroup
		insert into #tempMultiSupplierGroup(Item)
		select Item from #tempFlowDet
		group by Item having COUNT(*) > 1 
		
		insert into #tempMultiSupplierItem(FlowDetRowId, Flow, Item, MSGRowId, MrpTotal, MrpTotalAdj, MrpWeight)
		select det.FlowDetRowId, det.Flow, det.Item, msg.RowId, det.MrpTotal, det.MrpTotalAdj, det.MrpWeight 
		from #tempFlowDet as det inner join #tempMultiSupplierGroup as msg on det.Item = msg.Item
		-------------------↑按零件分组-----------------------
		
		
		-------------------↓零件和目的库位相同的路线都没有设置供货比例，按零件包装设置循环量-----------------------
		update tmp set MrpWeight = (CASE WHEN det.UC = 0 THEN 1 ELSE det.UC END)
		from #tempMultiSupplierItem as tmp
		inner join #tempFlowDet as det on det.FlowDetRowId = tmp.FlowDetRowId
		where MSGRowId in (select MSGRowId from #tempMultiSupplierItem group by MSGRowId having SUM(MrpWeight) = 0)
		-------------------↑零件和目的库位相同的路线都没有设置供货比例，按零件包装设置循环量-----------------------
		
		
		-------------------↓零件和目的库位相同的路线没有设置供货比例，忽略这些路线明细-----------------------						
		delete #tempFlowDet where FlowDetRowId in (select FlowDetRowId from #tempMultiSupplierItem where MrpWeight = 0)
		delete #tempMultiSupplierItem where FlowDetRowId in (select FlowDetRowId from #tempMultiSupplierItem where MrpWeight = 0)
		------------------↑零件和目的库位相同的路线没有设置供货比例，忽略这些路线明细-----------------------
		
		
		-------------------↓计算供货次数和余量-----------------------
		update #tempMultiSupplierItem set DeliveryCount = (ISNULL(MrpTotal, 0) + ISNULL(MrpTotalAdj, 0)) / MrpWeight, DeliveryBalance = (ISNULL(MrpTotal, 0) + ISNULL(MrpTotalAdj, 0)) % MrpWeight
		-------------------↑计算供货次数和余量-----------------------
		
		
		-------------------↓根据供货次数、循环量选取一条路线明细供货-----------------------
		truncate table #tempSortedMultiSupplierItem
		insert into #tempSortedMultiSupplierItem(GID, FlowDetRowId, Flow)
		select ROW_NUMBER() over(partition by MSGRowId order by DeliveryCount asc, MrpWeight desc, DeliveryBalance desc) as GID, FlowDetRowId, Flow 
		from #tempMultiSupplierItem
		delete #tempFlowDet where FlowDetRowId in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
		delete #tempMultiSupplierItem where FlowDetRowId in (select FlowDetRowId from #tempSortedMultiSupplierItem where GID <> 1)
		-------------------↑根据供货次数、循环量选取一条路线明细供货-----------------------
	end		
	
	select @RowCount= isnull(count(*),0) from (
	select * from #tempDistributionDet where DetId not in (select DetId from #tempDuplicateDet)
	union all 
	select d.* from #tempDistributionDet  as d 
	inner join #tempDuplicateDet as dt on d.FlowDetId=dt.FlowDetId and d.DetId=dt.DetId
	inner join #tempFlowDet as fd on d.Item=fd.Item and d.Flow=fd.Flow) t
	
	print @RowCount
			declare @SelectPageStatement nvarchar(MAX) = ''
			
			set @SelectPageStatement = 'select DetId, OrderNo, ExtNo, ExtSeq, Item, ItemDesc, RefItemCode, Uom, UC, LocFrom, LocTo, OrderQty, RecQty, CreateDate, Flow, FlowDesc, PartyFrom, PartyTo, Container, ContainerDesc, FlowDetId from (select ROW_NUMBER() OVER('+@SortDesc+') as RowId,rr.* from (
	select * from #tempDistributionDet where DetId not in (select DetId from #tempDuplicateDet)
	union all 
	select d.* from #tempDistributionDet  as d 
	inner join #tempDuplicateDet as dt on d.FlowDetId=dt.FlowDetId and d.DetId=dt.DetId
	inner join #tempFlowDet as fd on d.Item=fd.Item and d.Flow=fd.Flow) as rr
	) as t where RowId between ' + CONVERT(varchar, (@Page-1)*@PageSize) + ' and ' + CONVERT(varchar, @Page*@PageSize)
	exec sp_executesql @SelectPageStatement	
	
	--exec USP_Search_CreateRequisitionDetail '','','','','',null,null,'','',50,1,0
	
	
	drop table #tempFlowDet
	drop table #tempMultiSupplierGroup
	drop table #tempMultiSupplierItem
	drop table #tempSortedMultiSupplierItem
	drop table #tempSeqOrderTrace
	drop table #tempDistributionDet
	drop table #tempDuplicateDet

END
GO


