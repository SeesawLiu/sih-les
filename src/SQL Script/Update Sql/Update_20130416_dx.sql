drop table ORD_OrderItemTrace
go
CREATE TABLE [dbo].[ORD_OrderItemTrace](
	[Id] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
	[OrderBomId] [int] NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[ItemDesc] [varchar](100) NULL,
	[RefItemCode] [varchar](50) NULL,
	[Op] [int] NOT NULL,
	[OpRef] [varchar](50) NOT NULL,
	[Qty] [decimal](18, 8) NOT NULL,
	[ScanQty] [decimal](18, 8) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Version] [int] NOT NULL,
 CONSTRAINT [PK_ORD_ORDERITEMTRACE] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

drop table ORD_OrderItemTraceResult
go
CREATE TABLE [dbo].[ORD_OrderItemTraceResult](
	[Id] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[OrderItemTraceId] [int] NULL,
	[BarCode] [varchar](50) NOT NULL,
	[Supplier] [varchar](50) NULL,
	[LotNo] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[Item] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[RefItemCode] [varchar](50) NULL,
	[BomId] [int] NULL,
	[OrderNo] [varchar](50) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL
 CONSTRAINT [PK_ORD_ORDERITEMTRACERESULT] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
