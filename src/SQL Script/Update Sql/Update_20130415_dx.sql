alter table CUST_ProductLineMap drop column CabLoc
go
alter table CUST_ProductLineMap drop column ChaLoc
go
alter table CUST_ProductLineMap drop column VanLoc
go
alter table CUST_ProductLineMap drop column CabFlow
go
alter table CUST_ProductLineMap drop column ChaFlow
go
alter table CUST_ProductLineMap drop column ChaSapLoc
go
alter table CUST_ProductLineMap drop column VanSapLoc
go
alter table CUST_ProductLineMap drop column PowerFlow
go
alter table CUST_ProductLineMap drop column TireFlow
go
alter table CUST_ProductLineMap add Type tinyint
go
update CUST_ProductLineMap set Type = 0
go
alter table CUST_ProductLineMap alter column Type tinyint not null
go
alter table CUST_ProductLineMap alter column ProdLine varchar(50) null
go
insert into SYS_CodeMstr(Code, Desc1, Type) values ('ProductLineMapType', '生产线映射类型', 0)
go
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('ProductLineMapType', 0, 'ProductLineMapType_NotVan', 0, 1)
go
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('ProductLineMapType', 1, 'ProductLineMapType_Van', 0, 2)
go
alter table PRD_RoutingMstr drop column VirtualOpRef
go
alter table SCM_FlowMstr add VirtualOpRef varchar(50)
go
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PRD_ProdLineWorkCenter]') AND type in (N'U'))
DROP TABLE [dbo].[PRD_ProdLineWorkCenter]
GO
CREATE TABLE [dbo].[PRD_ProdLineWorkCenter](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NOT NULL,
	[WorkCenter] [varchar](50) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
 CONSTRAINT [PK_PRD_PRODLINEWORKCENTER] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PRD_ProdLineWorkCenter]  WITH CHECK ADD  CONSTRAINT [FK_PRD_PROD_REFERENCE_MD_WORKC] FOREIGN KEY([WorkCenter])
REFERENCES [dbo].[MD_WorkCenter] ([Code])
GO

ALTER TABLE [dbo].[PRD_ProdLineWorkCenter]  WITH CHECK ADD  CONSTRAINT [FK_PRD_PROD_REFERENCE_SCM_FLOW2] FOREIGN KEY([Flow])
REFERENCES [dbo].[SCM_FlowMstr] ([Code])
GO

alter table ORD_OrderOp add IsAutoReport bit
go

drop table SAP_ProdOpReport
go
CREATE TABLE [dbo].[SAP_ProdOpReport](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AUFNR] [varchar](50) NOT NULL,
	[WORKCENTER] [varchar](50) NOT NULL,
	[GAMNG] [decimal](18, 8) NOT NULL,
	[Status] [tinyint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[SCRAP] [decimal](18, 8) NOT NULL,
	[TEXT] [varchar](50) NULL
) ON [PRIMARY]
SET ANSI_PADDING OFF
go
ALTER TABLE [dbo].[SAP_ProdOpReport] ADD  CONSTRAINT [PK_SAP_ProdOpReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]


ALTER TABLE [dbo].[SAP_ProdOpReport] ADD  CONSTRAINT [DF_SAP_ProdOpReport_Status]  DEFAULT ((0)) FOR [Status]
GO


