
if exists(select * from sysobjects where name ='FIS_YieldReturn')
	drop table FIS_YieldReturn
go
Create table FIS_YieldReturn
(
	Id int identity(1,1) primary key,
	IpNo varchar(50) not null, --送货单号/退货单号
	ArriveTime DateTime not	null, --预计到货时间
	PartyFrom varchar(50) not null, --Buff退货为Buff代码。	发出方代码		
	PartyTo	varchar(50) not	null,--LOC代码	接收方代码
	Dock varchar(100)  null,--		收货道口
	IpCreateDate DateTime not null,--	创建日期
	Seq	varchar(50)	not null,--	行号 
	Item varchar(50) not null,-- 零件号
	ManufactureParty varchar(50)  null,-- 制造商/品牌
	Qty	Decimal(18,8) not null,-- 送货数
	IsConsignment bit not null ,--是否寄售
	CreateDate datetime not null,
	CreateDATDate datetime  null,
	DATFileName varchar(255)
)
go


insert into sys_menu values
('Url_OrderMstr_Procurement_Ship','发货','Url_OrderMstr_Procurement_Return',25,'发货','~/ProcurementOrder/ShipIndex','~/Content/Images/Nav/Default.png',1)
go
insert into acc_permission values
('Url_OrderMstr_Procurement_Ship','发货','Procurement')
go

set Identity_Insert BAT_Job on
insert into BAT_Job(Id, Name, Desc1, ServiceType) values(230, 'LesYieldReturnDATJob', 'LesYieldReturnDATJob', 'LesYieldReturnDATJob')
set Identity_Insert BAT_Job off
go
insert into BAT_JobParam(JobId, ParamKey, ParamValue) values(230, 'UserCode', 'su')
go
set Identity_Insert BAT_Trigger on
insert into BAT_Trigger(Id, JobId, Name, Desc1, PrevFireTime, NextFireTime, RepeatCount, Interval, IntervalType, TimesTriggered, Status) 
values(230, 230, 'LesYieldReturnDAT', 'Les创建退货单文件', GETDATE(), GETDATE(), 0, 10, 2, 0, 1)
set Identity_Insert BAT_Trigger off
go
insert into BAT_TriggerParam(TriggerId,ParamKey,ParamValue) values (230,'SystemCodes','PPC008')
go

insert into FIS_FtpCtrl values
('10.86.128.128',21,'infuser','infuser','INTERFACE/TNT/FILEOUT/YieldReturn/OutTemp','INTERFACE/TNT/FILEOUT/YieldReturn','*.DAT','C:\\DAT\\YieldReturn\\OutTemp','C:\\DAT\\YieldReturn','OUT',null,null,null)
go

insert into FIS_OutboundCtrl values
('PPC008','C:\DAT\YieldReturn','com.Sconit.Service.FIS.Impl.YieldReturnDATMgrImpl','C:\DAT\YieldReturnFolder',4,'C:\Temp','UTF-8',null,null,1,0,null,null,null,null,null)
go