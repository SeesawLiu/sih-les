USE [sconit5_NEW]

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[##sconit5_OrderOpCPTime](
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
 CONSTRAINT [PK_sconit5_OrderOpCPTime] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[##sconit5_OrderOpCPTime] ADD  CONSTRAINT [DF_sconit5_OrderOpCPTime_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[##sconit5_OrderBomCPTime](
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
 CONSTRAINT [PK_sconit5_OrderBomCPTime] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[##sconit5_OrderBomCPTime] ADD  CONSTRAINT [DF_sconit5_OrderBomCPTime_CreateDate]  DEFAULT (getdate()) FOR [CreateDate]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[##sconit5_OrderPlanSnapshot](
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
 CONSTRAINT [PK_sconit5_OrderPlanSnapshot] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[##sconit5_OrderBomCPTimeSnapshot](
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
 CONSTRAINT [PK_sconit5_OrderBomCPTimeSnapshot] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[##sconit5_LocationDetSnapshot](
	[Item] [varchar](50) NOT NULL,
	[Location] [varchar](50) NOT NULL,
	[Qty] [decimal](18, 8) NULL,
 CONSTRAINT [PK_sconit5_LocationDetSnapshot] PRIMARY KEY CLUSTERED 
(
	[Item] ASC,
	[Location] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[##sconit5_FlowMstrSnapShot](
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
 CONSTRAINT [PK_sconit5_FlowMstrSnapShot] PRIMARY KEY CLUSTERED 
(
	[Flow] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[##sconit5_FlowDetSnapShot](
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
 CONSTRAINT [PK_sconit5_FlowDetSnapShot_1] PRIMARY KEY CLUSTERED 
(
	[Item] ASC,
	[LocTo] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[##sconit5_FlowDetExtraDmdSourceSnapshot](
	[FlowDetId] [int] NOT NULL,
	[Location] [varchar](50) NOT NULL,
 CONSTRAINT [PK_sconit5_FlowDetExtraDmdSourceSnapshot] PRIMARY KEY CLUSTERED 
(
	[FlowDetId] ASC,
	[Location] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

declare @Flow varchar(50)
declare @RowId int
declare @MaxRowId int

select identity(int, 1, 1) as RowId, Code into #tempPL from SCM_FlowMstr where ProdLineType in (1,2,3,4,9)

if exists(select top 1 1 from #tempPL)
begin
	select @RowId = MIN(RowId), @MaxRowId = MAX(RowId) from #tempPL
	while (@RowId <= @MaxRowId)
	begin
		select @Flow = Code from #tempPL where RowId = @RowId
		
		exec USP_Busi_UpdateOrderBomConsumeTime @Flow, 2603, 'ÓÃ»§ ³¬¼¶'
		set @RowId = @RowId + 1
	end
end
go

drop table #tempPL
go

