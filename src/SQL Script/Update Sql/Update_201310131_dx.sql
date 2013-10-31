alter table SAP_TableIndex add Version int default(1)
go
update SAP_TableIndex set Version = 1
go
alter table SAP_InvTrans add Version int default(1)
go
update SAP_InvTrans set Version = 1
go
