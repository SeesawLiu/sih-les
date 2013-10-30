alter table ORD_OrderSeq add OrderNo varchar(50)
go
alter table ORD_OrderSeq add SubSeq int not null
go
alter table ORD_OrderSeq alter column TraceCode varchar(50) null
go
alter table SCM_FlowDet add ManufactureParty varchar(50)
go
alter table SCM_FlowDet add MrpTag varchar(50)
go
alter table SCM_FlowMstr add TaktTime int
go
alter table SCM_FlowBinding add JointOp int
go
