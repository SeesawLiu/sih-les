set Identity_Insert BAT_Job on
insert into BAT_Job(Id, Name, Desc1, ServiceType) values(200, 'LesCreateBarcodeDATJob', 'LesCreateBarcodeDATJob', 'LesCreateBarcodeDATJob')
set Identity_Insert BAT_Job off
go
insert into BAT_JobParam(JobId, ParamKey, ParamValue) values(200, 'UserCode', 'su')
go
set Identity_Insert BAT_Trigger on
insert into BAT_Trigger(Id, JobId, Name, Desc1, PrevFireTime, NextFireTime, RepeatCount, Interval, IntervalType, TimesTriggered, Status) values(200, 200, 'LesCreateBarcodeDAT', 'Les创建条码DAT文件', GETDATE(), GETDATE(), 0, 10, 2, 0, 1)
set Identity_Insert BAT_Trigger off
go
insert into BAT_TriggerParam(TriggerId,ParamKey,ParamValue) values (200,'SystemCodes','PPC005')

insert into acc_permission values('Url_AssemblyProductionFlow_Edit','整车生产线编辑','Production')