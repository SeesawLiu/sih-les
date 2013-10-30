IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_FlowDetExtraDmdSourceSnapshot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_FlowDetExtraDmdSourceSnapshot]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[LE_FlowDetExtraDmdSourceSnapshot](
	[FlowDetId] [int] NOT NULL,
	[Location] [varchar](50) NOT NULL,
 CONSTRAINT [PK_LE_FlowDetExtraDmdSourceSnapshot] PRIMARY KEY CLUSTERED 
(
	[FlowDetId] ASC,
	[Location] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_OrderBomCPTimeSnapshot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_OrderBomCPTimeSnapshot]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
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
	[CreateDate] [datetime] NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Uom] [varchar](5) NULL,
	[VanOp] [int] NULL,
	[AssOp] [int] NULL,
	[WorkCenter] [varchar](50) NULL,
 CONSTRAINT [PK_LE_OrderBomCPTimeSnapshot] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[LE_OrderBomCPTimeSnapshot] ADD  CONSTRAINT [DF_LE_OrderBomCPTimeSnapshot_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO



