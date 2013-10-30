IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_SnapshotFlowDet4LeanEngine]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_SnapshotFlowDet4LeanEngine]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[LOG_SnapshotFlowDet4LeanEngine](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[Item] [varchar](50) NULL,
	[LocFrom] [varchar](50) NULL,
	[LocTo] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[Lvl] [tinyint] NULL,
	[ErrorId] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_SnapshotFlowDet4LeanEngine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[LOG_SnapshotFlowDet4LeanEngine] ADD  CONSTRAINT [DF_LOG_SnapshotFlowDet4LeanEngine_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO