insert into SYS_EntityPreference(Id, Seq, Value, Desc1, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate)
values(11051, 200, 'localhost', 'SIServiceAddress', 2603, '用户 超级', '2012-01-01 00:00:00.000', 2603, '用户 超级', '2012-01-01 00:00:00.000')
,(11052, 201, '1122', 'SIServicePort', 2603, '用户 超级', '2012-01-01 00:00:00.000', 2603, '用户 超级', '2012-01-01 00:00:00.000')
go

alter table SAP_ProdOpReport add RecNo varchar(50)
go

alter table SAP_ProdOpReport add IsCancel bit
go
update SAP_ProdOpReport set IsCancel = 0
go
alter table SAP_ProdOpReport alter column IsCancel bit not null
go

alter table SAP_ProdOpReport add OrderNo varchar(50)
go
update r set OrderNo = m.OrderNo
from SAP_ProdOpReport as r inner join ORD_OrderMstr_4 as m on r.AUFNR = m.ExtOrderNo
go
alter table SAP_ProdOpReport alter column OrderNo varchar(50) not null
go

alter table SAP_ProdOpReport add OrderOpId int
go

alter table SAP_CancelProdSeqReport add RecNo varchar(50)
go

alter table SAP_CancelProdSeqReport add OrderNo varchar(50)
go

alter table SAP_CancelProdSeqReport add OrderOpId int
go

alter table ORD_OrderOp drop column IsBackflush
go
alter table ORD_OrderOp drop column IsReport
go
alter table ORD_OrderOp add ReportQty decimal(18, 8)
go
update ORD_OrderOp set ReportQty = 0
go
alter table ORD_OrderOp alter column ReportQty decimal(18, 8) not null
go