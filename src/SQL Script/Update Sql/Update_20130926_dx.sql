IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_OrderBomCPTimeSnapshot_Arch]') AND type in (N'U'))
DROP TABLE [dbo].[LE_OrderBomCPTimeSnapshot_Arch]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[LE_OrderBomCPTimeSnapshot_Arch](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[BatchNo] [int] NULL,
	[BomId] [int] NULL,
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
	[DISPO] [varchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Uom] [varchar](5) NULL,
	[VanOp] [int] NULL,
	[AssOp] [int] NULL,
	[WorkCenter] [varchar](50) NULL,
	[BESKZ] [varchar](50) NULL,
	[SOBSL] [varchar](50) NULL,
	[TraceCode] [varchar](50) NULL,
	[ZOPWZ] [varchar](50) NULL,
	[ZOPID] [varchar](50) NULL,
	[ZOPDS] [varchar](50) NULL,
 CONSTRAINT [PK_LE_OrderBomCPTimeSnapshot_Arch] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

alter table ORD_PickListMstr add DeliveryGroup varchar(50)
go