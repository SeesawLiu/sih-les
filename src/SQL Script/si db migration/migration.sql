IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_MapMoveTypeTCode]') AND type in (N'U'))
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


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_Item]') AND type in (N'U'))
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_Supplier]') AND type in (N'U'))
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_ProdOpReport]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_ProdOpReport]
GO

CREATE TABLE [dbo].[SAP_ProdOpReport](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AUFNR] [varchar](50) NOT NULL,
	[WORKCENTER] [varchar](50) NOT NULL,
	[GAMNG] [decimal](18, 8) NOT NULL,
	[Status] [tinyint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[SCRAP] [decimal](18, 8) NOT NULL,
	[TEXT] [varchar](50) NULL,
	[IsCancel] [bit] NOT NULL,
	[OrderNo] [varchar](50) NULL,
	[OrderOpId] [int] NULL,
	[RecNo] [varchar](50) NULL,
	[EffDate] [datetime] NOT NULL,
	[CreateUser] [int] NULL,
	[CreateUserNm] [varchar](50) NULL,
	[LastModifyUser] [int] NULL,
	[LastModifyUserNm] [varchar](50) NULL,
	[Version] [int] NOT NULL,
	[ProdLine] [varchar](50) NULL
CONSTRAINT [PK_SAP_ProdOpReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SAP_ProdOpReport] ADD  CONSTRAINT [DF_SAP_ProdOpReport_Status]  DEFAULT ((0)) FOR [Status]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_CancelProdOpReport]') AND type in (N'U'))
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_TableIndex]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_TableIndex]
GO

CREATE TABLE [dbo].[SAP_TableIndex](
	[TableName] [varchar](50) NOT NULL,
	[Id] [bigint] NULL,
	[LastModifyDate] [datetime] NULL,
 CONSTRAINT [PK_TableIndex] PRIMARY KEY CLUSTERED 
(
	[TableName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_ProdOrder]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_ProdOrder]
GO

CREATE TABLE [dbo].[SAP_ProdOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BatchNo] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[AUFNR] [varchar](50) NULL,
	[WERKS] [varchar](50) NULL,
	[DAUAT] [varchar](50) NULL,
	[MATNR] [varchar](50) NULL,
	[MAKTX] [varchar](50) NULL,
	[DISPO] [varchar](50) NULL,
	[CHARG] [varchar](50) NULL,
	[GSTRS] [datetime] NULL,
	[CY_SEQNR] [bigint] NULL,
	[GMEIN] [varchar](50) NULL,
	[GAMNG] [decimal](18, 8) NULL,
	[LGORT] [varchar](50) NULL,
	[LTEXT] [varchar](50) NULL,
	[ZLINE] [varchar](50) NULL,
	[RSNUM] [int] NULL,
	[AUFPL] [int] NULL,
 CONSTRAINT [PK_SAP_PRODORDER] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SAP_ProdOrder] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_ProdRoutingDet]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_ProdRoutingDet]
GO

CREATE TABLE [dbo].[SAP_ProdRoutingDet](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BatchNo] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[AUFNR] [varchar](50) NULL,
	[WERKS] [varchar](50) NULL,
	[AUFPL] [int] NULL,
	[APLZL] [int] NULL,
	[PLNTY] [varchar](50) NULL,
	[PLNNR] [varchar](50) NULL,
	[PLNAL] [varchar](50) NULL,
	[PLNFL] [varchar](50) NULL,
	[VORNR] [varchar](50) NULL,
	[ARBPL] [varchar](50) NULL,
	[RUEK] [varchar](50) NULL,
	[AUTWE] [varchar](50) NULL,
 CONSTRAINT [PK_SAP_PRODROUTINGDET] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[SAP_ProdRoutingDet] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_ProdBomDet]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_ProdBomDet]
GO

CREATE TABLE [dbo].[SAP_ProdBomDet](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[BatchNo] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[AUFNR] [varchar](50) NULL,
	[RSNUM] [int] NULL,
	[RSPOS] [int] NULL,
	[WERKS] [varchar](50) NULL,
	[MATERIAL] [varchar](50) NULL,
	[BISMT] [varchar](50) NULL,
	[MAKTX] [varchar](100) NULL,
	[DISPO] [varchar](50) NULL,
	[BESKZ] [varchar](50) NULL,
	[SOBSL] [varchar](50) NULL,
	[MEINS] [varchar](50) NULL,
	[MDMNG] [decimal](18, 8) NULL,
	[LGORT] [varchar](50) NULL,
	[BWART] [varchar](50) NULL,
	[AUFPL] [int] NULL,
	[PLNFL] [varchar](50) NULL,
	[VORNR] [varchar](50) NULL,
	[GW] [varchar](50) NULL,
	[WZ] [varchar](50) NULL,
	[ZOPID] [varchar](50) NULL,
	[ZOPDS] [varchar](50) NULL,
	[LIFNR] [varchar](50) NULL,
	[ICHARG] [varchar](50) NULL,
 CONSTRAINT [PK_SAP_PRODBOMDET] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SAP_ProdBomDet] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_AlterDO]') AND type in (N'U'))
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_PostDO]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_PostDO]
GO

CREATE TABLE [dbo].[SAP_PostDO](
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
 CONSTRAINT [PK_SAP_PostDO] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_Quota]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_Quota]
GO

CREATE TABLE [dbo].[SAP_Quota](
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
 CONSTRAINT [PK_SAP_Quota] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_SourceOrder]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_SourceOrder]
GO

CREATE TABLE [dbo].[SAP_SourceOrder](
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
 CONSTRAINT [PK_SAP_Source] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_InvLoc]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_InvLoc]
GO

CREATE TABLE [dbo].[SAP_InvLoc](
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

ALTER TABLE [dbo].[SAP_InvLoc] ADD  CONSTRAINT [DF_SAP_InvLoc_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_InvTrans]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_InvTrans]
GO

CREATE TABLE [dbo].[SAP_InvTrans](
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

ALTER TABLE [dbo].[SAP_InvTrans] ADD  CONSTRAINT [DF_SAP_InvTrans_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_TransCallBack]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_TransCallBack]
GO

CREATE TABLE [dbo].[SAP_TransCallBack](
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

ALTER TABLE [dbo].[SAP_TransCallBack] ADD  CONSTRAINT [DF_SAP_TransCallBack_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

truncate table SAP_CancelProdOpReport
go
truncate table SAP_ProdOpReport
go
truncate table SAP_TableIndex
go
truncate table SAP_MapMoveTypeTCode
go
truncate table SAP_InvLoc
go
truncate table SAP_InvTrans
go
truncate table SAP_TransCallBack
go
insert into SAP_CancelProdOpReport(AUFNR, [TEXT], [Status], CreateDate, LastModifyDate, ErrorCount) select AUFNR, [TEXT], [Status], CreateDate, LastModifyDate, ErrorCount from sconit5_si.dbo.SI_SAP_CancelProdSeqReport
go
insert into SAP_ProdOpReport(AUFNR, WORKCENTER, GAMNG, Status, CreateDate, LastModifyDate, EffDate, ErrorCount, SCRAP, [Text], IsCancel, [Version]) select AUFNR, WORKCENTER, GAMNG, Status, CreateDate, LastModifyDate, CreateDate, ErrorCount, SCRAP, [Text], 1, 1 from sconit5_si.dbo.SI_SAP_ProdSeqReport
go
insert into SAP_TableIndex(TableName, Id, LastModifyDate) select TableName, Id, LastModifyDate from sconit5_si.dbo.SI_SAP_TableIndex
go
insert into SAP_MapMoveTypeTCode(BWART, SOBKZ, TCode, Description) select BWART, SOBKZ, TCode, Description from sconit5_si.dbo.SI_SAP_MapMoveTypeTCode
go
set identity_insert SAP_InvLoc on
insert into SAP_InvLoc(Id, SourceType, SourceId, FRBNR, SGTXT, CreateUser, CreateDate) select Id, SourceType, SourceId, FRBNR, SGTXT, CreateUser, CreateDate from sconit5_si.dbo.SI_SAP_InvLoc
set identity_insert SAP_InvLoc off
go
INSERT INTO SAP_InvTrans (TCODE, BWART, BLDAT, BUDAT, EBELN, EBELP, VBELN, POSNR, LIFNR, WERKS, LGORT, SOBKZ, MATNR, ERFMG, ERFME, UMLGO, GRUND, KOSTL, XBLNR, RSNUM, RSPOS, FRBNR, SGTXT, OLD, INSMK, XABLN, AUFNR, UMMAT, UMWRK, POSID, CreateDate, LastModifyDate, Status, ErrorCount, BatchNo, CHARG, KZEAR, ErrorId, ErrorMessage, OrderNo, DetailId)
select TCODE, BWART, BLDAT, BUDAT, EBELN, EBELP, VBELN, POSNR, LIFNR, WERKS, LGORT, SOBKZ, MATNR, ERFMG, ERFME, UMLGO, GRUND, KOSTL, XBLNR, RSNUM, RSPOS, FRBNR, SGTXT, OLD, INSMK, XABLN, AUFNR, UMMAT, UMWRK, POSID, CreateDate, LastModifyDate, Status, ErrorCount, BatchNo, CHARG, KZEAR, ErrorId, ErrorMessage, OrderNo, DetailId from sconit5_si.dbo.SI_SAP_InvTrans
go
set identity_insert SAP_TransCallBack on
insert into SAP_TransCallBack(Id, FRBNR, SGTXT, MBLNR, ZEILE, BUDAT, CPUDT, MTYPE, MSTXT, CreateDate) select Id, FRBNR, SGTXT, MBLNR, ZEILE, BUDAT, CPUDT, MTYPE, MSTXT, CreateDate from sconit5_si.dbo.SI_SAP_TransCallBack
set identity_insert SAP_TransCallBack off
go

