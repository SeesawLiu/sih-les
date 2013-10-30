alter table ORD_OrderBackflushDet alter column OrderNo varchar(50) null
go
alter table ORD_OrderBackflushDet alter column OrderDetId int null
go
alter table ORD_OrderBackflushDet alter column OrderDetSeq int null
go
alter table ORD_OrderBomDet add JointOp int null
go
alter table ORD_OrderOpCPTime drop column OrgOp
go
alter table ORD_OrderBomCPTime drop column OrgOp
go
alter table ORD_OrderOpCPTime add VanOp int null
go
alter table ORD_OrderOpCPTime add AssOp int null
go
alter table ORD_OrderBomCPTime add VanOp int null
go
alter table ORD_OrderBomCPTime add AssOp int null
go
alter table LE_OrderBomCPTimeSnapshot drop column OrgOp
go
alter table LE_OrderBomCPTimeSnapshot add VanOp int null
go
alter table LE_OrderBomCPTimeSnapshot add AssOp int null
go