IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_ManualGenOrderTrace]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_ManualGenOrderTrace]
GO

CREATE TABLE [dbo].[CUST_ManualGenOrderTrace](
	[ProdOrderNo] [varchar](50) NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
 CONSTRAINT [PK_CUST_ManualGenOrderTrace] PRIMARY KEY CLUSTERED 
(
	[ProdOrderNo] ASC,
	[OrderNo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
