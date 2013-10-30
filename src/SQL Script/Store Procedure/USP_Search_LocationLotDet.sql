SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Search_LocationLotDet')
	DROP PROCEDURE USP_Search_LocationLotDet
GO	

CREATE  PROCEDURE [dbo].[USP_Search_LocationLotDet] 
( 
	@LocationArrayTable VarcharArrayType READONLY,
	@IsSapLocation bit,
	@ItemArrayTable VarcharArrayType READONLY,
	@IsShowCSSupplier bit,
	@SortCloumn varchar(50), 
	@SortRule varchar(50), 
	@PageSize int,
	@Pager int,
	@UserId int,
	@RowCount int output
) 
AS 
BEGIN 
	SET NOCOUNT ON
	
	Create table #tempLocation
	(
		RowId int identity(1, 1),
		Code varchar(50),
		PartSuffix varchar(50)
	)
	
	Create table #tempItem
	(
		RowId int identity(1, 1),
		Item varchar(50)
	)
	
	Create table #tempLocationLotDet
	(
		Item varchar(50),
		Location varchar(50),
		Qty decimal(18, 8),
		CSQty decimal(18, 8),
		QulifiedQty decimal(18, 8),
		InspectedQty decimal(18, 8),
		RejectedQty decimal(18, 8),
		CSSupplier varchar(50),
	)
	
	Create table #tempBom
	(
		Item varchar(50),
		Location varchar(50),
		BomQty decimal(18, 8),
	)
	
	if not exists(select top 1 1 from @LocationArrayTable)
	begin
		insert into #tempLocation(Code, PartSuffix)
		select distinct ml.Code, ml.PartSuffix from MD_Location as ml 
		--inner join VIEW_UserPermission as p on ml.Region = p.PermissionCode
		--where p.UserId = @UserId and p.Category = 'Region'
	end
	else if @IsSapLocation = 1
	begin
		insert into #tempLocation(Code, PartSuffix)
		select distinct ml.Code, ml.PartSuffix from MD_Location as ml 
		inner join @LocationArrayTable as tl on ml.SAPLocation = tl.Field
		--inner join VIEW_UserPermission as p on ml.Region = p.PermissionCode
		--where p.UserId = @UserId and p.Category = 'Region'
	end
	else
	begin
		insert into #tempLocation(Code, PartSuffix)
		select distinct ml.Code, ml.PartSuffix from MD_Location as ml 
		inner join @LocationArrayTable as tl on ml.Code = tl.Field
		--inner join VIEW_UserPermission as p on ml.Region = p.PermissionCode
		--where p.UserId = @UserId and p.Category = 'Region'
	end
	
	
	if exists(select top 1 1 from #tempLocation)
	begin
		insert into #tempItem(Item) select distinct Field from @ItemArrayTable
	
		declare @LocationRowId int
		declare @MaxLocationRowId int
		
		select @LocationRowId = MIN(RowId), @MaxLocationRowId = MAX(RowId) from #tempLocation
		
		while (@LocationRowId <= @MaxLocationRowId)
		begin
			declare @Statement nvarchar(MAX) = null
			declare @Location varchar(50) = null
			declare @PartSuffix varchar(50) = null
			select @Location = Code, @PartSuffix = PartSuffix from #tempLocation where RowId = @LocationRowId
			
			if (@IsShowCSSupplier = 1)
			begin
				set @Statement = 'select Item, Location, CSSupplier, SUM(Qty) as Qty, SUM(CASE WHEN QualityType = 0 THEN Qty else 0 end) as QualifiedQty, SUM(CASE WHEN QualityType = 1 THEN Qty else 0 end) as InspectedQty, SUM(CASE WHEN QualityType = 2 THEN Qty else 0 end) as RejectedQty from INV_LocationLotDet_' + CASE WHEN ISNULL(@PartSuffix, '') = '' THEN '0' ELSE @PartSuffix END + ' where Location = ''' + @Location + ''''
			end
			else
			begin
				set @Statement = 'select Item, Location, SUM(Qty) as Qty, SUM(CASE WHEN IsCS = 0 THEN 0 else Qty end) as CSQty, SUM(CASE WHEN QualityType = 0 THEN Qty else 0 end) as QualifiedQty, SUM(CASE WHEN QualityType = 1 THEN Qty else 0 end) as InspectedQty, SUM(CASE WHEN QualityType = 2 THEN Qty else 0 end) as RejectedQty from INV_LocationLotDet_' + CASE WHEN ISNULL(@PartSuffix, '') = '' THEN '0' ELSE @PartSuffix END + ' where Location = ''' + @Location + ''''
			end
			
			if exists(select top 1 1 from #tempItem)
			begin
				declare @ItemRowId int = 0
				declare @MaxItemRowId int = 0
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
			end
			
			if (@IsShowCSSupplier = 1)
			begin
				set @Statement = @Statement + ' group by Item, Location, CSSupplier'
				insert into #tempLocationLotDet(Item, Location, CSSupplier, Qty, QulifiedQty, InspectedQty, RejectedQty) exec sp_executesql @Statement
			end
			else
			begin
				set @Statement = @Statement + ' group by Item, Location'
				insert into #tempLocationLotDet(Item, Location, Qty, CSQty, QulifiedQty, InspectedQty, RejectedQty) exec sp_executesql @Statement
			end
			set @LocationRowId = @LocationRowId + 1
		end
		
		if exists(select top 1 1 from #tempLocation as l inner join SCM_FlowMstr as f on l.Code = f.LocFrom  where ProdLineType in (1, 2, 3, 4, 9))
		begin
			if exists(select top 1 1 from #tempItem)
			begin
				insert into #tempBom(Item, Location, BomQty)
				select bom.Item, bom.Location, SUM(bom.OrderQty)
				from (select bom.Item, bom.Location, bom.OrderQty
					from LE_OrderBomCPTimeSnapshot as bom 
					inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo and bom.VanOp < mstr.CurtOp
					inner join #tempLocation as l on bom.Location = l.Code
					inner join #tempItem as i on bom.Item = i.Item
					union all
					select bom.Item, bom.Location, bom.OrderQty
					from ORD_OrderBomDet as bom 
					inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
					inner join #tempLocation as l on bom.Location = l.Code
					inner join #tempItem as i on bom.Item = i.Item
					where mstr.ProdLineType in (1, 2, 3, 4, 9) and mstr.Status in (3)
					and not exists(select top 1 1 from LE_OrderBomCPTimeSnapshot as a where a.OrderNo = bom.OrderNo)
					) as bom
				group by bom.Item, bom.Location
			end
			else
			begin
				insert into #tempBom(Item, Location, BomQty)
				select bom.Item, bom.Location, SUM(bom.OrderQty)
				from (select bom.Item, bom.Location, bom.OrderQty
					from LE_OrderBomCPTimeSnapshot as bom 
					inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo and bom.VanOp < mstr.CurtOp
					inner join #tempLocation as l on bom.Location = l.Code
					union all
					select bom.Item, bom.Location, bom.OrderQty
					from ORD_OrderBomDet as bom 
					inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
					inner join #tempLocation as l on bom.Location = l.Code
					where mstr.ProdLineType in (1, 2, 3, 4, 9) and mstr.Status in (3)
					and not exists(select top 1 1 from LE_OrderBomCPTimeSnapshot as a where a.OrderNo = bom.OrderNo)
					) as bom
				group by bom.Item, bom.Location
			end
			
			if (@IsShowCSSupplier = 1)
			begin
				update l set Qty = l.Qty - b.BomQty
				from #tempLocationLotDet as l inner join #tempBom as b on l.Item = b.Item and l.Location = b.Location
				where l.CSSupplier is null
				
				insert into #tempLocationLotDet(Item, Location, CSSupplier, Qty, QulifiedQty, InspectedQty, RejectedQty)
				select b.Item, b.Location, -b.BomQty, 0, 0, 0, 0
				from #tempBom as b left join #tempLocationLotDet as l on l.Item = b.Item and l.Location = b.Location
				where l.Item is null and l.CSSupplier is null
			end
			else
			begin
				update l set Qty = l.Qty - b.BomQty
				from #tempLocationLotDet as l inner join #tempBom as b on l.Item = b.Item and l.Location = b.Location
				
				insert into #tempLocationLotDet(Item, Location, Qty, CSQty, QulifiedQty, InspectedQty, RejectedQty)
				select b.Item, b.Location, -b.BomQty, 0, 0, 0, 0
				from #tempBom as b left join #tempLocationLotDet as l on l.Item = b.Item and l.Location = b.Location
				where l.Item is null
			end
		end
		
		if ISNULL(@SortCloumn, '') = ''
		begin
			set @SortCloumn = 'Location'
		end
		
		if ISNULL(@SortRule, '') = ''
		begin
			set @SortRule = 'asc'
		end
		
		select @RowCount = COUNT(1) from  #tempLocationLotDet
		
		declare @RowIdFrom int = @PageSize * (@Pager - 1) + 1
		declare @RowIdTo int = @PageSize * @Pager
		declare @SelectPageStatement nvarchar(MAX) = ''
		if (@IsShowCSSupplier = 1)
		begin
			set @SelectPageStatement = 'select Item, Desc1, RefCode, Uom, Location, Qty, CSSupplier, QulifiedQty, InspectedQty, RejectedQty from (select ROW_NUMBER() OVER(order by t.' + @SortCloumn + ' ' + @SortRule + ') as RowId, t.Item, i.Desc1, i.RefCode, i.Uom, t.Location, t.Qty, t.CSSupplier, t.QulifiedQty, t.InspectedQty, t.RejectedQty from #tempLocationLotDet as t inner join MD_Item as i on t.Item = i.Code) as a where RowId between ' + CONVERT(varchar, @RowIdFrom) + ' and ' + CONVERT(varchar, @RowIdTo)
		end
		else
		begin
			set @SelectPageStatement = 'select Item, Desc1, RefCode, Uom, Location, Qty, CSQty, QulifiedQty, InspectedQty, RejectedQty from (select ROW_NUMBER() OVER(order by t.' + @SortCloumn + ' ' + @SortRule + ') as RowId, t.Item, i.Desc1, i.RefCode, i.Uom, t.Location, t.Qty, t.CSQty, t.QulifiedQty, t.InspectedQty, t.RejectedQty from #tempLocationLotDet as t inner join MD_Item as i on t.Item = i.Code) as a where RowId between ' + CONVERT(varchar, @RowIdFrom) + ' and ' + CONVERT(varchar, @RowIdTo)
		end
		exec sp_executesql @SelectPageStatement
	end
	else
	begin
		RAISERROR ('你没有查询指定库位的权限。' , 16, 1) WITH NOWAIT 
	end
	
	drop table #tempLocation
	drop table #tempItem
	drop table #tempLocationLotDet
END 

