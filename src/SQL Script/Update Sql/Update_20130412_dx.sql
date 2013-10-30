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




