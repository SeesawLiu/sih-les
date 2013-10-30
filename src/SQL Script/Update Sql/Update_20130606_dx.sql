alter table LE_OrderBomCPTimeSnapshot add WorkCenter varchar(50) null
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_OrderOpReport]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_OrderOpReport]
GO

CREATE TABLE [dbo].[ORD_OrderOpReport](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
	[OrderDetId] [int] NOT NULL,
	[OrderOpId] [int] NOT NULL,
	[Op] [int] NOT NULL,
	[ReportQty] [decimal](18, 8) NOT NULL,
	[ScrapQty] [decimal](18, 8) NOT NULL,
	[BackflushQty] [decimal](18, 8) NOT NULL,
	[WorkCenter] [varchar](50) NULL,
	[Status] [tinyint] NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ReceiptNo] [varchar](50) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[CancelDate] [datetime] NULL,
	[CancelUser] [int] NULL,
	[CancelUserNm] [varchar](100) NULL,
	[Version] [int] NOT NULL,
 CONSTRAINT [PK_ORD_OrderOpReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

alter table SAP_ProdOpReport add OrderOpReportId int
go

insert into SYS_CodeMstr(Code, Desc1, [Type]) values('OrderOpReportStatus', N'¹¤Ðò±¨¹¤×´Ì¬', 0)
go
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('OrderOpReportStatus', 0, 'CodeDetail_OrderOpReportStatus_Close', 1, 1)
go
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('OrderOpReportStatus', 1, 'CodeDetail_OrderOpReportStatus_Cancel', 0, 2)
go

alter table ORD_OrderBackflushDet drop column ProdSeqReportId
go
alter table ORD_OrderBackflushDet add OrderOpReportId int
go
