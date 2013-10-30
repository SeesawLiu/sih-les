IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_CabOut]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_CabOut]
GO

CREATE TABLE [dbo].[CUST_CabOut](
	[OrderNo] [varchar](50) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[CabType] [tinyint] NOT NULL,
	[CabItem] [varchar](50) NOT NULL,
	[QulityBarcode] [varchar](50) NULL,
	[Status] [tinyint] NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[OutUser] [int] NULL,
	[OutUserNm] [varchar](100) NULL,
	[OutDate] [datetime] NULL,
	[TransferUser] [int] NULL,
	[TransferNm] [varchar](100) NULL,
	[TransferDate] [datetime] NULL,
 CONSTRAINT [PK_CUST_CabOut] PRIMARY KEY CLUSTERED 
(
	[OrderNo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

insert into SYS_CodeMstr(Code, Desc1, [Type]) values('CabType', '¼ÝÊ»ÊÒ×ÔÖÆ/Íâ¹ºÀàÐÍ', 0)
go
insert into SYS_CodeMstr(Code, Desc1, [Type]) values('CabOutStatus', '¼ÝÊ»ÊÒ³ö¿â×´Ì¬', 0)
go
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('CabType', 0, 'CodeDetail_CabType_SelfMade', 1, 1)
go
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('CabType', 1, 'CodeDetail_CabType_Purchase', 0, 2)
go
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('CabOutStatus', 0, 'CodeDetail_CabOutStatus_NotOut', 1, 1)
go
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('CabOutStatus', 1, 'CodeDetail_CabOutStatus_Out', 0, 2)
go
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('CabOutStatus', 2, 'CodeDetail_CabOutStatus_Transfer', 0, 3)
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Log_GenVanProdOrder]') AND type in (N'U'))
DROP TABLE [dbo].[Log_GenVanProdOrder]
GO

CREATE TABLE [dbo].[LOG_GenVanProdOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AUFNR] [varchar](50) NULL,
	[ZLINE] [varchar](50) NULL,
	[ProdLine] [varchar](50) NULL,
	[BatchNo] [varchar](50) NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_LOG_GenVanProdOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[LOG_GenVanProdOrder] ADD  CONSTRAINT [DF_LOG_GenVanProdOrder_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO


