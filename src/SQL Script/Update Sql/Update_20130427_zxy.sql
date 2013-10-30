

alter table ORD_OrderOp add IsAutoReport bit null
go
update ORD_OrderOp set IsAutoReport =0
go
alter table ORD_OrderOp alter column IsAutoReport bit not null
go
