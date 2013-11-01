alter table SAP_TableIndex add Version int default(1)
go
update SAP_TableIndex set Version = 1
go
alter table SAP_InvTrans add Version int default(1)
go
update SAP_InvTrans set Version = 1
go
alter table SAP_InvLoc add BWART varchar(3)
go
alter table MD_Location add MergeLocLotDet bit default(1)
go
update MD_Location set MergeLocLotDet = 1
go
update MD_Location set MergeLocLotDet = 0 where Code in ('2800','2309')
go
CREATE Type InvTransType as Table (
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
	[OrderNo] [varchar](50) NULL,
	[DetailId] [int] NULL
)
go
CREATE Type [InvLocType] as table (
	[SourceType] [int] NOT NULL,
	[SourceId] [bigint] NOT NULL,
	[FRBNR] [varchar](16) NOT NULL,
	[SGTXT] [varchar](50) NOT NULL,
	[CreateUser] [varchar](50) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[BWART] [varchar](3) NULL
)
go