IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_CRSL]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_CRSL]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[SAP_CRSL](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EINDT] [varchar](50) NULL,
	[FRBNR] [varchar](50) NULL,
	[LIFNR] [varchar](50) NULL,
	[MATNR] [varchar](50) NULL,
	[MENGE] [decimal](18, 8) NULL,
	[SGTXT] [varchar](50) NULL,
	[WERKS] [varchar](50) NULL,
	[EBELN] [varchar](50) NULL,
	[EBELP] [varchar](50) NULL,
	[ETENR] [varchar](50) NULL,
	[MESSAGE] [varchar](256) NULL,
	[OrderDetId] [int] NULL,
	[Status] [tinyint] NULL,
	[ErrorCount] [int] NULL,
	[CreateDate] [datetime] NULL,
	[LastModifyDate] [datetime] NULL,
 CONSTRAINT [PK_SAP_CRSL] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[SAP_CRSL] ADD  CONSTRAINT [DF_SAP_CRSL_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

alter table LE_FlowMstrSnapshot drop column MrpTotal
go
alter table LE_FlowMstrSnapshot drop column MrpTotalAdj
go
alter table LE_FlowMstrSnapshot drop column MrpWeight
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_SeqOrderTrace]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_SeqOrderTrace]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ORD_SeqOrderTrace](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TraceCode] [varchar](50) NULL,
	[ProdLine] [varchar](50) NULL,
	[SeqGroup] [varchar](50) NULL,
	[DeliveryCount] [varchar](10) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_ORD_SeqOrderTrace] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[ORD_SeqOrderTrace] ADD  CONSTRAINT [DF_ORD_SeqOrderTrace_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_SeqOrderTrace] ON [dbo].[ORD_SeqOrderTrace] 
(
	[TraceCode] ASC,
	[SeqGroup] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

alter table LE_OrderBomCPTimeSnapshot add TraceCode varchar(50)
go

alter table SCM_FlowStrategy add SafeTime decimal(18, 8)
go

alter table LE_OrderBomCPTimeSnapshot add ZOPWZ varchar(50)
go

alter table LE_OrderBomCPTimeSnapshot add ZOPID varchar(50)
go

alter table LE_OrderBomCPTimeSnapshot add ZOPDS varchar(50)
go