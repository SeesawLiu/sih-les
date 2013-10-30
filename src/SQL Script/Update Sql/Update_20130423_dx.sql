alter table scm_flowmstr drop column WorkCenter
go
alter table MD_SpecialTime add ProdLine varchar(50)
go
alter table MD_SpecialTime alter column Region varchar(50) null
go
delete from BAT_JobParam where JobId in (select Id from BAT_Job where Name in ('SubmitCabOrderJob', 'GetVanOrderJob', 'DeferredFeedJob'))
go
delete from BAT_TriggerParam where TriggerId in (select Id from BAT_Trigger where JobId in (select Id from BAT_Job where Name in ('SubmitCabOrderJob', 'GetVanOrderJob', 'DeferredFeedJob')))
go
delete from BAT_Trigger where JobId in (select Id from BAT_Job where Name in ('SubmitCabOrderJob', 'GetVanOrderJob', 'DeferredFeedJob'))
go
delete from BAT_Job where Name in ('SubmitCabOrderJob', 'GetVanOrderJob', 'DeferredFeedJob')
go
set Identity_Insert BAT_Job on
insert into BAT_Job(Id, Name, Desc1, ServiceType) values(100, 'AutoCreateVanOrderJob', 'AutoCreateVanOrderJob', 'AutoCreateVanOrderJob')
set Identity_Insert BAT_Job off
go
insert into BAT_JobParam(JobId, ParamKey, ParamValue) values(100, 'UserCode', 'su')
go
set Identity_Insert BAT_Trigger on
insert into BAT_Trigger(Id, JobId, Name, Desc1, PrevFireTime, NextFireTime, RepeatCount, Interval, IntervalType, TimesTriggered, Status) values(100, 100, 'AutoCreateVanOrder', '整装A线获取SAP整车生产单', GETDATE(), GETDATE(), 0, 10, 2, 0, 1)
insert into BAT_Trigger(Id, JobId, Name, Desc1, PrevFireTime, NextFireTime, RepeatCount, Interval, IntervalType, TimesTriggered, Status) values(110, 100, 'AutoCreateVanOrder', '整装B线获取SAP整车生产单', GETDATE(), GETDATE(), 0, 10, 2, 0, 1)
set Identity_Insert BAT_Trigger off
go
insert into BAT_TriggerParam(TriggerId, ParamKey, ParamValue) values(100, 'FlowCode', 'A')
insert into BAT_TriggerParam(TriggerId, ParamKey, ParamValue) values(110, 'FlowCode', 'B')
go


