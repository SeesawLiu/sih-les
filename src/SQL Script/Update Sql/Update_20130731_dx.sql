IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_SeqOrderChange]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_SeqOrderChange]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[LOG_SeqOrderChange](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderDetId] [int] NULL,
	[OrderNo] [varchar](50) NULL,
	[Flow] [varchar](50) NULL,
	[Seq] [int] NULL,
	[ExtNo] [varchar](50) NULL,
	[ExtSeq] [varchar](50) NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[Uom] [varchar](5) NULL,
	[ManufactureParty] [varchar](50) NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[BinTo] [varchar](50) NULL,
	[StartDate] [datetime] NULL,
	[LocFrom] [varchar](50) NULL,
	[LocFromNm] [varchar](100) NULL,
	[LocTo] [varchar](50) NULL,
	[LocToNm] [varchar](100) NULL,
	[Status] [tinyint] NULL,
	[CreateDate] [datetime] NULL,
	[CreateUserId] [int] NULL,
	[CreateUserNm] [varchar](50) NULL,
 CONSTRAINT [PK_LOG_SeqOrderChange] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[LOG_SeqOrderChange] ADD  CONSTRAINT [DF_LOG_SeqOrderChange_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_OpRefBalanceChange]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_OpRefBalanceChange]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[LOG_OpRefBalanceChange](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Item] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[Qty] [decimal](18, 8) NULL,
	[Status] [tinyint] NULL,
	[Version] [int] NULL,
	[CreateDate] [datetime] NULL,
	[CreateUserId] [int] NULL,
	[CreateUserNm] [varchar](100) NULL,
 CONSTRAINT [PK_Log_OpRefBalanceChange] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[LOG_OpRefBalanceChange] ADD  CONSTRAINT [DF_Log_OpRefBalanceChange_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[SCM_OpRefBalance]') AND name = N'IX_OpRefBalance')
DROP INDEX [IX_OpRefBalance] ON [dbo].[SCM_OpRefBalance] WITH ( ONLINE = OFF )
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_OpRefBalance] ON [dbo].[SCM_OpRefBalance] 
(
	[Item] ASC,
	[OpRef] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

alter table BIL_PlanBill alter column OrderNo varchar(50) null
go
alter table BIL_ActBill alter column OrderNo varchar(50) null
go
alter table BIL_BillTrans alter column OrderNo varchar(50) null
go
