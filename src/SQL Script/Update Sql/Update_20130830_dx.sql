IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PRD_LotNoTrace]') AND type in (N'U'))
DROP TABLE [dbo].[PRD_LotNoTrace]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[PRD_LotNoTrace](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[OpRef] [varchar](50) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[LotNo] [varchar](50) NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
	[TraceCode] [varchar](50) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_PRD_LotNoTrace] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PRD_LotNoScan]') AND type in (N'U'))
DROP TABLE [dbo].[PRD_LotNoScan]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[PRD_LotNoScan](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[OpRef] [varchar](50) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[LotNo] [varchar](50) NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
	[TraceCode] [varchar](50) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_PRD_LotNoScan] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO




