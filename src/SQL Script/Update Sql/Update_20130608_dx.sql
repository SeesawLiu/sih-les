IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_SAPRoutingDet]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_SAPRoutingDet]
GO

CREATE TABLE [dbo].[CUST_SAPRoutingDet](
	[Id] [int] IDENTITY(1,1) NOT NULL,	
	[CreateDate] [datetime] NOT NULL,
	[AUFNR] [varchar](50) NULL,
	[WERKS] [varchar](50) NULL,
	[AUFPL] [int] NULL,
	[APLZL] [int] NULL,
	[PLNTY] [varchar](50) NULL,
	[PLNNR] [varchar](50) NULL,
	[PLNAL] [varchar](50) NULL,
	[PLNFL] [varchar](50) NULL,
	[VORNR] [varchar](50) NULL,
	[ARBPL] [varchar](50) NULL,
	[RUEK] [varchar](50) NULL,
	[AUTWE] [varchar](50) NULL,
 CONSTRAINT [PK_CUST_SAPRoutingDet] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[CUST_SAPRoutingDet] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO
