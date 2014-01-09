IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_BAK_OrderMaster_2]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[USP_BAK_OrderMaster_2]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[USP_BAK_OrderMaster_2] 
AS 
BEGIN
	BEGIN TRY
		DECLARE @BakDate datetime
		DECLARE @COUNT INT
		SET @BakDate=DATEADD(DAY,-90,GETDATE())
		Create table #Temp_OrderNo
		(
			OrderNo varchar(50) Primary Key
		)	
		CREATE INDEX IX_Temp_OrderNo ON #Temp_OrderNo(OrderNo)
		
		update ORD_OrderMstr_2 set CloseDate = GETDATE() where Status = 4 and CloseDate is null
		update ORD_OrderMstr_2 set CancelDate = GETDATE() where Status = 5 and CancelDate is null
		
		insert into #Temp_OrderNo(OrderNo) SELECT OrderNo FROM ORD_OrderMstr_2 WHERE Status = 4 AND CloseDate<@BakDate
		insert into #Temp_OrderNo(OrderNo) SELECT OrderNo FROM ORD_OrderMstr_2 WHERE Status = 5 AND CancelDate<@BakDate
		
		Create table #Temp_RecNo
		(
			RecNo varchar(50) Primary Key
		)
		CREATE INDEX IX_Temp_RecNo ON #Temp_RecNo(RecNo)
		
		INSERT INTO #Temp_RecNo(RecNo) SELECT DISTINCT RecNo  FROM ORD_RecDet_2 rd 
		INNER JOIN #Temp_OrderNo ord ON rd.OrderNo=ord.OrderNo
		
		PRINT 'BEGIN BACKUP ORD_RecLocationDet_2'
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM ORD_RecLocationDet_2 rld INNER JOIN #Temp_RecNo rn ON rld.RecNo=rn.RecNo)
		BEGIN
			DELETE TOP(10000) rld  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_RecLocationDet_2 
			FROM ORD_RecLocationDet_2 rld 
			INNER JOIN #Temp_RecNo rn 
				ON rld.RecNo=rn.RecNo	
			SET @COUNT = @COUNT + 1
		END
		
		PRINT 'BEGIN BACKUP ORD_RecDet_2'
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM ORD_RecDet_2 rd INNER JOIN #Temp_RecNo rn ON rd.RecNo=rn.RecNo)
		BEGIN
			DELETE TOP(10000) rd  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_RecDet_2 
			FROM ORD_RecDet_2 rd
			INNER JOIN #Temp_RecNo rn 
				ON rd.RecNo=rn.RecNo	
			SET @COUNT = @COUNT + 1
		END	
		
		PRINT 'BEGIN BACKUP ORD_RecMstr_2'
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM ORD_RecMstr_2 rm INNER JOIN #Temp_RecNo rn ON rm.RecNo=rn.RecNo)
		BEGIN
			DELETE TOP(10000) rm  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_RecMstr_2 
			FROM ORD_RecMstr_2 rm
			INNER JOIN #Temp_RecNo rn 
				ON rm.RecNo=rn.RecNo	
			SET @COUNT = @COUNT + 1
		END
		
		Create table #Temp_IpNo
		(
			IpNo varchar(50) Primary Key
		)
		CREATE INDEX IX_Temp_IpNo ON #Temp_IpNo(IpNo)
		
		INSERT INTO #Temp_IpNo(IpNo) SELECT DISTINCT IpNo  FROM ORD_IpDet_2 id 
		INNER JOIN #Temp_OrderNo ord ON id.OrderNo=ord.OrderNo
		
		PRINT 'BEGIN BACKUP ORD_IpLocationDet_2'
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM ORD_IpLocationDet_2 rld INNER JOIN #Temp_IpNo rn ON rld.IpNo=rn.IpNo)
		BEGIN
			DELETE TOP(10000) rld  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_IpLocationDet_2 
			FROM ORD_IpLocationDet_2 rld 
			INNER JOIN #Temp_IpNo rn 
				ON rld.IpNo=rn.IpNo	
			SET @COUNT = @COUNT + 1
		END
		
		PRINT 'BEGIN BACKUP ORD_IpDet_2'
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM ORD_IpDet_2 rd INNER JOIN #Temp_IpNo rn ON rd.IpNo=rn.IpNo)
		BEGIN
			DELETE TOP(10000) rd  OUTPUT
			deleted.[Id]
           ,deleted.[Type]
           ,deleted.[IpNo]
           ,deleted.[Seq]
           ,deleted.[OrderNo]
           ,deleted.[OrderType]
           ,deleted.[OrderSubType]
           ,deleted.[OrderDetId]
           ,deleted.[OrderDetSeq]
           ,deleted.[ExtNo]
           ,deleted.[ExtSeq]
           ,deleted.[Item]
           ,deleted.[ItemDesc]
           ,deleted.[RefItemCode]
           ,deleted.[Uom]
           ,deleted.[BaseUom]
           ,deleted.[UC]
           ,deleted.[UCDesc]
           ,deleted.[Container]
           ,deleted.[ContainerDesc]
           ,deleted.[QualityType]
           ,deleted.[ManufactureParty]
           ,deleted.[Qty]
           ,deleted.[RecQty]
           ,deleted.[UnitQty]
           ,deleted.[LocFrom]
           ,deleted.[LocFromNm]
           ,deleted.[LocTo]
           ,deleted.[LocToNm]
           ,deleted.[IsInspect]
           ,deleted.[BillAddr]
           ,deleted.[PriceList]
           ,deleted.[UnitPrice]
           ,deleted.[Currency]
           ,deleted.[IsProvEst]
           ,deleted.[Tax]
           ,deleted.[IsIncludeTax]
           ,deleted.[BillTerm]
           ,deleted.[IsClose]
           ,deleted.[GapRecNo]
           ,deleted.[GapIpDetId]
           ,deleted.[CreateUser]
           ,deleted.[CreateUserNm]
           ,deleted.[CreateDate]
           ,deleted.[LastModifyUser]
           ,deleted.[LastModifyUserNm]
           ,deleted.[LastModifyDate]
           ,deleted.[Version]
           ,deleted.[StartTime]
           ,deleted.[Windowtime]
           ,deleted.[BinTo]
           ,deleted.[IsScanHu]
           ,deleted.[IsChangeUC]
           ,deleted.[Flow]
           ,deleted.[BWART]
           ,deleted.[PSTYP]
            INTO [Sconit5_Arch].[DBO].ORD_IpDet_2 
			FROM ORD_IpDet_2 rd
			INNER JOIN #Temp_IpNo rn 
			ON rd.IpNo=rn.IpNo	
			SET @COUNT = @COUNT + 1
		END	
		
		PRINT 'BEGIN BACKUP ORD_IpMstr_2'
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM ORD_IpMstr_2 rm INNER JOIN #Temp_IpNo rn ON rm.IpNo=rn.IpNo)
		BEGIN
			DELETE TOP(10000) rm  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_IpMstr_2 
			FROM ORD_IpMstr_2 rm
			INNER JOIN #Temp_IpNo rn 
				ON rm.IpNo=rn.IpNo	
			SET @COUNT = @COUNT + 1
		END
		
		PRINT 'BEGIN BACKUP FIS_WMSDatFile'
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM FIS_WMSDatFile as wms
			INNER JOIN ORD_OrderDet_2 as det on wms.OrderDetId = det.Id
			INNER JOIN #Temp_OrderNo ord ON det.OrderNo=ord.OrderNo)
		BEGIN
			DELETE TOP(10000) wms OUTPUT 
			deleted.[MoveType]
           ,deleted.[BLDAT]
           ,deleted.[BUDAT]
           ,deleted.[PO]
           ,deleted.[POLine]
           ,deleted.[VBELN]
           ,deleted.[POSNR]
           ,deleted.[LIFNR]
           ,deleted.[WERKS]
           ,deleted.[LGORT]
           ,deleted.[SOBKZ]
           ,deleted.[Item]
           ,deleted.[Qty]
           ,deleted.[Uom]
           ,deleted.[UMLGO]
           ,deleted.[GRUND]
           ,deleted.[KOSTL]
           ,deleted.[WmsNo]
           ,deleted.[RSNUM]
           ,deleted.[RSPOS]
           ,deleted.[WMSId]
           ,deleted.[WmsLine]
           ,deleted.[OLD]
           ,deleted.[INSMK]
           ,deleted.[XABLN]
           ,deleted.[AUFNR]
           ,deleted.[UMMAT]
           ,deleted.[UMWRK]
           ,deleted.[IsHand]
           ,deleted.[CreateDate]
           ,deleted.[FileName]
           ,deleted.[WBS]
           ,deleted.[HuId]
           ,deleted.[ReceiveTotal]
           ,deleted.[CancelQty]
           ,deleted.[Version]
			INTO Sconit5_Arch.dbo.FIS_WMSDatFile
			FROM FIS_WMSDatFile as wms
			INNER JOIN ORD_OrderDet_2 as det on wms.OrderDetId = det.Id
			INNER JOIN #Temp_OrderNo ord ON det.OrderNo=ord.OrderNo
			SET @COUNT = @COUNT + 1
		END	
		
		PRINT 'BEGIN BACKUP ORD_OrderDet_2'
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT od.* FROM ORD_OrderDet_2 od INNER JOIN #Temp_OrderNo ord ON od.OrderNo=ord.OrderNo)
		BEGIN
			DELETE TOP(10000) od  OUTPUT  			 
			deleted.Id
		  ,deleted.OrderNo
		  ,deleted.OrderType
		  ,deleted.OrderSubType
		  ,deleted.Seq
		  ,deleted.ExtNo
		  ,deleted.ExtSeq
		  ,deleted.StartDate
		  ,deleted.EndDate
		  ,deleted.ScheduleType
		  ,deleted.Item
		  ,deleted.ItemDesc
		  ,deleted.RefItemCode
		  ,deleted.Uom
		  ,deleted.BaseUom
		  ,deleted.UC
		  ,deleted.UCDesc
		  ,deleted.MinUC
		  ,deleted.Container
		  ,deleted.ContainerDesc
		  ,deleted.QualityType
		  ,deleted.ManufactureParty
		  ,deleted.ReqQty
		  ,deleted.OrderQty
		  ,deleted.ShipQty
		  ,deleted.RecQty
		  ,deleted.RejQty
		  ,deleted.ScrapQty
		  ,deleted.PickQty
		  ,deleted.UnitQty
		  ,deleted.RecLotSize
		  ,deleted.LocFrom
		  ,deleted.LocFromNm
		  ,deleted.LocTo
		  ,deleted.LocToNm
		  ,deleted.IsInspect
		  ,deleted.BillAddr
		  ,deleted.BillAddrDesc
		  ,deleted.PriceList
		  ,deleted.UnitPrice
		  ,deleted.IsProvEst
		  ,deleted.Tax
		  ,deleted.IsIncludeTax
		  ,deleted.Currency
		  ,deleted.Bom
		  ,deleted.Routing
		  ,deleted.BillTerm
		  ,deleted.PickStrategy
		  ,deleted.ExtraDmdSource
		  ,deleted.IsScanHu
		  ,deleted.ReserveNo
		  ,deleted.ReserveLine
		  ,deleted.ZOPWZ
		  ,deleted.ZOPID
		  ,deleted.ZOPDS
		  ,deleted.CreateUser
		  ,deleted.CreateUserNm
		  ,deleted.CreateDate
		  ,deleted.LastModifyUser
		  ,deleted.LastModifyUserNm
		  ,deleted.LastModifyDate
		  ,deleted.Version
		  ,deleted.BinTo
		  ,deleted.WMSSeq
		  ,deleted.IsChangeUC
		  ,deleted.AUFNR
		  ,deleted.ICHARG
		  ,deleted.BWART
		  ,deleted.IsCreatePickList 
		  INTO [Sconit5_Arch].[DBO].ORD_OrderDet_2 
			FROM ORD_OrderDet_2 od
			INNER JOIN #Temp_OrderNo ord 
				ON od.OrderNo=ord.OrderNo
			SET @COUNT = @COUNT + 1
		END
		
		PRINT 'BEGIN BACKUP ORD_OrderMstr_2'
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT om.* FROM ORD_OrderMstr_2 om INNER JOIN #Temp_OrderNo ord ON om.OrderNo=ord.OrderNo)
		BEGIN
			DELETE TOP(10000) om  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderMstr_2 
			FROM ORD_OrderMstr_2 om
			INNER JOIN #Temp_OrderNo ord 
				ON om.OrderNo=ord.OrderNo
			SET @COUNT = @COUNT + 1
		END
		
		DROP TABLE #Temp_OrderNo
		DROP TABLE #Temp_RecNo
	END TRY
	BEGIN CATCH
		Declare @ErrorMsg varchar(max) = Error_Message() 
		RAISERROR(@ErrorMsg , 16, 1) 
	END CATCH
END 

GO
