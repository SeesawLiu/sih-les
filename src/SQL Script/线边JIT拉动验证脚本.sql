------------------------------验证所有整车物料+消耗库位都有路线明细对应（排除没有采购入库地点的物料）-------------------------------
--应有路线的零件+库位
select tar.Item, tar.Location from
	--获取所有线边物料和消耗库位，排除自制件
	(select distinct Item, Location from LE_OrderBomCPTimeSnapshot where BESKZ <> 'E') as tar
	left join 
	--没有采购入库地点的零件
	(select distinct Item from LOG_SnapshotFlowDet4LeanEngine where ErrorId = 15) as i1
	on tar.Item = i1.Item
	where i1.Item is null
	
except

--所有路线明细（零件+入库库位）
(select det.Item, ISNULL(det.LocTo, mstr.LocTo) from SCM_FlowMstr as mstr 
inner join SCM_FlowDet as det on mstr.Code = det.Flow
where mstr.Type in (1, 2)  --采购、移库
union
--系统根据规则补完的路线明细（零件+入库库位）
select Item, LocTo from LE_FlowDetSnapShot where FlowDetId = 0)
 


----------------------------验证JIT拉动是否有遗漏----------------------------------
select * from LE_FlowDetSnapShot as det 
inner join LE_FlowMstrSnapShot as mstr on det.Flow = mstr.Flow
inner join LE_OrderBomCPTimeSnapshot as bom 
	on det.Item = bom.Item and det.LocTo = bom.Location 
		and bom.CPTime < mstr.ReqTimeTo
where mstr.Strategy = 3 and bom.IsCreateOrder = 0




----------------------------验证订单数量+ 工位消耗量是否满足Bom用量----------------------------------
select bom.Item, bom.OpRef, bom.BomQty, ISNULL(ord.OrderQty, 0) as OrderQty, ISNULL(ord.OpRefConsumeQty, 0) as OpRefConsumeQty
from
--物料工位用量
(select bom.Item, bom.OpRef, SUM(bom.OrderQty) as BomQty
from LE_FlowDetSnapShot as det 
inner join LE_FlowMstrSnapShot as mstr on det.Flow = mstr.Flow
inner join LE_OrderBomCPTimeSnapshot as bom 
	on det.Item = bom.Item and det.LocTo = bom.Location 
		and bom.CPTime < mstr.ReqTimeTo
group by bom.Item, bom.OpRef) as bom
left join
--订单数量 + 工位消耗量
(select det.Item, det.BinTo as OpRef, SUM(det.ReqQty) as ReqQty, SUM(det.OrderQty) as OrderQty, SUM(ISNULL(trace.OpRefConsumeQty, 0)) as OpRefConsumeQty
from VIEW_OrderDet as det 
inner join VIEW_OrderMstr as mstr on det.OrderNo = mstr.OrderNo
left join 
	(select Item, OpRef, SUM(OrgOpRefQty - OpRefQty) as OpRefConsumeQty
	from LOG_VanOrderTrace 
	group by Item, OpRef) as trace on det.Item = trace.Item and det.BinTo = trace.OpRef
where mstr.OrderStrategy = 3
group by det.Item, det.BinTo) as ord on bom.Item = ord.Item and bom.OpRef = ord.OpRef
--Bom用量 > 订单数量 + 工位消耗量
where bom.BomQty <> ISNULL(ord.OrderQty, 0) + ISNULL(ord.OpRefConsumeQty, 0)
--where bom.Item = '5801276239' and bom.OpRef = 'AF050'
