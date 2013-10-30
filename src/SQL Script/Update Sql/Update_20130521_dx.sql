IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_CabOut]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_CabOut]
GO

CREATE TABLE [dbo].[CUST_CabOut](
	[OrderNo] [varchar](50) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[CabType] [tinyint] NOT NULL,
	[CabItem] [varchar](50) NOT NULL,
	[QulityBarcode] [varchar](50) NULL,
	[Status] [tinyint] NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[OutUser] [int] NULL,
	[OutUserNm] [varchar](100) NULL,
	[OutDate] [datetime] NULL,
	[TransferUser] [int] NULL,
	[TransferUserNm] [varchar](100) NULL,
	[TransferDate] [datetime] NULL,
 CONSTRAINT [PK_CUST_CabOut] PRIMARY KEY CLUSTERED 
(
	[OrderNo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
