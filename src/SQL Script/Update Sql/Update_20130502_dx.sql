IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_RunLeanEngine]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_RunLeanEngine]
GO

CREATE TABLE [dbo].[LOG_RunLeanEngine](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SeqGroup] [varchar](50) NULL,
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

ALTER TABLE ORD_OrderBomCPTime ADD Uom varchar(5)
go

ALTER TABLE SCM_FlowStrategy drop column WinTimeInternal
go

ALTER TABLE SCM_FlowStrategy add WinTimeInterval decimal(18, 8)
go

delete from SYS_CodeDet where Desc1 = 'CodeDetail_FlowStrategy_JIT2'
go