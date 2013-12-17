alter table SAP_ProdOrder add VHVIN varchar(50)
go
alter table SAP_ProdOrder add ZENGINE varchar(50)
go
alter table SAP_ProdOrder add LTEXT1 varchar(50)
go
alter table SAP_ProdOrder add LTEXT2 varchar(50)
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_EngineTrace]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_EngineTrace]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[CUST_EngineTrace](
	[TraceCode] [varchar](50) NOT NULL,
	[VHVIN] [varchar](50) NULL,
	[ZENGINE] [varchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[CreateUser] [int] NULL,
	[CreateUserNm] [varchar](100) NULL,
	[LastModifyDate] [datetime] NULL,
	[LastModifyUser] [int] NULL,
	[LastModifyUserNm] [varchar](100) NULL,
 CONSTRAINT [PK_CUST_EngineTrace] PRIMARY KEY CLUSTERED 
(
	[TraceCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_EngineTraceDet]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_EngineTraceDet]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[CUST_EngineTraceDet](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TraceCode] [varchar](50) NULL,
	[VHVIN] [varchar](50) NULL,
	[ZENGINE] [varchar](50) NULL,
	[ScanZENGINE] [varchar](50) NULL,
	[CreateDate] [datetime] NULL,
	[CreateUser] [int] NULL,
	[CreateUserNm] [varchar](100) NULL,
 CONSTRAINT [PK_CUST_EngineTraceDet] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO
