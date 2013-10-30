SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_PIK_CreatePickList4Qty') 
     DROP PROCEDURE USP_PIK_CreatePickList4Qty
GO

CREATE PROCEDURE [dbo].[USP_PIK_CreatePickList4Qty] 
(
	@CreatePickListTable CreatePickListType READONLY,
	@DeliveryGroup varchar(50),
	@UserId int,
	@UserName varchar(100),
	@PLNo varchar(50) output
) --WITH ENCRYPTION
AS 
BEGIN 
	set nocount on
	declare @DateTimeNow datetime = GetDate()
	declare @ErrorMsg nvarchar(MAX)
	
	create table #tempMsg
	(
		Lvl tinyint,
		Msg varchar(500)
	)
	
	create table #tempOrderMstr
	(
		OrderNo varchar(50), 
		[Status] tinyint, 
		[Type] tinyint, 
		SubType tinyint, 
		QualityType tinyint, 
		StartTime datetime, 
		WindowTime datetime, 
		PartyFrom varchar(50), 
		PartyFromNm varchar(100), 
		PartyTo varchar(50), 
		PartyToNm varchar(100),
	)
	
	create table #tempOrderDet
	(
		RowId int identity(1, 1),
		OrderNo varchar(50),
		OrderType tinyint,
		OrderSubType tinyint,
		OrderDetId int,
		OrderDetSeq int, 
		Item varchar(50),
		ItemDesc varchar(100),
		RefItemCode varchar(50),
		Uom varchar(5),
		BaseUom varchar(5),
		UnitQty decimal(18, 8),
		UC decimal(18, 8),
		UCDesc varchar(50),
		Container varchar(50),
		ContainerDesc varchar(50),
		QualityType tinyint,
		ManufactureParty varchar(50),
		OrderQty decimal(18, 8),
		PickQty decimal(18, 8),
		ShipQty decimal(18, 8),
		RecQty decimal(18, 8),
		ThisPickQty decimal(18, 8),
		LocFrom varchar(50),
		LocFromNm varchar(100),
		LocTo varchar(50), 
		LocToNm varchar(100),
		BinTo varchar(50),
		[Version] int
	)
	
	create table #tempPickListDet
	(
		OrderNo varchar(50),
		OrderType tinyint,
		OrderSubType tinyint,
		OrderDetId int,
		OrderDetSeq int, 
		Item varchar(50),
		ItemDesc varchar(100),
		RefItemCode varchar(50),
		Uom varchar(5),
		BaseUom varchar(5),
		UnitQty decimal(18, 8),
		UC decimal(18, 8),
		UCDesc varchar(50),
		Container varchar(50),
		ContainerDesc varchar(50),
		QualityType tinyint,
		ManufactureParty varchar(50),
		CSSupplier varchar(50),
		Qty decimal(18, 8),
		LocFrom varchar(50),
		LocFromNm varchar(100),
		LocTo varchar(50), 
		LocToNm varchar(100),
		BinTo varchar(50),
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
	
	create table #tempLocFromAndItem
	(
		RowId int identity(1, 1),
		Item varchar(50),
		LocFrom varchar(50)
	)
	
	create table #tempLocationLotDet
	(
		Id int,
		Item varchar(50),
		Location varchar(50),
		Qty decimal(18, 8),
		CSSupplier varchar(50),
		OccupyQty decimal(18, 8) default(0),
	)
	
	create table #tempOccupiedLocationLotDet
	(
		RowId int identity(1, 1),
		Item varchar(50), 
		Location varchar(50), 
		CSSupplier varchar(50), 
		OccupiedQty decimal(18, 8)
	)
	
	create table #tempMatchedLocationLotDet
	(
		RowId int identity(1, 1),
		Id int,
		CSSupplier varchar(50),
		RemainQty decimal(18, 8),
	)
	
	begin try
		if not exists(select top 1 1 from @CreatePickListTable)
		begin
			RAISERROR(N'拣货明细不能为空。', 16, 1)
		end
	
		if ISNULL(@DeliveryGroup, '') = ''
		begin
			RAISERROR(N'配送组不能为空。', 16, 1)
		end
		
		-------------------↓获取要货单头-----------------------
		insert into #tempOrderMstr(OrderNo, [Status], [Type], SubType, QualityType, StartTime, WindowTime, PartyFrom, PartyFromNm, PartyTo, PartyToNm)
		select OrderNo, [Status], [Type], SubType, QualityType, StartTime, WindowTime, PartyFrom, PartyFromNm, PartyTo, PartyToNm
		from ORD_OrderMstr_2 with(NOLOCK) where OrderNo in (select distinct m.OrderNo
												from @CreatePickListTable as t 
												inner join ORD_OrderDet_2 as d with(NOLOCK) on t.OrderDetId = d.Id
												inner join ORD_OrderMstr_2 as m with(NOLOCK) on d.OrderNo = m.OrderNo)
		-------------------↑获取要货单头-----------------------
		
		
		
		-------------------↓检查要货单头-----------------------
		if exists(select top 1 1 from #tempOrderMstr where [Status] not in (1, 2))
		begin
			insert into #tempMsg(Lvl, Msg)
			select 0, N'要货单' + OrderNo + N'的状态不是已释放或已开始，不能进行拣货操作。' from #tempOrderMstr where [Status] not in (1, 2)
		end
		
		if (select COUNT(1) from (select SubType from #tempOrderMstr group by SubType) as A) > 1
		begin
			insert into #tempMsg(Lvl, Msg) values(0, N'要货单的子类型不一致不能合并拣货。')
		end
		
		if (select COUNT(1) from (select QualityType from #tempOrderMstr group by QualityType) as A) > 1
		begin
			insert into #tempMsg(Lvl, Msg) values(0, N'要货单的质量状态不一致不能合并拣货。')
		end
		
		if (select COUNT(1) from (select PartyFrom from #tempOrderMstr group by PartyFrom) as A) > 1
		begin
			insert into #tempMsg(Lvl, Msg) values(0, N'要货单的来源区域不一致不能合并拣货。')
		end
		
		if (select COUNT(1) from (select PartyTo from #tempOrderMstr group by PartyTo) as A) > 1
		begin
			insert into #tempMsg(Lvl, Msg) values(0, N'要货单的目的区域不一致不能合并拣货。')
		end
		-------------------↑检查要货单头-----------------------
	
		
		
		-------------------↓获取要货明细-----------------------
		insert into #tempOrderDet(OrderNo, OrderType, OrderSubType, OrderDetId, OrderDetSeq, 
		Item, ItemDesc, RefItemCode, Uom, BaseUom, UnitQty, UC, UCDesc, Container, ContainerDesc, QualityType, ManufactureParty,
		OrderQty, PickQty, ShipQty, RecQty, ThisPickQty, LocFrom, LocFromNm, LocTo, LocToNm, BinTo, [Version])
		select m.OrderNo, m.[Type] as OrderType, m.SubType as OrderSubType, t.OrderDetId, d.Seq as OrderDetSeq, 
		d.Item, d.ItemDesc, d.RefItemCode, d.Uom, d.BaseUom, d.UnitQty, d.UC, d.UCDesc, d.Container, d.ContainerDesc, d.QualityType, d.ManufactureParty,
		d.OrderQty, d.PickQty, d.ShipQty, d.RecQty, t.PickQty as ThisPickQty, ISNULL(d.LocFrom, m.LocFrom), ISNULL(d.LocFromNm, m.LocFromNm), ISNULL(d.LocTo, m.LocTo), ISNULL(d.LocToNm, m.LocFromNm), d.BinTo, d.[Version]
		from @CreatePickListTable as t 
		inner join ORD_OrderDet_2 as d with(NOLOCK) on t.OrderDetId = d.Id
		inner join ORD_OrderMstr_2 as m with(NOLOCK) on d.OrderNo = m.OrderNo
		order by d.ManufactureParty desc, m.WindowTime, d.Id
		-------------------↑获取要货明细-----------------------
		
		
		
		-------------------↓检查要货单明细-----------------------
		insert into #tempMsg(Lvl, Msg)
		select 0, N'要货单' + OrderNo + N'行号' + CONVERT(varchar, OrderDetSeq) + N'物料号' + Item + N'的待拣货数不足。' from #tempOrderDet where OrderQty < (PickQty + ShipQty + ThisPickQty)
		-------------------↑检查要货单明细-----------------------
		
		
		
		if not exists(select top 1 1 from #tempMsg)
		begin
			-------------------↓获取库存明细-----------------------
			declare @Statement nvarchar(4000) 
			insert into #tempLocFrom(LocFrom) select distinct LocFrom from #tempOrderDet
			
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
					set @Statement = 'select Id, Item, Location, Qty, CSSupplier from INV_LocationLotDet_' + CASE WHEN ISNULL(@PartSuffix, '') = '' THEN '0' ELSE @PartSuffix END + ' where Location = ''' + @LocFrom + ''' and Qty > 0 and OccupyType = 0 and IsATP = 1'
				end
				else
				begin
					set @Statement = @Statement + ' UNION ALL'
					set @Statement = @Statement + ' select Id, Item, Location, Qty, CSSupplier from INV_LocationLotDet_' + CASE WHEN ISNULL(@PartSuffix, '') = '' THEN '0' ELSE @PartSuffix END + ' where Location = ''' + @LocFrom + ''' and Qty > 0 and OccupyType = 0 and IsATP = 1'
				end
				
				insert into #tempItem(Item) select distinct Item from #tempOrderDet where LocFrom = @LocFrom
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
				set @Statement = @Statement + ')'
				
				set @LocFromRowId = @LocFromRowId + 1
			end
			
			print @Statement
			insert into #tempLocationLotDet(Id, Item, Location, Qty, CSSupplier) exec sp_executesql @Statement
			-------------------↑获取库存明细-----------------------
			
			
			-------------------↓扣减拣货单占用-----------------------
			declare @OccupiedLocationLotDetRowId int
			declare @MaxOccupiedLocationLotDetRowId int
			declare @OccupiedItem varchar(50)
			declare @OccupiedLocation varchar(50)
			declare @OccupiedCSSupplier varchar(50)
			declare @OccupiedQty decimal(18, 8)
			declare @LastQty decimal(18, 8) = 0
			
			-------------------↓第1次扣减自有库存-----------------------
			truncate table #tempOccupiedLocationLotDet
			insert into #tempOccupiedLocationLotDet(Item, Location, OccupiedQty)
			select det.Item, det.LocFrom, SUM(det.Qty) as PickedQty 
			from ORD_PickListDet as det with(NOLOCK)
			inner join (select distinct Item, Location from #tempLocationLotDet where CSSupplier is null) as loc on det.Item = loc.Item and det.LocFrom = loc.Location
			where det.IsClose = 0 and det.CSSupplier is null
			group by det.Item, det.LocFrom
			
			if exists(select top 1 1 from #tempOccupiedLocationLotDet)
			begin
				select @OccupiedLocationLotDetRowId = MIN(RowId), @MaxOccupiedLocationLotDetRowId = MAX(RowId) from #tempOccupiedLocationLotDet
				
				while (@OccupiedLocationLotDetRowId <= @MaxOccupiedLocationLotDetRowId)
				begin
					select @OccupiedItem = Item, @OccupiedLocation = Location, @OccupiedQty = OccupiedQty from #tempOccupiedLocationLotDet where RowId = @OccupiedLocationLotDetRowId
					set @LastQty = 0
					
					--UPDATE的赋值次序1 先变量再字段
					--UPDATE的赋值次序2 变量之间, 从左到右
					--UPDATE的赋值次序3 字段之间, 并行执行
					update #tempLocationLotDet set OccupyQty = OccupyQty + (CASE WHEN @OccupiedQty >= Qty THEN Qty ELSE @OccupiedQty END), 
					@OccupiedQty = @OccupiedQty - @LastQty, @LastQty = Qty
					where Item = @OccupiedItem and Location = @OccupiedLocation and CSSupplier is null
					
					set @OccupiedLocationLotDetRowId = @OccupiedLocationLotDetRowId + 1
				end
			end
			-------------------↑第1次扣减自有库存-----------------------

			
			-------------------↓第2次扣减寄售库存-----------------------
			truncate table #tempOccupiedLocationLotDet
			insert into #tempOccupiedLocationLotDet(Item, Location, CSSupplier, OccupiedQty)
			select det.Item, det.LocFrom, det.CSSupplier, SUM(det.Qty) as PickedQty 
			from ORD_PickListDet as det with(NOLOCK)
			inner join (select distinct Item, Location, CSSupplier from #tempLocationLotDet where CSSupplier is not null) as loc on det.Item = loc.Item and det.LocFrom = loc.Location and det.CSSupplier = loc.CSSupplier
			where det.IsClose = 0 and det.CSSupplier is not null
			group by det.Item, det.LocFrom, det.CSSupplier
			
			if exists(select top 1 1 from #tempOccupiedLocationLotDet)
			begin
				select @OccupiedLocationLotDetRowId = MIN(RowId), @MaxOccupiedLocationLotDetRowId = MAX(RowId) from #tempOccupiedLocationLotDet
				
				while (@OccupiedLocationLotDetRowId <= @MaxOccupiedLocationLotDetRowId)
				begin
					select @OccupiedItem = Item, @OccupiedLocation = Location, @OccupiedCSSupplier = CSSupplier, @OccupiedQty = OccupiedQty from #tempOccupiedLocationLotDet where RowId = @OccupiedLocationLotDetRowId
					set @LastQty = 0
					
					--UPDATE的赋值次序1 先变量再字段
					--UPDATE的赋值次序2 变量之间, 从左到右
					--UPDATE的赋值次序3 字段之间, 并行执行
					update #tempLocationLotDet set OccupyQty = OccupyQty + (CASE WHEN @OccupiedQty >= Qty THEN Qty ELSE @OccupiedQty END), 
					@OccupiedQty = @OccupiedQty - @LastQty, @LastQty = Qty
					where Item = @OccupiedItem and Location = @OccupiedLocation and CSSupplier = @OccupiedCSSupplier
					
					set @OccupiedLocationLotDetRowId = @OccupiedLocationLotDetRowId + 1
				end
			end
			-------------------↑第2次扣减寄售库存-----------------------
			-------------------↑扣减拣货单占用-----------------------
			
			declare @trancount int
			begin try
				set @trancount = @@trancount
				
				if @trancount = 0
				begin
					begin tran
				end
				
				-------------------↓循环占用库存-----------------------
				declare @OrderDetRowId int
				declare @MaxOrderDetRowId int
				
				select @OrderDetRowId = MIN(RowId), @MaxOrderDetRowId = MAX(RowId) from #tempOrderDet 
				
				while(@OrderDetRowId <= @MaxOrderDetRowId)
				begin
					declare @OrderDetId int = null
					declare @OrderDetItem varchar(50) = null
					declare @ManufactureParty varchar(50) = null
					declare @ThisPickQty decimal(18, 8) = null
					declare @ThisPickedQty decimal(18, 8) = 0
					declare @Version int = null
					
					select @OrderDetId = OrderDetId, @OrderDetItem = Item, @ManufactureParty = ManufactureParty, @ThisPickQty = ThisPickQty, @Version = [Version]
					from #tempOrderDet where RowId = @OrderDetRowId
					
					truncate table #tempMatchedLocationLotDet
					if (ISNULL(@ManufactureParty, '') <> '')
					begin  --指定供应商
						insert into #tempMatchedLocationLotDet(Id, CSSupplier, RemainQty)
						select Id, CSSupplier, Qty - OccupyQty from #tempLocationLotDet 
						where Item = @OrderDetItem and CSSupplier = @ManufactureParty and Qty > OccupyQty order by Id
					end
					else
					begin
						insert into #tempMatchedLocationLotDet(Id, CSSupplier, RemainQty)
						select Id, CSSupplier, Qty - OccupyQty from #tempLocationLotDet 
						where Item = @OrderDetItem and CSSupplier is null and Qty > OccupyQty order by Id
						
						insert into #tempMatchedLocationLotDet(Id, CSSupplier, RemainQty)
						select Id, CSSupplier, Qty - OccupyQty from #tempLocationLotDet 
						where Item = @OrderDetItem and CSSupplier is not null and Qty > OccupyQty order by Id
					end
					
					if exists(select top 1 1 from #tempMatchedLocationLotDet)
					begin
						declare @MatchedLocationLotDetRowId int = null
						declare @MaxMatchedLocationLotDetRowId int = null
						select @MatchedLocationLotDetRowId = MIN(RowId), @MaxMatchedLocationLotDetRowId = MAX(RowId) from #tempMatchedLocationLotDet
						
						while (@MatchedLocationLotDetRowId <= @MaxMatchedLocationLotDetRowId)
						begin
							declare @LocationLotDetId int
							declare @CSSupplier varchar(50)
							declare @RemainQty decimal(18, 8)
							declare @ThisLocationLotDetPickedQty decimal(18, 8)
							
							select @LocationLotDetId = Id, @CSSupplier = CSSupplier, @RemainQty = RemainQty 
							from #tempMatchedLocationLotDet where RowId = @MatchedLocationLotDetRowId
							
							if (@ThisPickQty >= @RemainQty)
							begin
								update #tempLocationLotDet set OccupyQty = OccupyQty + @RemainQty where Id = @LocationLotDetId
								set @ThisLocationLotDetPickedQty = @RemainQty
								set @ThisPickQty = @ThisPickQty - @RemainQty
							end
							else
							begin
								update #tempLocationLotDet set OccupyQty = OccupyQty + @ThisPickQty where Id = @LocationLotDetId
								set @ThisLocationLotDetPickedQty = @ThisPickQty
								set @ThisPickQty = 0
							end
							set @ThisPickedQty = @ThisPickedQty + @ThisLocationLotDetPickedQty
						
							insert into #tempPickListDet(OrderNo, OrderType, OrderSubType, OrderDetId, OrderDetSeq, 
							Item, ItemDesc, RefItemCode, Uom, BaseUom, UnitQty, UC, UCDesc, Container, ContainerDesc, QualityType, ManufactureParty, CSSupplier,
							Qty, LocFrom, LocFromNm, LocTo, LocToNm, BinTo)
							select OrderNo, OrderType, OrderSubType, OrderDetId, OrderDetSeq, 
							Item, ItemDesc, RefItemCode, Uom, BaseUom, UnitQty, UC, UCDesc, Container, ContainerDesc, QualityType, ManufactureParty, @CSSupplier,
							@ThisLocationLotDetPickedQty, LocFrom, LocFromNm, LocTo, LocToNm, BinTo
							from #tempOrderDet where RowId = @OrderDetRowId
							
							if @ThisPickQty = 0
							begin
								break
							end
							
							set @MatchedLocationLotDetRowId = @MatchedLocationLotDetRowId + 1
						end
					end
		
					if (@ThisPickQty > 0)
					begin
						insert into #tempMsg(Lvl, Msg) select 1, N'要货单' + OrderNo + N'行号' + CONVERT(varchar, OrderDetSeq) + N'物料号' + Item + N'的库存小于拣货数，剩余' + CONVERT(varchar, CONVERT(decimal(18, 2), @ThisPickQty)) + N'未拣货。' 
						from #tempOrderDet where RowId = @OrderDetRowId 
					end
					
					if (@ThisPickedQty > 0)
					begin
						update ORD_OrderDet_2 set PickQty = PickQty + @ThisPickedQty, LastModifyDate = @DateTimeNow, LastModifyUser = @UserId, LastModifyUserNm = @UserName, [Version] = [Version] + 1
						where Id = @OrderDetId and [Version] = @Version
						
						if (@@ROWCOUNT = 0)
						begin
							select top 1 @ErrorMsg = N'要货单' + OrderNo + N'行号' + CONVERT(varchar, Seq) + N'物料号' + Item + N'已经更新，请重新创建拣货单。' from ORD_OrderDet_2 where Id = @OrderDetId
							RAISERROR(@ErrorMsg, 16, 1) 
						end
					end
					
					set @OrderDetRowId = @OrderDetRowId + 1
				end
				-------------------↑循环占用库存-----------------------
				
				-------------------↓创建拣货单头-----------------------
				if exists(select top 1 1 from #tempPickListDet)
				begin
					declare @OrderType tinyint
					declare @QualityType tinyint
					declare @StartTime datetime
					declare @WindowTime datetime
					declare @PartyFrom varchar(50)
					declare @PartyFromNm varchar(100)
					declare @PartyTo varchar(50)
					declare @PartyToNm varchar(100)
					
					select top 1 @OrderType = [Type], @QualityType = QualityType, @StartTime = MIN(StartTime), @WindowTime = MIN(WindowTime),
					@PartyFrom = PartyFrom, @PartyFromNm = PartyFromNm, @PartyTo = PartyTo, @PartyToNm = PartyToNm
					from #tempOrderMstr group by [Type], QualityType, PartyFrom, PartyFromNm, PartyTo, PartyToNm
					
					--获取拣货单号
					exec USP_GetDocNo_PIK @OrderType, @PartyFrom, @PartyTo, null, @PLNo output
					
					INSERT INTO ORD_PickListMstr 
					(
					PLNo,
					[Status],
					OrderType,
					QualityType,
					StartTime,
					WinTime,
					PartyFrom,
					PartyFromNm,
					PartyTo,
					PartyToNm,
					IsAutoReceive,
					IsPrintAsn,
					IsPrintRec,
					IsRecExceed,
					IsRecFulfillUC,
					IsRecFifo,
					IsAsnUniqueRec,
					IsCheckPartyFromAuth,
					IsCheckPartyToAuth,
					EffDate,
					CreateUser,
					CreateUserNm,
					CreateDate,
					LastModifyUser,
					LastModifyUserNm,
					LastModifyDate,
					[Version],
					CreateHuOpt,
					IsRecScanHu,
					RecGapTo,
					DeliveryGroup
					)
					VALUES
					(
					@PLNo,
					0,
					@OrderType,
					@QualityType,
					@StartTime,
					@WindowTime,
					@PartyFrom,
					@PartyFromNm,
					@PartyTo,
					@PartyToNm,
					1,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					0,
					@DateTimeNow,
					@UserId,
					@UserName,
					@DateTimeNow,
					@UserId,
					@UserName,
					@DateTimeNow,
					1,
					0,
					0,
					0,
					@DeliveryGroup
					)
					-------------------↑创建拣货单头-----------------------
					
					
					-------------------↓创建拣货单明细-----------------------
					INSERT INTO ORD_PickListDet
					(
					PLNo,
					OrderNo,
					OrderType,
					OrderSubType,
					OrderDetId,
					OrderDetSeq,
					Seq,
					Item,
					ItemDesc,
					RefItemCode,
					Uom,
					BaseUom,
					UnitQty,
					UC,
					UCDesc,
					Container,
					ContainerDesc,
					QualityType,
					ManufactureParty,
					CSSupplier,
					LocFrom,
					LocFromNm,
					Bin,
					LocTo,
					LocToNm,
					Qty,
					PickedQty,
					IsInspect,
					IsClose,
					IsOdd,
					IsDevan,
					IsInventory,
					CreateUser,
					CreateUserNm,
					CreateDate,
					LastModifyUser,
					LastModifyUserNm,
					LastModifyDate,
					[Version]
					)
					select 
					@PLNo,
					OrderNo,
					OrderType,
					OrderSubType,
					OrderDetId,
					OrderDetSeq,
					ROW_NUMBER() over(order by Item, OrderDetId),
					Item,
					ItemDesc,
					RefItemCode,
					Uom,
					BaseUom,
					UnitQty,
					UC,
					UCDesc,
					Container,
					ContainerDesc,
					QualityType,
					ManufactureParty,
					CSSupplier,
					LocFrom,
					LocFromNm,
					BinTo,
					LocTo,
					LocToNm,
					SUM(Qty),
					0,
					0,
					0,
					0,
					0,
					0,
					@UserId,
					@UserName,
					@DateTimeNow,
					@UserId,
					@UserName,
					@DateTimeNow,
					1
					from #tempPickListDet
					group by 
					OrderNo,
					OrderType,
					OrderSubType,
					OrderDetId,
					OrderDetSeq,
					Item,
					ItemDesc,
					RefItemCode,
					Uom,
					BaseUom,
					UnitQty,
					UC,
					UCDesc,
					Container,
					ContainerDesc,
					QualityType,
					ManufactureParty,
					CSSupplier,
					LocFrom,
					LocFromNm,
					BinTo,
					LocTo,
					LocToNm
					-------------------↑创建拣货单明细-----------------------
				end
				
				if @trancount = 0 
				begin  
					commit
				end
			end try 
			begin catch
				if @trancount = 0
				begin
					rollback
				end 
		        
				set @ErrorMsg = Error_Message() 
				RAISERROR(@ErrorMsg, 16, 1) 
			end catch 
		end
	end try 
	begin catch
		set @ErrorMsg = Error_Message() 
		RAISERROR(@ErrorMsg, 16, 1) 
	end catch 
	
	select Msg from #tempMsg
END 
