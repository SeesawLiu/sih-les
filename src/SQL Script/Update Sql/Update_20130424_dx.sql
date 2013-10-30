IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SCM_SeqGroup]') AND type in (N'U'))
DROP TABLE [dbo].[SCM_SeqGroup]
GO

CREATE TABLE [dbo].[SCM_SeqGroup](
	[Code] [varchar](50) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[SeqBatch] [int] NOT NULL,
	[PrevOrderNo] [varchar](50) NULL,
	[PrevTraceCode] [varchar](50) NULL,
	[PrevSeq] [bigint] NULL,
	[PrevSubSeq] [int] NULL,
	[PrevDeliveryDate] [date] NULL,
	[PrevDeliveryCount] [int] NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Version] [int] NOT NULL
 CONSTRAINT [PK_SCM_SEQGROUP] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

alter table ORD_OrderBomCPTime alter column Seq bigint
go
alter table ORD_OrderBomCPTime alter column OrderQty decimal(18, 8)
go

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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SYS_MsgSubscirber]') AND type in (N'U'))
DROP TABLE [dbo].[SYS_MsgSubscirber]
GO

CREATE TABLE [dbo].[SYS_MsgSubscirber](
	[Id] [int] NOT NULL,
	[Descritpion] [nvarchar](100) NOT NULL,
	[Emails] [varchar](500) NULL,
	[Mobiles] [varchar](500) NULL,
 CONSTRAINT [PK_SYS_MsgSubscirber] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

alter table MD_Item alter column ItemCategory varchar(50) null
go

 
delete from SYS_EntityPreference where Id in (1111, 10013, 10014, 10015)
go

insert into SYS_EntityPreference(Id, Seq, Value, Desc1, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate)
values(11005, 104, '10.86.128.31', 'SAPServiceAddress', 2603, '用户 超级', '2012-01-01 00:00:00.000', 2603, '用户 超级', '2012-01-01 00:00:00.000')
,(11006, 105, '8000', 'SAPServicePort', 2603, '用户 超级', '2012-01-01 00:00:00.000', 2603, '用户 超级', '2012-01-01 00:00:00.000')
,(11007, 106, '10000', 'SAPTransSave2TempTableBatchSize', 2603, '用户 超级', '2012-01-01 00:00:00.000', 2603, '用户 超级', '2012-01-01 00:00:00.000')
,(11008, 107, '200', 'SAPTransPost2SAPBatchSize', 2603, '用户 超级', '2012-01-01 00:00:00.000', 2603, '用户 超级', '2012-01-01 00:00:00.000')
,(11009, 108, '10', 'SAPDataExchangeMaxFailCount', 2603, '用户 超级', '2012-01-01 00:00:00.000', 2603, '用户 超级', '2012-01-01 00:00:00.000')
go

update SYS_EntityPreference set Value = 'dms_wangjun' where Id = 11001
update SYS_EntityPreference set Value = '12345678' where Id = 11002
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SI_SAP_CancelProdSeqReport]') AND type in (N'U'))
DROP TABLE [dbo].[SI_SAP_CancelProdSeqReport]
GO

CREATE TABLE [dbo].[SAP_CancelProdSeqReport](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AUFNR] [varchar](50) NOT NULL,
	[TEXT] [varchar](50) NOT NULL,
	[Status] [tinyint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[ErrorCount] [int] NOT NULL,
 CONSTRAINT [PK_SAP_CancelProdSeqReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

