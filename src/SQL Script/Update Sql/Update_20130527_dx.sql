IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_ProdSeqReport]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_ProdSeqReport]
GO

CREATE TABLE [dbo].[SAP_ProdSeqReport](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AUFNR] [varchar](50) NOT NULL,
	[WORKCENTER] [varchar](50) NOT NULL,
	[GAMNG] [decimal](18, 8) NOT NULL,
	[Status] [tinyint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[SCRAP] [decimal](18, 8) NOT NULL,
	[TEXT] [varchar](50) NOT NULL
) ON [PRIMARY]
SET ANSI_PADDING OFF
ALTER TABLE [dbo].[SAP_ProdSeqReport] ADD  CONSTRAINT [PK_SAP_ProdSeqReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

alter table ORD_OrderBackflushDet add ProdSeqReportId int
go
alter table ORD_OrderBackflushDet add OrderOpId int
go
alter table ORD_OrderBackflushDet add WorkCenter varchar(50)
go
alter table ORD_OrderBackflushDet alter column RecNo varchar(50) null
go
alter table ORD_OrderBackflushDet alter column RecDetId int null
go
alter table ORD_OrderBackflushDet alter column RecDetSeq int null
go