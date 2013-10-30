-----------------------------------------KB_KanbanCalc
CREATE TABLE [dbo].[KB_KanbanCalc](
	[Id] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[BatchNo] [varchar](200) NOT NULL,
	[Flow] [varchar](50) NULL,
	[FlowDetId] [int] NULL,
	[Region] [varchar](50) NOT NULL,
	[RegionName] [varchar](100) NOT NULL,
	[LCCode] [varchar](50) NULL,
	[Item] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[Supplier] [varchar](50) NULL,
	[SupplierName] [varchar](100) NULL,
	[Qty] [decimal](18, 8) NOT NULL,
	[Container] [varchar](50) NULL,
	[LocBin] [varchar](50) NULL,
	[Shelf] [varchar](50) NULL,
	[QiTiaoBian] [varchar](50) NULL,
	[Ret] [tinyint] NOT NULL,
	[Msg] [varchar](500) NULL,
	[CardNo] [varchar](50) NULL,
	[Seq] [varchar](50) NULL,
	[EffDate] [datetime] NULL,
	[FreezeDate] [datetime] NULL,
	[LastUseDate] [datetime] NULL,
	[PONo] [varchar](50) NULL,
	[POLineNo] [varchar](50) NULL,
	[Status] [tinyint] NOT NULL,
	[MultiSupplyGroup] [varchar](50) NULL,
	[OpTime] [datetime] NULL,
	[KanbanDeltaNum] [int] NOT NULL,
	[KanbanNum] [int] NOT NULL,
	[NeedReprint] [bit] NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[CalcKanbanNum] [decimal](18, 2) NULL,
	[CalcConsumeDate] [datetime] NULL,
	[CalcConsumeQty] [decimal](18, 8) NULL,
	[SafeKanbanNum] [decimal](18, 8) NULL,
	[BatchKanbanNum] [decimal](18, 8) NULL,
	[Location] [varchar](50) NULL,
	[opref] [varchar](50) NULL,
	[IsKit] [bit] NULL,
	[KBCalc] [tinyint] NULL,
	[LCName] [varchar](50) NULL,
	[Tiao] [int] NULL,
	[Bian] [int] NULL,
	[IsTrace] [bit] NULL,
	[GroupNo] [varchar](50) NULL,
	[GroupDesc] [varchar](50) NULL,
	[kitCount] [int] NULL,
 CONSTRAINT [PK_KB_KanbanCalc] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[KB_KanbanCalc] ADD  DEFAULT ((0)) FOR [kitCount]
GO
---------------------------------------------------------------KB_KanbanCard
go
CREATE TABLE [dbo].[KB_KanbanCard](
	[CardNo] [varchar](50) NOT NULL,
	[Seq] [varchar](50) NOT NULL,
	[Flow] [varchar](50) NOT NULL,
	[FlowDetId] [int] NOT NULL,
	[Region] [varchar](50) NOT NULL,
	[RegionName] [varchar](100) NOT NULL,
	[LCCode] [varchar](50) NULL,
	[Supplier] [varchar](50) NOT NULL,
	[SupplierName] [varchar](100) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[ItemDesc] [varchar](100) NOT NULL,
	[PONo] [varchar](50) NULL,
	[POLineNo] [varchar](50) NULL,
	[Status] [tinyint] NOT NULL,
	[MultiSupplyGroup] [varchar](50) NULL,
	[EffDate] [datetime] NOT NULL,
	[FreezeDate] [datetime] NOT NULL,
	[LastUseDate] [datetime] NOT NULL,
	[NeedReprint] [bit] NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Qty] [decimal](18, 8) NULL,
	[Container] [varchar](50) NULL,
	[LocBin] [varchar](50) NULL,
	[Shelf] [varchar](50) NULL,
	[QiTiaoBian] [varchar](50) NULL,
	[TotalCount] [int] NULL,
	[Location] [varchar](50) NULL,
	[opref] [varchar](50) NULL,
	[IsKit] [bit] NULL,
	[KBCalc] [tinyint] NULL,
	[LCName] [varchar](50) NULL,
	[IsTrace] [bit] NULL,
	[GroupNo] [varchar](50) NULL,
	[GroupDesc] [varchar](50) NULL,
	[Number] [int] NULL,
	[kitCount] [int] NULL,
	[IsLost] [bit] NULL,
	[LostCount] [int] NULL,
 CONSTRAINT [PK_KB_KanbanCard] PRIMARY KEY CLUSTERED 
(
	[CardNo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[KB_KanbanCard] ADD  DEFAULT ((0)) FOR [Number]
GO

ALTER TABLE [dbo].[KB_KanbanCard] ADD  DEFAULT ((0)) FOR [kitCount]
GO

ALTER TABLE [dbo].[KB_KanbanCard] ADD  DEFAULT ((0)) FOR [IsLost]
GO

ALTER TABLE [dbo].[KB_KanbanCard] ADD  DEFAULT ((0)) FOR [LostCount]
GO
-----------------------------------------KB_KanbanLost

CREATE TABLE [dbo].[KB_KanbanLost](
	[Id] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[CardNo] [varchar](50) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO
--------------------------------------KB_KanbanScan

CREATE TABLE [dbo].[KB_KanbanScan](
	[Id] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[CardNo] [varchar](50) NOT NULL,
	[Seq] [varchar](50) NOT NULL,
	[Flow] [varchar](50) NULL,
	[FlowDetId] [int] NULL,
	[LCCode] [varchar](50) NULL,
	[Region] [varchar](50) NOT NULL,
	[RegionName] [varchar](100) NOT NULL,
	[Supplier] [varchar](50) NOT NULL,
	[SupplierName] [varchar](100) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[ItemDesc] [varchar](100) NOT NULL,
	[PONo] [varchar](50) NULL,
	[POLineNo] [varchar](50) NULL,
	[ScanUser] [int] NOT NULL,
	[ScanUserNm] [varchar](100) NOT NULL,
	[ScanTime] [datetime] NOT NULL,
	[ScanQty] [decimal](18, 8) NOT NULL,
	[IsOrdered] [bit] NULL,
	[OrderUser] [int] NULL,
	[OrderUserNm] [varchar](100) NULL,
	[OrderTime] [datetime] NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[OrderNo] [varchar](50) NULL,
	[IsKit] [bit] NULL,
	[KBCalc] [tinyint] NULL,
	[TempKanbanCard] [varchar](50) NULL,
 CONSTRAINT [PK_KB_KanbanScan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

----------------------------------KB_KanbanTrans

CREATE TABLE [dbo].[KB_KanbanTrans](
	[Id] [bigint] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[CardNo] [varchar](50) NOT NULL,
	[TransType] [tinyint] NOT NULL,
	[TransDate] [datetime] NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
 CONSTRAINT [PK_KB_KanbanTrans] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO
  go
  insert into SYS_CodeMstr values('KBCalculation','ø¥∞Â¿‡–Õ',0)
  go
  insert into SYS_CodeDet values('KBCalculation','0','CodeDetail_KBCalculation_Normal',1,1)
  go
  insert into SYS_CodeDet values('KBCalculation','1','CodeDetail_KBCalculation_CatItem',0,2)
go
update SYS_Menu set Seq=300 where Code='Url_SI_SAP'
go
update SYS_Menu set IsActive =0 where Parent='Url_SI_SAP' and Code <> 'Url_SI_SAP_InvTrans_View'


update SYS_Menu set Seq=300 where Code='Url_SI_SAP'
go
update SYS_Menu set IsActive =0 where Parent like 'Url_SI_%'
go
update SYS_Menu set IsActive =1 where Code  in( 'Url_SI_SAP_InvTrans_View'
,'Url_SI_SAP_CancelProdOpReport_View','Url_SI_SAP_ProdOpReport_View')
go
update SYS_Menu set Code='Url_SI_SAP_CancelProdOpReport_View',PageUrl='~/CancelProdOpReport/Index' where Code='Url_SI_SAP_CancelProdSeqReport_View'
go
update SYS_Menu set Code='Url_SI_SAP_ProdOpReport_View',PageUrl='~/SAPProdOpReport/Index' where Code='Url_SI_SAP_ProdSeqReport_View'
go
update ACC_Permission set Code='Url_SI_SAP_CancelProdOpReport_View' where Code='Url_SI_SAP_CancelProdSeqReport_View'
go
update ACC_Permission set code='Url_SI_SAP_ProdOpReport_View' where Code='Url_SI_SAP_ProdSeqReport_View'










