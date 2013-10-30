IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SCM_Quota]') AND type in (N'U'))
DROP TABLE [dbo].[SCM_Quota]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[SCM_Quota](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Supplier] [varchar](50) NULL,
	[SupplierShortCode] [varchar](50) NULL,
	[SupplierNm] [varchar](100) NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[Weight] [decimal](18, 8) NULL,
	[Rate] [decimal](18, 8) NULL,
	[CycleQty] [decimal](18, 8) NULL,
	[AccumulateQty] [decimal](18, 8) NULL,
	[AdjQty] [decimal](18, 8) NULL,
	[IsActive] [bit] NULL,
	[CreateUser] [int] NULL,
	[CreateUserNm] [varchar](100) NULL,
	[CreateDate] [datetime] NULL,
	[LastModifyUser] [int] NULL,
	[LastModifyUserNm] [varchar](100) NULL,
	[LastModifyDate] [datetime] NULL,
	[Version] [int] NULL,
 CONSTRAINT [PK_SCM_Quota] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SCM_QuotaCycleQty]') AND type in (N'U'))
DROP TABLE [dbo].[SCM_QuotaCycleQty]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[SCM_QuotaCycleQty](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[CycleQty] [decimal](18, 8) NULL,
	[CreateUser] [int] NULL,
	[CreateUserNm] [varchar](100) NULL,
	[CreateDate] [datetime] NULL,
	[LastModifyUser] [int] NULL,
	[LastModifyUserNm] [varchar](100) NULL,
	[LastModifyDate] [datetime] NULL,
	[Version] [int] NULL,
 CONSTRAINT [PK_SCM_QuotaCycleQty] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SCM_Quota]') AND name = N'IX_SCM_Quota_Item_Supplier')
DROP INDEX [IX_SCM_Quota_Item_Supplier] ON [dbo].[SCM_Quota] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_SCM_Quota_Item_Supplier] ON [dbo].[SCM_Quota] 
(
	[Item] ASC,
	[Supplier] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SAP_Quota]') AND name = N'IX_SAP_Quota_1')
DROP INDEX [IX_SAP_Quota_1] ON [dbo].[SAP_Quota] WITH ( ONLINE = OFF )
GO

CREATE NONCLUSTERED INDEX [IX_SAP_Quota_1] ON [dbo].[SAP_Quota] 
(
	[MATNR] ASC,
	[LIFNR] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_QuotaSnapShot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_QuotaSnapShot]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[LE_QuotaSnapShot](
	[Item] [varchar](50) NOT NULL,
	[Supplier] [varchar](50) NULL,
 CONSTRAINT [PK_LE_QuotaSnapShot] PRIMARY KEY CLUSTERED 
(
	[Item] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

