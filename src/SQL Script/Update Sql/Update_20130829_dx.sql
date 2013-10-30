alter table LOG_ProdOrderPauseResume add Seq bigint
go
alter table LOG_ProdOrderPauseResume add SubSeq int
go
alter table LOG_ProdOrderPauseResume add BeforeVanCode varchar(50)
go
alter table LOG_ProdOrderPauseResume add BeforeSeq bigint
go
alter table LOG_ProdOrderPauseResume add BeforeSubSeq int
go
alter table MD_Item add BESKZ varchar(50)
go
alter table MD_Item add SOBSL varchar(50)
go
alter table SAP_Item add BESKZ varchar(50)
go
alter table SAP_Item add SOBSL varchar(50)
go
alter table LE_FlowDetSnapShot add Container varchar(50)
go
alter table LE_FlowDetSnapShot add ContainerDesc varchar(50)
go
alter table LE_FlowDetSnapShot add UCDesc varchar(50)
go