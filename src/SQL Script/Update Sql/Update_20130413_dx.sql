alter table PRD_RoutingDet add Capacity int
go
alter table ORD_OrderBomDet add AssProdLine varchar(50)
go
alter table ORD_OrderBomDet add IsCreateOrder bit
go
alter table ORD_OrderBomDet add DISPO varchar(50)
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
	[OrgOp] [int] NULL,
	[Op] [int] NULL,
	[OpTaktTime] [int] NULL,
	[CPTime] [datetime] NULL,
	[Item] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[OrderQty] [decimal](18, 18) NULL,
	[Location] [varchar](50) NULL,
	[IsCreateOrder] [bit] NULL,
	[BomId] [int] NULL,
	[DISPO] [varchar](50) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ORD_OrderBomCPTime] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ORD_OrderBomCPTime] ADD  CONSTRAINT [DF_ORD_OrderBomCPTime_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_OrderOpCPTime]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_OrderOpCPTime]
GO

CREATE TABLE [dbo].[ORD_OrderOpCPTime](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](50) NULL,
	[VanProdLine] [varchar](50) NULL,
	[AssProdLine] [varchar](50) NULL,
	[Seq] [bigint] NULL,
	[SubSeq] [int] NULL,
	[OrgOp] [int] NULL,
	[Op] [int] NULL,
	[OpTaktTime] [int] NULL,
	[CPTime] [datetime] NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ORD_OrderOpCPTime] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ORD_OrderOpCPTime] ADD  CONSTRAINT [DF_ORD_OrderOpCPTime_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

alter table CUST_ProductLineMap add MaxOrderCount int
go

alter table CUST_ProductLineMap add InitVanOrder varchar(50)
go

alter table CUST_ProductLineMap add Plant varchar(50)
go

alter table CUST_ProductLineMap add IsActive bit
go



