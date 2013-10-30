IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_CreateVanProdOrder') 
     DROP PROCEDURE USP_Busi_CreateVanProdOrder
GO 

CREATE TABLE [dbo].[INV_OpRefBalance](
	[Id] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[OpRef] [varchar](50) NOT NULL,
	[Qty] [decimal](18, 8) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Version] [int] NULL,
 CONSTRAINT [PK_INV_OpRefBalance] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[INV_OpRefBalance]  WITH CHECK ADD  CONSTRAINT [FK_INV_OPRE_REFERENCE_MD_ITEM] FOREIGN KEY([Item])
REFERENCES [dbo].[MD_Item] ([Code])
GO

