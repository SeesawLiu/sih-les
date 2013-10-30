update BIL_ActBill set BillTerm = 3 where BillTerm = 5
go
update BIL_PlanBill set BillTerm = 3 where BillTerm = 5
go
update ORD_IpDet_1 set BillTerm = 3 where BillTerm = 5
go
update ORD_RecDet_1 set BillTerm = 3 where BillTerm = 5
go
update ORD_OrderDet_1 set BillTerm = 3 where BillTerm = 5
go
insert into BIL_PlanBill([Type], Item, ItemDesc, Uom, UC, BillTerm, BillAddr, 
Party, PartyNm, UnitPrice, IsProvEst, IsIncludeTax,
PlanAmount, ActAmount, VoidAmount, PlanQty, ActQty, VoidQty,
UnitQty, EffDate, IsClose, 
CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, [Version])
select 0 as [Type], pb.Item, I.Desc1 as ItemDesc, i.Uom, i.UC, pb.BillTerm, pb.BillAddr, 
pa.Party, p.Name as PartyNm, 0 as UnitPrice, 0 as IsProvEst, 0 as IsIncludeTax,
0 as PlanAmount, 0 as ActAmount, 0 as VoidAmount, pb.PlanQty, pb.ActQty, pb.VoidQty,
1 as UnitQty, pb.EffDate, 0 as IsClose, 
1 as CreateUser, 'su' as CreateUserNm, GETDATE() as CreateDate, 1 as LastModifyUser, 'su' as LastModifyUserNm, GETDATE() as LastModifyDate, 1 as [Version]
from
(select pb.Item, pb.BillTerm, pb.BillAddr, SUM(pb.PlanQty) as PlanQty, SUM(pb.ActQty) as ActQty, SUM(pb.VoidQty) as VoidQty, MIN(EffDate) as EffDate
from BIL_PlanBill as pb
where pb.BillTerm <> 1 
group by pb.BillAddr, pb.Item, pb.BillTerm) as pb
inner join MD_Item as i on pb.Item = i.Code
inner join MD_PartyAddr as pa on pb.BillAddr = pa.[Address]
inner join MD_Party as p on pa.Party = p.Code
go
select tpb.Id as [NewId], pb.Id as OldId
into #tempPlanBillMap
from BIL_PlanBill as tpb 
inner join BIL_PlanBill as pb on tpb.Item = pb.Item and tpb.BillAddr = pb.BillAddr and tpb.BillTerm = pb.BillTerm
where tpb.RecNo is null and tpb.OrderNo is null and (pb.RecNo is not null or pb.OrderNo is not null) and pb.BillTerm <> 1
go
update det set PlanBill = map.NewId
from INV_LocationLotDet as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_0 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_1 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_2 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_3 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_4 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_5 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_6 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_7 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_8 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_9 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_10 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_11 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_12 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_13 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_14 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_15 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_16 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_17 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_18 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_19 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
go
update det set PlanBill = map.NewId
from INV_LocTrans as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_0 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_1 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_2 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_3 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_4 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_5 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_6 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_7 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_8 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_9 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_10 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_11 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_12 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_13 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_14 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_15 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_16 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_17 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_18 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_19 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
go
update det set PlanBill = map.NewId
from ORD_IpLocationDet as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_0 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_1 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_2 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_3 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_4 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_5 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_6 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_7 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_8 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
go
update det set PlanBill = map.NewId
from ORD_RecLocationDet as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_0 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_1 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_2 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_3 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_4 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_5 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_6 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_7 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_8 as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
go
update det set PlanBill = map.NewId
from ORD_MiscOrderLocationDet as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_OrderBackflushDet as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_PickListResult as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from PRD_ProdLineLocationDet as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
go
update det set PlanBill = map.NewId
from BIL_ActBill as det inner join #tempPlanBillMap as map on det.PlanBill = map.OldId
go
delete pb
from BIL_PlanBill as pb inner join #tempPlanBillMap as map on pb.Id = map.OldId
go
drop table #tempPlanBillMap
go
select a.Id as NewId, b.Id as OldId into #tempPlanBillMap2 from
(select MIN(Id) as Id, Item, BillAddr, RecNo
from BIL_PlanBill as pb
group by Item, BillAddr, RecNo having COUNT(1) > 1) as a 
inner join BIL_PlanBill as b on a.Item = b.Item and a.BillAddr = b.BillAddr and a.RecNo = b.RecNo and a.Id <> b.Id
go
update det set PlanBill = map.NewId
from INV_LocationLotDet as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_0 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_1 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_2 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_3 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_4 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_5 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_6 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_7 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_8 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_9 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_10 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_11 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_12 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_13 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_14 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_15 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_16 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_17 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_18 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocationLotDet_19 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
go
update det set PlanBill = map.NewId
from INV_LocTrans as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_0 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_1 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_2 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_3 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_4 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_5 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_6 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_7 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_8 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_9 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_10 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_11 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_12 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_13 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_14 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_15 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_16 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_17 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_18 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from INV_LocTrans_19 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
go
update det set PlanBill = map.NewId
from ORD_IpLocationDet as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_0 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_1 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_2 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_3 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_4 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_5 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_6 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_7 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_IpLocationDet_8 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
go
update det set PlanBill = map.NewId
from ORD_RecLocationDet as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_0 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_1 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_2 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_3 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_4 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_5 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_6 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_7 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_RecLocationDet_8 as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
go
update det set PlanBill = map.NewId
from ORD_MiscOrderLocationDet as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_OrderBackflushDet as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from ORD_PickListResult as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
update det set PlanBill = map.NewId
from PRD_ProdLineLocationDet as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
go
update det set PlanBill = map.NewId
from BIL_ActBill as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
go
update det set PlanBill = map.NewId
from BIL_BillTrans as det inner join #tempPlanBillMap2 as map on det.PlanBill = map.OldId
go
update BIL_PlanBill set PlanQty = a.PlanQty + b.PlanQty, ActQty = a.ActQty + b.ActQty, VoidQty = a.VoidQty + b.VoidQty
from BIL_PlanBill as a inner join
(select b.Item, b.BillAddr, b.RecNo, SUM(b.PlanQty) as PlanQty, SUM(b.ActQty) as ActQty, SUM(b.VoidQty) as VoidQty
from #tempPlanBillMap2 as map
inner join BIL_PlanBill as b on map.OldId = b.Id
group by b.Item, b.BillAddr, b.RecNo) as b on a.Item = b.Item and a.BillAddr = b.BillAddr and a.RecNo = b.RecNo
go
delete pb
from BIL_PlanBill as pb inner join #tempPlanBillMap2 as map on pb.Id = map.OldId
go
drop table #tempPlanBillMap2
go
select a.Id as [NewId], b.Id as OldId into #tempActBillMap from
(select MIN(Id) as Id, PlanBill from BIL_ActBill group by PlanBill) as a
inner join BIL_ActBill as b on a.PlanBill = b.PlanBill and a.Id <> b.Id
go
update det set ActBill = map.NewId
from INV_LocTrans as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_0 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_1 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_2 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_3 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_4 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_5 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_6 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_7 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_8 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_9 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_10 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_11 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_12 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_13 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_14 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_15 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_16 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_17 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_18 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from INV_LocTrans_19 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
go
update det set ActBill = map.NewId
from ORD_IpLocationDet as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_IpLocationDet_0 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_IpLocationDet_1 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_IpLocationDet_2 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_IpLocationDet_3 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_IpLocationDet_4 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_IpLocationDet_5 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_IpLocationDet_6 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_IpLocationDet_7 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_IpLocationDet_8 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
go
update det set ActBill = map.NewId
from ORD_RecLocationDet as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_RecLocationDet_0 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_RecLocationDet_1 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_RecLocationDet_2 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_RecLocationDet_3 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_RecLocationDet_4 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_RecLocationDet_5 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_RecLocationDet_6 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_RecLocationDet_7 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
update det set ActBill = map.NewId
from ORD_RecLocationDet_8 as det inner join #tempActBillMap as map on det.ActBill = map.OldId
go
update det set ActBill = map.NewId
from ORD_MiscOrderLocationDet as det inner join #tempActBillMap as map on det.ActBill = map.OldId
go
update det set ActBill = map.NewId
from BIL_BillTrans as det inner join #tempActBillMap as map on det.ActBill = map.OldId
go
update a set BillQty = a.BillQty + b.BillQty, BilledQty = a.BilledQty + b.BilledQty, VoidQty = a.VoidQty + b.VoidQty
from BIL_ActBill as a inner join
(select a.PlanBill, SUM(a.BillQty) as BillQty, SUM(a.BilledQty) as BilledQty, SUM(a.VoidQty) as VoidQty
from #tempActBillMap as map
inner join BIL_ActBill as a on map.OldId = a.Id
group by a.PlanBill) as b on a.PlanBill = b.PlanBill
go
delete pb
from BIL_ActBill as pb inner join #tempActBillMap as map on pb.Id = map.OldId
go
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BIL_PlanBill]') AND name = N'IX_PlanBill_1')
DROP INDEX [IX_PlanBill_1] ON [dbo].[BIL_PlanBill] WITH ( ONLINE = OFF )
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PlanBill_1] ON [dbo].[BIL_PlanBill] 
(
	[Item] ASC,
	[BillAddr] ASC,
	[RecNo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[BIL_ActBill]') AND name = N'IX_ActBill_1')
DROP INDEX [IX_ActBill_1] ON [dbo].[BIL_ActBill] WITH ( ONLINE = OFF )
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ActBill_1] ON [dbo].[BIL_ActBill] 
(
	[PlanBill] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
