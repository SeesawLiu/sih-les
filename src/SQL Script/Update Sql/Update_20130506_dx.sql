alter table LE_FlowDetSnapShot add SafeStock decimal(18, 8)
go
alter table LE_FlowDetSnapShot add MaxStock decimal(18, 8)
go
alter table LE_FlowDetSnapShot add MinLotSize decimal(18, 8)
go
alter table LE_FlowDetSnapShot add RoundUpOpt tinyint
go
alter table LE_FlowDetSnapShot add Strategy tinyint
go

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


