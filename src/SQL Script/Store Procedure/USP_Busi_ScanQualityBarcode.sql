SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_Busi_ScanQualityBarcode')
	DROP PROCEDURE USP_Busi_ScanQualityBarcode	
GO

CREATE PROCEDURE [dbo].[USP_Busi_ScanQualityBarcode]
(
	@TraceCode varchar(50),
	@Barcode varchar(50),
	@OpRef varchar(50),
	@OrderItemTraceId int,
	@IsForce bit,
	@IsVI bit,
	@CreateUserId int,
	@CreateUserNm varchar(100)
) --WITH ENCRYPTION
AS
BEGIN
	set nocount on
	declare @DateTimeNow datetime = GetDate()
	declare @ErrorMsg nvarchar(Max)
	declare @trancount int = @@trancount
	
	begin try
		if @trancount = 0
		begin
            begin tran
        end
		
		declare @SupplierShortCode varchar(50)
		declare @ItemShortCode varchar(50)
		declare @LotNo varchar(50)
		declare @SupplierCode varchar(50)
		declare @ItemCode varchar(50)
		declare @ItemDesc varchar(100)
		declare @RefItemCode varchar(50)
		declare @BomId int
		declare @Qty decimal(18, 8)
		declare @ScanQty decimal(18, 8)
		declare @ProdOrderPreFix varchar(50)
		declare @HuPreFix varchar(50)
		declare @OrderNo varchar(50)
		
		if ISNULL(@Barcode, '') = ''
		begin
			RAISERROR(N'条码不能为空。', 16, 1)
		end
		
		if @IsVI = 1 and exists(select top 1 1 from ORD_OrderItemTraceResult where TraceCode = @TraceCode and OrderItemTraceId is null and IsWithdraw = 0)
		begin
			RAISERROR(N'已经扫描过车架号。', 16, 1)
		end
		
		select @HuPreFix = PreFixed from SYS_SNRule where Code = 2001
		
		if SUBSTRING(@Barcode, 1, 2) = @HuPreFix
		begin
			select top 1 @ItemCode = Item, @ItemDesc = ItemDesc, @RefItemCode = RefItemCode 
			from INV_Hu where HuId = @Barcode
			
			if @ItemCode is null
			begin
				RAISERROR(N'条码不存在。', 16, 1)
			end
		end
		else
		begin
			if @IsForce = 0 and LEN(@Barcode) <> 17
			begin
				RAISERROR(N'条码长度不是17位。', 16, 1)
			end
		
			if LEN(@Barcode) = 17
			begin
				set @SupplierShortCode = Substring(@Barcode, 1, 4)
				set @ItemShortCode = Substring(@Barcode, 5, 5)
				set @LotNo = Substring(@Barcode, 10, 4)
				
				select top 1 @SupplierCode = Code from MD_Supplier where ShortCode = @SupplierShortCode
				
				if @IsForce = 0 and @SupplierCode is null
				begin
					RAISERROR(N'条码中的供应商短代码不正确。', 16, 1)
				end
				
				if @OrderItemTraceId is null
				begin
					declare @ItemShotCodeCount int	
					
					select @ItemShotCodeCount = COUNT(1) 
					from MD_Item where ShortCode = @ItemShortCode	
					
					if @ItemShotCodeCount = 0
					begin
						if @IsVI = 0
						begin
							RAISERROR(N'条码中的零件短代码不存在。', 16, 1)
						end
					end
					else if @ItemShotCodeCount = 1
					begin
						select @ItemCode = Code, @ItemDesc = Desc1, @RefItemCode = RefCode 
						from MD_Item where ShortCode = @ItemShortCode	
					end
					else
					begin
						Create table #tempItem
						(
							Code varchar(50),
							Desc1 varchar(100),
							RefCode varchar(50),
						)
						
						insert into #tempItem(Code, Desc1, RefCode) select i.Code, i.Desc1, i.RefCode
						from MD_Item as i inner join ORD_OrderItemTrace as t on i.Code = t.Item
						where i.ShortCode = @ItemShortCode and t.TraceCode = @TraceCode
						group by i.Code, i.Desc1, i.RefCode
								
						if @IsForce = 0 and not exists(select top 1 1 from #tempItem)
						begin
							RAISERROR(N'条码中的零件短代码不是关键件短代码。', 16, 1)
						end
						
						select @ItemShotCodeCount = COUNT(1) from #tempItem
						if @ItemShotCodeCount > 1
						begin
							RAISERROR(N'和条码中零件短代码匹配的关键件有多个，请手工指定。', 16, 1)
						end
						
						drop table #tempItem
						
						select @ItemCode = i.Code, @ItemDesc = i.Desc1, @RefItemCode = i.RefCode 
						from MD_Item as i inner join ORD_OrderItemTrace as t on i.Code = t.Item
						where i.ShortCode = @ItemShortCode and t.TraceCode = @TraceCode
						
						if @ItemCode is null and @IsForce = 1
						begin
							select @ItemShotCodeCount = COUNT(1) 
							from MD_Item as i inner join ORD_OrderBomDet as bom on i.Code = bom.Item
							inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
							where mstr.TraceCode = @TraceCode and i.ShortCode = @ItemShortCode
							
							if @ItemShotCodeCount = 0
							begin
								RAISERROR(N'没有和条码中零件短代码匹配的物料。', 16, 1)
							end
							else if @ItemShotCodeCount > 1
							begin
								RAISERROR(N'和条码中零件短代码匹配的物料有多个。', 16, 1)
							end
							
							select @ItemCode = Code, @ItemDesc = Desc1, @RefItemCode = RefCode 
							from MD_Item as i inner join ORD_OrderBomDet as bom on i.Code = bom.Item
							inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
							where mstr.TraceCode = @TraceCode and i.ShortCode = @ItemShortCode
						end
					end
				end
				else
				begin
					select top 1 @ItemCode = i.Code, @ItemDesc = i.Desc1, @RefItemCode = i.RefCode 
					from MD_Item as i inner join ORD_OrderItemTrace as t on i.Code = t.Item
					where t.Id = @OrderItemTraceId
				end
				
			end
		end
		
		if exists(select top 1 1 from ORD_OrderItemTraceResult where BarCode = @Barcode and IsWithdraw = 0)
		begin
			select top 1 @ErrorMsg = N'条码已经和VAN号' + TraceCode + '关联。' from ORD_OrderItemTraceResult where BarCode = @Barcode and IsWithdraw = 0
			RAISERROR(@ErrorMsg, 16, 1)
		end
	
		if @IsVI = 0
		begin
			if @OrderItemTraceId is null
			begin
				if @OpRef is null
				begin
					if @IsForce = 0 and exists(select top 1 1 from ORD_OrderItemTrace where TraceCode = @TraceCode and Item = @ItemCode group by Item having COUNT(1) > 1)
					begin
						--RAISERROR(N'条码匹配的关键件有多条，请手工指定。', 16, 1)
						select top 1 @OrderItemTraceId = Id, @Qty = Qty, @ScanQty = ScanQty, @OpRef = OpRef, @OrderNo = OrderNo from ORD_OrderItemTrace where TraceCode = @TraceCode and Item = @ItemCode and Qty > ScanQty
					
						if @IsForce = 0 and @OrderItemTraceId is null
						begin
							RAISERROR(N'扫描的条码数量已经大于等于Bom用量。', 16, 1)
						end
					end
					else
					begin
						select @OrderItemTraceId = Id, @Qty = Qty, @ScanQty = ScanQty, @OpRef = OpRef, @OrderNo = OrderNo from ORD_OrderItemTrace where TraceCode = @TraceCode and Item = @ItemCode
					end
				end
				else
				begin
					select @OrderItemTraceId = Id, @Qty = Qty, @ScanQty = ScanQty, @OrderNo = OrderNo from ORD_OrderItemTrace where TraceCode = @TraceCode and Item = @ItemCode and OpRef = @OpRef
				end
				
				if @IsForce = 0 and @OrderItemTraceId is null
				begin
					RAISERROR(N'没有找到和条码匹配的关键件。', 16, 1)
				end
				else if @IsForce = 1 and @OrderItemTraceId is null
				begin
					if @OpRef is null
					begin
						if exists(select top 1 1 from ORD_OrderBomDet as bom inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
								where mstr.TraceCode = @TraceCode and bom.Item = @ItemCode and bom.OrderQty > 0)
						begin
							INSERT INTO ORD_OrderItemTrace(
							OrderNo,
							Item,
							ItemDesc,
							RefItemCode,
							Op,
							OpRef,
							Qty,
							ScanQty,
							CreateUser,
							CreateUserNm,
							CreateDate,
							LastModifyUser,
							LastModifyUserNm,
							LastModifyDate,
							Version,
							TraceCode)
							select
							mstr.OrderNo,
							bom.Item,
							bom.ItemDesc,
							bom.RefItemCode,
							bom.Op,
							bom.OpRef,
							SUM(bom.OrderQty),
							0,
							@CreateUserId,
							@CreateUserNm,
							@DateTimeNow,
							@CreateUserId,
							@CreateUserNm,
							@DateTimeNow,
							1,
							mstr.TraceCode
							from ORD_OrderBomDet as bom inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
							where mstr.TraceCode = @TraceCode and bom.Item = @ItemCode and bom.OrderQty > 0
							group by mstr.OrderNo, bom.Item, bom.ItemDesc, bom.RefItemCode, bom.Op, bom.OpRef, mstr.TraceCode
							
							select top 1 @OrderItemTraceId = SCOPE_IDENTITY()
							select @Qty = Qty, @ScanQty = ScanQty, @OrderNo = OrderNo, @OpRef = OpRef from ORD_OrderItemTrace where Id = @OrderItemTraceId
						end
						else
						begin
							set @ErrorMsg = N'没有和关键件' + @ItemCode + '匹配的Bom。'
							RAISERROR(@ErrorMsg, 16, 1)
						end
					end
					else
					begin
						if exists(select top 1 1 from ORD_OrderBomDet as bom inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
								where mstr.TraceCode = @TraceCode and bom.Item = @ItemCode and OpRef = @OpRef and bom.OrderQty > 0)
						begin
							INSERT INTO ORD_OrderItemTrace(
							OrderNo,
							Item,
							ItemDesc,
							RefItemCode,
							Op,
							OpRef,
							Qty,
							ScanQty,
							CreateUser,
							CreateUserNm,
							CreateDate,
							LastModifyUser,
							LastModifyUserNm,
							LastModifyDate,
							Version,
							TraceCode)
							select
							mstr.OrderNo,
							bom.Item,
							bom.ItemDesc,
							bom.RefItemCode,
							bom.Op,
							bom.OpRef,
							SUM(bom.OrderQty),
							0,
							@CreateUserId,
							@CreateUserNm,
							@DateTimeNow,
							@CreateUserId,
							@CreateUserNm,
							@DateTimeNow,
							1,
							mstr.TraceCode
							from ORD_OrderBomDet as bom inner join ORD_OrderMstr_4 as mstr on bom.OrderNo = mstr.OrderNo
							where mstr.TraceCode = @TraceCode and bom.Item = @ItemCode and OpRef = @OpRef and bom.OrderQty > 0
							group by mstr.OrderNo, bom.Item, bom.ItemDesc, bom.RefItemCode, bom.Op, bom.OpRef, mstr.TraceCode
							
							select top 1 @OrderItemTraceId = SCOPE_IDENTITY()
							select @Qty = Qty, @ScanQty = ScanQty, @OrderNo = OrderNo, @OpRef = OpRef from ORD_OrderItemTrace where Id = @OrderItemTraceId
						end
						else
						begin
							set @ErrorMsg = N'没有和关键件' + @ItemCode + '匹配的Bom。'
							RAISERROR(@ErrorMsg, 16, 1)
						end
					end
				end
			end
			else
			begin
				select @Qty = Qty, @ScanQty = ScanQty, @OrderNo = OrderNo, @OpRef = OpRef from ORD_OrderItemTrace where Id = @OrderItemTraceId
				
				if (@Qty is null)
				begin
					RAISERROR(N'指定的关键件标示不正确。', 16, 1)
				end
			end
			
			if @IsForce = 0 and (@Qty <= @ScanQty)
			begin
				RAISERROR(N'扫描的条码数量已经大于等于Bom用量。', 16, 1)
			end
		end
		
		INSERT INTO ORD_OrderItemTraceResult(OrderItemTraceId, BarCode, Supplier, LotNo, OpRef, Item, ItemDesc, RefItemCode, 
				TraceCode, OrderNo, IsWithdraw, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate)
		values(@OrderItemTraceId, @Barcode, @SupplierCode, @LotNo, @OpRef, @ItemCode, @ItemDesc, @RefItemCode,
				@TraceCode, @OrderNo, 0, @CreateUserId, @CreateUserNm, @DateTimeNow, @CreateUserId, @CreateUserNm, @DateTimeNow)
				
		update ORD_OrderItemTrace set ScanQty = ScanQty + 1 where Id = @OrderItemTraceId
		
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
END
