alter table dbo.SCM_FlowBinding add LeadTime decimal(18,8) null
go

update ACC_Permission set Category = 'Distribution' where Code = 'Url_SequenceOrder_Detail' or Code = 'Url_SequenceOrder_View'
go