alter table ORD_OrderItemTraceResult drop column BomId
go
alter table ORD_OrderItemTraceResult add IsWithdraw bit not null
go