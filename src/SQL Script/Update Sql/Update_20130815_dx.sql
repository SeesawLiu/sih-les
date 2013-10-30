alter table SCM_FlowStrategy add IsCreatePL bit
go

alter table SCM_SeqGroup add IsActive bit default(1) not null
go
