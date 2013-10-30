alter table PRD_RoutingMstr drop  constraint FK_PRD_ROUT_REFERENCE_MD_REGIO
go
alter table  PRD_RoutingMstr drop column Region
go
alter table  PRD_RoutingMstr drop column TaktTime
go
alter table  PRD_RoutingMstr drop column TaktTimeUnit
go
alter table  PRD_RoutingMstr drop column WaitTime
go
alter table  PRD_RoutingMstr drop column WaitTimeUnit
go
alter table  PRD_RoutingMstr drop column VirtualOpRef
go


alter table  PRD_RoutingDet drop column LeadTime
go
alter table  PRD_RoutingDet drop column TimeUnit
go
alter table  PRD_RoutingDet drop column StartDate
go
alter table  PRD_RoutingDet drop column EndDate
go


insert into sys_codemstr values('WorkingCalendarCategory','工作日历类型',0)
go
insert into sys_codedet values('WorkingCalendarCategory',0,'CodeDetail_WorkingCalendarCategory_Region',1,1)
insert into sys_codedet values('WorkingCalendarCategory',1,'CodeDetail_WorkingCalendarCategory_ProdLine',0,2)
go


alter table PRD_StandardWorkingCalendar alter column RegionName varchar(50) null
go
alter table PRD_WorkingCalendar alter column RegionName varchar(50) null
go

update sys_menu set name ='区域标准工作日历',desc1 ='区域标准工作日历' where code ='Url_StandardWorkingCalendar_View'
update acc_permission set desc1 ='查看区域标准工作日历' where code ='Url_StandardWorkingCalendar_View'
update acc_permission set desc1 ='编辑区域标准工作日历' where code ='Url_StandardWorkingCalendar_Edit'
update acc_permission set desc1 ='删除区域标准工作日历' where code ='Url_StandardWorkingCalendar_Delete'
go

insert into sys_menu  values
('Url_ProdLineStandardWorkingCalendar_View','生产线标准工作日历','Menu.Production.Setup',50405,'生产线标准工作日历','~/ProdLineStandardWorkingCalendar/Index','~/Content/Images/Nav/Default.png',1)
insert into acc_permission values('Url_ProdLineStandardWorkingCalendar_View','查看生产线标准工作日历','Production')
insert into acc_permission values('Url_ProdLineStandardWorkingCalendar_Edit','编辑生产线标准工作日历','Production')
insert into acc_permission values('Url_ProdLineStandardWorkingCalendar_Delete','删除生产线标准工作日历','Production')
go

insert into sys_menu  values
('Url_ProdLineWorkingCalendar_View','生产线工作日历','Menu.Production.Setup',50406,'生产线工作日历','~/ProdLineWorkingCalendar/Index','~/Content/Images/Nav/Default.png',1)
insert into acc_permission values('Url_ProdLineWorkingCalendar_View','查看生产线工作日历','Production')
insert into acc_permission values('Url_ProdLineWorkingCalendar_Edit','编辑生产线工作日历','Production')
insert into acc_permission values('Url_ProdLineWorkingCalendar_Delete','删除生产线工作日历','Production')
go

