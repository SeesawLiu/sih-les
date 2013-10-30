SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_LE_SpecifySequenceOrderCSSupplier')
	DROP PROCEDURE USP_LE_SpecifySequenceOrderCSSupplier	
GO

CREATE PROCEDURE [dbo].[USP_LE_SpecifySequenceOrderCSSupplier]
(
	@OrderNo varchar(50)
) --WITH ENCRYPTION
AS
BEGIN
	set nocount on
	create table #tempSeqOrderDet
	(
		RowId int identity(1, 1),
		TraceCode varchar(50), 
		Item varchar(50), 
		LocFrom varchar(50),
		OrderQty decimal(18, 8), 
		ManufactureParty varchar(50)
	)
	
	create table #tempOccupyOrderDet
	(
		Item varchar(50),
		Location varchar(50),
		Qty decimal(18, 8),
		ManufactureParty varchar(50),
	)
	
	create table #tempLocFrom
	(
		RowId int identity(1, 1),
		LocFrom varchar(50)
	)
	
	create table #tempItem
	(
		RowId int identity(1, 1),
		Item varchar(50),
	)
	
	create table #tempLocationLotDet
	(
		Id int Primary KEY,
		Item varchar(50),
		Location varchar(50),
		Qty decimal(18, 8),
		OccupyQty decimal(18, 8) default 0,
		CSSupplier varchar(50),
	)

	begin try
		-------------------↓按Van号分组-----------------------
		--指定供应商的放前面，优先分配
		insert into #tempSeqOrderDet(TraceCode, Item, LocFrom, ManufactureParty, OrderQty)
		select det.ReserveNo, det.Item, det.LocFrom, det.ManufactureParty, SUM(det.OrderQty)
		from ORD_OrderDet_2 as det 
		where det.OrderNo = @OrderNo and det.OrderQty > 0 and det.ManufactureParty is not null
		group by det.ReserveNo, det.ReserveLine, det.Item, det.LocFrom, det.ManufactureParty
		order by det.ReserveLine
		
		insert into #tempSeqOrderDet(TraceCode, Item, LocFrom, OrderQty)
		select det.ReserveNo, det.Item, det.LocFrom, SUM(det.OrderQty)
		from ORD_OrderDet_2 as det 
		where det.OrderNo = @OrderNo and det.OrderQty > 0 and det.ManufactureParty is null
		group by det.ReserveNo, det.ReserveLine, det.Item, det.LocFrom
		order by det.ReserveLine
		-------------------↑按Van号分组-----------------------
		
		if exists(select top 1 1 from #tempSeqOrderDet)
		begin
			-------------------↓获取库存明细-----------------------
			declare @Statement nvarchar(4000) 
			insert into #tempLocFrom(LocFrom) select distinct LocFrom from #tempSeqOrderDet
			declare @LocFromRowId int
			declare @MaxLocFromRowId int
			select @LocFromRowId = MIN(RowId), @MaxLocFromRowId = MAX(RowId) from #tempLocFrom
			while @LocFromRowId <= @MaxLocFromRowId
			begin
				declare @LocFrom varchar(50) = null
				declare @PartSuffix varchar(50) = null
				select @LocFrom = lf.LocFrom, @PartSuffix = l.PartSuffix from #tempLocFrom as lf 
				inner join MD_Location as l on lf.LocFrom = l.Code
				where RowId = @LocFromRowId
				
				if @LocFromRowId = 1
				begin
					set @Statement = 'select MIN(Id) as Id, Item, Location, SUM(Qty) as Qty, CSSupplier from INV_LocationLotDet_' + CASE WHEN ISNULL(@PartSuffix, '') = '' THEN '0' ELSE @PartSuffix END + ' where Location = ''' + @LocFrom + ''' and Qty > 0 and OccupyType = 0 and IsATP = 1'
				end
				else
				begin
					set @Statement = @Statement + ' UNION ALL'
					set @Statement = @Statement + ' select MIN(Id) as Id, Item, Location, SUM(Qty) as Qty, CSSupplier from INV_LocationLotDet_' + CASE WHEN ISNULL(@PartSuffix, '') = '' THEN '0' ELSE @PartSuffix END + ' where Location = ''' + @LocFrom + ''' and Qty > 0 and OccupyType = 0 and IsATP = 1'
				end
				
				insert into #tempItem(Item) select distinct Item from #tempSeqOrderDet where LocFrom = @LocFrom
				declare @ItemRowId int
				declare @MaxItemRowId int
				select @ItemRowId = MIN(RowId), @MaxItemRowId = MAX(RowId) from #tempItem
				while @ItemRowId <= @MaxItemRowId
				begin
					declare @Item varchar(50) = null
					select @Item = Item from #tempItem where RowId = @ItemRowId
					
					if @ItemRowId = 1
					begin
						set @Statement = @Statement + ' and Item in (''' + @Item + ''''
					end
					else
					begin
						set @Statement = @Statement + ',''' + @Item+ ''''
					end
					
					set @ItemRowId = @ItemRowId + 1
				end
				set @Statement = @Statement + ') group by Item, Location, CSSupplier'
				
				set @LocFromRowId = @LocFromRowId + 1
			end
			
			insert into #tempLocationLotDet(Id, Item, Location, Qty, CSSupplier) exec sp_executesql @Statement
			-------------------↑获取库存明细-----------------------
			
			if exists(select top 1 1 from #tempLocationLotDet)
			begin
				-------------------↓扣减其它拣货单占用-----------------------
				insert into #tempOccupyOrderDet(Item, Location, ManufactureParty, Qty)
				select det.Item, det.LocFrom, det.ICHARG, SUM(det.OrderQty - det.ShipQty) 
				from ORD_OrderDet_2 as det 
				inner join ORD_OrderMstr_2 as mstr on mstr.OrderNo = @OrderNo
				inner join (select distinct Item, LocFrom from #tempSeqOrderDet) as t on det.Item = t.Item and det.LocFrom = t.LocFrom
				where det.OrderNo <> @OrderNo and mstr.OrderStrategy = 4 
					and mstr.[Status] in (1, 2) and mstr.SubType = 0 and det.OrderQty > det.ShipQty
				group by det.Item, det.LocFrom, det.ICHARG
				
				if exists(select top 1 1 from #tempOccupyOrderDet)
				begin
					-------------------↓第1次扣减自有库存-----------------------
					update loc set OccupyQty = ord.Qty
					from #tempLocationLotDet as loc 
					inner join #tempOccupyOrderDet as ord on loc.Item = ord.Item and loc.Location = ord.Location
					where loc.CSSupplier is null and ord.ManufactureParty is null
					-------------------↑第1次扣减自有库存-----------------------
					
					
					-------------------↓第2次扣减寄售库存-----------------------
					update loc set OccupyQty = ord.Qty
					from #tempLocationLotDet as loc 
					inner join #tempOccupyOrderDet as ord on loc.Item = ord.Item and loc.Location = ord.Location and loc.CSSupplier = ord.ManufactureParty
					where loc.CSSupplier is not null and ord.ManufactureParty is not null
					-------------------↑第2次扣减寄售库存-----------------------
				end
				-------------------↑扣减其它拣货单占用-----------------------
				
			
				-------------------↓循环占用库存-----------------------
				if exists(select top 1 1 from #tempLocationLotDet where Qty > OccupyQty)
				begin
					declare @RowId int
					declare @MaxRowId int
					
					select @RowId = MIN(RowId), @MaxRowId = MAX(RowId) from #tempSeqOrderDet
					
					while @RowId <= @MaxRowId
					begin
						declare @TraceCode varchar(50) = null
						declare @SeqDetItem varchar(50) = null
						declare @SeqDetLocFrom varchar(50) = null
						declare @ManufactureParty varchar(50) = null
						declare @CSSupplier varchar(50) = null
						declare @OrderQty decimal(18, 8) = null
						declare @LocationLotDetId int = null
					
						select @TraceCode = TraceCode, @SeqDetItem = Item, @SeqDetLocFrom = LocFrom, @ManufactureParty = ManufactureParty, @OrderQty = OrderQty 
						from #tempSeqOrderDet where RowId = @RowId
					
						if (ISNULL(@ManufactureParty, '') <> '')
						begin  --指定供应商
							select @LocationLotDetId = Id, @CSSupplier = CSSupplier from #tempLocationLotDet
							where Item = @SeqDetItem and Location = @SeqDetLocFrom and CSSupplier = @ManufactureParty and Qty >= (OccupyQty + @OrderQty)
						
							if @LocationLotDetId is not null
							begin
								update ORD_OrderDet_2 set ICHARG = @CSSupplier
								where OrderNo = @OrderNo and Item = @SeqDetItem and LocFrom = @SeqDetLocFrom and ManufactureParty = @ManufactureParty and ReserveNo = @TraceCode and OrderQty > 0
								
								update #tempLocationLotDet set OccupyQty = OccupyQty + @OrderQty where Id = @LocationLotDetId
							end
							else
							begin
								update ORD_OrderDet_2 set BWART = N'指定供应商' + @ManufactureParty + N'的库存不足。'
								where OrderNo = @OrderNo and Item = @SeqDetItem and LocFrom = @SeqDetLocFrom and ManufactureParty = @ManufactureParty and ReserveNo = @TraceCode and OrderQty > 0
							end
						end
						else
						begin
							select top 1 @LocationLotDetId = Id, @CSSupplier = CSSupplier from #tempLocationLotDet
							where Item = @SeqDetItem and Location = @SeqDetLocFrom and Qty >= (OccupyQty + @OrderQty)
							order by Id
							
							if @LocationLotDetId is not null
							begin
								update ORD_OrderDet_2 set BWART = null, ICHARG = @CSSupplier
								where OrderNo = @OrderNo and Item = @SeqDetItem and LocFrom = @SeqDetLocFrom and ManufactureParty is null and ReserveNo = @TraceCode and OrderQty > 0
								
								update #tempLocationLotDet set OccupyQty = OccupyQty + @OrderQty where Id = @LocationLotDetId
							end
							else
							begin
								update ORD_OrderDet_2 set BWART = N'库存不足。', ICHARG = null
								where OrderNo = @OrderNo and Item = @SeqDetItem and LocFrom = @SeqDetLocFrom and ManufactureParty is null and ReserveNo = @TraceCode and OrderQty > 0
							end
						end
					
						set @RowId = @RowId + 1
					end
				end
				else
				begin
					update ORD_OrderDet_2 set BWART = N'库存不足。'
					where OrderNo = @OrderNo and OrderQty > 0
				end
				-------------------↑循环占用库存-----------------------
			end
			else
			begin
				update ORD_OrderDet_2 set BWART = N'库存不足。'
				where OrderNo = @OrderNo and OrderQty > 0
			end
		end
	end try
	begin catch
		declare @Msg nvarchar(Max) = Error_Message() 
		RAISERROR(@Msg, 16, 1) 
	end catch
	
	drop table #tempSeqOrderDet
	drop table #tempOccupyOrderDet
	drop table #tempLocFrom
	drop table #tempItem
	drop table #tempLocationLotDet
END