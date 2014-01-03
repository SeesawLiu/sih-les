IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_BAK_OrderMaster_4_ProdOrder]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[USP_BAK_OrderMaster_4_ProdOrder]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[USP_BAK_OrderMaster_4_ProdOrder] 
AS 
BEGIN
	---归档非整车生产数据
	DECLARE @BakDate datetime
	SET @BakDate=DATEADD(DAY,-60,GETDATE())
	
	IF EXISTS(SELECT * FROM sys.objects WHERE name='Temp_OrderNo_4_ProdOrder' AND type='U')
	BEGIN
		IF NOT EXISTS(SELECT TOP 1 * FROM Temp_OrderNo_4_ProdOrder)
		BEGIN
			DROP TABLE Temp_OrderNo_4_ProdOrder
			
			SELECT OrderNo INTO Temp_OrderNo_4_ProdOrder FROM ORD_OrderMstr_4 om WHERE om.Status IN(4,5) AND om.ProdLineType NOT IN (1,2,3,4,9) AND om.LastModifyDate<@BakDate
			CREATE INDEX IX_Temp_OrderNo_4_ProdOrder_OrderNo ON Temp_OrderNo_4_ProdOrder(OrderNo)
		END
	END
	ELSE
	BEGIN
		SELECT OrderNo INTO Temp_OrderNo_4_ProdOrder FROM ORD_OrderMstr_4 om WHERE om.Status IN(4,5) AND om.ProdLineType NOT IN (1,2,3,4,9) AND om.LastModifyDate<@BakDate
		CREATE INDEX IX_Temp_OrderNo_4_ProdOrder_OrderNo ON Temp_OrderNo_4_ProdOrder(OrderNo)
	END
	
	DELETE t FROM Temp_OrderNo_4_ProdOrder t WHERE EXISTS(SELECT 1 FROM SAP_ProdOpReport pr WHERE pr.OrderNO=t.OrderNo AND pr.Status=0)
	
	DELETE t FROM Temp_OrderNo_4_ProdOrder t WHERE EXISTS(SELECT 1 FROM SAP_ProdOpBackflush pb WHERE pb.OrderNO=t.OrderNo AND pb.Status=0)
	
	WHILE EXISTS(SELECT 1 FROM ORD_OrderBackflushDet obd INNER JOIN Temp_OrderNo_4_ProdOrder ord ON obd.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) obd OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderBackflushDet 
		FROM ORD_OrderBackflushDet obd
		INNER JOIN Temp_OrderNo_4_ProdOrder ord 
			ON obd.OrderNo=ord.OrderNo
	END
	
	WHILE EXISTS(SELECT TOP 1 obd.* FROM ORD_OrderBomDet obd INNER JOIN Temp_OrderNo_4_ProdOrder ord ON obd.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) obd  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderBomDet 
		FROM ORD_OrderBomDet obd
		INNER JOIN Temp_OrderNo_4_ProdOrder ord 
			ON obd.OrderNo=ord.OrderNo
	END
	
	IF EXISTS(SELECT * FROM sys.objects WHERE name='Temp_RecNo_4' AND type='U')
	BEGIN
		IF NOT EXISTS(SELECT TOP 1 * FROM Temp_RecNo_4)
		BEGIN
			DROP TABLE Temp_RecNo_4_ProdOrder
			SELECT DISTINCT RecNo INTO Temp_RecNo_4_ProdOrder FROM ORD_RecDet_4 rd INNER JOIN Temp_OrderNo_4_ProdOrder ord ON rd.OrderNo=ord.OrderNo
			CREATE INDEX IX_Temp_RecNo_4_OrderNo ON Temp_RecNo_4_ProdOrder(RecNo)
		END
		ELSE
		BEGIN
			INSERT INTO Temp_RecNo_4_ProdOrder(RecNo)
			SELECT DISTINCT RecNo FROM ORD_RecDet_4 rd INNER JOIN Temp_OrderNo_4_ProdOrder ord ON rd.OrderNo=ord.OrderNo
		END
	END
	ELSE
	BEGIN
		SELECT DISTINCT RecNo INTO Temp_RecNo_4_ProdOrder FROM ORD_RecDet_4 rd INNER JOIN Temp_OrderNo_4 ord ON rd.OrderNo=ord.OrderNo
		CREATE INDEX IX_Temp_RecNo_4_OrderNo ON Temp_RecNo_4_ProdOrder(RecNo)
	END
	---SELECT DISTINCT RecNo INTO #Temp_OrderNo_4 FROM ORD_RecDet_4 rd INNER JOIN Temp_OrderNo_4 ord ON rd.OrderNo=ord.OrderNo
	
	WHILE EXISTS(SELECT TOP 1 * FROM ORD_RecLocationDet_4 rld INNER JOIN Temp_RecNo_4_ProdOrder rn ON rld.RecNo=rn.RecNo)
	BEGIN
		DELETE TOP(10000) rld  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_RecLocationDet_4 
		FROM ORD_RecLocationDet_4 rld 
		INNER JOIN Temp_RecNo_4_ProdOrder rn 
			ON rld.RecNo=rn.RecNo	
	END
	
	WHILE EXISTS(SELECT TOP 1 * FROM ORD_RecDet_4 rd INNER JOIN Temp_RecNo_4_ProdOrder rn ON rd.RecNo=rn.RecNo)
	BEGIN
		DELETE TOP(10000) rd  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_RecDet_4 
		FROM ORD_RecDet_4 rd
		INNER JOIN Temp_RecNo_4_ProdOrder rn 
			ON rd.RecNo=rn.RecNo	
	END	
	
	WHILE EXISTS(SELECT TOP 1 * FROM ORD_RecMstr_4 rm INNER JOIN Temp_RecNo_4_ProdOrder rn ON rm.RecNo=rn.RecNo)
	BEGIN
		DELETE TOP(10000) rm  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_RecMstr_4 
		FROM ORD_RecMstr_4 rm
		INNER JOIN Temp_RecNo_4_ProdOrder rn 
			ON rm.RecNo=rn.RecNo	
	END		
	
	WHILE EXISTS(SELECT TOP 1 od.* FROM ORD_OrderDet_4 od INNER JOIN Temp_OrderNo_4_ProdOrder ord ON od.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) od  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderDet_4 
		FROM ORD_OrderDet_4 od
		INNER JOIN Temp_OrderNo_4_ProdOrder ord 
			ON od.OrderNo=ord.OrderNo
	END
	
	WHILE EXISTS(SELECT TOP 1 os.* FROM ORD_OrderSeq os INNER JOIN ORD_OrderMstr_4 om ON om.TraceCode=os.TraceCode INNER JOIN Temp_OrderNo_4_ProdOrder ord ON om.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) os  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderSeq 
		FROM ORD_OrderSeq os
		INNER JOIN Temp_OrderNo_4_ProdOrder ord 
			ON os.OrderNo=ord.OrderNo
	END	
	
	--select top 100 * from ORD_OrderSeq
	
	WHILE EXISTS(SELECT TOP 1 om.* FROM ORD_OrderMstr_4 om INNER JOIN Temp_OrderNo_4_ProdOrder ord ON om.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) om  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderMstr_4 
		FROM ORD_OrderMstr_4 om
		INNER JOIN Temp_OrderNo_4_ProdOrder ord 
			ON om.OrderNo=ord.OrderNo
	END
	
	--select top 100 * from ORD_OrderOpReport
	WHILE EXISTS(SELECT TOP 1 op.* FROM ORD_OrderOpReport op INNER JOIN Temp_OrderNo_4_ProdOrder ord ON op.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) op  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderOpReport 
		FROM ORD_OrderOpReport op 
		INNER JOIN Temp_OrderNo_4_ProdOrder ord 
			ON op.OrderNo=ord.OrderNo
	END
	
	
	WHILE EXISTS(SELECT 1 FROM ORD_OrderBomDet obd
		INNER JOIN Temp_OrderNo_4_ProdOrder ord on ord.OrderNo=obd.OrderNo)
	BEGIN
		DELETE TOP(20000) obd OUTPUT deleted.* 
		INTO Sconit5_Arch.dbo.ORD_OrderBomDet FROM ORD_OrderBomDet obd
		INNER JOIN Temp_OrderNo_4_ProdOrder ord on ord.OrderNo=obd.OrderNo
	END	

	WHILE EXISTS(SELECT 1 FROM ORD_OrderBackflushDet obd
		INNER JOIN Temp_OrderNo_4_ProdOrder ord on ord.OrderNo=obd.OrderNo)
	BEGIN
		DELETE TOP(20000) obd OUTPUT deleted.* 
		INTO Sconit5_Arch.dbo.ORD_OrderBackflushDet FROM ORD_OrderBackflushDet obd
		INNER JOIN Temp_OrderNo_4_ProdOrder ord on ord.OrderNo=obd.OrderNo
	END		
	
	WHILE EXISTS(SELECT 1 FROM SAP_ProdOpBackflush pf
		INNER JOIN Temp_OrderNo_4_ProdOrder ord on ord.OrderNo=obd.OrderNo)
	BEGIN
		DELETE TOP(20000) obd OUTPUT deleted.* 
		INTO Sconit5_Arch.dbo.SAP_ProdOpBackflush FROM SAP_ProdOpBackflush pf
		INNER JOIN Temp_OrderNo_4_ProdOrder ord on ord.OrderNo=pf.OrderNo
	END	
	
	WHILE EXISTS(SELECT TOP 1 * FROM SAP_ProdOpReport pr
		INNER JOIN Temp_OrderNo_4_ProdOrder ord on ord.OrderNo=pr.OrderNo)
	BEGIN
		DELETE TOP(20000) obd OUTPUT deleted.* 
		INTO Sconit5_Arch.dbo.SAP_ProdOpReport FROM SAP_ProdOpReport pr
		INNER JOIN Temp_OrderNo_4_ProdOrder ord on ord.OrderNo=obd.OrderNo
	END	

	WHILE EXISTS(SELECT 1 FROM SAP_CancelProdOpReport cpr
		INNER JOIN Temp_OrderNo_4_ProdOrder ord on ord.OrderNo=obd.OrderNo)
	BEGIN
		DELETE TOP(20000) obd OUTPUT deleted.* 
		INTO Sconit5_Arch.dbo.SAP_CancelProdOpReport FROM SAP_CancelProdOpReport cpr
		INNER JOIN Temp_OrderNo_4_ProdOrder ord on ord.OrderNo=cpr.OrderNo
	END				
	
	DROP TABLE Temp_RecNo_4_ProdOrder
	DROP TABLE Temp_OrderNo_4_ProdOrder
	--SELECT TOP 100 * FROM SAP_CancelProdOpReport
	--SELECT TOP 100 * FROM SAP_ProdOpReport
	--SELECT TOP 100 * FROM SAP_ProdOpBackflush
END 

GO
