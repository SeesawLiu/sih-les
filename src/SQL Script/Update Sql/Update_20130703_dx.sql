alter table ORD_OrderItemTrace add TraceCode varchar(50)
go
alter table ORD_OrderItemTraceResult add TraceCode varchar(50)
go
update oit set TraceCode = mstr.TraceCode
from ORD_OrderItemTrace as oit inner join ORD_OrderMstr_4 as mstr on oit.OrderNo = mstr.OrderNo
go
update oitr set TraceCode = mstr.TraceCode
from ORD_OrderItemTraceResult as oitr inner join ORD_OrderMstr_4 as mstr on oitr.OrderNo = mstr.OrderNo
go
