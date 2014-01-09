IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_BAK_LOG]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[USP_BAK_LOG]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[USP_BAK_LOG]
AS
BEGIN
	BEGIN TRY
		DECLARE @BakDate datetime
		DECLARE @COUNT INT
		
		SET @BakDate=DATEADD(DAY,-90,GETDATE())
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_DeleteVanProdOrder WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_DeleteVanProdOrder
			FROM LOG_DeleteVanProdOrder as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END

		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_DistributionRequisition WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_DistributionRequisition
			FROM LOG_DistributionRequisition as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_GenJITOrder WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_GenJITOrder
			FROM LOG_GenJITOrder as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_GenKBOrder WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_GenKBOrder
			FROM LOG_GenKBOrder as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_GenOrder4VanProdLine WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_GenOrder4VanProdLine
			FROM LOG_GenOrder4VanProdLine as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_GenProductOrder WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_GenProductOrder
			FROM LOG_GenProductOrder as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	

		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_GenSequenceOrder WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_GenSequenceOrder
			FROM LOG_GenSequenceOrder as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END

		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_GenVanProdOrder WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_GenVanProdOrder
			FROM LOG_GenVanProdOrder as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_OpRefBalanceChange WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_OpRefBalanceChange
			FROM LOG_OpRefBalanceChange as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_OrderTrace WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_OrderTrace
			FROM LOG_OrderTrace as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_OrderTraceDet WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_OrderTraceDet
			FROM LOG_OrderTraceDet as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_ProdOrderPauseResume WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_ProdOrderPauseResume
			FROM LOG_ProdOrderPauseResume as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_DistributionRequisition WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_DistributionRequisition
			FROM LOG_DistributionRequisition as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_RunLeanEngine WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_RunLeanEngine
			FROM LOG_RunLeanEngine as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_SeqOrderChange WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_SeqOrderChange
			FROM LOG_SeqOrderChange as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_SnapshotFlowDet4LeanEngine WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_SnapshotFlowDet4LeanEngine
			FROM LOG_SnapshotFlowDet4LeanEngine as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_VanOrderBomTrace WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_VanOrderBomTrace
			FROM LOG_VanOrderBomTrace as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM LOG_VanOrderTrace WHERE CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.LOG_VanOrderTrace
			FROM LOG_VanOrderTrace as lg WHERE CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM FIS_CancelReceiptMaster WHERE IsCreateDat = 1 and CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.FIS_CancelReceiptMaster
			FROM FIS_CancelReceiptMaster as lg WHERE IsCreateDat = 1 and CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM FIS_CreateBarCode WHERE IsCreateDat = 1 and CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.FIS_CreateBarCode
			FROM FIS_CreateBarCode as lg WHERE IsCreateDat = 1 and CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM FIS_CreateIpDAT WHERE IsCreateDat = 1 and TIME_STAMP1<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.FIS_CreateIpDAT
			FROM FIS_CreateIpDAT as lg WHERE IsCreateDat = 1 and TIME_STAMP1<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM FIS_CreateProcurementOrderDAT WHERE IsCreateDat = 1 and CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.FIS_CreateProcurementOrderDAT
			FROM FIS_CreateProcurementOrderDAT as lg WHERE IsCreateDat = 1 and CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM FIS_CreateSeqOrderDAT WHERE IsCreateDat = 1 and CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.FIS_CreateSeqOrderDAT
			FROM FIS_CreateSeqOrderDAT as lg WHERE IsCreateDat = 1 and CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM FIS_ItemStandardPack WHERE IsCreateDat = 1 and CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.FIS_ItemStandardPack
			FROM FIS_ItemStandardPack as lg WHERE IsCreateDat = 1 and CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM FIS_LesINLog WHERE IsCreateDat = 1 and HandTime<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.FIS_LesINLog
			FROM FIS_LesINLog as lg WHERE IsCreateDat = 1 and HandTime<@BakDate
			SET @COUNT = @COUNT + 1
		END
		
		--WHILE EXISTS(SELECT * FROM FIS_WMSDatFile WHERE Qty <= ReceiveTotal - CancelQty and CreateDate<@BakDate)
		--BEGIN
		--	DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.FIS_WMSDatFile
		--	FROM FIS_WMSDatFile as lg WHERE Qty <= ReceiveTotal - CancelQty and CreateDate<@BakDate
		--END	
		
		SET @COUNT = 1
		WHILE @COUNT < 1000 AND EXISTS(SELECT * FROM FIS_YieldReturn WHERE IsCreateDat = 1 and CreateDate<@BakDate)
		BEGIN
			DELETE TOP(10000) lg OUTPUT deleted.* INTO Sconit5_Arch.dbo.FIS_YieldReturn
			FROM FIS_YieldReturn as lg WHERE IsCreateDat = 1 and CreateDate<@BakDate
			SET @COUNT = @COUNT + 1
		END	
	END TRY
	BEGIN CATCH
		Declare @ErrorMsg varchar(max) = Error_Message() 
		RAISERROR(@ErrorMsg , 16, 1) 
	END CATCH				
END
GO


