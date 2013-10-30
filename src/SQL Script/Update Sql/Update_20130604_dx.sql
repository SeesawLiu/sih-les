update SYS_Menu set Name = 'ÅÅÐò×é', Desc1 = 'ÅÅÐò×é' where Code = 'Url_SequenceGroup_View'
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenSequenceOrder]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_GenSequenceOrder]
GO

CREATE TABLE [dbo].[LOG_GenSequenceOrder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SeqGroup] [varchar](50) NULL,
	[Flow] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[Lvl] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_GenSequenceOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[LOG_GenSequenceOrder] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenOrder4VanProdLine]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_GenOrder4VanProdLine]
GO

CREATE TABLE [dbo].[LOG_GenOrder4VanProdLine](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[Lvl] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_GenOrder4VanProdLine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[LOG_GenOrder4VanProdLine] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

alter table SCM_FlowBinding add LeadTime decimal(18, 8)
go

