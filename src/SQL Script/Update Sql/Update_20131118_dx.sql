IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_ProdOpBackflush]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_ProdOpBackflush]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[SAP_ProdOpBackflush](
	[Id] [int] NOT NULL,
	[SAPOpReportId] [int] NULL,
	[AUFNR] [varchar](50) NULL,
	[WERKS] [varchar](50) NULL,
	[AUFPL] [varchar](50) NULL,
	[APLZL] [varchar](50) NULL,
	[PLNTY] [varchar](50) NULL,
	[PLNNR] [varchar](50) NULL,
	[PLNAL] [varchar](50) NULL,
	[PLNFL] [varchar](50) NULL,
	[VORNR] [varchar](50) NULL,
	[ARBPL] [varchar](50) NULL,
	[RUEK] [varchar](50) NULL,
	[AUTWE] [varchar](50) NULL,
	[WORKCENTER] [varchar](50) NULL,
	[GAMNG] [decimal](18, 8) NULL,
	[SCRAP] [decimal](18, 8) NULL,
	[Status] [tinyint] NULL,
	[CreateDate] [datetime] NULL,
	[LastModifyDate] [datetime] NULL,
	[ErrorCount] [int] NULL,
	[ProdLine] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[RecNo] [varchar](50) NULL,
	[OrderOpId] [int] NULL,
	[OrderOpReportId] [int] NULL,
	[Version] [int] NULL,
	[EffectiveDate] [datetime] NULL,
 CONSTRAINT [PK_SAP_ProdOpBackflush] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

insert into SYS_MsgSubscirber(Id, Desc1, Emails, MaxMsgSize) values(10802, '自制件物料反冲失败', 'dingxin@sconit.com;jienyuan@sconit.com;zhangsheng@sconit.com', 10)
go

set Identity_Insert BAT_Job on
insert into BAT_Job(Id, Name, Desc1, ServiceType) values(300, 'BackFlushProductionOrderJob', '自制件物料反冲', 'BackFlushProductionOrderJob')
set Identity_Insert BAT_Job off
go
insert into BAT_JobParam(JobId, ParamKey, ParamValue) values(300, 'UserCode', 'su')
go
set Identity_Insert BAT_Trigger on
insert into BAT_Trigger(Id, JobId, Name, Desc1, PrevFireTime, NextFireTime, RepeatCount, Interval, IntervalType, TimesTriggered, Status) values(300, 300, 'BackFlushProductionOrderJob', '自制件物料反冲', '2013-11-18', '2013-11-18', 0, 20, 2, 0, 1)
set Identity_Insert BAT_Trigger off
go