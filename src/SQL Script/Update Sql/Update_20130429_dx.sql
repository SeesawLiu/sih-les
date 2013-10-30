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

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CUST_ProdLineLocationMap]') AND name = N'PK_CUST_ProdLineLocationMap')
ALTER TABLE [dbo].[CUST_ProdLineLocationMap] DROP CONSTRAINT [PK_CUST_ProdLineLocationMap]
GO

ALTER TABLE [dbo].[CUST_ProdLineLocationMap] ADD  CONSTRAINT [PK_CUST_ProdLineLocationMap] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

alter table ORD_OrderOp drop column SAPOp
go
alter table ORD_OrderOp add AUFPL varchar(50)
go
alter table ORD_OrderOp add PLNFL varchar(50)
go
alter table ORD_OrderOp add VORNR varchar(50)
go
alter table ORD_OrderOp add NeedReport bit
go
update ORD_OrderOp set NeedReport = 1
go
alter table ORD_OrderOp alter column NeedReport bit not null 
go
alter table ORD_OrderOp add IsRecFG bit
go
update ORD_OrderOp set IsRecFG = 0
go
alter table ORD_OrderOp alter column IsRecFG bit not null 
go
alter table SAP_ProdOpReport add RecNo varchar(50)
go
alter table ORD_OrderOp add ScrapQty decimal(18, 8)
go
update ORD_OrderOp set ScrapQty = 0
go
alter table ORD_OrderOp alter column ScrapQty decimal(18, 8) not null 
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_CancelProdSeqReport]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_CancelProdSeqReport]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_SAP_CancelProdOpReport]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_CancelProdOpReport]
GO

CREATE TABLE [dbo].[SAP_CancelProdOpReport](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AUFNR] [varchar](50) NOT NULL,
	[TEXT] [varchar](50) NOT NULL,
	[Status] [tinyint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[RecNo] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[OrderOpId] [int] NULL,
 CONSTRAINT [PK_SAP_CancelProdOpReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



