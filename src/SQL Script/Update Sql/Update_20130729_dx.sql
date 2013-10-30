alter table SCM_SeqGroup alter column OpRef varchar(50) null
go
alter table SCM_FlowStrategy add MRPWeight int
go
alter table SCM_FlowStrategy add MRPTotal decimal(18, 8)
go
alter table SCM_FlowStrategy add MRPTotalAdj decimal(18, 8)
go
alter table SCM_FlowStrategy add MRPTag varchar(50)
go
alter table LE_FlowMstrSnapshot add MrpTotal decimal(18, 8) NULL
go
alter table LE_FlowMstrSnapshot add MrpTotalAdj decimal(18, 8) NULL
go
alter table LE_FlowMstrSnapshot add MrpWeight int NULL
go
