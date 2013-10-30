alter table ORD_SeqOrderTrace add Flow varchar(50)
go
alter table ORD_SeqOrderTrace add OrderNo varchar(50)
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PRD_ItemConsume]') AND type in (N'U'))
DROP TABLE [dbo].[PRD_ItemConsume]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[PRD_ItemConsume](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[ItemDesc] [varchar](100) NULL,
	[RefItemCode] [varchar](50) NULL,
	[Qty] [decimal](18, 8) NOT NULL,
	[ConsumeQty] [decimal](18, 8) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Version] [int] NOT NULL,
 CONSTRAINT [PK_PRD_ItemConsume] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

insert into SYS_Menu values('Url_ItemConsume_View','厂内消化','Menu.Production.Setup',50650,'厂内消化','~/ItemConsume/Index','~/Content/Images/Nav/Default.png',1)
go
insert into ACC_Permission values('Url_ItemConsume_View','厂内消化','Production')


