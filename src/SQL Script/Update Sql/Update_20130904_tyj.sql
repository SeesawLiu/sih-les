alter table PRD_LotNoScan add ItemDesc varchar(50) null
go
alter table PRD_LotNoTrace add ItemDesc varchar(50) null
go
update SYS_Menu set Desc1='配额调整量维护',Name='配额调整量维护' where Code='Url_Quota_View'
go
insert into SYS_Menu values('Url_QuotaCycleQty_View','配额循环量维护','Menu.Procurement.Setup',30,'配额循环量维护','~/Quota/CycleQtyIndex','~/Content/Images/Nav/Default.png',1)
go
insert into ACC_Permission values('Url_QuotaCycleQty_View','配额循环量维护','Procurement')
go
update sys_menu set isactive=0 where code='Url_FlowDetailImport_View'
go

update SYS_Menu set Desc1='采购路线明细缺失报表',Name='采购路线明细缺失报表',Parent='Menu.Procurement.Setup',Seq=5 where Code='Url_Cust_ScheduleLineItem'
go
update ACC_Permission set Category='Procurement' where Code='Url_Cust_ScheduleLineItem'
go
update SYS_Menu set Desc1='移库路线明细缺失报表',name='移库路线明细缺失报表',Parent='Menu.Inventory.Setup',Seq=350 where Code='Url_SnapshotFlowDet4LeanEngine_View'
go
update ACC_Permission set Category='Inventory' where Code='Url_SnapshotFlowDet4LeanEngine_View'
go
set identity_insert BAT_Job on
insert into BAT_Job(Id, Name, Desc1, ServiceType) values(260,'ImportSAPQuotaJob','导入SAP配额','ImportSAPQuotaJob')
set identity_insert BAT_Job off
insert into BAT_JobParam values(260,'UserCode','su')
go
set identity_insert BAT_Trigger on
insert into BAT_Trigger(Id, JobId, Name, Desc1, PrevFireTime, NextFireTime, RepeatCount, Interval, IntervalType, TimesTriggered, Status) values(260,260,'ImportSAPQuotaJob','导入SAP配额','2013-09-04 11:00:00.000','2013-09-05 01:00:00.000',0,1,4,0,1)
set identity_insert BAT_Trigger off
go
insert into BAT_TriggerParam values(260,'Plant','0084')
go
insert into ACC_Permission values('Url_QuotaCycleQty_Edit','配额循环量维护-编辑','Procurement')
go
update ACC_Permission set Desc1='配额循环量维护-查询' where Code='Url_QuotaCycleQty_View'
go
insert into ACC_Permission values('Client_LotNoScan','批号管理','Terminal')
go
 insert into ACC_Permission  values('Url_Distribution_PickShipConfirmation','配送组发货-确认','Distribution')
go
insert into SYS_Menu values('Url_Scheduling_DemandPlan','需求计划','Menu.ProcurementInfo',50,'需求计划','~/SupplierScheduling/DemandPlanIndex','~/Content/Images/Nav/Default.png',1)
go
insert into ACC_Permission values('Url_Scheduling_DemandPlan','计划协议-需求计划','SupplierMenu')
go
update SYS_Menu set PageUrl='~/TransferOrder/Index/10' where Code='Url_TransferOrder_View'
go
update SYS_Menu set PageUrl='~/TransferOrder/Index/20' where Code='Url_OrderMstr_Procurement_Import'
go
update SYS_Menu set PageUrl='~/TransferOrder/Index/30' where Code='Url_OrderMstr_Procurement_ReturnNew'
go
update SYS_Menu set PageUrl='~/TransferOrder/Index/40' where Code='Url_OrderMstr_Procurement_ReturnQuickNew'

