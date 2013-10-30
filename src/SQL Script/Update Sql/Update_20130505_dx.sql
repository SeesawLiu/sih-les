IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_OrderBomCPTimeSnapshot]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_OrderBomCPTimeSnapshot]
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
	[OrgOp] [int] NULL,
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
 CONSTRAINT [PK_LE_OrderBomCPTimeSnapshot] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_VanOrderBomTrace]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_VanOrderBomTrace]
GO

CREATE TABLE [dbo].[LOG_VanOrderBomTrace](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UUID] [varchar](50) NULL,
	[ProdLine] [varchar](50) NULL,
	[VanOrderNo] [varchar](50) NULL,
	[VanOrderBomDetId] int NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[OpRef] [varchar](50) NULL,
	[Location] [varchar](50) NULL,
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
 CONSTRAINT [PK_LE_FlowDetSnapShot_1] PRIMARY KEY CLUSTERED 
(
	[Item] ASC,
	[LocTo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[LE_FlowDetSnapShot]') AND name = N'IX_FlowDetSnapshot_Flow')
DROP INDEX [IX_FlowDetSnapshot_Flow] ON [dbo].[LE_FlowDetSnapShot] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_FlowDetSnapshot_Flow] ON [dbo].[LE_FlowDetSnapShot] 
(
	[Flow] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
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