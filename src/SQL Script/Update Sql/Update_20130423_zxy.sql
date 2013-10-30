alter table scm_flowmstr add VirtualOpRef varchar(50) null
go

alter table scm_flowstrategy add WaitTime decimal(18,8) null
go

alter table scm_flowstrategy add WaitBatch int null
go


update sys_menu set name ='区域工作日历',desc1 ='区域工作日历' where code ='Url_WorkingCalendar_View'
update acc_permission set desc1 ='查看区域准工作日历' where code ='Url_WorkingCalendar_View'
update acc_permission set desc1 ='编辑区域工作日历' where code ='Url_WorkingCalendar_Edit'
update acc_permission set desc1 ='删除区域工作日历' where code ='Url_WorkingCalendar_Delete'
go

update CUST_ProductLineMap set IsActive = 1 where IsActive is null
go
alter table CUST_ProductLineMap alter column IsActive bit not null
go

update sys_menu set name ='非整车生产线映射',desc1='非整车生产线映射' where code ='Url_CUST_ProductLineMap_View'
update acc_permission set desc1='非整车生产线映射' where code ='Url_CUST_ProductLineMap_View'
go

insert into sys_menu values
('Url_CUST_VanProductLineMap_View','整车生产线映射','Menu.Production.Setup',491,'整车生产线映射','~/VanProductLineMap/Index','~/Content/Images/Nav/Default.png',1)
insert into acc_permission values('Url_CUST_VanProductLineMap_View','整车生产线映射','Production')
go
