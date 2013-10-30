IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_ManualGenOrderOpRefBalanceTrace]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_ManualGenOrderOpRefBalanceTrace]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[CUST_ManualGenOrderOpRefBalanceTrace](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProdOrderNo] [varchar](50) NULL,
	[Item] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[Qty] [decimal](18, 8) NULL,
	[CreateUser] [varchar](50) NULL,
	[CreateUserNm] [varchar](100) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_CUST_ManualGenOrderOpRefBalanceTrace] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


