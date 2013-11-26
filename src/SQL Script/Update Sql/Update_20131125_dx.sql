IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_OpRefMap]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_OpRefMap]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[CUST_OpRefMap](
	[Id] [int] NOT NULL Identity(1, 1),
	[SAPProdLine] [varchar](50) NULL,
	[ProdLine] [varchar](50) NULL,
	[Item] [varchar](50) NULL,
	[ItemDesc] [varchar](50) NULL,
	[ItemRefCode] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[RefOpRef] [varchar](256) NULL,
	[IsPrimary] [bit] NULL,
	[CreateUser] [int] NULL,
	[CreateUserNm] [varchar](100) NULL,
	[CreateDate] [datetime] NULL,
	[LastModifyUser] [int] NULL,
	[LastModifyUserNm] [varchar](100) NULL,
	[LastModifyDate] [datetime] NULL,
 CONSTRAINT [PK_CUST_OpRefMap] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CUST_OpRefMap]') AND name = N'OpRefMap_ProdLine_Item')
DROP INDEX [OpRefMap_ProdLine_Item] ON [dbo].[CUST_OpRefMap] WITH ( ONLINE = OFF )
GO

CREATE UNIQUE NONCLUSTERED INDEX [OpRefMap_ProdLine_Item] ON [dbo].[CUST_OpRefMap] 
(
	[ProdLine] ASC,
	[Item] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO


alter table ORD_OrderBomDet add RefOpRef varchar(256)
GO

alter table LE_OrderBomCPTimeSnapshot add RefOpRef varchar(256)
GO

