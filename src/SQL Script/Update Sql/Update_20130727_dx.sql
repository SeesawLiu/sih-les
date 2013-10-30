IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_SeqOrderTrace]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_SeqOrderTrace]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ORD_SeqOrderTrace](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TraceCode] [varchar](50) NULL,
	[ProdLine] [varchar](50) NULL,
	[SeqGroup] [varchar](50) NULL,
	[DeliveryCount] [varchar](10) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_ORD_SeqOrderTrace] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[ORD_SeqOrderTrace] ADD  CONSTRAINT [DF_ORD_SeqOrderTrace_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO


