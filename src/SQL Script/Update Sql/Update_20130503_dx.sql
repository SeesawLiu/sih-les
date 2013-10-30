IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_RunLeanEngine]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_RunLeanEngine]
GO

CREATE TABLE [dbo].[LOG_RunLeanEngine](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[Lvl] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_RunLeanEngine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_RunLeanEngine] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SCM_OpRefBalance]') AND type in (N'U'))
DROP TABLE [dbo].[SCM_OpRefBalance]
GO

CREATE TABLE [dbo].[SCM_OpRefBalance](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[OpRef] [varchar](50) NOT NULL,
	[Qty] [decimal](18, 8) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Version] [int] NULL,
 CONSTRAINT [PK_SCM_OPREFBALANCE] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

