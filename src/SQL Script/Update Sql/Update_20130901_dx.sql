IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_Quota]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_Quota]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[SAP_Quota](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[QUNUM] [varchar](50) NULL,
	[QUPOS] [varchar](50) NULL,
	[LIFNR] [varchar](50) NULL,
	[WERKS] [varchar](50) NULL,
	[BEWRK] [varchar](50) NULL,
	[MATNR] [varchar](50) NULL,
	[VDATU] [varchar](50) NULL,
	[BDATU] [varchar](50) NULL,
	[BESKZ] [varchar](50) NULL,
	[SOBES] [varchar](50) NULL,
	[QUOTE] [decimal](18, 8) NULL,
	[CreateDate] [datetime] NULL,
	[BatchNo] [int] NULL,
 CONSTRAINT [PK_SAP_Quota] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[SAP_Quota] ADD  CONSTRAINT [DF_SAP_Quota_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO


