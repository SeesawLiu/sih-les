alter table LOG_GenJITOrder add BatchNo int
go
alter table LOG_GenKBOrder add BatchNo int
go
alter table LOG_GenOrder4VanProdLine add BatchNo int
go
alter table LOG_GenSequenceOrder add BatchNo int
go
alter table LOG_OrderTrace add BatchNo int
go
alter table LOG_OrderTraceDet add BatchNo int
go
alter table LOG_RunLeanEngine add BatchNo int
go
alter table LOG_SnapshotFlowDet4LeanEngine add BatchNo int
go
alter table LOG_VanOrderBomTrace add BatchNo int
go
alter table LOG_VanOrderTrace add BatchNo int
go
alter table LE_FlowDetSnapShot add MinUC decimal(18, 8)
go