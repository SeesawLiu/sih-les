IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_RunLeanEngine]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_RunLeanEngine]
GO

CREATE TABLE [dbo].[LOG_RunLeanEngine](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[Item] [varchar](50) NULL,
	[LocFrom] [varchar](50) NULL,
	[LocTo] [varchar](50) NULL,
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

ALTER TABLE [dbo].[LOG_RunLeanEngine] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_OrderBomCPTimeSnapshot]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_OrderBomCPTimeSnapshot]
GO

CREATE TABLE [dbo].[ORD_OrderBomCPTimeSnapshot](
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
 CONSTRAINT [PK_ORD_OrderBomCPTimeSnapshot] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]


