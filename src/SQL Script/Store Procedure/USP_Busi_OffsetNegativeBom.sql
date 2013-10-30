SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Busi_OffsetNegativeBom') 
     DROP PROCEDURE USP_Busi_OffsetNegativeBom
GO

CREATE PROCEDURE [dbo].[USP_Busi_OffsetNegativeBom] 
(
	@BatchNo int
) --WITH ENCRYPTION
AS 
BEGIN 
	set nocount on
	
	Create table #tempNegativeOrderBom
	(
		RowId int identity(1, 1),
		BomId int,
		Item varchar(50),
		PLNFL varchar(50), 
		VORNR varchar(50),
		AUFPL varchar(50),
		OrderQty decimal(18, 8)
	)
	
	Create table #tempPositiveBom
	(
		RowId int identity(1, 1),
		BomId int,
		MDMNG decimal(18, 8)
	)
		
	--查找负数Bom
	insert into #tempNegativeOrderBom(BomId, Item, PLNFL, VORNR, AUFPL, OrderQty)
	select Id, MATERIAL, PLNFL, VORNR, AUFPL, MDMNG from SAP_ProdBomDet where BatchNo = @BatchNo and BWART = '531'
	
	if exists(select top 1 1 from #tempNegativeOrderBom)
	begin  --负数Bom和正数Bom抵消
		declare @NRowId int = null
		declare @MaxNRowId int = null
		declare @NBomId int = null
		declare @NBomItem varchar(50) = null
		declare @NBomPLNFL varchar(50) = null
		declare @NBomVORNR varchar(50) = null
		declare @NBomAUFPL varchar(50) = null
		declare @NBomOrderQty decimal(18, 8) = null
		
		declare @PositiveBomRowId int = null
		declare @MaxPositiveBomRowId int = null
		declare @PositiveBomId int = null
		declare @PositiveMDMNG decimal(18, 8) = null
		
		select @NRowId = MIN(RowId), @MaxNRowId = MAX(RowId) from #tempNegativeOrderBom
		
		while (@NRowId <= @MaxNRowId)
		begin
			select @NBomId = BomId, @NBomItem = Item, @NBomPLNFL = PLNFL, @NBomVORNR = VORNR, @NBomAUFPL = AUFPL, @NBomOrderQty = OrderQty from #tempNegativeOrderBom where RowId = @NRowId
			
			--if exists(select top 1 1 from SAP_ProdBomDet where BatchNo = @BatchNo and MATERIAL = @NBomItem and MDMNG = @NBomOrderQty and BWART = '261' and PLNFL = @NBomPLNFL and VORNR = @NBomVORNR and AUFPL = @NBomAUFPL)
			--begin
			--	update SAP_ProdBomDet set MDMNG = 0 where Id in (select top 1 Id from SAP_ProdBomDet where BatchNo = @BatchNo and MATERIAL = @NBomItem and MDMNG = @NBomOrderQty and BWART = '261' and PLNFL = @NBomPLNFL and VORNR = @NBomVORNR and AUFPL = @NBomAUFPL)
			--	update SAP_ProdBomDet set MDMNG = 0 where Id = @NBomId
			--	set @NBomOrderQty = 0
			--end
			--else if exists(select top 1 1 from SAP_ProdBomDet where BatchNo = @BatchNo and MATERIAL = @NBomItem and MDMNG = @NBomOrderQty and BWART = '261')
			--begin
			--	update SAP_ProdBomDet set MDMNG = 0 where Id in (select top 1 Id from SAP_ProdBomDet where BatchNo = @BatchNo and MATERIAL = @NBomItem and MDMNG = @NBomOrderQty and BWART = '261')
			--	update SAP_ProdBomDet set MDMNG = 0 where Id = @NBomId
			--	set @NBomOrderQty = 0
			--end
			
			truncate table #tempPositiveBom
			insert into #tempPositiveBom(BomId, MDMNG)
			select Id, MDMNG from SAP_ProdBomDet where BatchNo = @BatchNo and MATERIAL = @NBomItem and BWART = '261' 
			and PLNFL = @NBomPLNFL and VORNR = @NBomVORNR and AUFPL = @NBomAUFPL and MDMNG > 0
		
			if exists(select top 1 1 from #tempPositiveBom)
			begin
				set @PositiveBomRowId = null
				set @MaxPositiveBomRowId = null
				set @PositiveBomId = null
				set @PositiveMDMNG = null
				
				select @PositiveBomRowId = MIN(RowId), @MaxPositiveBomRowId = MAX(RowId) from #tempPositiveBom
				
				while @PositiveBomRowId <= @MaxPositiveBomRowId
				begin
					select @PositiveBomId = BomId, @PositiveMDMNG = MDMNG from #tempPositiveBom where RowId = @PositiveBomRowId
					
					if (@NBomOrderQty > @PositiveMDMNG)
					begin
						update SAP_ProdBomDet set MDMNG = 0 where Id = @PositiveBomId
						update SAP_ProdBomDet set MDMNG = MDMNG - @PositiveMDMNG where Id = @NBomId
						
						set @NBomOrderQty = @NBomOrderQty - @PositiveMDMNG
					end
					else
					begin
						update SAP_ProdBomDet set MDMNG = MDMNG - @NBomOrderQty where Id = @PositiveBomId
						update SAP_ProdBomDet set MDMNG = 0 where Id = @NBomId
						
						set @NBomOrderQty = 0
						break
					end
					
					set @PositiveBomRowId = @PositiveBomRowId + 1
				end
			end
			
			if (@NBomOrderQty > 0)
			begin
				truncate table #tempPositiveBom
				insert into #tempPositiveBom(BomId, MDMNG)
				select Id, MDMNG from SAP_ProdBomDet where BatchNo = @BatchNo and MATERIAL = @NBomItem and BWART = '261' and MDMNG > 0

				if exists(select top 1 1 from #tempPositiveBom)
				begin
					set @PositiveBomRowId = null
					set @MaxPositiveBomRowId = null
					set @PositiveBomId = null
					set @PositiveMDMNG = null
					
					select @PositiveBomRowId = MIN(RowId), @MaxPositiveBomRowId = MAX(RowId) from #tempPositiveBom
					
					while @PositiveBomRowId <= @MaxPositiveBomRowId
					begin
						select @PositiveBomId = BomId, @PositiveMDMNG = MDMNG from #tempPositiveBom where RowId = @PositiveBomRowId
						
						if (@NBomOrderQty > @PositiveMDMNG)
						begin
							update SAP_ProdBomDet set MDMNG = 0 where Id = @PositiveBomId
							update SAP_ProdBomDet set MDMNG = MDMNG - @PositiveMDMNG where Id = @NBomId
							
							set @NBomOrderQty = @NBomOrderQty - @PositiveMDMNG
						end
						else
						begin
							update SAP_ProdBomDet set MDMNG = MDMNG - @NBomOrderQty where Id = @PositiveBomId
							update SAP_ProdBomDet set MDMNG = 0 where Id = @NBomId
							
							set @NBomOrderQty = 0
							break
						end
						
						set @PositiveBomRowId = @PositiveBomRowId + 1
					end
				end
			end
			
			set @NRowId = @NRowId + 1
		end
	end
	
	drop table #tempNegativeOrderBom
END 
