IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenJITOrder]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_GenJITOrder]
GO

CREATE TABLE [dbo].[LOG_GenJITOrder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[Lvl] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_GenJITOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_GenJITOrder] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenKBOrder]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_GenKBOrder]
GO

CREATE TABLE [dbo].[LOG_GenKBOrder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[Lvl] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_GenKBOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_GenKBOrder] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenProductOrder]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_GenProductOrder]
GO

CREATE TABLE [dbo].[LOG_GenProductOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BatchNo] [int] NULL,
	[Lvl] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_LOG_GenProductOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_GenProductOrder] ADD  CONSTRAINT [DF_LOG_GenProductOrder_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO
