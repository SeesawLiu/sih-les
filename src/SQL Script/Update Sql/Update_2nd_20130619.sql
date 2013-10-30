while exists(select Code from ACC_Permission group by Code having COUNT(1) > 1)
begin
	delete from ACC_PermissionGroupPermission where PermissionId in (select MAX(Id) from ACC_Permission group by Code having COUNT(1) > 1)
	delete from ACC_RolePermission where PermissionId in (select MAX(Id) from ACC_Permission group by Code having COUNT(1) > 1)
	delete from ACC_UserPermission where PermissionId in (select MAX(Id) from ACC_Permission group by Code having COUNT(1) > 1)
	delete from ACC_Permission where Id in (select MAX(Id) from ACC_Permission group by Code having COUNT(1) > 1)
end
go

CREATE UNIQUE NONCLUSTERED INDEX [IX_Permission] ON [dbo].[ACC_Permission] 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

while exists(select Code from SYS_CodeDet group by Code, Value having COUNT(1) > 1)
begin
	delete from SYS_CodeDet where Id in (select MAX(Id) from SYS_CodeDet group by Code, Value having COUNT(1) > 1)
end
go

CREATE UNIQUE NONCLUSTERED INDEX [IX_CodeDet] ON [dbo].[SYS_CodeDet] 
(
	[Code] ASC,
	[Value] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

insert into SYS_Menu(Code, Name, Parent, Seq, Desc1, PageUrl, ImageUrl, IsActive)
select m1.Code, m1.Name, m1.Parent, m1.Seq, m1.Desc1, m1.PageUrl, m1.ImageUrl, m1.IsActive
from sconit5_2nd.dbo.SYS_Menu as m1
left join SYS_Menu as m2 on m1.Code = m2.Code
where m2.Code is null
go

update m2 set Name = m1.Name, Parent = m1.Parent, Seq = m1.Seq, Desc1 = m1.Desc1, PageUrl = m1.PageUrl, ImageUrl = m1.ImageUrl, IsActive = m1.IsActive
from sconit5_2nd.dbo.SYS_Menu as m1
left join SYS_Menu as m2 on m1.Code = m2.Code
go

insert into ACC_Permission(Code, Desc1, Category)
select p1.Code, p1.Desc1, p1.Category
from sconit5_2nd.dbo.ACC_Permission as p1
left join ACC_Permission as p2 on p1.Code = p2.Code
where p2.Code is null
go

insert into SYS_EntityPreference(Id, Seq, Value, Desc1, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate)
select p1.Id, p1.Seq, p1.Value, p1.Desc1, 2603, '用户 超级', '2011-01-01 00:00:00.000', 2603, '用户 超级', '2011-01-01 00:00:00.000'
from sconit5_2nd.dbo.SYS_EntityPreference as p1
left join SYS_EntityPreference as p2 on p1.Id = p2.Id
where p2.Id is null
go

insert into SYS_CodeMstr(Code, Desc1, [Type])
select m1.Code, m1.Desc1, m1.[Type] from sconit5_2nd.dbo.SYS_CodeMstr as m1
left join SYS_CodeMstr as m2 on m1.Code = m2.Code
where m2.Code is null
go

update m2 set Desc1 = m1.Desc1, [Type] = m1.[Type]
from sconit5_2nd.dbo.SYS_CodeMstr as m1
inner join SYS_CodeMstr as m2 on m1.Code = m2.Code
go

insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq)
select d1.Code, d1.Value, d1.Desc1, d1.IsDefault, d1.Seq 
from sconit5_2nd.dbo.SYS_CodeDet as d1
left join SYS_CodeDet as d2 on d1.Code = d2.Code and d1.Value = d2.Value
where d2.Code is null
go

update d2 set Desc1 = d1.Desc1, IsDefault = d1.IsDefault, Seq = d1.Seq
from sconit5_2nd.dbo.SYS_CodeDet as d1
inner join SYS_CodeDet as d2 on d1.Code = d2.Code and d1.Value = d2.Value
go

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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_ProdLineLocationMap]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_ProdLineLocationMap]
GO

CREATE TABLE [dbo].[CUST_ProdLineLocationMap](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[SapLocation] [varchar](50) NOT NULL,
	[Location] [varchar](50) NOT NULL,
 CONSTRAINT [PK_CUST_ProdLineLocationMap] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

select SAPProdLine, ProdLine into #tempProductLineMap from CUST_ProductLineMap where SAPProdLine not in ('A', 'B')
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CUST_ProductLineMap]') AND type in (N'U'))
DROP TABLE [dbo].[CUST_ProductLineMap]
CREATE TABLE [dbo].[CUST_ProductLineMap](
	[SAPProdLine] [varchar](50) NOT NULL,
	[ProdLine] [varchar](50) NULL,
	[TransFlow] [varchar](50) NULL,
	[SaddleFlow] [varchar](50) NULL,
	[CabProdLine] [varchar](50) NULL,
	[ChassisProdLine] [varchar](50) NULL,
	[AssemblyProdLine] [varchar](50) NULL,
	[SpecialProdLine] [varchar](50) NULL,
	[MaxOrderCount] [int] NULL,
	[InitVanOrder] [varchar](50) NULL,
	[Plant] [varchar](50) NULL,
	[IsActive] [bit] NOT NULL,
	[Type] [tinyint] NOT NULL,
 CONSTRAINT [PK_CUST_PRODUCTLINEMAP] PRIMARY KEY CLUSTERED 
(
	[SAPProdLine] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
insert into CUST_ProductLineMap(SAPProdLine, ProdLine, IsActive, [Type]) select SAPProdLine, ProdLine, 1, 0 from #tempProductLineMap
drop table #tempProductLineMap
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FIS_CancelReceiptMaster]') AND type in (N'U'))
DROP TABLE [dbo].[FIS_CancelReceiptMaster]
GO

CREATE TABLE [dbo].[FIS_CancelReceiptMaster](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WMSNo] [varchar](50) NOT NULL,
	[WMSSeq] [int] NULL,
	[RecQty] [decimal](18, 8) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[CreateDATDate] [datetime] NULL,
	[DATFileName] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FIS_CreateBarCode]') AND type in (N'U'))
DROP TABLE [dbo].[FIS_CreateBarCode]
GO

CREATE TABLE [dbo].[FIS_CreateBarCode](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[HuId] [varchar](50) NOT NULL,
	[LotNo] [varchar](50) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[Qty] [decimal](18, 8) NOT NULL,
	[ManufactureParty] [varchar](50) NOT NULL,
	[ASN] [varchar](50) NULL,
	[CreateDate] [datetime] NOT NULL,
	[CreateDATDate] [datetime] NULL,
	[DATFileName] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FIS_ItemStandardPack]') AND type in (N'U'))
DROP TABLE [dbo].[FIS_ItemStandardPack]
GO

CREATE TABLE [dbo].[FIS_ItemStandardPack](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FlowDetId] [int] NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[Pack] [varchar](50) NULL,
	[UC] [decimal](18, 8) NOT NULL,
	[IOType] [varchar](50) NOT NULL,
	[Location] [char](4) NOT NULL,
	[Plant] [char](4) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[CreateDATDate] [datetime] NULL,
	[DATFileName] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

alter table FIS_LesINLog add Qty decimal(18, 8)
go
alter table FIS_LesINLog add QtyMark bit
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FIS_YieldReturn]') AND type in (N'U'))
DROP TABLE [dbo].[FIS_YieldReturn]
GO

CREATE TABLE [dbo].[FIS_YieldReturn](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IpNo] [varchar](50) NOT NULL,
	[ArriveTime] [datetime] NOT NULL,
	[PartyFrom] [varchar](50) NOT NULL,
	[PartyTo] [varchar](50) NOT NULL,
	[Dock] [varchar](100) NULL,
	[IpCreateDate] [datetime] NOT NULL,
	[Seq] [varchar](50) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Qty] [decimal](18, 8) NOT NULL,
	[IsConsignment] [bit] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[CreateDATDate] [datetime] NULL,
	[DATFileName] [varchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[INV_OpRefBalance]') AND type in (N'U'))
DROP TABLE [dbo].[INV_OpRefBalance]
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

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_FlowDetExtraDmdSource]') AND type in (N'U'))
DROP TABLE [dbo].[LE_FlowDetExtraDmdSource]
GO

CREATE TABLE [dbo].[LE_FlowDetExtraDmdSource](
	[FlowDetId] [int] NOT NULL,
	[Location] [varchar](50) NOT NULL,
 CONSTRAINT [PK_LE_FlowDetExtraDmdSource] PRIMARY KEY CLUSTERED 
(
	[FlowDetId] ASC,
	[Location] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_FlowDetSnapShot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_FlowDetSnapShot]
GO

CREATE TABLE [dbo].[LE_FlowDetSnapShot](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[FlowDetId] [int] NULL,
	[Item] [varchar](50) NOT NULL,
	[Uom] [varchar](5) NULL,
	[UC] [decimal](18, 8) NULL,
	[ManufactureParty] [varchar](50) NULL,
	[LocFrom] [varchar](50) NULL,
	[LocTo] [varchar](50) NOT NULL,
	[IsRefFlow] [bit] NULL,
	[SafeStock] [decimal](18, 8) NULL,
	[MaxStock] [decimal](18, 8) NULL,
	[MinLotSize] [decimal](18, 8) NULL,
	[RoundUpOpt] [tinyint] NULL,
	[Strategy] [tinyint] NULL,
 CONSTRAINT [PK_LE_FlowDetSnapShot_1] PRIMARY KEY CLUSTERED 
(
	[Item] ASC,
	[LocTo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_FlowMstrSnapShot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_FlowMstrSnapShot]
GO

CREATE TABLE [dbo].[LE_FlowMstrSnapShot](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NOT NULL,
	[Type] [tinyint] NULL,
	[Strategy] [tinyint] NULL,
	[PartyFrom] [varchar](50) NULL,
	[PartyTo] [varchar](50) NULL,
	[LocFrom] [varchar](50) NULL,
	[LocTo] [varchar](50) NULL,
	[Dock] [varchar](50) NULL,
	[ExtraDmdSource] [varchar](255) NULL,
	[OrderTime] [datetime] NULL,
	[ReqTimeFrom] [datetime] NULL,
	[ReqTimeTo] [datetime] NULL,
	[WindowTime] [datetime] NULL,
	[EMWindowTime] [datetime] NULL,
 CONSTRAINT [PK_LE_FlowMstrSnapShot] PRIMARY KEY CLUSTERED 
(
	[Flow] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_LocationDetSnapshot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_LocationDetSnapshot]
GO

CREATE TABLE [dbo].[LE_LocationDetSnapshot](
	[Item] [varchar](50) NOT NULL,
	[Location] [varchar](50) NOT NULL,
	[Qty] [decimal](18, 8) NULL,
 CONSTRAINT [PK_LE_LocationDetSnapshot] PRIMARY KEY CLUSTERED 
(
	[Item] ASC,
	[Location] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_OrderBomCPTimeSnapshot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_OrderBomCPTimeSnapshot]
GO

CREATE TABLE [dbo].[LE_OrderBomCPTimeSnapshot](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](50) NULL,
	[VanProdLine] [varchar](50) NULL,
	[AssProdLine] [varchar](50) NULL,
	[Seq] [bigint] NULL,
	[SubSeq] [int] NULL,
	[Op] [bigint] NULL,
	[OpTaktTime] [int] NULL,
	[CPTime] [datetime] NULL,
	[Item] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[Location] [varchar](50) NULL,
	[IsCreateOrder] [bit] NULL,
	[BomId] [int] NULL,
	[DISPO] [varchar](50) NULL,
	[CreateDate] [datetime] NOT NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Uom] [varchar](5) NULL,
	[VanOp] [int] NULL,
	[AssOp] [int] NULL,
	[WorkCenter] [varchar](50) NULL,
 CONSTRAINT [PK_LE_OrderBomCPTimeSnapshot] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LE_OrderPlanSnapshot]') AND type in (N'U'))
DROP TABLE [dbo].[LE_OrderPlanSnapshot]
GO

CREATE TABLE [dbo].[LE_OrderPlanSnapshot](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Item] [varchar](50) NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Location] [varchar](50) NULL,
	[ReqTime] [datetime] NULL,
	[OrderNo] [varchar](50) NULL,
	[IRType] [tinyint] NULL,
	[OrderType] [tinyint] NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[FinishQty] [decimal](18, 8) NULL,
 CONSTRAINT [PK_LE_OrderPlanSnapshot] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenJITOrder]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_GenJITOrder]
GO

CREATE TABLE [dbo].[LOG_GenJITOrder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[Lvl] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_GenJITOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_GenJITOrder] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenKBOrder]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_GenKBOrder]
GO

CREATE TABLE [dbo].[LOG_GenKBOrder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[Lvl] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_GenKBOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_GenKBOrder] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenProductOrder]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_GenProductOrder]
GO

CREATE TABLE [dbo].[LOG_GenProductOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BatchNo] [int] NULL,
	[Lvl] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_LOG_GenProductOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_GenProductOrder] ADD  CONSTRAINT [DF_LOG_GenProductOrder_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenSequenceOrder]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_GenSequenceOrder]
GO

CREATE TABLE [dbo].[LOG_GenSequenceOrder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SeqGroup] [varchar](50) NULL,
	[Flow] [varchar](50) NULL,
	[Lvl] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_GenSequenceOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_GenSequenceOrder] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_GenVanProdOrder]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_GenVanProdOrder]
GO

CREATE TABLE [dbo].[LOG_GenVanProdOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AUFNR] [varchar](50) NULL,
	[ZLINE] [varchar](50) NULL,
	[ProdLine] [varchar](50) NULL,
	[BatchNo] [varchar](50) NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_LOG_GenVanProdOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_GenVanProdOrder] ADD  CONSTRAINT [DF_LOG_GenVanProdOrder_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_OrderTrace]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_OrderTrace]
GO

CREATE TABLE [dbo].[LOG_OrderTrace](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UUID] [varchar](50) NULL,
	[Flow] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[OrderDetSeq] [int] NULL,
	[OrderDetId] [bigint] NULL,
	[Priority] [tinyint] NULL,
	[StartTime] [datetime] NULL,
	[WindowTime] [datetime] NULL,
	[EMWindowTime] [datetime] NULL,
	[ReqTimeFrom] [datetime] NULL,
	[ReqTimeTo] [datetime] NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[Uom] [varchar](50) NULL,
	[UC] [decimal](18, 8) NULL,
	[ManufactureParty] [varchar](50) NULL,
	[LocFrom] [varchar](50) NULL,
	[LocTo] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[SafeStock] [decimal](18, 8) NULL,
	[MaxStock] [decimal](18, 8) NULL,
	[MinLotSize] [decimal](18, 8) NULL,
	[RoundUpOpt] [tinyint] NULL,
	[ReqQty] [decimal](18, 8) NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_LOG_ORDERTRACE] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_OrderTrace] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_OrderTraceDet]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_OrderTraceDet]
GO

CREATE TABLE [dbo].[LOG_OrderTraceDet](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UUID] [varchar](50) NULL,
	[Type] [varchar](5) NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Location] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[ReqTime] [datetime] NOT NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[FinishQty] [decimal](18, 8) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_LOG_ORDERTRACEDET] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_OrderTraceDet] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_RunLeanEngine]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_RunLeanEngine]
GO

CREATE TABLE [dbo].[LOG_RunLeanEngine](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[Item] [varchar](50) NULL,
	[LocFrom] [varchar](50) NULL,
	[LocTo] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[Lvl] [tinyint] NULL,
	[ErrorId] [tinyint] NULL,
	[Msg] [varchar](500) NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_RunLeanEngine] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_RunLeanEngine] ADD  CONSTRAINT [DF__LOG_RunLe__Creat__13A973D6]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_VanOrderBomTrace]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_VanOrderBomTrace]
GO

CREATE TABLE [dbo].[LOG_VanOrderBomTrace](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UUID] [varchar](50) NULL,
	[ProdLine] [varchar](50) NULL,
	[VanOrderNo] [varchar](50) NULL,
	[VanOrderBomDetId] [int] NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[OpRef] [varchar](50) NULL,
	[LocFrom] [varchar](50) NULL,
	[LocTo] [varchar](50) NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[CPTime] [datetime] NULL,
	[CreateDate] [datetime] NULL,
 CONSTRAINT [PK_LOG_VanOrderBomTrace] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_VanOrderBomTrace] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LOG_VanOrderTrace]') AND type in (N'U'))
DROP TABLE [dbo].[LOG_VanOrderTrace]
GO

CREATE TABLE [dbo].[LOG_VanOrderTrace](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[UUID] [varchar](50) NULL,
	[Flow] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[OrderDetSeq] [int] NULL,
	[OrderDetId] [bigint] NULL,
	[Priority] [tinyint] NULL,
	[StartTime] [datetime] NULL,
	[WindowTime] [datetime] NULL,
	[EMWindowTime] [datetime] NULL,
	[ReqTimeFrom] [datetime] NULL,
	[ReqTimeTo] [datetime] NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[Uom] [varchar](50) NULL,
	[UC] [decimal](18, 8) NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Location] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[NetOrderQty] [decimal](18, 8) NULL,
	[OrgOpRefQty] [decimal](18, 8) NULL,
	[GrossOrderQty] [decimal](18, 8) NULL,
	[OpRefQty] [decimal](18, 8) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_LOG_JITORDERTRACE] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LOG_VanOrderTrace] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

ALTER TABLE MD_Item alter column ItemCategory varchar(50) null
go

ALTER TABLE MD_SpecialTime add ProdLine varchar(50) null
go

ALTER TABLE MD_SpecialTime add Category tinyint not null
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_AliquotStartTask]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_AliquotStartTask]
GO

CREATE TABLE [dbo].[ORD_AliquotStartTask](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NOT NULL,
	[VanNo] [varchar](50) NULL,
	[IsStart] [bit] NOT NULL,
	[StartTime] [datetime] NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

alter table ORD_IpMstr add ShipNo varchar(50) null
go
alter table ORD_IpMstr add Vehicle varchar(50) null
go
alter table ORD_IpMstr_0 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_0 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_1 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_1 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_2 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_2 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_3 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_3 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_4 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_4 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_5 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_5 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_6 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_6 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_7 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_7 add Vehicle varchar(50) null
go
alter table ORD_IpMstr_8 add ShipNo varchar(50) null
go
alter table ORD_IpMstr_8 add Vehicle varchar(50) null
go

alter table ORD_OrderBackflushDet add ProdSeqReportId int
go
alter table ORD_OrderBackflushDet add OrderOpId int
go
alter table ORD_OrderBackflushDet add WorkCenter varchar(50)
go
alter table ORD_OrderBackflushDet alter column RecNo varchar(50) null
go
alter table ORD_OrderBackflushDet alter column RecDetId int null
go
alter table ORD_OrderBackflushDet alter column RecDetSeq int null
go
alter table ORD_OrderBackflushDet alter column OrderNo varchar(50) null
go
alter table ORD_OrderBackflushDet alter column OrderDetId int null
go
alter table ORD_OrderBackflushDet alter column OrderDetSeq int null
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_OrderBomCPTime]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_OrderBomCPTime]
GO

CREATE TABLE [dbo].[ORD_OrderBomCPTime](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](50) NULL,
	[VanProdLine] [varchar](50) NULL,
	[AssProdLine] [varchar](50) NULL,
	[Seq] [bigint] NULL,
	[SubSeq] [int] NULL,
	[Op] [int] NULL,
	[OpTaktTime] [int] NULL,
	[CPTime] [datetime] NULL,
	[Item] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[Location] [varchar](50) NULL,
	[IsCreateOrder] [bit] NULL,
	[BomId] [int] NULL,
	[DISPO] [varchar](50) NULL,
	[CreateDate] [datetime] NOT NULL,
	[ManufactureParty] [varchar](50) NULL,
	[Uom] [varchar](5) NULL,
	[WorkCenter] [varchar](50) NULL,
	[VanOp] [int] NULL,
	[AssOp] [int] NULL,
 CONSTRAINT [PK_ORD_OrderBomCPTime] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ORD_OrderBomCPTime] ADD  CONSTRAINT [DF_ORD_OrderBomCPTime_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

ALTER TABLE ORD_OrderBomDet ALTER COLUMN [BWART] [varchar] (50)
GO
ALTER TABLE ORD_OrderBomDet add [AssProdLine] [varchar] (50)
GO
ALTER TABLE ORD_OrderBomDet add [IsCreateOrder] [bit] NULL
GO
ALTER TABLE ORD_OrderBomDet add [DISPO] [varchar] (50)
GO
ALTER TABLE ORD_OrderBomDet add [BESKZ] [varchar] (100)
GO
ALTER TABLE ORD_OrderBomDet add [SOBSL] [varchar] (100)
GO
ALTER TABLE ORD_OrderBomDet add [WorkCenter] [varchar] (50)
GO
ALTER TABLE ORD_OrderBomDet add [JointOp] [int] NULL
GO
ALTER TABLE ORD_OrderBomDet add [PLNFL] [varchar] (50)
GO
ALTER TABLE ORD_OrderBomDet add [VORNR] [varchar] (50)
GO
ALTER TABLE ORD_OrderBomDet add [AUFPL] [varchar] (50)
GO

alter table ORD_OrderDet drop CONSTRAINT FK_ORD_ORDE_0_REFERENCE_INP_PROD
go
alter table ORD_OrderDet drop column ProdScan
GO
alter table ORD_OrderDet_0 drop CONSTRAINT FK_ORD_OrderDet0_ProdScan_REFERENCE_INP_ProdScanMstr_Code
go
alter table ORD_OrderDet_0 drop column ProdScan
GO
alter table ORD_OrderDet_1 drop CONSTRAINT FK_ORD_OrderDet1_ProdScan_REFERENCE_INP_ProdScanMstr_Code
go
alter table ORD_OrderDet_1 drop column ProdScan
GO
alter table ORD_OrderDet_2 drop CONSTRAINT FK_ORD_OrderDet2_ProdScan_REFERENCE_INP_ProdScanMstr_Code
go
alter table ORD_OrderDet_2 drop column ProdScan
GO
alter table ORD_OrderDet_3 drop CONSTRAINT FK_ORD_OrderDet3_ProdScan_REFERENCE_INP_ProdScanMstr_Code
go
alter table ORD_OrderDet_3 drop column ProdScan
GO
alter table ORD_OrderDet_4 drop CONSTRAINT FK_ORD_OrderDet4_ProdScan_REFERENCE_INP_ProdScanMstr_Code
go
alter table ORD_OrderDet_4 drop column ProdScan
GO
alter table ORD_OrderDet_5 drop CONSTRAINT FK_ORD_OrderDet5_ProdScan_REFERENCE_INP_ProdScanMstr_Code
go
alter table ORD_OrderDet_5 drop column ProdScan
GO
alter table ORD_OrderDet_6 drop CONSTRAINT FK_ORD_OrderDet6_ProdScan_REFERENCE_INP_ProdScanMstr_Code
go
alter table ORD_OrderDet_6 drop column ProdScan
GO
alter table ORD_OrderDet_7 drop CONSTRAINT FK_ORD_OrderDet7_ProdScan_REFERENCE_INP_ProdScanMstr_Code
go
alter table ORD_OrderDet_7 drop column ProdScan
GO
alter table ORD_OrderDet_8 drop CONSTRAINT FK_ORD_OrderDet8_ProdScan_REFERENCE_INP_ProdScanMstr_Code
go
alter table ORD_OrderDet_8 drop column ProdScan
GO

alter table ORD_OrderDet add IsCreatePickList bit
GO
alter table ORD_OrderDet_0 add IsCreatePickList bit
GO
alter table ORD_OrderDet_1 add IsCreatePickList bit
GO
alter table ORD_OrderDet_2 add IsCreatePickList bit
GO
alter table ORD_OrderDet_3 add IsCreatePickList bit
GO
alter table ORD_OrderDet_4 add IsCreatePickList bit
GO
alter table ORD_OrderDet_5 add IsCreatePickList bit
GO
alter table ORD_OrderDet_6 add IsCreatePickList bit
GO
alter table ORD_OrderDet_7 add IsCreatePickList bit
GO
alter table ORD_OrderDet_8 add IsCreatePickList bit
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_OrderItemTrace]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_OrderItemTrace]
GO

CREATE TABLE [dbo].[ORD_OrderItemTrace](
	[Id] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
	[OrderBomId] [int] NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[ItemDesc] [varchar](100) NULL,
	[RefItemCode] [varchar](50) NULL,
	[Op] [int] NOT NULL,
	[OpRef] [varchar](50) NOT NULL,
	[Qty] [decimal](18, 8) NOT NULL,
	[ScanQty] [decimal](18, 8) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Version] [int] NOT NULL,
 CONSTRAINT [PK_ORD_ORDERITEMTRACE] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_OrderItemTraceResult]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_OrderItemTraceResult]
GO

CREATE TABLE [dbo].[ORD_OrderItemTraceResult](
	[Id] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[OrderItemTraceId] [int] NULL,
	[BarCode] [varchar](50) NOT NULL,
	[Supplier] [varchar](50) NULL,
	[LotNo] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[Item] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[RefItemCode] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[IsWithdraw] [bit] NOT NULL,
 CONSTRAINT [PK_ORD_ORDERITEMTRACERESULT] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

alter table ORD_OrderMstr drop column IsPlanPause
go
alter table ORD_OrderMstr_0 drop column IsPlanPause
go
alter table ORD_OrderMstr_1 drop column IsPlanPause
go
alter table ORD_OrderMstr_2 drop column IsPlanPause
go
alter table ORD_OrderMstr_3 drop column IsPlanPause
go
alter table ORD_OrderMstr_4 drop column IsPlanPause
go
alter table ORD_OrderMstr_5 drop column IsPlanPause
go
alter table ORD_OrderMstr_6 drop column IsPlanPause
go
alter table ORD_OrderMstr_7 drop column IsPlanPause
go
alter table ORD_OrderMstr_8 drop column IsPlanPause
go

alter table ORD_OrderMstr drop column IsPause
go
alter table ORD_OrderMstr_0 drop column IsPause
go
alter table ORD_OrderMstr_1 drop column IsPause
go
alter table ORD_OrderMstr_2 drop column IsPause
go
alter table ORD_OrderMstr_3 drop column IsPause
go
alter table ORD_OrderMstr_4 drop column IsPause
go
alter table ORD_OrderMstr_5 drop column IsPause
go
alter table ORD_OrderMstr_6 drop column IsPause
go
alter table ORD_OrderMstr_7 drop column IsPause
go
alter table ORD_OrderMstr_8 drop column IsPause
go

alter table ORD_OrderMstr drop column IsPLPause
go
alter table ORD_OrderMstr_0 drop column IsPLPause
go
alter table ORD_OrderMstr_1 drop column IsPLPause
go
alter table ORD_OrderMstr_2 drop column IsPLPause
go
alter table ORD_OrderMstr_3 drop column IsPLPause
go
alter table ORD_OrderMstr_4 drop column IsPLPause
go
alter table ORD_OrderMstr_5 drop column IsPLPause
go
alter table ORD_OrderMstr_6 drop column IsPLPause
go
alter table ORD_OrderMstr_7 drop column IsPLPause
go
alter table ORD_OrderMstr_8 drop column IsPLPause
go

alter table ORD_OrderMstr drop column Seq
go
alter table ORD_OrderMstr_0 drop column Seq
go
alter table ORD_OrderMstr_1 drop column Seq
go
alter table ORD_OrderMstr_2 drop column Seq
go
alter table ORD_OrderMstr_3 drop column Seq
go
alter table ORD_OrderMstr_4 drop column Seq
go
alter table ORD_OrderMstr_5 drop column Seq
go
alter table ORD_OrderMstr_6 drop column Seq
go
alter table ORD_OrderMstr_7 drop column Seq
go
alter table ORD_OrderMstr_8 drop column Seq
go

alter table ORD_OrderMstr drop column SapSeq
go
alter table ORD_OrderMstr_0 drop column SapSeq
go
alter table ORD_OrderMstr_1 drop column SapSeq
go
alter table ORD_OrderMstr_2 drop column SapSeq
go
alter table ORD_OrderMstr_3 drop column SapSeq
go
alter table ORD_OrderMstr_4 drop column SapSeq
go
alter table ORD_OrderMstr_5 drop column SapSeq
go
alter table ORD_OrderMstr_6 drop column SapSeq
go
alter table ORD_OrderMstr_7 drop column SapSeq
go
alter table ORD_OrderMstr_8 drop column SapSeq
go

alter table ORD_OrderMstr drop column IsCheckPartyFromAuth
go
alter table ORD_OrderMstr_0 drop column IsCheckPartyFromAuth
go
alter table ORD_OrderMstr_1 drop column IsCheckPartyFromAuth
go
alter table ORD_OrderMstr_2 drop column IsCheckPartyFromAuth
go
alter table ORD_OrderMstr_3 drop column IsCheckPartyFromAuth
go
alter table ORD_OrderMstr_4 drop column IsCheckPartyFromAuth
go
alter table ORD_OrderMstr_5 drop column IsCheckPartyFromAuth
go
alter table ORD_OrderMstr_6 drop column IsCheckPartyFromAuth
go
alter table ORD_OrderMstr_7 drop column IsCheckPartyFromAuth
go
alter table ORD_OrderMstr_8 drop column IsCheckPartyFromAuth
go

alter table ORD_OrderMstr drop column IsCheckPartyToAuth
go
alter table ORD_OrderMstr_0 drop column IsCheckPartyToAuth
go
alter table ORD_OrderMstr_1 drop column IsCheckPartyToAuth
go
alter table ORD_OrderMstr_2 drop column IsCheckPartyToAuth
go
alter table ORD_OrderMstr_3 drop column IsCheckPartyToAuth
go
alter table ORD_OrderMstr_4 drop column IsCheckPartyToAuth
go
alter table ORD_OrderMstr_5 drop column IsCheckPartyToAuth
go
alter table ORD_OrderMstr_6 drop column IsCheckPartyToAuth
go
alter table ORD_OrderMstr_7 drop column IsCheckPartyToAuth
go
alter table ORD_OrderMstr_8 drop column IsCheckPartyToAuth
go

alter table ORD_OrderMstr add ProdLineType tinyint
go
alter table ORD_OrderMstr_0 add ProdLineType tinyint
go
alter table ORD_OrderMstr_1 add ProdLineType tinyint
go
alter table ORD_OrderMstr_2 add ProdLineType tinyint
go
alter table ORD_OrderMstr_3 add ProdLineType tinyint
go
alter table ORD_OrderMstr_4 add ProdLineType tinyint
go
alter table ORD_OrderMstr_5 add ProdLineType tinyint
go
alter table ORD_OrderMstr_6 add ProdLineType tinyint
go
alter table ORD_OrderMstr_7 add ProdLineType tinyint
go
alter table ORD_OrderMstr_8 add ProdLineType tinyint
go
update ORD_OrderMstr set ProdLineType = 0
go
update ORD_OrderMstr_0 set ProdLineType = 0
go
update ORD_OrderMstr_1 set ProdLineType = 0
go
update ORD_OrderMstr_2 set ProdLineType = 0
go
update ORD_OrderMstr_3 set ProdLineType = 0
go
update ORD_OrderMstr_4 set ProdLineType = 0
go
update ORD_OrderMstr_5 set ProdLineType = 0
go
update ORD_OrderMstr_6 set ProdLineType = 0
go
update ORD_OrderMstr_7 set ProdLineType = 0
go
update ORD_OrderMstr_8 set ProdLineType = 0
go
alter table ORD_OrderMstr alter column ProdLineType tinyint not null
go
alter table ORD_OrderMstr_0 alter column ProdLineType tinyint not null
go
alter table ORD_OrderMstr_1 alter column ProdLineType tinyint not null
go
alter table ORD_OrderMstr_2 alter column ProdLineType tinyint not null
go
alter table ORD_OrderMstr_3 alter column ProdLineType tinyint not null
go
alter table ORD_OrderMstr_4 alter column ProdLineType tinyint not null
go
alter table ORD_OrderMstr_5 alter column ProdLineType tinyint not null
go
alter table ORD_OrderMstr_6 alter column ProdLineType tinyint not null
go
alter table ORD_OrderMstr_7 alter column ProdLineType tinyint not null
go
alter table ORD_OrderMstr_8 alter column ProdLineType tinyint not null
go

alter table ORD_OrderMstr add PauseStatus tinyint
go
alter table ORD_OrderMstr_0 add PauseStatus tinyint
go
alter table ORD_OrderMstr_1 add PauseStatus tinyint
go
alter table ORD_OrderMstr_2 add PauseStatus tinyint
go
alter table ORD_OrderMstr_3 add PauseStatus tinyint
go
alter table ORD_OrderMstr_4 add PauseStatus tinyint
go
alter table ORD_OrderMstr_5 add PauseStatus tinyint
go
alter table ORD_OrderMstr_6 add PauseStatus tinyint
go
alter table ORD_OrderMstr_7 add PauseStatus tinyint
go
alter table ORD_OrderMstr_8 add PauseStatus tinyint
go
update ORD_OrderMstr set PauseStatus = 0
go
update ORD_OrderMstr_0 set PauseStatus = 0
go
update ORD_OrderMstr_1 set PauseStatus = 0
go
update ORD_OrderMstr_2 set PauseStatus = 0
go
update ORD_OrderMstr_3 set PauseStatus = 0
go
update ORD_OrderMstr_4 set PauseStatus = 0
go
update ORD_OrderMstr_5 set PauseStatus = 0
go
update ORD_OrderMstr_6 set PauseStatus = 0
go
update ORD_OrderMstr_7 set PauseStatus = 0
go
update ORD_OrderMstr_8 set PauseStatus = 0
go
alter table ORD_OrderMstr alter column PauseStatus tinyint not null
go
alter table ORD_OrderMstr_0 alter column PauseStatus tinyint not null
go
alter table ORD_OrderMstr_1 alter column PauseStatus tinyint not null
go
alter table ORD_OrderMstr_2 alter column PauseStatus tinyint not null
go
alter table ORD_OrderMstr_3 alter column PauseStatus tinyint not null
go
alter table ORD_OrderMstr_4 alter column PauseStatus tinyint not null
go
alter table ORD_OrderMstr_5 alter column PauseStatus tinyint not null
go
alter table ORD_OrderMstr_6 alter column PauseStatus tinyint not null
go
alter table ORD_OrderMstr_7 alter column PauseStatus tinyint not null
go
alter table ORD_OrderMstr_8 alter column PauseStatus tinyint not null
go

alter table ORD_OrderMstr add SeqGroup varchar(50)
go
alter table ORD_OrderMstr_0 add SeqGroup varchar(50)
go
alter table ORD_OrderMstr_1 add SeqGroup varchar(50)
go
alter table ORD_OrderMstr_2 add SeqGroup varchar(50)
go
alter table ORD_OrderMstr_3 add SeqGroup varchar(50)
go
alter table ORD_OrderMstr_4 add SeqGroup varchar(50)
go
alter table ORD_OrderMstr_5 add SeqGroup varchar(50)
go
alter table ORD_OrderMstr_6 add SeqGroup varchar(50)
go
alter table ORD_OrderMstr_7 add SeqGroup varchar(50)
go
alter table ORD_OrderMstr_8 add SeqGroup varchar(50)
go

alter table ORD_OrderOp drop column SAPOp
go
alter table ORD_OrderOp drop column IsBackFlush
go
alter table ORD_OrderOp drop column IsReport
go
alter table ORD_OrderOp add IsAutoReport bit
go
update ORD_OrderOp set IsAutoReport = 1
go
alter table ORD_OrderOp alter column IsAutoReport bit not null
go
alter table ORD_OrderOp add ReportQty decimal(18, 8)
go
update ORD_OrderOp set ReportQty = 0
go
alter table ORD_OrderOp alter column ReportQty decimal(18, 8) not null
go
alter table ORD_OrderOp add BackflushQty decimal(18, 8)
go
update ORD_OrderOp set BackflushQty = 0
go
alter table ORD_OrderOp alter column BackflushQty decimal(18, 8) not null
go
alter table ORD_OrderOp add AUFPL varchar(50)
go
alter table ORD_OrderOp add PLNFL varchar(50)
go
alter table ORD_OrderOp add VORNR varchar(50)
go
alter table ORD_OrderOp add NeedReport bit
go
update ORD_OrderOp set NeedReport = 1
go
alter table ORD_OrderOp alter column NeedReport bit not null
go
alter table ORD_OrderOp add IsRecFG bit
go
update ORD_OrderOp set IsRecFG = 1
go
alter table ORD_OrderOp alter column IsRecFG bit not null
go
alter table ORD_OrderOp add ScrapQty decimal(18, 8)
go
update ORD_OrderOp set ScrapQty = 0
go
alter table ORD_OrderOp alter column ScrapQty decimal(18, 8) not null
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_OrderOpCPTime]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_OrderOpCPTime]
GO

CREATE TABLE [dbo].[ORD_OrderOpCPTime](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](50) NULL,
	[VanProdLine] [varchar](50) NULL,
	[AssProdLine] [varchar](50) NULL,
	[Seq] [bigint] NULL,
	[SubSeq] [int] NULL,
	[Op] [int] NULL,
	[OpTaktTime] [int] NULL,
	[CPTime] [datetime] NULL,
	[CreateDate] [datetime] NOT NULL,
	[VanOp] [int] NULL,
	[AssOp] [int] NULL,
 CONSTRAINT [PK_ORD_OrderOpCPTime] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ORD_OrderOpCPTime] ADD  CONSTRAINT [DF_ORD_OrderOpCPTime_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_OrderOpReport]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_OrderOpReport]
GO

CREATE TABLE [dbo].[ORD_OrderOpReport](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
	[OrderDetId] [int] NOT NULL,
	[OrderOpId] [int] NOT NULL,
	[Op] [int] NOT NULL,
	[ReportQty] [decimal](18, 8) NOT NULL,
	[ScrapQty] [decimal](18, 8) NOT NULL,
	[BackflushQty] [decimal](18, 8) NOT NULL,
	[WorkCenter] [varchar](50) NULL,
	[Status] [tinyint] NOT NULL,
	[EffDate] [datetime] NOT NULL,
	[ReceiptNo] [varchar](50) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[CancelDate] [datetime] NULL,
	[CancelUser] [int] NULL,
	[CancelUserNm] [varchar](100) NULL,
	[Version] [int] NOT NULL,
 CONSTRAINT [PK_ORD_OrderOpReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_OrderSeq]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_OrderSeq]
GO

CREATE TABLE [dbo].[ORD_OrderSeq](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[TraceCode] [varchar](50) NULL,
	[Seq] [bigint] NOT NULL,
	[SapSeq] [bigint] NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Version] [int] NOT NULL,
	[OrderNo] [varchar](50) NULL,
	[SubSeq] [int] NOT NULL,
 CONSTRAINT [PK_ORD_ORDERSEQ] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_PickHu]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_PickHu]
GO

CREATE TABLE [dbo].[ORD_PickHu](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PickId] [varchar](50) NOT NULL,
	[RepackHu] [varchar](50) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ORD_PickHu] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_PickResult]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_PickResult]
GO

CREATE TABLE [dbo].[ORD_PickResult](
	[ResultId] [varchar](50) NOT NULL,
	[PickId] [varchar](50) NOT NULL,
	[PickedHu] [varchar](50) NULL,
	[HuQty] [decimal](18, 8) NULL,
	[PickedQty] [decimal](18, 8) NULL,
	[Picker] [varchar](50) NULL,
	[PickDate] [datetime] NULL,
	[AsnNo] [varchar](50) NULL,
	[Memo] [varchar](256) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ORD_PickResult] PRIMARY KEY CLUSTERED 
(
	[ResultId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_PickTask]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_PickTask]
GO

CREATE TABLE [dbo].[ORD_PickTask](
	[PickId] [varchar](50) NOT NULL,
	[OrderNo] [varchar](50) NOT NULL,
	[OrdDetId] [int] NOT NULL,
	[DemandType] [tinyint] NOT NULL,
	[IsHold] [bit] NOT NULL,
	[Flow] [varchar](50) NULL,
	[FlowDesc] [varchar](100) NULL,
	[Item] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[Uom] [varchar](5) NULL,
	[BaseUom] [varchar](5) NULL,
	[PartyFrom] [varchar](50) NULL,
	[PartyFromName] [varchar](100) NULL,
	[PartyTo] [varchar](50) NULL,
	[PartyToName] [varchar](100) NULL,
	[LocationFrom] [varchar](50) NULL,
	[LocationFromName] [varchar](100) NULL,
	[LocationTo] [varchar](50) NULL,
	[LocationToName] [varchar](100) NULL,
	[WindowTime] [datetime] NULL,
	[ReleaseDate] [datetime] NULL,
	[Supplier] [varchar](50) NULL,
	[SupplierName] [varchar](100) NULL,
	[UnitCount] [decimal](18, 8) NULL,
	[OrderedQty] [decimal](18, 8) NULL,
	[PickedQty] [decimal](18, 8) NULL,
	[Picker] [varchar](50) NULL,
	[PrintCount] [int] NOT NULL,
	[Memo] [varchar](256) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ORD_PickTask] PRIMARY KEY CLUSTERED 
(
	[PickId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ORD_ShipList]') AND type in (N'U'))
DROP TABLE [dbo].[ORD_ShipList]
GO

CREATE TABLE [dbo].[ORD_ShipList](
	[ShipNo] [varchar](50) NOT NULL,
	[Vehicle] [varchar](50) NULL,
	[Shipper] [varchar](50) NULL,
	[Status] [tinyint] NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[CloseDate] [datetime] NULL,
	[CloseUser] [int] NULL,
	[CloseUserNm] [varchar](100) NULL,
	[CancelDate] [datetime] NULL,
	[CancelUser] [int] NULL,
	[CancelUserNm] [varchar](100) NULL,
 CONSTRAINT [PK_ORD_ShipList] PRIMARY KEY CLUSTERED 
(
	[ShipNo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

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

alter table PRD_RoutingDet drop column LeadTime
go
alter table PRD_RoutingDet drop column TimeUnit
go
alter table PRD_RoutingDet drop column StartDate
go
alter table PRD_RoutingDet drop column EndDate
go
alter table PRD_RoutingDet add Capacity int default 0
go

alter table PRD_RoutingMstr drop CONSTRAINT FK_PRD_ROUT_REFERENCE_MD_REGIO
go
alter table PRD_RoutingMstr drop column Region
go
alter table PRD_RoutingMstr drop column TaktTime
go
alter table PRD_RoutingMstr drop column TaktTimeUnit
go
alter table PRD_RoutingMstr drop column WaitTime
go
alter table PRD_RoutingMstr drop column WaitTimeUnit
go

alter table PRD_ShiftDet drop column ShiftTime
go
alter table PRD_ShiftDet drop column StartDate
go
alter table PRD_ShiftDet drop column EndDate
go
alter table PRD_ShiftDet add StartTime varchar(5)
go
alter table PRD_ShiftDet add EndTime varchar(5)
go
alter table PRD_ShiftDet add IsOvernight int default 0 not null
go
alter table PRD_ShiftDet add Seq int default 0 not null
go

alter table PRD_ShiftMstr add ShiftCount int default 0 not null
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PRD_StandardWorkingCalendar]') AND type in (N'U'))
DROP TABLE [dbo].[PRD_StandardWorkingCalendar]
GO

CREATE TABLE [dbo].[PRD_StandardWorkingCalendar](
	[Id] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Region] [varchar](50) NULL,
	[Shift] [varchar](50) NOT NULL,
	[DayOfWeek] [tinyint] NOT NULL,
	[Type] [tinyint] NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[RegionName] [varchar](50) NULL,
	[Category] [tinyint] NOT NULL,
	[ProdLine] [varchar](50) NULL,
 CONSTRAINT [PK_PRD_STANDARDWORKINGCALENDAR] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PRD_StandardWorkingCalendar]  WITH CHECK ADD  CONSTRAINT [FK_PRD_STAN_REFERENCE_MD_REGIO] FOREIGN KEY([Region])
REFERENCES [dbo].[MD_Region] ([Code])
GO

ALTER TABLE [dbo].[PRD_StandardWorkingCalendar]  WITH CHECK ADD  CONSTRAINT [FK_PRD_STAN_REFERENCE_PRD_SHIF] FOREIGN KEY([Shift])
REFERENCES [dbo].[PRD_ShiftMstr] ([Code])
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PRD_WorkingCalendar]') AND type in (N'U'))
DROP TABLE [dbo].[PRD_WorkingCalendar]
GO

CREATE TABLE [dbo].[PRD_WorkingCalendar](
	[Id] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Region] [varchar](50) NULL,
	[Shift] [varchar](50) NOT NULL,
	[WorkingDate] [date] NOT NULL,
	[Type] [tinyint] NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[DayOfWeek] [tinyint] NOT NULL,
	[RegionName] [varchar](50) NULL,
	[Category] [tinyint] NOT NULL,
	[ProdLine] [varchar](50) NULL,
 CONSTRAINT [PK_PRD_WORKINGCALENDAR] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PRD_WorkingCalendar]  WITH CHECK ADD  CONSTRAINT [FK_PRD_WORK_REFERENCE_MD_REGIO] FOREIGN KEY([Region])
REFERENCES [dbo].[MD_Region] ([Code])
GO

ALTER TABLE [dbo].[PRD_WorkingCalendar]  WITH CHECK ADD  CONSTRAINT [FK_PRD_WORKingCalendar_REFERENCE_PRD_SHIFMSTR_CODE] FOREIGN KEY([Shift])
REFERENCES [dbo].[PRD_ShiftMstr] ([Code])
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_AlterDO]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_AlterDO]
GO

CREATE TABLE [dbo].[SAP_AlterDO](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](10) NULL,
	[Sequence] [int] NULL,
	[Item] [varchar](18) NULL,
	[Plant] [char](4) NULL,
	[Location] [char](4) NULL,
	[Qty] [decimal](18, 8) NULL,
	[Uom] [char](3) NULL,
	[KUNAG] [varchar](20) NULL,
	[KUNNR] [varchar](20) NULL,
	[Action] [char](1) NULL,
	[Status] [tinyint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[WindowTime] [datetime] NULL,
	[ExternalOrderno] [varchar](50) NULL,
 CONSTRAINT [PK_SAP_DeliveryOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_CancelProdOpReport]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_CancelProdOpReport]
GO

CREATE TABLE [dbo].[SAP_CancelProdOpReport](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AUFNR] [varchar](50) NOT NULL,
	[TEXT] [varchar](50) NOT NULL,
	[Status] [tinyint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[RecNo] [varchar](50) NULL,
	[OrderNo] [varchar](50) NULL,
	[OrderOpId] [int] NULL,
 CONSTRAINT [PK_SAP_CancelProdOpReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_InvLoc]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_InvLoc]
GO

CREATE TABLE [dbo].[SAP_InvLoc](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SourceType] [int] NOT NULL,
	[SourceId] [bigint] NOT NULL,
	[FRBNR] [varchar](16) NOT NULL,
	[SGTXT] [varchar](50) NOT NULL,
	[CreateUser] [varchar](50) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_InvLoc] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SAP_InvLoc] ADD  CONSTRAINT [DF_SAP_InvLoc_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_InvTrans]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_InvTrans]
GO

CREATE TABLE [dbo].[SAP_InvTrans](
	[TCODE] [varchar](20) NULL,
	[BWART] [varchar](3) NULL,
	[BLDAT] [char](8) NULL,
	[BUDAT] [char](8) NULL,
	[EBELN] [varchar](50) NULL,
	[EBELP] [varchar](5) NULL,
	[VBELN] [varchar](10) NULL,
	[POSNR] [varchar](6) NULL,
	[LIFNR] [varchar](10) NULL,
	[WERKS] [varchar](4) NULL,
	[LGORT] [varchar](4) NULL,
	[SOBKZ] [varchar](1) NULL,
	[MATNR] [varchar](18) NULL,
	[ERFMG] [decimal](18, 2) NULL,
	[ERFME] [varchar](3) NULL,
	[UMLGO] [varchar](4) NULL,
	[GRUND] [varchar](4) NULL,
	[KOSTL] [varchar](10) NULL,
	[XBLNR] [varchar](50) NULL,
	[RSNUM] [varchar](10) NULL,
	[RSPOS] [varchar](4) NULL,
	[FRBNR] [varchar](50) NOT NULL,
	[SGTXT] [varchar](50) NOT NULL,
	[OLD] [varchar](3) NULL,
	[INSMK] [varchar](1) NULL,
	[XABLN] [varchar](10) NULL,
	[AUFNR] [varchar](50) NULL,
	[UMMAT] [varchar](18) NULL,
	[UMWRK] [varchar](4) NULL,
	[POSID] [varchar](24) NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Status] [tinyint] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[BatchNo] [int] NOT NULL,
	[CHARG] [varchar](50) NULL,
	[KZEAR] [varchar](1) NULL,
	[ErrorId] [int] NULL,
	[ErrorMessage] [varchar](256) NULL,
	[OrderNo] [varchar](50) NULL,
	[DetailId] [int] NULL,
 CONSTRAINT [PK_InvTrans] PRIMARY KEY CLUSTERED 
(
	[FRBNR] ASC,
	[SGTXT] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SAP_InvTrans] ADD  CONSTRAINT [DF_SAP_InvTrans_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_Item]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_Item]
GO

CREATE TABLE [dbo].[SAP_Item](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](20) NULL,
	[RefCode] [varchar](20) NULL,
	[ShortCode] [varchar](10) NULL,
	[Desc1] [nvarchar](60) NULL,
	[Uom] [varchar](5) NULL,
	[Plant] [varchar](4) NULL,
	[DISPO] [varchar](50) NULL,
	[PLIFZ] [varchar](50) NULL,
	[CreateDate] [datetime] NULL DEFAULT(GETDATE()),
	[BatchNo] [int] NULL
 CONSTRAINT [PK_SAP_Item] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_MapMoveTypeTCode]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_MapMoveTypeTCode]
GO

CREATE TABLE [dbo].[SAP_MapMoveTypeTCode](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BWART] [char](3) NOT NULL,
	[SOBKZ] [char](1) NULL,
	[TCode] [varchar](10) NOT NULL,
	[Description] [varchar](100) NULL,
 CONSTRAINT [PK_SAP_MapMoveTypeTCode] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_PostDO]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_PostDO]
GO

CREATE TABLE [dbo].[SAP_PostDO](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderNo] [varchar](50) NULL,
	[ReceiptNo] [varchar](50) NULL,
	[Result] [varchar](500) NULL,
	[ZTCODE] [varchar](50) NULL,
	[Success] [varchar](50) NULL,
	[LastModifyDate] [datetime] NULL,
	[Status] [int] NULL,
	[CreateDate] [datetime] NULL,
	[ErrorCount] [int] NULL,
 CONSTRAINT [PK_SAP_PostDO] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_ProdBomDet]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_ProdBomDet]
GO

CREATE TABLE [dbo].[SAP_ProdBomDet](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[BatchNo] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[AUFNR] [varchar](50) NULL,
	[RSNUM] [int] NULL,
	[RSPOS] [int] NULL,
	[WERKS] [varchar](50) NULL,
	[MATERIAL] [varchar](50) NULL,
	[BISMT] [varchar](50) NULL,
	[MAKTX] [varchar](100) NULL,
	[DISPO] [varchar](50) NULL,
	[BESKZ] [varchar](50) NULL,
	[SOBSL] [varchar](50) NULL,
	[MEINS] [varchar](50) NULL,
	[MDMNG] [decimal](18, 8) NULL,
	[LGORT] [varchar](50) NULL,
	[BWART] [varchar](50) NULL,
	[AUFPL] [int] NULL,
	[PLNFL] [varchar](50) NULL,
	[VORNR] [varchar](50) NULL,
	[GW] [varchar](50) NULL,
	[WZ] [varchar](50) NULL,
	[ZOPID] [varchar](50) NULL,
	[ZOPDS] [varchar](50) NULL,
	[LIFNR] [varchar](50) NULL,
	[ICHARG] [varchar](50) NULL,
 CONSTRAINT [PK_SAP_PRODBOMDET] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SAP_ProdBomDet] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_ProdOpReport]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_ProdOpReport]
GO

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
	[TEXT] [varchar](50) NULL,
	[IsCancel] [bit] NOT NULL,
	[OrderNo] [varchar](50) NULL,
	[OrderOpId] [int] NULL,
	[RecNo] [varchar](50) NULL,
	[EffDate] [datetime] NOT NULL,
	[CreateUser] [int] NULL,
	[CreateUserNm] [varchar](50) NULL,
	[LastModifyUser] [int] NULL,
	[LastModifyUserNm] [varchar](50) NULL,
	[Version] [int] NOT NULL,
	[ProdLine] [varchar](50) NULL,
	[OrderOpReportId] [int] NULL,
 CONSTRAINT [PK_SAP_ProdOpReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SAP_ProdOpReport] ADD  CONSTRAINT [DF_SAP_ProdOpReport_Status]  DEFAULT ((0)) FOR [Status]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_ProdOrder]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_ProdOrder]
GO

CREATE TABLE [dbo].[SAP_ProdOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BatchNo] [int] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[AUFNR] [varchar](50) NULL,
	[WERKS] [varchar](50) NULL,
	[DAUAT] [varchar](50) NULL,
	[MATNR] [varchar](50) NULL,
	[MAKTX] [varchar](50) NULL,
	[DISPO] [varchar](50) NULL,
	[CHARG] [varchar](50) NULL,
	[GSTRS] [datetime] NULL,
	[CY_SEQNR] [bigint] NULL,
	[GMEIN] [varchar](50) NULL,
	[GAMNG] [decimal](18, 8) NULL,
	[LGORT] [varchar](50) NULL,
	[LTEXT] [varchar](50) NULL,
	[ZLINE] [varchar](50) NULL,
	[RSNUM] [int] NULL,
	[AUFPL] [int] NULL,
 CONSTRAINT [PK_SAP_PRODORDER] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SAP_ProdOrder] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_ProdRoutingDet]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_ProdRoutingDet]
GO

CREATE TABLE [dbo].[SAP_ProdRoutingDet](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BatchNo] [int] NOT NULL,
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
 CONSTRAINT [PK_SAP_PRODROUTINGDET] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SAP_ProdRoutingDet] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_ProdSeqReport]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_ProdSeqReport]
GO

CREATE TABLE [dbo].[SAP_ProdSeqReport](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AUFNR] [varchar](50) NOT NULL,
	[WORKCENTER] [varchar](50) NOT NULL,
	[GAMNG] [decimal](18, 8) NOT NULL,
	[Status] [tinyint] NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[ErrorCount] [int] NOT NULL,
	[SCRAP] [decimal](18, 8) NOT NULL,
	[TEXT] [varchar](50) NOT NULL,
 CONSTRAINT [PK_SAP_ProdSeqReport] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_Supplier]') AND type in (N'U'))
	DROP TABLE [dbo].[SAP_Supplier]
GO

CREATE TABLE [dbo].[SAP_Supplier](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](20) NULL,
	[ShortCode] [varchar](20) NULL,
	[Name] [nvarchar](60) NULL,
	[CreateDate] [datetime] NULL DEFAULT(GETDATE()),
	[BatchNo] [int] NULL
 CONSTRAINT [PK_SAP_Supplier] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_TableIndex]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_TableIndex]
GO

CREATE TABLE [dbo].[SAP_TableIndex](
	[TableName] [varchar](50) NOT NULL,
	[Id] [bigint] NULL,
	[LastModifyDate] [datetime] NULL,
 CONSTRAINT [PK_TableIndex] PRIMARY KEY CLUSTERED 
(
	[TableName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SAP_TransCallBack]') AND type in (N'U'))
DROP TABLE [dbo].[SAP_TransCallBack]
GO

CREATE TABLE [dbo].[SAP_TransCallBack](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[FRBNR] [varchar](16) NULL,
	[SGTXT] [varchar](50) NULL,
	[MBLNR] [varchar](10) NULL,
	[ZEILE] [varchar](4) NULL,
	[BUDAT] [char](14) NULL,
	[CPUDT] [char](14) NULL,
	[MTYPE] [varchar](4) NULL,
	[MSTXT] [nvarchar](220) NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_TransCallBack] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SAP_TransCallBack] ADD  CONSTRAINT [DF_SAP_TransCallBack_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

truncate table SAP_CancelProdOpReport
go
truncate table SAP_ProdOpReport
go
truncate table SAP_TableIndex
go
truncate table SAP_MapMoveTypeTCode
go
truncate table SAP_InvLoc
go
truncate table SAP_InvTrans
go
truncate table SAP_TransCallBack
go
insert into SAP_CancelProdOpReport(AUFNR, [TEXT], [Status], CreateDate, LastModifyDate, ErrorCount) select AUFNR, [TEXT], [Status], CreateDate, LastModifyDate, ErrorCount from sconit5_si.dbo.SI_SAP_CancelProdSeqReport
go
insert into SAP_ProdOpReport(AUFNR, WORKCENTER, GAMNG, Status, CreateDate, LastModifyDate, EffDate, ErrorCount, SCRAP, [Text], IsCancel, [Version]) select AUFNR, WORKCENTER, GAMNG, Status, CreateDate, LastModifyDate, CreateDate, ErrorCount, SCRAP, [Text], 1, 1 from sconit5_si.dbo.SI_SAP_ProdSeqReport
go
insert into SAP_TableIndex(TableName, Id, LastModifyDate) select TableName, Id, LastModifyDate from sconit5_si.dbo.SI_SAP_TableIndex
go
insert into SAP_MapMoveTypeTCode(BWART, SOBKZ, TCode, Description) select BWART, SOBKZ, TCode, Description from sconit5_si.dbo.SI_SAP_MapMoveTypeTCode
go
set identity_insert SAP_InvLoc on
insert into SAP_InvLoc(Id, SourceType, SourceId, FRBNR, SGTXT, CreateUser, CreateDate) select Id, SourceType, SourceId, FRBNR, SGTXT, CreateUser, CreateDate from sconit5_si.dbo.SI_SAP_InvLoc
set identity_insert SAP_InvLoc off
go
INSERT INTO SAP_InvTrans (TCODE, BWART, BLDAT, BUDAT, EBELN, EBELP, VBELN, POSNR, LIFNR, WERKS, LGORT, SOBKZ, MATNR, ERFMG, ERFME, UMLGO, GRUND, KOSTL, XBLNR, RSNUM, RSPOS, FRBNR, SGTXT, OLD, INSMK, XABLN, AUFNR, UMMAT, UMWRK, POSID, CreateDate, LastModifyDate, Status, ErrorCount, BatchNo, CHARG, KZEAR, ErrorId, ErrorMessage, OrderNo, DetailId)
select TCODE, BWART, BLDAT, BUDAT, EBELN, EBELP, VBELN, POSNR, LIFNR, WERKS, LGORT, SOBKZ, MATNR, ERFMG, ERFME, UMLGO, GRUND, KOSTL, XBLNR, RSNUM, RSPOS, FRBNR, SGTXT, OLD, INSMK, XABLN, AUFNR, UMMAT, UMWRK, POSID, CreateDate, LastModifyDate, Status, ErrorCount, BatchNo, CHARG, KZEAR, ErrorId, ErrorMessage, OrderNo, DetailId from sconit5_si.dbo.SI_SAP_InvTrans
go
set identity_insert SAP_TransCallBack on
insert into SAP_TransCallBack(Id, FRBNR, SGTXT, MBLNR, ZEILE, BUDAT, CPUDT, MTYPE, MSTXT, CreateDate) select Id, FRBNR, SGTXT, MBLNR, ZEILE, BUDAT, CPUDT, MTYPE, MSTXT, CreateDate from sconit5_si.dbo.SI_SAP_TransCallBack
set identity_insert SAP_TransCallBack off
go

alter table SCM_FlowBinding add JointOp int
go
alter table SCM_FlowBinding add LeadTime decimal(18, 8)
go

alter table SCM_FlowDet add ManufactureParty varchar(50)
go
alter table SCM_FlowDet add MrpTag varchar(50)
go
alter table SCM_FlowDet add IsCreatePickList bit
go

alter table SCM_FlowMstr add ProdLineType tinyint default 0 not null
go
alter table SCM_FlowMstr add TaktTime int
go
alter table SCM_FlowMstr add VirtualOpRef varchar(50)
go

alter table SCM_FlowStrategy drop column WaitTime
go
alter table SCM_FlowStrategy drop column WaitBatch
go
alter table SCM_FlowStrategy alter column [WinTimeDiff] [decimal] (18, 8) NOT NULL
go
alter table SCM_FlowStrategy add [SeqGroup] [varchar] (50)
go
alter table SCM_FlowStrategy add [QiTiaoBian] [varchar] (10)
go
alter table SCM_FlowStrategy add [SupplierGroup] [varchar] (50)
go
alter table SCM_FlowStrategy add [KBCalc] [tinyint] NULL
go
alter table SCM_FlowStrategy add [SupplierGroupSeq] [int] NULL
go
alter table SCM_FlowStrategy add [NextWinTime2] [datetime] NULL
go
alter table SCM_FlowStrategy add [PreOrderTime] [datetime] NULL
go
alter table SCM_FlowStrategy add [PreWinTime] [datetime] NULL
go
alter table SCM_FlowStrategy add [PreWinTime2] [datetime] NULL
go
alter table SCM_FlowStrategy add [WaitTime] [decimal] (18, 8) NULL
go
alter table SCM_FlowStrategy add [WaitBatch] [int] NULL
go
alter table SCM_FlowStrategy add [WinTimeInterval] [decimal] (18, 8) NULL
go

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SCM_SeqGroup]') AND type in (N'U'))
DROP TABLE [dbo].[SCM_SeqGroup]
GO

CREATE TABLE [dbo].[SCM_SeqGroup](
	[Code] [varchar](50) NOT NULL,
	[ProdLine] [varchar](50) NOT NULL,
	[SeqBatch] [int] NOT NULL,
	[PrevOrderNo] [varchar](50) NULL,
	[PrevTraceCode] [varchar](50) NULL,
	[PrevSeq] [bigint] NULL,
	[PrevSubSeq] [int] NULL,
	[PrevDeliveryDate] [date] NULL,
	[PrevDeliveryCount] [int] NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
	[Version] [int] NOT NULL,
	[OpRef] [varchar](50) NOT NULL,
 CONSTRAINT [PK_SCM_SEQGROUP] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SYS_MsgSubscirber]') AND type in (N'U'))
DROP TABLE [dbo].[SYS_MsgSubscirber]
GO

CREATE TABLE [dbo].[SYS_MsgSubscirber](
	[Id] [int] NOT NULL,
	[Desc1] [nvarchar](100) NOT NULL,
	[Emails] [varchar](500) NULL,
	[Mobiles] [varchar](500) NULL,
	[MaxMsgSize] int NOT NULL
 CONSTRAINT [PK_SYS_MsgSubscirber] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10101, '导入SAP物料失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10201, '导入SAP供应商失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10301, '导入SAP整车生产单失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10801, '导出生产单报工失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10901, '创建交货单失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10902, '调用SAP交货单过账失败 ', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(11301, '生成库存报表失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10701, '导入移动类型连接Web服务失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10702, '移动类型导入到中间表失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10703, '导入移动类型读取LES数据失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10704, '更新移动类型中间表失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10705, '导入移动类型记录表之间关系失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10706, '导入移动类型记录SAP返回结果失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) values(10707, '导入盘点移动类型中失败', 'dingxin@sconit.com;jienyuan@sconit.com', null, 10)
--insert into SYS_MsgSubscirber(Id, Desc1, Emails, Mobiles, MaxMsgSize) select Id, Descritpion, Emails, Mobiles, 10 from sconit5_si.dbo.SI_LogToUser
go

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VIEW_OrderMstr]'))
DROP VIEW [dbo].[VIEW_OrderMstr]
GO

CREATE VIEW [dbo].[VIEW_OrderMstr]
AS
SELECT     om.OrderNo, om.Flow, om.ProdLineFact, om.TraceCode, om.OrderStrategy, om.RefOrderNo, om.ExtOrderNo, om.Type, om.SubType, om.QualityType, om.StartTime, 
                      om.WindowTime, om.PauseSeq, om.PauseTime, om.EffDate, om.IsQuick, om.Priority, om.Status, om.PartyFrom, om.PartyFromNm, om.PartyTo, om.PartyToNm, 
                      om.ShipFrom, om.ShipFromAddr, om.ShipFromTel, om.ShipFromCell, om.ShipFromFax, om.ShipFromContact, om.ShipTo, om.ShipToAddr, om.ShipToTel, 
                      om.ShipToCell, om.ShipToFax, om.ShipToContact, om.Shift, om.LocFrom, om.LocFromNm, om.LocTo, om.LocToNm, om.IsInspect, om.BillAddr, om.BillAddrDesc, 
                      om.PriceList, om.Currency, om.Dock, om.Routing, om.CurtOp, om.IsAutoRelease, om.IsAutoStart, om.IsAutoShip, om.IsAutoReceive, om.IsAutoBill, 
                      om.IsManualCreateDet, om.IsListPrice, om.IsPrintOrder, om.IsOrderPrinted, om.IsPrintAsn, om.IsPrintRec, om.IsShipExceed, om.IsRecExceed, om.IsOrderFulfillUC, 
                      om.IsShipFulfillUC, om.IsRecFulfillUC, om.IsShipScanHu, om.IsRecScanHu, om.IsCreatePL, om.IsPLCreate, om.IsShipFifo, om.IsRecFifo, om.IsShipByOrder, 
                      om.IsOpenOrder, om.IsAsnUniqueRec, om.RecGapTo, om.RecTemplate, om.OrderTemplate, om.AsnTemplate, om.HuTemplate, om.BillTerm, om.CreateHuOpt, 
                      om.ReCalculatePriceOpt, om.PickStrategy, om.ExtraDmdSource, om.CreateUser, om.CreateUserNm, om.CreateDate, om.LastModifyUser, om.LastModifyUserNm, 
                      om.LastModifyDate, om.ReleaseDate, om.ReleaseUser, om.ReleaseUserNm, om.StartDate, om.StartUser, om.StartUserNm, om.CompleteDate, om.CompleteUser, 
                      om.CompleteUserNm, om.CloseDate, om.CloseUser, om.CloseUserNm, om.CancelDate, om.CancelUser, om.CancelUserNm, om.CancelReason, om.Version, 
                      om.FlowDesc, om.WMSNo, om.ProdLineType, om.PauseStatus, om.SeqGroup
FROM         Ord_OrderMstr_0 AS om
UNION ALL
SELECT     om.OrderNo, om.Flow, om.ProdLineFact, om.TraceCode, om.OrderStrategy, om.RefOrderNo, om.ExtOrderNo, om.Type, om.SubType, om.QualityType, om.StartTime, 
                      om.WindowTime, om.PauseSeq, om.PauseTime, om.EffDate, om.IsQuick, om.Priority, om.Status, om.PartyFrom, om.PartyFromNm, om.PartyTo, om.PartyToNm, 
                      om.ShipFrom, om.ShipFromAddr, om.ShipFromTel, om.ShipFromCell, om.ShipFromFax, om.ShipFromContact, om.ShipTo, om.ShipToAddr, om.ShipToTel, 
                      om.ShipToCell, om.ShipToFax, om.ShipToContact, om.Shift, om.LocFrom, om.LocFromNm, om.LocTo, om.LocToNm, om.IsInspect, om.BillAddr, om.BillAddrDesc, 
                      om.PriceList, om.Currency, om.Dock, om.Routing, om.CurtOp, om.IsAutoRelease, om.IsAutoStart, om.IsAutoShip, om.IsAutoReceive, om.IsAutoBill, 
                      om.IsManualCreateDet, om.IsListPrice, om.IsPrintOrder, om.IsOrderPrinted, om.IsPrintAsn, om.IsPrintRec, om.IsShipExceed, om.IsRecExceed, om.IsOrderFulfillUC, 
                      om.IsShipFulfillUC, om.IsRecFulfillUC, om.IsShipScanHu, om.IsRecScanHu, om.IsCreatePL, om.IsPLCreate, om.IsShipFifo, om.IsRecFifo, om.IsShipByOrder, 
                      om.IsOpenOrder, om.IsAsnUniqueRec, om.RecGapTo, om.RecTemplate, om.OrderTemplate, om.AsnTemplate, om.HuTemplate, om.BillTerm, om.CreateHuOpt, 
                      om.ReCalculatePriceOpt, om.PickStrategy, om.ExtraDmdSource, om.CreateUser, om.CreateUserNm, om.CreateDate, om.LastModifyUser, om.LastModifyUserNm, 
                      om.LastModifyDate, om.ReleaseDate, om.ReleaseUser, om.ReleaseUserNm, om.StartDate, om.StartUser, om.StartUserNm, om.CompleteDate, om.CompleteUser, 
                      om.CompleteUserNm, om.CloseDate, om.CloseUser, om.CloseUserNm, om.CancelDate, om.CancelUser, om.CancelUserNm, om.CancelReason, om.Version, 
                      om.FlowDesc, om.WMSNo, om.ProdLineType, om.PauseStatus, om.SeqGroup
FROM         Ord_OrderMstr_1 AS om
UNION ALL
SELECT     om.OrderNo, om.Flow, om.ProdLineFact, om.TraceCode, om.OrderStrategy, om.RefOrderNo, om.ExtOrderNo, om.Type, om.SubType, om.QualityType, om.StartTime, 
                      om.WindowTime, om.PauseSeq, om.PauseTime, om.EffDate, om.IsQuick, om.Priority, om.Status, om.PartyFrom, om.PartyFromNm, om.PartyTo, om.PartyToNm, 
                      om.ShipFrom, om.ShipFromAddr, om.ShipFromTel, om.ShipFromCell, om.ShipFromFax, om.ShipFromContact, om.ShipTo, om.ShipToAddr, om.ShipToTel, 
                      om.ShipToCell, om.ShipToFax, om.ShipToContact, om.Shift, om.LocFrom, om.LocFromNm, om.LocTo, om.LocToNm, om.IsInspect, om.BillAddr, om.BillAddrDesc, 
                      om.PriceList, om.Currency, om.Dock, om.Routing, om.CurtOp, om.IsAutoRelease, om.IsAutoStart, om.IsAutoShip, om.IsAutoReceive, om.IsAutoBill, 
                      om.IsManualCreateDet, om.IsListPrice, om.IsPrintOrder, om.IsOrderPrinted, om.IsPrintAsn, om.IsPrintRec, om.IsShipExceed, om.IsRecExceed, om.IsOrderFulfillUC, 
                      om.IsShipFulfillUC, om.IsRecFulfillUC, om.IsShipScanHu, om.IsRecScanHu, om.IsCreatePL, om.IsPLCreate, om.IsShipFifo, om.IsRecFifo, om.IsShipByOrder, 
                      om.IsOpenOrder, om.IsAsnUniqueRec, om.RecGapTo, om.RecTemplate, om.OrderTemplate, om.AsnTemplate, om.HuTemplate, om.BillTerm, om.CreateHuOpt, 
                      om.ReCalculatePriceOpt, om.PickStrategy, om.ExtraDmdSource, om.CreateUser, om.CreateUserNm, om.CreateDate, om.LastModifyUser, om.LastModifyUserNm, 
                      om.LastModifyDate, om.ReleaseDate, om.ReleaseUser, om.ReleaseUserNm, om.StartDate, om.StartUser, om.StartUserNm, om.CompleteDate, om.CompleteUser, 
                      om.CompleteUserNm, om.CloseDate, om.CloseUser, om.CloseUserNm, om.CancelDate, om.CancelUser, om.CancelUserNm, om.CancelReason, om.Version, 
                      om.FlowDesc, om.WMSNo, om.ProdLineType, om.PauseStatus, om.SeqGroup
FROM         Ord_OrderMstr_2 AS om
UNION ALL
SELECT     om.OrderNo, om.Flow, om.ProdLineFact, om.TraceCode, om.OrderStrategy, om.RefOrderNo, om.ExtOrderNo, om.Type, om.SubType, om.QualityType, om.StartTime, 
                      om.WindowTime, om.PauseSeq, om.PauseTime, om.EffDate, om.IsQuick, om.Priority, om.Status, om.PartyFrom, om.PartyFromNm, om.PartyTo, om.PartyToNm, 
                      om.ShipFrom, om.ShipFromAddr, om.ShipFromTel, om.ShipFromCell, om.ShipFromFax, om.ShipFromContact, om.ShipTo, om.ShipToAddr, om.ShipToTel, 
                      om.ShipToCell, om.ShipToFax, om.ShipToContact, om.Shift, om.LocFrom, om.LocFromNm, om.LocTo, om.LocToNm, om.IsInspect, om.BillAddr, om.BillAddrDesc, 
                      om.PriceList, om.Currency, om.Dock, om.Routing, om.CurtOp, om.IsAutoRelease, om.IsAutoStart, om.IsAutoShip, om.IsAutoReceive, om.IsAutoBill, 
                      om.IsManualCreateDet, om.IsListPrice, om.IsPrintOrder, om.IsOrderPrinted, om.IsPrintAsn, om.IsPrintRec, om.IsShipExceed, om.IsRecExceed, om.IsOrderFulfillUC, 
                      om.IsShipFulfillUC, om.IsRecFulfillUC, om.IsShipScanHu, om.IsRecScanHu, om.IsCreatePL, om.IsPLCreate, om.IsShipFifo, om.IsRecFifo, om.IsShipByOrder, 
                      om.IsOpenOrder, om.IsAsnUniqueRec, om.RecGapTo, om.RecTemplate, om.OrderTemplate, om.AsnTemplate, om.HuTemplate, om.BillTerm, om.CreateHuOpt, 
                      om.ReCalculatePriceOpt, om.PickStrategy, om.ExtraDmdSource, om.CreateUser, om.CreateUserNm, om.CreateDate, om.LastModifyUser, om.LastModifyUserNm, 
                      om.LastModifyDate, om.ReleaseDate, om.ReleaseUser, om.ReleaseUserNm, om.StartDate, om.StartUser, om.StartUserNm, om.CompleteDate, om.CompleteUser, 
                      om.CompleteUserNm, om.CloseDate, om.CloseUser, om.CloseUserNm, om.CancelDate, om.CancelUser, om.CancelUserNm, om.CancelReason, om.Version, 
                      om.FlowDesc, om.WMSNo, om.ProdLineType, om.PauseStatus, om.SeqGroup
FROM         Ord_OrderMstr_3 AS om
UNION ALL
SELECT     om.OrderNo, om.Flow, om.ProdLineFact, om.TraceCode, om.OrderStrategy, om.RefOrderNo, om.ExtOrderNo, om.Type, om.SubType, om.QualityType, om.StartTime, 
                      om.WindowTime, om.PauseSeq, om.PauseTime, om.EffDate, om.IsQuick, om.Priority, om.Status, om.PartyFrom, om.PartyFromNm, om.PartyTo, om.PartyToNm, 
                      om.ShipFrom, om.ShipFromAddr, om.ShipFromTel, om.ShipFromCell, om.ShipFromFax, om.ShipFromContact, om.ShipTo, om.ShipToAddr, om.ShipToTel, 
                      om.ShipToCell, om.ShipToFax, om.ShipToContact, om.Shift, om.LocFrom, om.LocFromNm, om.LocTo, om.LocToNm, om.IsInspect, om.BillAddr, om.BillAddrDesc, 
                      om.PriceList, om.Currency, om.Dock, om.Routing, om.CurtOp, om.IsAutoRelease, om.IsAutoStart, om.IsAutoShip, om.IsAutoReceive, om.IsAutoBill, 
                      om.IsManualCreateDet, om.IsListPrice, om.IsPrintOrder, om.IsOrderPrinted, om.IsPrintAsn, om.IsPrintRec, om.IsShipExceed, om.IsRecExceed, om.IsOrderFulfillUC, 
                      om.IsShipFulfillUC, om.IsRecFulfillUC, om.IsShipScanHu, om.IsRecScanHu, om.IsCreatePL, om.IsPLCreate, om.IsShipFifo, om.IsRecFifo, om.IsShipByOrder, 
                      om.IsOpenOrder, om.IsAsnUniqueRec, om.RecGapTo, om.RecTemplate, om.OrderTemplate, om.AsnTemplate, om.HuTemplate, om.BillTerm, om.CreateHuOpt, 
                      om.ReCalculatePriceOpt, om.PickStrategy, om.ExtraDmdSource, om.CreateUser, om.CreateUserNm, om.CreateDate, om.LastModifyUser, om.LastModifyUserNm, 
                      om.LastModifyDate, om.ReleaseDate, om.ReleaseUser, om.ReleaseUserNm, om.StartDate, om.StartUser, om.StartUserNm, om.CompleteDate, om.CompleteUser, 
                      om.CompleteUserNm, om.CloseDate, om.CloseUser, om.CloseUserNm, om.CancelDate, om.CancelUser, om.CancelUserNm, om.CancelReason, om.Version, 
                      om.FlowDesc, om.WMSNo, om.ProdLineType, om.PauseStatus, om.SeqGroup
FROM         Ord_OrderMstr_4 AS om
UNION ALL
SELECT     om.OrderNo, om.Flow, om.ProdLineFact, om.TraceCode, om.OrderStrategy, om.RefOrderNo, om.ExtOrderNo, om.Type, om.SubType, om.QualityType, om.StartTime, 
                      om.WindowTime, om.PauseSeq, om.PauseTime, om.EffDate, om.IsQuick, om.Priority, om.Status, om.PartyFrom, om.PartyFromNm, om.PartyTo, om.PartyToNm, 
                      om.ShipFrom, om.ShipFromAddr, om.ShipFromTel, om.ShipFromCell, om.ShipFromFax, om.ShipFromContact, om.ShipTo, om.ShipToAddr, om.ShipToTel, 
                      om.ShipToCell, om.ShipToFax, om.ShipToContact, om.Shift, om.LocFrom, om.LocFromNm, om.LocTo, om.LocToNm, om.IsInspect, om.BillAddr, om.BillAddrDesc, 
                      om.PriceList, om.Currency, om.Dock, om.Routing, om.CurtOp, om.IsAutoRelease, om.IsAutoStart, om.IsAutoShip, om.IsAutoReceive, om.IsAutoBill, 
                      om.IsManualCreateDet, om.IsListPrice, om.IsPrintOrder, om.IsOrderPrinted, om.IsPrintAsn, om.IsPrintRec, om.IsShipExceed, om.IsRecExceed, om.IsOrderFulfillUC, 
                      om.IsShipFulfillUC, om.IsRecFulfillUC, om.IsShipScanHu, om.IsRecScanHu, om.IsCreatePL, om.IsPLCreate, om.IsShipFifo, om.IsRecFifo, om.IsShipByOrder, 
                      om.IsOpenOrder, om.IsAsnUniqueRec, om.RecGapTo, om.RecTemplate, om.OrderTemplate, om.AsnTemplate, om.HuTemplate, om.BillTerm, om.CreateHuOpt, 
                      om.ReCalculatePriceOpt, om.PickStrategy, om.ExtraDmdSource, om.CreateUser, om.CreateUserNm, om.CreateDate, om.LastModifyUser, om.LastModifyUserNm, 
                      om.LastModifyDate, om.ReleaseDate, om.ReleaseUser, om.ReleaseUserNm, om.StartDate, om.StartUser, om.StartUserNm, om.CompleteDate, om.CompleteUser, 
                      om.CompleteUserNm, om.CloseDate, om.CloseUser, om.CloseUserNm, om.CancelDate, om.CancelUser, om.CancelUserNm, om.CancelReason, om.Version, 
                      om.FlowDesc, om.WMSNo, om.ProdLineType, om.PauseStatus, om.SeqGroup
FROM         Ord_OrderMstr_5 AS om
UNION ALL
SELECT     om.OrderNo, om.Flow, om.ProdLineFact, om.TraceCode, om.OrderStrategy, om.RefOrderNo, om.ExtOrderNo, om.Type, om.SubType, om.QualityType, om.StartTime, 
                      om.WindowTime, om.PauseSeq, om.PauseTime, om.EffDate, om.IsQuick, om.Priority, om.Status, om.PartyFrom, om.PartyFromNm, om.PartyTo, om.PartyToNm, 
                      om.ShipFrom, om.ShipFromAddr, om.ShipFromTel, om.ShipFromCell, om.ShipFromFax, om.ShipFromContact, om.ShipTo, om.ShipToAddr, om.ShipToTel, 
                      om.ShipToCell, om.ShipToFax, om.ShipToContact, om.Shift, om.LocFrom, om.LocFromNm, om.LocTo, om.LocToNm, om.IsInspect, om.BillAddr, om.BillAddrDesc, 
                      om.PriceList, om.Currency, om.Dock, om.Routing, om.CurtOp, om.IsAutoRelease, om.IsAutoStart, om.IsAutoShip, om.IsAutoReceive, om.IsAutoBill, 
                      om.IsManualCreateDet, om.IsListPrice, om.IsPrintOrder, om.IsOrderPrinted, om.IsPrintAsn, om.IsPrintRec, om.IsShipExceed, om.IsRecExceed, om.IsOrderFulfillUC, 
                      om.IsShipFulfillUC, om.IsRecFulfillUC, om.IsShipScanHu, om.IsRecScanHu, om.IsCreatePL, om.IsPLCreate, om.IsShipFifo, om.IsRecFifo, om.IsShipByOrder, 
                      om.IsOpenOrder, om.IsAsnUniqueRec, om.RecGapTo, om.RecTemplate, om.OrderTemplate, om.AsnTemplate, om.HuTemplate, om.BillTerm, om.CreateHuOpt, 
                      om.ReCalculatePriceOpt, om.PickStrategy, om.ExtraDmdSource, om.CreateUser, om.CreateUserNm, om.CreateDate, om.LastModifyUser, om.LastModifyUserNm, 
                      om.LastModifyDate, om.ReleaseDate, om.ReleaseUser, om.ReleaseUserNm, om.StartDate, om.StartUser, om.StartUserNm, om.CompleteDate, om.CompleteUser, 
                      om.CompleteUserNm, om.CloseDate, om.CloseUser, om.CloseUserNm, om.CancelDate, om.CancelUser, om.CancelUserNm, om.CancelReason, om.Version, 
                      om.FlowDesc, om.WMSNo, om.ProdLineType, om.PauseStatus, om.SeqGroup
FROM         Ord_OrderMstr_6 AS om
UNION ALL
SELECT     om.OrderNo, om.Flow, om.ProdLineFact, om.TraceCode, om.OrderStrategy, om.RefOrderNo, om.ExtOrderNo, om.Type, om.SubType, om.QualityType, om.StartTime, 
                      om.WindowTime, om.PauseSeq, om.PauseTime, om.EffDate, om.IsQuick, om.Priority, om.Status, om.PartyFrom, om.PartyFromNm, om.PartyTo, om.PartyToNm, 
                      om.ShipFrom, om.ShipFromAddr, om.ShipFromTel, om.ShipFromCell, om.ShipFromFax, om.ShipFromContact, om.ShipTo, om.ShipToAddr, om.ShipToTel, 
                      om.ShipToCell, om.ShipToFax, om.ShipToContact, om.Shift, om.LocFrom, om.LocFromNm, om.LocTo, om.LocToNm, om.IsInspect, om.BillAddr, om.BillAddrDesc, 
                      om.PriceList, om.Currency, om.Dock, om.Routing, om.CurtOp, om.IsAutoRelease, om.IsAutoStart, om.IsAutoShip, om.IsAutoReceive, om.IsAutoBill, 
                      om.IsManualCreateDet, om.IsListPrice, om.IsPrintOrder, om.IsOrderPrinted, om.IsPrintAsn, om.IsPrintRec, om.IsShipExceed, om.IsRecExceed, om.IsOrderFulfillUC, 
                      om.IsShipFulfillUC, om.IsRecFulfillUC, om.IsShipScanHu, om.IsRecScanHu, om.IsCreatePL, om.IsPLCreate, om.IsShipFifo, om.IsRecFifo, om.IsShipByOrder, 
                      om.IsOpenOrder, om.IsAsnUniqueRec, om.RecGapTo, om.RecTemplate, om.OrderTemplate, om.AsnTemplate, om.HuTemplate, om.BillTerm, om.CreateHuOpt, 
                      om.ReCalculatePriceOpt, om.PickStrategy, om.ExtraDmdSource, om.CreateUser, om.CreateUserNm, om.CreateDate, om.LastModifyUser, om.LastModifyUserNm, 
                      om.LastModifyDate, om.ReleaseDate, om.ReleaseUser, om.ReleaseUserNm, om.StartDate, om.StartUser, om.StartUserNm, om.CompleteDate, om.CompleteUser, 
                      om.CompleteUserNm, om.CloseDate, om.CloseUser, om.CloseUserNm, om.CancelDate, om.CancelUser, om.CancelUserNm, om.CancelReason, om.Version, 
                      om.FlowDesc, om.WMSNo, om.ProdLineType, om.PauseStatus, om.SeqGroup
FROM         Ord_OrderMstr_7 AS om
UNION ALL
SELECT     om.OrderNo, om.Flow, om.ProdLineFact, om.TraceCode, om.OrderStrategy, om.RefOrderNo, om.ExtOrderNo, om.Type, om.SubType, om.QualityType, om.StartTime, 
                      om.WindowTime, om.PauseSeq, om.PauseTime, om.EffDate, om.IsQuick, om.Priority, om.Status, om.PartyFrom, om.PartyFromNm, om.PartyTo, om.PartyToNm, 
                      om.ShipFrom, om.ShipFromAddr, om.ShipFromTel, om.ShipFromCell, om.ShipFromFax, om.ShipFromContact, om.ShipTo, om.ShipToAddr, om.ShipToTel, 
                      om.ShipToCell, om.ShipToFax, om.ShipToContact, om.Shift, om.LocFrom, om.LocFromNm, om.LocTo, om.LocToNm, om.IsInspect, om.BillAddr, om.BillAddrDesc, 
                      om.PriceList, om.Currency, om.Dock, om.Routing, om.CurtOp, om.IsAutoRelease, om.IsAutoStart, om.IsAutoShip, om.IsAutoReceive, om.IsAutoBill, 
                      om.IsManualCreateDet, om.IsListPrice, om.IsPrintOrder, om.IsOrderPrinted, om.IsPrintAsn, om.IsPrintRec, om.IsShipExceed, om.IsRecExceed, om.IsOrderFulfillUC, 
                      om.IsShipFulfillUC, om.IsRecFulfillUC, om.IsShipScanHu, om.IsRecScanHu, om.IsCreatePL, om.IsPLCreate, om.IsShipFifo, om.IsRecFifo, om.IsShipByOrder, 
                      om.IsOpenOrder, om.IsAsnUniqueRec, om.RecGapTo, om.RecTemplate, om.OrderTemplate, om.AsnTemplate, om.HuTemplate, om.BillTerm, om.CreateHuOpt, 
                      om.ReCalculatePriceOpt, om.PickStrategy, om.ExtraDmdSource, om.CreateUser, om.CreateUserNm, om.CreateDate, om.LastModifyUser, om.LastModifyUserNm, 
                      om.LastModifyDate, om.ReleaseDate, om.ReleaseUser, om.ReleaseUserNm, om.StartDate, om.StartUser, om.StartUserNm, om.CompleteDate, om.CompleteUser, 
                      om.CompleteUserNm, om.CloseDate, om.CloseUser, om.CloseUserNm, om.CancelDate, om.CancelUser, om.CancelUserNm, om.CancelReason, om.Version, 
                      om.FlowDesc, om.WMSNo, om.ProdLineType, om.PauseStatus, om.SeqGroup
FROM         Ord_OrderMstr_8 AS om

GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VIEW_IpMstr]'))
DROP VIEW [dbo].[VIEW_IpMstr]
GO

CREATE VIEW [dbo].[VIEW_IpMstr]
AS
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, 
                      LastModifyUser, CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, 
                      IsRecScanHu, IsAsnUniqueRec, IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, 
                      ShipToContact, ShipToFax, ShipToCell, ShipToTel, ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, 
                      ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, 
                      ExtIpNo, SeqNo, WMSNo, Flow,ShipNo,Vehicle
FROM         dbo.ORD_IpMstr_1
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, 
                      LastModifyUser, CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, 
                      IsRecScanHu, IsAsnUniqueRec, IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, 
                      ShipToContact, ShipToFax, ShipToCell, ShipToTel, ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, 
                      ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, 
                      ExtIpNo, SeqNo, WMSNo, Flow,ShipNo,Vehicle
FROM         dbo.ORD_IpMstr_2
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, 
                      LastModifyUser, CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, 
                      IsRecScanHu, IsAsnUniqueRec, IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, 
                      ShipToContact, ShipToFax, ShipToCell, ShipToTel, ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, 
                      ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, 
                      ExtIpNo, SeqNo, WMSNo, Flow,ShipNo,Vehicle
FROM         dbo.ORD_IpMstr_3
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, 
                      LastModifyUser, CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, 
                      IsRecScanHu, IsAsnUniqueRec, IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, 
                      ShipToContact, ShipToFax, ShipToCell, ShipToTel, ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, 
                      ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, 
                      ExtIpNo, SeqNo, WMSNo, Flow,ShipNo,Vehicle
FROM         dbo.ORD_IpMstr_4
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, 
                      LastModifyUser, CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, 
                      IsRecScanHu, IsAsnUniqueRec, IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, 
                      ShipToContact, ShipToFax, ShipToCell, ShipToTel, ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, 
                      ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, 
                      ExtIpNo, SeqNo, WMSNo, Flow,ShipNo,Vehicle
FROM         dbo.ORD_IpMstr_5
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, 
                      LastModifyUser, CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, 
                      IsRecScanHu, IsAsnUniqueRec, IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, 
                      ShipToContact, ShipToFax, ShipToCell, ShipToTel, ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, 
                      ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, 
                      ExtIpNo, SeqNo, WMSNo, Flow,ShipNo,Vehicle
FROM         dbo.ORD_IpMstr_6
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, 
                      LastModifyUser, CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, 
                      IsRecScanHu, IsAsnUniqueRec, IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, 
                      ShipToContact, ShipToFax, ShipToCell, ShipToTel, ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, 
                      ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, 
                      ExtIpNo, SeqNo, WMSNo, Flow,ShipNo,Vehicle
FROM         dbo.ORD_IpMstr_7
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, 
                      LastModifyUser, CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, 
                      IsRecScanHu, IsAsnUniqueRec, IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, 
                      ShipToContact, ShipToFax, ShipToCell, ShipToTel, ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, 
                      ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, 
                      ExtIpNo, SeqNo, WMSNo, Flow,ShipNo,Vehicle
FROM         dbo.ORD_IpMstr_8
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, 
                      LastModifyUser, CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, 
                      IsRecScanHu, IsAsnUniqueRec, IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, 
                      ShipToContact, ShipToFax, ShipToCell, ShipToTel, ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, 
                      ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, 
                      ExtIpNo, SeqNo, WMSNo, Flow,ShipNo,Vehicle
FROM         dbo.ORD_IpMstr_0
GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VIEW_OrderDet]'))
DROP VIEW [dbo].[VIEW_OrderDet]
GO

CREATE VIEW [dbo].[VIEW_OrderDet]
AS
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, OrderQty, ShipQty, 
                      RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, UnitPrice, IsProvEst, 
                      Tax, IsIncludeTax, Bom, Routing, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, CreateUserNm, CreateDate, LastModifyUser, 
                      LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, 
                      ContainerDesc, IsScanHu, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, IsCreatePickList
FROM         dbo.ORD_OrderDet_1
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, OrderQty, ShipQty, 
                      RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, UnitPrice, IsProvEst, 
                      Tax, IsIncludeTax, Bom, Routing, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, CreateUserNm, CreateDate, LastModifyUser, 
                      LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, 
                      ContainerDesc, IsScanHu, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, IsCreatePickList
FROM         dbo.ORD_OrderDet_2
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, OrderQty, ShipQty, 
                      RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, UnitPrice, IsProvEst, 
                      Tax, IsIncludeTax, Bom, Routing, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, CreateUserNm, CreateDate, LastModifyUser, 
                      LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, 
                      ContainerDesc, IsScanHu, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, IsCreatePickList
FROM         dbo.ORD_OrderDet_3
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, OrderQty, ShipQty, 
                      RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, UnitPrice, IsProvEst, 
                      Tax, IsIncludeTax, Bom, Routing, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, CreateUserNm, CreateDate, LastModifyUser, 
                      LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, 
                      ContainerDesc, IsScanHu, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, IsCreatePickList
FROM         dbo.ORD_OrderDet_4
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, OrderQty, ShipQty, 
                      RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, UnitPrice, IsProvEst, 
                      Tax, IsIncludeTax, Bom, Routing, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, CreateUserNm, CreateDate, LastModifyUser, 
                      LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, 
                      ContainerDesc, IsScanHu, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, IsCreatePickList
FROM         dbo.ORD_OrderDet_5
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, OrderQty, ShipQty, 
                      RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, UnitPrice, IsProvEst, 
                      Tax, IsIncludeTax, Bom, Routing, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, CreateUserNm, CreateDate, LastModifyUser, 
                      LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, 
                      ContainerDesc, IsScanHu, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, IsCreatePickList
FROM         dbo.ORD_OrderDet_6
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, OrderQty, ShipQty, 
                      RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, UnitPrice, IsProvEst, 
                      Tax, IsIncludeTax, Bom, Routing, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, CreateUserNm, CreateDate, LastModifyUser, 
                      LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, 
                      ContainerDesc, IsScanHu, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, IsCreatePickList
FROM         dbo.ORD_OrderDet_7
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, OrderQty, ShipQty, 
                      RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, UnitPrice, IsProvEst, 
                      Tax, IsIncludeTax, Bom, Routing, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, CreateUserNm, CreateDate, LastModifyUser, 
                      LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, 
                      ContainerDesc, IsScanHu, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, IsCreatePickList
FROM         dbo.ORD_OrderDet_8
UNION ALL
SELECT     Id, OrderNo, Seq, ExtNo, ExtSeq, StartDate, EndDate, ScheduleType, Item, ItemDesc, RefItemCode, Uom, UC, Container, QualityType, ReqQty, OrderQty, ShipQty, 
                      RecQty, RejQty, ScrapQty, PickQty, UnitQty, RecLotSize, LocFrom, LocFromNm, LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, UnitPrice, IsProvEst, 
                      Tax, IsIncludeTax, Bom, Routing, BillTerm, ReserveNo, ReserveLine, ZOPWZ, ZOPID, ZOPDS, CreateUser, CreateUserNm, CreateDate, LastModifyUser, 
                      LastModifyUserNm, LastModifyDate, Version, Currency, OrderType, ManufactureParty, OrderSubType, BaseUom, PickStrategy, ExtraDmdSource, UCDesc, MinUC, 
                      ContainerDesc, IsScanHu, BinTo, WMSSeq, IsChangeUC, AUFNR, ICHARG, BWART, IsCreatePickList
FROM         dbo.ORD_OrderDet_0
GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VIEW_LocationDet]'))
DROP VIEW [dbo].[VIEW_LocationDet]
GO

CREATE VIEW [dbo].[VIEW_LocationDet]
AS
/*GROUP BY Location, Item*/ SELECT max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) 
                      AS CsQty, sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) 
                      AS InspectQty, sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_0 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_1 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_2 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_3 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_4 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_5 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_6 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_7 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_8 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_9 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_10 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_11 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_12 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_13 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_14 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_15 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_16 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_17 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_18 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION ALL
SELECT     max(det.Id) AS Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_19 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
GO

IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[VIEW_VanOrderSeq]'))
DROP VIEW [dbo].[VIEW_VanOrderSeq]
GO

CREATE VIEW [dbo].[VIEW_VanOrderSeq]
AS
SELECT ROW_NUMBER() OVER (ORDER BY os.Seq ASC,os.SubSeq ASC) AS RowId, os.Id,
			 om.Flow,om.OrderNo ,om.TraceCode,om.ExtOrderNo,om.Status, om.PauseStatus, om.CurtOp,os.SubSeq,os.Seq
			 FROM ORD_OrderSeq os LEFT JOIN ORD_OrderMstr_4 om
			ON om.TraceCode=os.TraceCode AND om.Flow=os.ProdLine AND om.OrderNo=os.OrderNo AND om.Status IN (2) 
GO

CREATE INDEX [IX_FlowDetSnapshot_Flow] ON [dbo].[LE_FlowDetSnapShot] ([Flow])
GO

ALTER table ORD_OrderBackflushDet drop column ProdSeqReportId
go
alter table ORD_OrderBackflushDet add OrderOpReportId int
go
alter table ORD_OrderItemTrace alter column OrderBomId int null
go