CREATE TABLE [dbo].[ORD_PickTask](
	[PickId] [varchar](50) NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
	[OrdDetId] [int] NOT NULL,
	[DemandType] [tinyint] NOT NULL,
	[IsHold] [bit] NOT NULL,
	[Flow] [varchar](50) NULL,
	[FlowDesc] [varchar](100) NULL,
	[Item] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[Uom] [varchar](5) NULL,
	[BaseUom] [varchar](5) NULL,
	[PartyFrom] [varchar](50) NULL,
	[PartyFromName] [varchar](100) NULL,
	[PartyTo] [varchar](50) NULL,
	[PartyToName] [varchar](100) NULL,
	[LocationFrom] [varchar](50) NULL,
	[LocationFromName] [varchar](100) NULL,
	[LocationTo] [varchar](50) NULL,
	[LocationToName] [varchar](100) NULL,
	[WindowTime] [datetime] NULL,
	[ReleaseDate] [datetime] NULL,
	[Supplier] [varchar](50) NULL,
	[SupplierName] [varchar](100) NULL,
	[UnitCount] [decimal](18, 8) NULL,
	[OrderedQty] [decimal](18, 8) NULL,
	[PickedQty] [decimal](18, 8) NULL,
	[Picker] [varchar](50) NULL,
	[PrintCount] [int] NOT NULL,
	[Memo] [varchar](256) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ORD_PickTask] PRIMARY KEY CLUSTERED 
(
	[PickId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[ORD_PickHu](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PickId] [varchar](50) NOT NULL,
	[RepackHu] [varchar](50) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ORD_PickHu] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[ORD_PickResult](
	[ResultId] [varchar](50) NOT NULL,
	[PickId] [varchar](50) NOT NULL,
	[PickedHu] [varchar](50) NULL,
	[HuQty] [decimal](18, 8) NULL,
	[PickedQty] [decimal](18, 8) NULL,
	[Picker] [varchar](50) NULL,
	[PickDate] [datetime] NULL,
	[AsnNo] [varchar](50) NULL,
	[Memo] [varchar](256) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ORD_PickResult] PRIMARY KEY CLUSTERED 
(
	[ResultId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[ORD_ShipList](
	[ShipNo] [varchar](50) NOT NULL,
	[Vehicle] [varchar](50) NULL,
	[Shipper] [varchar](50) NULL,
	[Status] [tinyint] NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[CloseDate] [datetime] NULL,
	[CloseUser] [int] NULL,
	[CloseUserNm] [varchar](100) NULL,
	[CancelDate] [datetime] NULL,
	[CancelUser] [int] NULL,
	[CancelUserNm] [varchar](100) NULL,
 CONSTRAINT [PK_ORD_ShipList] PRIMARY KEY CLUSTERED 
(
	[ShipNo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

alter table SCM_FlowDet add IsCreatePickList bit null
update SCM_FlowDet set IsCreatePickList = 0

alter table ORD_OrderDet add IsCreatePickList bit null
go
alter table ORD_OrderDet_0 add IsCreatePickList bit null
go
alter table ORD_OrderDet_1 add IsCreatePickList bit null
go
alter table ORD_OrderDet_2 add IsCreatePickList bit null
go
alter table ORD_OrderDet_3 add IsCreatePickList bit null
go
alter table ORD_OrderDet_4 add IsCreatePickList bit null
go
alter table ORD_OrderDet_5 add IsCreatePickList bit null
go
alter table ORD_OrderDet_6 add IsCreatePickList bit null
go
alter table ORD_OrderDet_7 add IsCreatePickList bit null
go
alter table ORD_OrderDet_8 add IsCreatePickList bit null
go

insert into sys_menu values
('Url_PickTask_View','查询拣货任务','Url_PickList',200,'查询拣货任务','~/PickTask/Index','~/Content/Images/Nav/Default.png',1)
go
insert into acc_permission values('Url_PickTask_View','查询拣货任务','Distribution')
go

insert into sys_menu values
('Url_PickTask_Handle','操作拣货任务','Url_PickList',203,'操作拣货任务','~/PickTask/Handle','~/Content/Images/Nav/Default.png',1)
go
insert into acc_permission values('Url_PickTask_Handle','操作拣货任务','Distribution')
go

insert into sys_menu values
('Url_PickTask_New','创建拣货任务','Url_PickList',205,'创建拣货任务','~/PickTask/New','~/Content/Images/Nav/Default.png',1)
go
insert into acc_permission values('Url_PickTask_New','创建拣货任务','Distribution')
go

insert into sys_menu values
('Url_PickResult_View','拣货结果','Url_PickList',210,'拣货结果','~/PickResult/Index','~/Content/Images/Nav/Default.png',1)
go
insert into acc_permission values('Url_PickResult_View','拣货结果','Distribution')
go

insert into sys_menu values
('Url_ShipList_View','运单','Url_PickList',220,'运单','~/ShipList/Index','~/Content/Images/Nav/Default.png',1)
go
insert into acc_permission values('Url_ShipList_View','运单','Distribution')
go

insert into acc_permission values('Url_PickTask_Hold','冻结拣货','Distribution')
go
insert into acc_permission values('Url_PickTask_Unhold','解冻拣货','Distribution')
go
insert into acc_permission values('Url_PickTask_Assign','分派拣货','Distribution')
go
insert into acc_permission values('Url_PickTask_Print','打印配送条码','Distribution')
go
insert into acc_permission values('Url_PickTask_Pick','拣货','Distribution')
go
insert into acc_permission values('Url_PickTask_Move','分播','Distribution')
go
insert into acc_permission values('Url_PickTask_Ship','发货','Distribution')
go
insert into acc_permission values('Url_PickResult_Cancel','取消拣货','Distribution')
go
insert into acc_permission values('Url_ShipList_New','创建运单','Distribution')
go
insert into acc_permission values('Url_ShipList_Print','打印运单','Distribution')
go
insert into acc_permission values('Url_ShipList_Cancel','取消运单','Distribution')
go
insert into acc_permission values('Url_ShipList_Close','关闭运单','Distribution')
go

alter table ORD_IpMstr add ShipNo varchar(50) null
go
alter table ORD_IpMstr add Vehicle varchar(50) null
go
alter table ORD_IpMstr_0 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_0 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_1 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_1 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_2 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_2 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_3 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_3 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_4 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_4 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_5 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_5 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_6 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_6 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_7 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_7 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_8 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_8 add Vehicle varchar(50) null
go

insert into SYS_EntityPreference values(11040,70,0,'测试系统标识',1,'用户 超级','2011-01-01 00:00:00.000',1,'用户 超级','2011-01-01 00:00:00.000')

insert into SYS_Menu values('Url_OrderItemTrace_New','批号刷读','Menu.Quality.Trans',500,'批号刷读',null,'~/Content/Images/Nav/Default.png',1)

insert into ACC_Permission values('Url_OrderItemTrace_New','批号刷读','Quality')
