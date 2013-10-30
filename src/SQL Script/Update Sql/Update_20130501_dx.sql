IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenSequenceOrder]') AND type in (N'U'))
	DROP TABLE [dbo].[LOG_GenSequenceOrder]
GO

CREATE TABLE [dbo].[LOG_GenSequenceOrder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SeqGroup] [varchar](50) NULL,
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

alter table SCM_SeqGroup add OpRef varchar(50) not null
go

alter table ORD_OrderBomCPTime drop column IsPause
go

alter table ORD_OrderBomCPTime add ManufactureParty varchar(50) null
go
