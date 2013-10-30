alter table SAP_ProdOpReport add EffDate datetime
go
update SAP_ProdOpReport set EffDate = GETDATE()
go
alter table SAP_ProdOpReport alter column EffDate datetime not null
go
alter table SAP_ProdOpReport add CreateUser int
go
alter table SAP_ProdOpReport add CreateUserNm varchar(50)
go
alter table SAP_ProdOpReport add LastModifyUser int
go
alter table SAP_ProdOpReport add LastModifyUserNm varchar(50)
go
alter table SAP_ProdOpReport add [Version] int
go
update SAP_ProdOpReport set [Version] = 1
go
alter table SAP_ProdOpReport alter column [Version] int not null
go
alter table SAP_ProdOpReport add ProdLine varchar(50)
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SI_SAP_Item]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_Item]
GO

CREATE TABLE [dbo].[SAP_Item](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](20) NULL,
	[ReferenceCode] [varchar](20) NULL,
	[Description] [nvarchar](60) NULL,
	[Uom] [varchar](5) NULL,
	[Plant] [varchar](4) NULL,
	[IOStatus] [int] NOT NULL,
	[InboundDate] [datetime] NOT NULL,
	[OutboundDate] [datetime] NULL,
	[ShortCode] [varchar](10) NULL,
 CONSTRAINT [PK_SAP_Item] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SI_SAP_Supplier]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_Supplier]
GO

CREATE TABLE [dbo].[SAP_Supplier](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](20) NULL,
	[OldSupplierCode] [varchar](20) NULL,
	[Name] [nvarchar](60) NULL,
	[IOStatus] [int] NOT NULL,
	[InboundDate] [datetime] NULL,
	[OutBoundDate] [datetime] NULL,
 CONSTRAINT [PK_SAP_Supplier] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SI_SAP_MapMoveTypeTCode]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_MapMoveTypeTCode]
GO

CREATE TABLE [dbo].[SAP_MapMoveTypeTCode](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BWART] [char](3) NOT NULL,
	[SOBKZ] [char](1) NULL,
	[TCode] [varchar](10) NOT NULL,
	[Description] [varchar](100) NULL,
 CONSTRAINT [PK_SAP_MapMoveTypeTCode] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SI_SAP_AlterDO]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_AlterDO]
GO

CREATE TABLE [dbo].[SAP_AlterDO](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](10) NULL,
	[Sequence] [int] NULL,
	[Item] [varchar](18) NULL,
	[Plant] [char](4) NULL,
	[Location] [char](4) NULL,
	[Qty] [decimal](18, 8) NULL,
	[Uom] [char](3) NULL,
	[KUNAG] [varchar](20) NULL,
	[KUNNR] [varchar](20) NULL,
	[Action] [char](1) NULL,
	[Status] [tinyint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[WindowTime] [datetime] NULL,
	[ExternalOrderno] [varchar](50) NULL,
 CONSTRAINT [PK_SAP_DeliveryOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SI_SAP_PostDO]') AND type in (N'U'))
DROP TABLE [dbo].[SI_SAP_PostDO]
GO

CREATE TABLE [dbo].[SI_SAP_PostDO](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](50) NULL,
	[ReceiptNo] [varchar](50) NULL,
	[Result] [varchar](500) NULL,
	[ZTCODE] [varchar](50) NULL,
	[Success] [varchar](50) NULL,
	[LastModifyDate] [datetime] NULL,
	[Status] [int] NULL,
	[CreateDate] [datetime] NULL,
	[ErrorCount] [int] NULL,
 CONSTRAINT [PK_SI_SAP_PostDO] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SI_SAP_Quota]') AND type in (N'U'))
DROP TABLE [dbo].[SI_SAP_Quota]
GO

CREATE TABLE [dbo].[SI_SAP_Quota](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](20) NOT NULL,
	[Sequence] [varchar](20) NULL,
	[Supplier] [varchar](20) NULL,
	[Plant] [varchar](4) NULL,
	[PlantFrom] [varchar](4) NULL,
	[Item] [varchar](20) NOT NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[PoType] [char](1) NULL,
	[SubPoType] [char](1) NULL,
	[Weight] [decimal](18, 8) NULL,
	[Status] [tinyint] NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NULL,
	[ErrorCount] [int] NOT NULL,
 CONSTRAINT [PK_SI_SAP_Quota] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SI_SAP_SourceOrder]') AND type in (N'U'))
DROP TABLE [dbo].[SI_SAP_SourceOrder]
GO

CREATE TABLE [dbo].[SI_SAP_SourceOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](50) NULL,
	[Sequence] [varchar](50) NULL,
	[Item] [varchar](50) NULL,
	[Plant] [varchar](50) NULL,
	[ZEORD] [varchar](50) NULL,
	[Supplier] [varchar](50) NULL,
	[PlantFrom] [varchar](50) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[Status] [tinyint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[Uom] [varchar](50) NULL,
	[BaseUom] [varchar](50) NULL,
	[UomQty] [decimal](18, 8) NULL,
	[BaseUomQty] [decimal](18, 8) NULL,
 CONSTRAINT [PK_SI_SAP_Source] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SI_SAP_InvLoc]') AND type in (N'U'))
DROP TABLE [dbo].[SI_SAP_InvLoc]
GO

CREATE TABLE [dbo].[SI_SAP_InvLoc](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SourceType] [int] NOT NULL,
	[SourceId] [bigint] NOT NULL,
	[FRBNR] [varchar](16) NOT NULL,
	[SGTXT] [varchar](50) NOT NULL,
	[CreateUser] [varchar](50) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_InvLoc] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SI_SAP_InvLoc] ADD  CONSTRAINT [DF_SI_SAP_InvLoc_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SI_SAP_InvTrans]') AND type in (N'U'))
DROP TABLE [dbo].[SI_SAP_InvTrans]
GO

CREATE TABLE [dbo].[SI_SAP_InvTrans](
	[TCODE] [varchar](20) NULL,
	[BWART] [varchar](3) NULL,
	[BLDAT] [char](8) NULL,
	[BUDAT] [char](8) NULL,
	[EBELN] [varchar](50) NULL,
	[EBELP] [varchar](5) NULL,
	[VBELN] [varchar](10) NULL,
	[POSNR] [varchar](6) NULL,
	[LIFNR] [varchar](10) NULL,
	[WERKS] [varchar](4) NULL,
	[LGORT] [varchar](4) NULL,
	[SOBKZ] [varchar](1) NULL,
	[MATNR] [varchar](18) NULL,
	[ERFMG] [decimal](18, 2) NULL,
	[ERFME] [varchar](3) NULL,
	[UMLGO] [varchar](4) NULL,
	[GRUND] [varchar](4) NULL,
	[KOSTL] [varchar](10) NULL,
	[XBLNR] [varchar](50) NULL,
	[RSNUM] [varchar](10) NULL,
	[RSPOS] [varchar](4) NULL,
	[FRBNR] [varchar](50) NOT NULL,
	[SGTXT] [varchar](50) NOT NULL,
	[OLD] [varchar](3) NULL,
	[INSMK] [varchar](1) NULL,
	[XABLN] [varchar](10) NULL,
	[AUFNR] [varchar](50) NULL,
	[UMMAT] [varchar](18) NULL,
	[UMWRK] [varchar](4) NULL,
	[POSID] [varchar](24) NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Status] [tinyint] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[BatchNo] [int] NOT NULL,
	[CHARG] [varchar](50) NULL,
	[KZEAR] [varchar](1) NULL,
	[ErrorId] [int] NULL,
	[ErrorMessage] [varchar](256) NULL,
	[OrderNo] [varchar](50) NULL,
	[DetailId] [int] NULL,
 CONSTRAINT [PK_InvTrans] PRIMARY KEY CLUSTERED 
(
	[FRBNR] ASC,
	[SGTXT] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SI_SAP_InvTrans] ADD  CONSTRAINT [DF_SI_SAP_InvTrans_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SI_SAP_TransCallBack]') AND type in (N'U'))
DROP TABLE [dbo].[SI_SAP_TransCallBack]
GO

CREATE TABLE [dbo].[SI_SAP_TransCallBack](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[FRBNR] [varchar](16) NULL,
	[SGTXT] [varchar](50) NULL,
	[MBLNR] [varchar](10) NULL,
	[ZEILE] [varchar](4) NULL,
	[BUDAT] [char](14) NULL,
	[CPUDT] [char](14) NULL,
	[MTYPE] [varchar](4) NULL,
	[MSTXT] [nvarchar](220) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_TransCallBack] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SI_SAP_TransCallBack] ADD  CONSTRAINT [DF_SI_SAP_TransCallBack_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
