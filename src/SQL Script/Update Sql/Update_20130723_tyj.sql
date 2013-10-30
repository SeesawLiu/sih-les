insert into FIS_OutboundCtrl values('PPC005','C:\DAT\LESSEQORD','com.Sconit.Service.FIS.Impl.CreateSeqOrderDATOutboundMgrImpl','C:\DAT\SEQFolder',4,'C:\Temp','UTF-8',null,null,1,0,null,null,null,null,null)
go
update FIS_OutboundCtrl set ArchiveFolder='C:\DAT\ORDFolder' where id=2
go
insert into FIS_FtpCtrl values('10.86.128.128',21,'infuser','infuser','INTERFACE/TNT/FILEOUT/LESORD/OutTemp','INTERFACE/TNT/FILEOUT/LESORD','*.DAT','C:\\DAT\\LESSEQORD\\OutTemp','C:\\DAT\\LESSEQORD','OUT',null,null,null)
go
insert into BAT_Job values('LesCreateSeqOrderDATJob','LesCreateSeqOrderDATJob','LesCreateSeqOrderDATJob')
go
insert into BAT_JobParam values(241,'UserCode','su')
go
insert into BAT_Trigger values(241,'LesCreateSeqOrderDATJob','LES创建排序单DAT','2013-07-05 12:10:00.000','2013-07-05 12:15:00.000',0,5,2,0,1) 
go
insert into BAT_TriggerParam values(241,'SystemCodes','PPC010')
go
insert into SYS_Menu values('Url_ShortCode_View','短代码查询','Menu.SupplierMenu',95,'短代码查询','~/Item/ShortIndex','~/Content/Images/Nav/Default.png',1)
go
insert into ACC_Permission values('Url_ShortCode_View','短代码查询','SupplierMenu')
go
create table dbo.FIS_CreateSeqOrderDAT
(
	Id				int identity(1,1) primary key  not null,
	SeqNo			varchar(50)		not null,
	Seq				int				not null,
	Flow			varchar(50)		not null,
	StartTime		datetime		not null,
	WindowTime		datetime		not null,
	PartyFrom		varchar(50)		not null,
	PartyTo			varchar(50)		not null,
	LocationTo		varchar(50)		not null,
	Container		varchar(50)		 null,
	CreateDate		datetime		not null,
	Item			varchar(50)		not null,
	ManufactureParty varchar(50)	 null,
	Qty				varchar(50)		not null,
	SequenceNumber	varchar(50)		not null,
	Van				varchar(50)		not null,
	Line			varchar(50)		 null,
	Station			varchar(50)		 null,
	ErrorCount		varchar(50)		not null,
	UploadDate		varchar(50)		not null,
	[FileName]		varchar(50)		not null,
	IsCreateDat		varchar(50)		not null
)
go
update  SYS_Menu set IsActive=0 where Code='Url_SequenceOrder_Detail'
go
update SYS_Menu set IsActive=1 where Code='Url_SequenceOrder'