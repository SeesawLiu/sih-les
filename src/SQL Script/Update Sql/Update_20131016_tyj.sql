
/****** Object:  Table [dbo].[CUST_FailCode]    Script Date: 10/16/2013 09:28:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[CUST_CreateOrderCode](
	[Code] [varchar](50) NOT NULL PRIMARY KEY,
	[Desc1] [varchar](50) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
)
go
insert into SYS_Menu values('Url_CreateOrderCode_View','要货原因代码','Menu.Procurement.Setup',200,'要货原因代码','~/CreateOrderCode/Index','~/Content/Images/Nav/Default.png',1)
go
insert into ACC_Permission values('Url_CreateOrderCode_View','要货原因代码','Procurement')


