SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[SCM_SeqGroup](
	[Code] [varchar](50) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[LastProdCode] [varchar](50) NULL,
	[LastProdSeq] [varchar](50) NULL,
	[LastTraceCode] [varchar](50) NULL,
	[LastSeq] [bigint] NULL,
	[CurrentProdCode] [varchar](50) NULL,
	[CurrentProdSeq] [varchar](50) NULL,
	[CurrentTraceCode] [varchar](50) NULL,
	[CurrentSeq] [bigint] NULL,
	[DeliveryDate] [datetime] NULL,
	[DeliveryCount] [int] NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Version] [int] NOT NULL,
	[SeqBatch] [int] NOT NULL,
	[DeliveryQty] [int] NULL,
	[ContainBlankSeq] [bit] NOT NULL,
	[AppVanSeries] [varchar](100) NULL,
	[CurrentWinTime] [datetime] NULL,
	[NextWinTime] [datetime] NULL,
	[AccumulateQty] [decimal](18, 8) NOT NULL,
	[Type] [tinyint] NOT NULL,
	[Name] [varchar](255) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
 CONSTRAINT [PK_SCM_SEQGROUP] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[SCM_SeqGroup]  WITH CHECK ADD  CONSTRAINT [FK_SCM_SEQG_REFERENCE_SCM_FLOW] FOREIGN KEY([ProdLine])
REFERENCES [dbo].[SCM_FlowMstr] ([Code])
GO

ALTER TABLE [dbo].[SCM_SeqGroup] CHECK CONSTRAINT [FK_SCM_SEQG_REFERENCE_SCM_FLOW]
GO


insert into sys_menu values 
('Url_SequenceGroup_View','厂外序列组','Menu.Production.Setup',50600,'厂外序列组','~/SequenceGroup/Index','~/Content/Images/Nav/Default.png',1)
insert into acc_permission values('Url_SequenceGroup_View','厂外序列组','Production')
go

insert into sys_menu values 
('Url_InSequenceGroup_View','厂内序列组','Menu.Production.Setup',50601,'厂内序列组','~/InSequenceGroup/Index','~/Content/Images/Nav/Default.png',1)
insert into acc_permission values('Url_InSequenceGroup_View','厂内序列组','Production')
go


insert into SYS_EntityPreference values
(10020,22,'06:00','工作日开始时间',3088,'jey',getdate(),3088,'jey',getdate())
go

alter table ORD_OrderMstr add SeqGroup varchar(50) null
go
alter table ORD_OrderMstr_0 add SeqGroup varchar(50) null
go
alter table ORD_OrderMstr_1 add SeqGroup varchar(50) null
go
alter table ORD_OrderMstr_2 add SeqGroup varchar(50) null
go
alter table ORD_OrderMstr_3 add SeqGroup varchar(50) null
go
alter table ORD_OrderMstr_4 add SeqGroup varchar(50) null
go
alter table ORD_OrderMstr_5 add SeqGroup varchar(50) null
go
alter table ORD_OrderMstr_6 add SeqGroup varchar(50) null
go
alter table ORD_OrderMstr_7 add SeqGroup varchar(50) null
go
alter table ORD_OrderMstr_8 add SeqGroup varchar(50) null
go
