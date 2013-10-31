alter table SAP_TableIndex add Version int default(1)
go
update SAP_TableIndex set Version = 1
go
alter table SAP_InvTrans add Version int default(1)
go
update SAP_InvTrans set Version = 1
go
alter table SAP_InvLoc add BWART varchar(3)
go
alter table MD_Location add MergeLocLotDet bit default(1)
go
update MD_Location set MergeLocLotDet = 1
go
update MD_Location set MergeLocLotDet = 0 where Code in ('2800','2309')
go