
create table FIS_CreateProcurementOrderDAT
(
Id				int identity(1,1) primary key not null,
OrderNo			varchar(50)					  not null,
Van				varchar(50)						  null,
OrderStrategy	int							 not null,
StartTime		datetime					not null,
WindowTime		datetime					not null,
Priority		int							not null,
Sequence		varchar(50)					null,
PartyFrom		varchar(50)					not null,
PartyTo			varchar(50)					not null,
Dock			varchar(100)				null,
CreateDate		datetime					not null,
Flow			varchar(50)					not null,
LineSeq			int							not null,
Item			varchar(50)					not null,
ManufactureParty varchar(50)				null,
LocationTo		 varchar(50)				null,
Bin				varchar(50)					null,
OrderedQty		decimal(18,8)				not null,
IsShipExceed	bit							not null,
FileName		varchar(50)					 null,
IsCreateDat		bit							not null
)
---------------- job
/* 排序单 */
go
insert into BAT_Job values('LesCreateSeqOrderDATJob','LES创建排序单DAT','LesCreateSeqOrderDATJob')
go
insert into BAT_JobParam values(241,'UserCode','su')
go
insert into BAT_Trigger values(241,'LesCreateSeqOrderDATJob','LES创建排序单DAT','2013-07-05 12:10:00.000','2013-07-05 12:15:00.000',0,5,2,0,1) 
go
insert into BAT_TriggerParam values(241,'SystemCodes','PPC010')
go
/* 新格式要货单 */
insert into BAT_Job values('LesCreateProcurementOrderDATJob','新格式要货单','LesCreateProcurementOrderDATJob')
go
insert into BAT_JobParam values(242,'UserCode','su')
go
insert into BAT_Trigger values(242,'LesCreateProcurementOrderDATJob','LES创建要货单DAT','2013-07-04 20:00:00.000','2013-07-04 20:00:00.000',0,5,2,0,1)
go
insert into BAT_TriggerParam values(242,'SystemCodes','PPC011')
go
/* 路线代码表 */
insert into BAT_Job values('SeqKitFlowDATJob','排序KIT单路线代码表','SeqKitFlowDATJob')
go
insert into BAT_JobParam values(243,'UserCode','su')
go
insert into BAT_Trigger values(243,'SeqKitFlowDATJob','路线代码表DAT','2013-07-04 20:00:00.000','2013-07-05 20:00:00.000',0,1,4,0,1)
go
insert into BAT_TriggerParam values(243,'SystemCodes','PPC012')
go
insert into BAT_Job values('LesCreateSeqOrderDATJob','LES创建排序单DAT','LesCreateSeqOrderDATJob')
go
/*上传所有DAT文件*/
go
insert into BAT_Job values('UploadAllDATJob','上传所有DAT文件','UploadAllDATJob')
go
insert into BAT_Trigger values(244,'UploadAllDATJob','上传所有DAT文件','2013-07-05 12:10:00.000','2013-07-05 12:15:00.000',0,5,2,0,1) 
go
---------------输出文件控制类
/*退货单*/
insert into FIS_OutboundCtrl values
('PPC008','C:\DAT\ASN','com.Sconit.Service.FIS.Impl.YieldReturnDATMgrImpl','C:\DAT\YieldReturnFolder',5,'C:\Temp','UTF-8',null,null,1,0,null,null,null,null,null)
go
/* 要货单 */
insert into FIS_OutboundCtrl values('PPC011','C:\DAT\LESORD','com.Sconit.Service.FIS.Impl.CreateProcurementOrderDATOutMgrImpl','C:\DAT\ORDFolder',6,'C:\Temp','UTF-8',null,null,1,0,null,null,null,null,null)
/* 排序单 */
insert into FIS_OutboundCtrl values('PPC010','C:\DAT\LESORD','com.Sconit.Service.FIS.Impl.CreateSeqOrderDATOutboundMgrImpl','C:\\DAT\\SEQFolder',7,'C:\Temp','UTF-8',null,null,1,0,null,null,null,null,null)
go
update FIS_OutboundCtrl set ArchiveFolder='C:\DAT\ORDFolder' where id=2
go
--/* 收货单 */
--insert into FIS_OutboundCtrl values('PPC005','C:\DAT\LESLog','com.Sconit.Service.FIS.Impl.CreateSeqOrderDATOutboundMgrImpl','C:\\DAT\\SEQFolder',7,'C:\Temp','UTF-8',null,null,1,0,null,null,null,null,null)
/* 冲销单 */
insert into FIS_OutboundCtrl values('PPC007','C:\DAT\LESLog','com.Sconit.Service.FIS.Impl.CancelReceiptMasterDATMgrImpl','C:\\DAT\\CancelFolder',8,'C:\Temp','UTF-8',null,null,1,0,null,null,null,null,null)
/* 条码 */
insert into FIS_FtpCtrl values
('10.86.128.128',21,'infuser','infuser','INTERFACE_TEST/TNT/FILEOUT/LESbarcode/OutTemp','INTERFACE_TEST/TNT/FILEOUT/LESbarcode','*.DAT','C:\\DAT\\LESbarcode\\OutTemp','C:\\DAT\\LESbarcode','OUT',null,null,null)
go
insert into FIS_OutboundCtrl values('PPC005','C:\DAT\LESbarcode','com.Sconit.Service.FIS.Impl.CreateBarCodeDATMgrImpl','C:\DAT\BarCodeFolder',9,'C:\Temp','UTF-8',null,null,1,0,null,null,null,null,null)
go
/*零件标准包装*/
insert into FIS_FtpCtrl values
('10.86.128.128',21,'infuser','infuser','INTERFACE_TEST/TNT/FILEOUT/LESMASTERDATA/OutTemp','INTERFACE_TEST/TNT/FILEOUT/LESMASTERDATA','*.DAT','C:\\DAT\\LESMASTERDATA\\OutTemp','C:\\DAT\\LESMASTERDATA','OUT',null,null,null)
go
insert into FIS_OutboundCtrl values
('PPC009','C:\DAT\LESMASTERDATA','com.Sconit.Service.FIS.Impl.ItemStandardPackDATMgrImpl','C:\DAT\LESMASTERDATAFolder',10,'C:\Temp','UTF-8',null,null,1,0,null,null,null,null,null)
go
/*库位表*/
insert into FIS_OutboundCtrl values
('PPC006','C:\DAT\LESMASTERDATA','com.Sconit.Service.FIS.Impl.LocationDATMgrImpl','C:\DAT\LESMASTERDATAFolder',11,'C:\Temp','UTF-8',null,null,1,0,null,null,null,null,null)
go
/*排序KIT单路线代码表*/
insert into FIS_OutboundCtrl values
('PPC012','C:\DAT\LESMASTERDATA','com.Sconit.Service.FIS.Impl.SeqKitFlowDATMgrImpl','C:\DAT\LESMASTERDATAFolder',12,'C:\Temp','UTF-8',null,null,1,0,null,null,null,null,null)
go
insert into SYS_Menu values('Url_Supplier_SequenceOrder','排序单','Menu.SupplierMenu',65,'排序单',null,'~/Content/Images/Nav/Default.png',1)
go	
insert into SYS_Menu values('Url_Supplier_SequenceOrder_View','查看','Url_Supplier_SequenceOrder',100,'查看','~/SequenceMaster/Index','~/Content/Images/Nav/Default.png',1)
go	
insert into ACC_Permission values('Url_Supplier_SequenceOrder_View','排序单-查看','SupplierMenu')
go
update  BAT_Trigger set Interval=1 ,IntervalType=4 where id =210
go
alter table FIS_CreateSeqOrderDAT alter column uploadDate datetime null
go
alter table FIS_CreateSeqOrderDAT alter column IsCreateDat bit not null
