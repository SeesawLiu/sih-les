while exists(select Code from ACC_Permission group by Code having COUNT(1) > 1)
begin
	delete from ACC_PermissionGroupPermission where PermissionId in (select MAX(Id) from ACC_Permission group by Code having COUNT(1) > 1)
	delete from ACC_RolePermission where PermissionId in (select MAX(Id) from ACC_Permission group by Code having COUNT(1) > 1)
	delete from ACC_UserPermission where PermissionId in (select MAX(Id) from ACC_Permission group by Code having COUNT(1) > 1)
	delete from ACC_Permission where Id in (select MAX(Id) from ACC_Permission group by Code having COUNT(1) > 1)
end
go

CREATE UNIQUE NONCLUSTERED INDEX [IX_Permission] ON [dbo].[ACC_Permission] 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

while exists(select Code from SYS_CodeDet group by Code, Value having COUNT(1) > 1)
begin
	delete from SYS_CodeDet where Id in (select MAX(Id) from SYS_CodeDet group by Code, Value having COUNT(1) > 1)
end
go

CREATE UNIQUE NONCLUSTERED INDEX [IX_CodeDet] ON [dbo].[SYS_CodeDet] 
(
	[Code] ASC,
	[Value] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_CabOut]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_CabOut]
GO

CREATE TABLE [dbo].[CUST_CabOut](
	[OrderNo] [varchar](50) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[CabType] [tinyint] NOT NULL,
	[CabItem] [varchar](50) NOT NULL,
	[QulityBarcode] [varchar](50) NULL,
	[Status] [tinyint] NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[OutUser] [int] NULL,
	[OutUserNm] [varchar](100) NULL,
	[OutDate] [datetime] NULL,
	[TransferUser] [int] NULL,
	[TransferUserNm] [varchar](100) NULL,
	[TransferDate] [datetime] NULL,
 CONSTRAINT [PK_CUST_CabOut] PRIMARY KEY CLUSTERED 
(
	[OrderNo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_ManualGenOrderTrace]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_ManualGenOrderTrace]
GO

CREATE TABLE [dbo].[CUST_ManualGenOrderTrace](
	[ProdOrderNo] [varchar](50) NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
 CONSTRAINT [PK_CUST_ManualGenOrderTrace] PRIMARY KEY CLUSTERED 
(
	[ProdOrderNo] ASC,
	[OrderNo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_ProdLineLocationMap]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_ProdLineLocationMap]
GO

CREATE TABLE [dbo].[CUST_ProdLineLocationMap](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[SapLocation] [varchar](50) NOT NULL,
	[Location] [varchar](50) NOT NULL,
 CONSTRAINT [PK_CUST_ProdLineLocationMap] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_VanJob]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_VanJob]
GO

CREATE TABLE [dbo].[CUST_VanJob](
	[JobId] [int] IDENTITY(1,1) NOT NULL,
	[FlowCode] [varchar](50) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[CompleteDate] [datetime] NULL,
	[Result_SapVanOrderNo] [varchar](50) NULL,
 CONSTRAINT [PK_CUST_VanJob] PRIMARY KEY CLUSTERED 
(
	[JobId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FIS_ItemStandardPack]') AND type in (N'U'))
DROP TABLE [dbo].[FIS_ItemStandardPack]
GO

CREATE TABLE [dbo].[FIS_ItemStandardPack](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FlowDetId] [int] NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[Pack] [varchar](50) NULL,
	[UC] [decimal](18, 8) NOT NULL,
	[IOType] [varchar](50) NOT NULL,
	[Location] [char](4) NOT NULL,
	[Plant] [char](4) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[CreateDATDate] [datetime] NULL,
	[DATFileName] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

alter table FIS_LesINLog add Qty decimal(18, 8)
go
alter table FIS_LesINLog add QtyMark bit
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FIS_YieldReturn]') AND type in (N'U'))
DROP TABLE [dbo].[FIS_YieldReturn]
GO

CREATE TABLE [dbo].[FIS_YieldReturn](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IpNo] [varchar](50) NOT NULL,
	[ArriveTime] [datetime] NOT NULL,
	[PartyFrom] [varchar](50) NOT NULL,
	[PartyTo] [varchar](50) NOT NULL,
	[Dock] [varchar](100) NULL,
	[IpCreateDate] [datetime] NOT NULL,
	[Seq] [varchar](50) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Qty] [decimal](18, 8) NOT NULL,
	[IsConsignment] [bit] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[CreateDATDate] [datetime] NULL,
	[DATFileName] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[INV_OpRefBalance]') AND type in (N'U'))
DROP TABLE [dbo].[INV_OpRefBalance]
GO

USE [sconit5_sih]
GO

CREATE TABLE [dbo].[INV_OpRefBalance](
	[Id] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[OpRef] [varchar](50) NOT NULL,
	[Qty] [decimal](18, 8) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Version] [int] NULL,
 CONSTRAINT [PK_INV_OpRefBalance] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[INV_OpRefBalance]  WITH CHECK ADD  CONSTRAINT [FK_INV_OPRE_REFERENCE_MD_ITEM] FOREIGN KEY([Item])
REFERENCES [dbo].[MD_Item] ([Code])
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_FlowDetExtraDmdSource]') AND type in (N'U'))
DROP TABLE [dbo].[LE_FlowDetExtraDmdSource]
GO

CREATE TABLE [dbo].[LE_FlowDetExtraDmdSource](
	[FlowDetId] [int] NOT NULL,
	[Location] [varchar](50) NOT NULL,
 CONSTRAINT [PK_LE_FlowDetExtraDmdSource] PRIMARY KEY CLUSTERED 
(
	[FlowDetId] ASC,
	[Location] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_FlowDetSnapShot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_FlowDetSnapShot]
GO

CREATE TABLE [dbo].[LE_FlowDetSnapShot](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[FlowDetId] [int] NULL,
	[Item] [varchar](50) NOT NULL,
	[Uom] [varchar](5) NULL,
	[UC] [decimal](18, 8) NULL,
	[ManufactureParty] [varchar](50) NULL,
	[LocFrom] [varchar](50) NULL,
	[LocTo] [varchar](50) NOT NULL,
	[IsRefFlow] [bit] NULL,
	[SafeStock] [decimal](18, 8) NULL,
	[MaxStock] [decimal](18, 8) NULL,
	[MinLotSize] [decimal](18, 8) NULL,
	[RoundUpOpt] [tinyint] NULL,
	[Strategy] [tinyint] NULL,
 CONSTRAINT [PK_LE_FlowDetSnapShot_1] PRIMARY KEY CLUSTERED 
(
	[Item] ASC,
	[LocTo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_FlowMstrSnapShot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_FlowMstrSnapShot]
GO

CREATE TABLE [dbo].[LE_FlowMstrSnapShot](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NOT NULL,
	[Type] [tinyint] NULL,
	[Strategy] [tinyint] NULL,
	[PartyFrom] [varchar](50) NULL,
	[PartyTo] [varchar](50) NULL,
	[LocFrom] [varchar](50) NULL,
	[LocTo] [varchar](50) NULL,
	[Dock] [varchar](50) NULL,
	[ExtraDmdSource] [varchar](255) NULL,
	[OrderTime] [datetime] NULL,
	[ReqTimeFrom] [datetime] NULL,
	[ReqTimeTo] [datetime] NULL,
	[WindowTime] [datetime] NULL,
	[EMWindowTime] [datetime] NULL,
 CONSTRAINT [PK_LE_FlowMstrSnapShot] PRIMARY KEY CLUSTERED 
(
	[Flow] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_LocationDetSnapshot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_LocationDetSnapshot]
GO

CREATE TABLE [dbo].[LE_LocationDetSnapshot](
	[Item] [varchar](50) NOT NULL,
	[Location] [varchar](50) NOT NULL,
	[Qty] [decimal](18, 8) NULL,
 CONSTRAINT [PK_LE_LocationDetSnapshot] PRIMARY KEY CLUSTERED 
(
	[Item] ASC,
	[Location] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_OrderBomCPTimeSnapshot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_OrderBomCPTimeSnapshot]
GO

CREATE TABLE [dbo].[LE_OrderBomCPTimeSnapshot](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](50) NULL,
	[VanProdLine] [varchar](50) NULL,
	[AssProdLine] [varchar](50) NULL,
	[Seq] [bigint] NULL,
	[SubSeq] [int] NULL,
	[Op] [bigint] NULL,
	[OpTaktTime] [int] NULL,
	[CPTime] [datetime] NULL,
	[Item] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[Location] [varchar](50) NULL,
	[IsCreateOrder] [bit] NULL,
	[BomId] [int] NULL,
	[DISPO] [varchar](50) NULL,
	[CreateDate] [datetime] NOT NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Uom] [varchar](5) NULL,
	[VanOp] [int] NULL,
	[AssOp] [int] NULL,
 CONSTRAINT [PK_LE_OrderBomCPTimeSnapshot] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_OrderPlanSnapshot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_OrderPlanSnapshot]
GO

CREATE TABLE [dbo].[LE_OrderPlanSnapshot](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Item] [varchar](50) NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Location] [varchar](50) NULL,
	[ReqTime] [datetime] NULL,
	[OrderNo] [varchar](50) NULL,
	[IRType] [tinyint] NULL,
	[OrderType] [tinyint] NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[FinishQty] [decimal](18, 8) NULL,
 CONSTRAINT [PK_LE_OrderPlanSnapshot] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenSequenceOrder]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_GenSequenceOrder]
GO

CREATE TABLE [dbo].[LOG_GenSequenceOrder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SeqGroup] [varchar](50) NULL,
	[Lvl] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_GenSequenceOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[LOG_GenSequenceOrder] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenVanProdOrder]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_GenVanProdOrder]
GO

CREATE TABLE [dbo].[LOG_GenVanProdOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AUFNR] [varchar](50) NULL,
	[ZLINE] [varchar](50) NULL,
	[ProdLine] [varchar](50) NULL,
	[BatchNo] [varchar](50) NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_LOG_GenVanProdOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[LOG_GenVanProdOrder] ADD  CONSTRAINT [DF_LOG_GenVanProdOrder_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_OrderTrace]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_OrderTrace]
GO

CREATE TABLE [dbo].[LOG_OrderTrace](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UUID] [varchar](50) NULL,
	[Flow] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[OrderDetSeq] [int] NULL,
	[OrderDetId] [bigint] NULL,
	[Priority] [tinyint] NULL,
	[StartTime] [datetime] NULL,
	[WindowTime] [datetime] NULL,
	[EMWindowTime] [datetime] NULL,
	[ReqTimeFrom] [datetime] NULL,
	[ReqTimeTo] [datetime] NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[Uom] [varchar](50) NULL,
	[UC] [decimal](18, 8) NULL,
	[ManufactureParty] [varchar](50) NULL,
	[LocFrom] [varchar](50) NULL,
	[LocTo] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[SafeStock] [decimal](18, 8) NULL,
	[MaxStock] [decimal](18, 8) NULL,
	[MinLotSize] [decimal](18, 8) NULL,
	[RoundUpOpt] [tinyint] NULL,
	[ReqQty] [decimal](18, 8) NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_LOG_ORDERTRACE] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[LOG_OrderTrace] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_OrderTraceDet]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_OrderTraceDet]
GO

CREATE TABLE [dbo].[LOG_OrderTraceDet](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UUID] [varchar](50) NULL,
	[Type] [varchar](5) NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Location] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[ReqTime] [datetime] NOT NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[FinishQty] [decimal](18, 8) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_LOG_ORDERTRACEDET] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[LOG_OrderTraceDet] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_RunLeanEngine]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_RunLeanEngine]
GO

CREATE TABLE [dbo].[LOG_RunLeanEngine](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[Item] [varchar](50) NULL,
	[LocFrom] [varchar](50) NULL,
	[LocTo] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[Lvl] [tinyint] NULL,
	[ErrorId] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_RunLeanEngine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[LOG_RunLeanEngine] ADD  CONSTRAINT [DF__LOG_RunLe__Creat__13A973D6]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_VanOrderBomTrace]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_VanOrderBomTrace]
GO

CREATE TABLE [dbo].[LOG_VanOrderBomTrace](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UUID] [varchar](50) NULL,
	[ProdLine] [varchar](50) NULL,
	[VanOrderNo] [varchar](50) NULL,
	[VanOrderBomDetId] [int] NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[OpRef] [varchar](50) NULL,
	[LocFrom] [varchar](50) NULL,
	[LocTo] [varchar](50) NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[CPTime] [datetime] NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_VanOrderBomTrace] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[LOG_VanOrderBomTrace] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_VanOrderTrace]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_VanOrderTrace]
GO

CREATE TABLE [dbo].[LOG_VanOrderTrace](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UUID] [varchar](50) NULL,
	[Flow] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[OrderDetSeq] [int] NULL,
	[OrderDetId] [bigint] NULL,
	[Priority] [tinyint] NULL,
	[StartTime] [datetime] NULL,
	[WindowTime] [datetime] NULL,
	[EMWindowTime] [datetime] NULL,
	[ReqTimeFrom] [datetime] NULL,
	[ReqTimeTo] [datetime] NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[Uom] [varchar](50) NULL,
	[UC] [decimal](18, 8) NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Location] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[NetOrderQty] [decimal](18, 8) NULL,
	[OrgOpRefQty] [decimal](18, 8) NULL,
	[GrossOrderQty] [decimal](18, 8) NULL,
	[OpRefQty] [decimal](18, 8) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_LOG_JITORDERTRACE] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[LOG_VanOrderTrace] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

ALTER TABLE	MD_Item ALTER COLUMN ItemCategory varchar(50)
GO

ALTER TABLE	MD_Item ADD DISPO varchar(100)
GO

ALTER TABLE	MD_Item ADD PLIFZ varchar(100)
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MD_Picker]') AND type in (N'U'))
DROP TABLE [dbo].[MD_Picker]
GO

CREATE TABLE [dbo].[MD_Picker](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](50) NOT NULL,
	[Decs1] [varchar](100) NOT NULL,
	[Location] [varchar](50) NOT NULL,
	[UserCode] [varchar](50) NOT NULL,
	[UserNm] [varchar](100) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MD_PickRule]') AND type in (N'U'))
DROP TABLE [dbo].[MD_PickRule]
GO

CREATE TABLE [dbo].[MD_PickRule](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[ItemDesc] [varchar](100) NULL,
	[Location] [varchar](50) NOT NULL,
	[Picker] [varchar](50) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MD_Shipper]') AND type in (N'U'))
DROP TABLE [dbo].[MD_Shipper]
GO

CREATE TABLE [dbo].[MD_Shipper](
	[Code] [varchar](50) NOT NULL,
	[Desc1] [varchar](100) NOT NULL,
	[Location] [varchar](50) NULL,
	[Address] [varchar](50) NULL,
	[Contact] [varchar](50) NULL,
	[Tel] [varchar](50) NULL,
	[Email] [varchar](50) NULL,
	[IsActive] [bit] NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE MD_SpecialTime add ProdLine varchar(50)
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MD_WorkCenter]') AND type in (N'U'))
DROP TABLE [dbo].[MD_WorkCenter]
GO

CREATE TABLE [dbo].[MD_WorkCenter](
	[Code] [varchar](50) NOT NULL,
	[Location] [varchar](50) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
 CONSTRAINT [PK_MD_WORKCENTER] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[MD_WorkCenter]  WITH CHECK ADD  CONSTRAINT [FK_MD_WORKC_REFERENCE_MD_LOCAT] FOREIGN KEY([Location])
REFERENCES [dbo].[MD_Location] ([Code])
GO

ALTER TABLE [dbo].[MD_WorkCenter] CHECK CONSTRAINT [FK_MD_WORKC_REFERENCE_MD_LOCAT]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_AliquotStartTask]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_AliquotStartTask]
GO

CREATE TABLE [dbo].[ORD_AliquotStartTask](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NOT NULL,
	[VanNo] [varchar](50) NULL,
	[IsStart] [bit] NOT NULL,
	[StartTime] [datetime] NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


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

alter table ORD_OrderBackflushDet add ProdSeqReportId int
go
alter table ORD_OrderBackflushDet add OrderOpId int
go
alter table ORD_OrderBackflushDet add WorkCenter varchar(50)
go
alter table ORD_OrderBackflushDet alter column RecNo varchar(50) null
go
alter table ORD_OrderBackflushDet alter column RecDetId int null
go
alter table ORD_OrderBackflushDet alter column RecDetSeq int null
go
alter table ORD_OrderBackflushDet alter column OrderNo varchar(50) null
go
alter table ORD_OrderBackflushDet alter column OrderDetId int null
go
alter table ORD_OrderBackflushDet alter column OrderDetSeq int null
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_OrderBomCPTime]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_OrderBomCPTime]
GO

CREATE TABLE [dbo].[ORD_OrderBomCPTime](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](50) NULL,
	[VanProdLine] [varchar](50) NULL,
	[AssProdLine] [varchar](50) NULL,
	[Seq] [bigint] NULL,
	[SubSeq] [int] NULL,
	[Op] [int] NULL,
	[OpTaktTime] [int] NULL,
	[CPTime] [datetime] NULL,
	[Item] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[Location] [varchar](50) NULL,
	[IsCreateOrder] [bit] NULL,
	[BomId] [int] NULL,
	[DISPO] [varchar](50) NULL,
	[CreateDate] [datetime] NOT NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Uom] [varchar](5) NULL,
	[VanOp] [int] NULL,
	[AssOp] [int] NULL,
	[WorkCenter] [varchar](50) NULL,
 CONSTRAINT [PK_ORD_OrderBomCPTime] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ORD_OrderBomCPTime] ADD  CONSTRAINT [DF_ORD_OrderBomCPTime_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO