SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.objects WHERE type='P' AND name='USP_LE_SnapshotLocationDet4LeanEngine')
	DROP PROCEDURE USP_LE_SnapshotLocationDet4LeanEngine
GO

CREATE PROCEDURE [dbo].[USP_LE_SnapshotLocationDet4LeanEngine]
(
	@BatchNo int
)
--WITH ENCRYPTION
AS
BEGIN
	SET NOCOUNT ON
	declare @Msg nvarchar(Max)
	
	set @Msg = N'快照LocationDet开始'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	truncate table LE_LocationDetSnapshot
	
	insert into LE_LocationDetSnapshot(Location, Item, Qty)
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_0 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_1 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_2 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_3 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_4 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_5 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_6 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_7 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_8 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_9 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_10 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_11 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_12 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_13 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_14 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_15 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_16 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_17 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_18 where Qty <> 0 and IsATP = 1 group by Location, Item
	union all
	select Location, Item, SUM(Qty) as Qty from INV_LocationLotDet_19 where Qty <> 0 and IsATP = 1 group by Location, Item
	
	set @Msg = N'快照LocationDet结束'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
END
GO

