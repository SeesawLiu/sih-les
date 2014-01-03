IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[USP_BAK_VanOrder]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[USP_BAK_VanOrder]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[USP_BAK_VanOrder] 
AS 
BEGIN
	DECLARE @BakDate datetime
	SET @BakDate=DATEADD(DAY,-7,GETDATE())
	
	Create table #Temp_OrderNo
	(
		OrderNo varchar(50) Primary Key
	)	
	CREATE INDEX IX_Temp_OrderNo ON #Temp_OrderNo(OrderNo)
	
	insert into #Temp_OrderNo(OrderNo) SELECT OrderNo FROM ORD_OrderMstr_4 om 
	WHERE om.ProdLineType in (1,2,3,4,9) AND om.CloseDate < @BakDate
	AND NOT EXISTS(SELECT * FROM ORD_OrderMstr_4 b WHERE om.TraceCode = b.TraceCode AND b.Status<>4)
	AND NOT EXISTS(SELECT * FROM SAP_ProdOpReport por WHERE om.OrderNo = por.OrderNo and por.Status<>1)
	AND NOT EXISTS(SELECT * FROM SAP_CancelProdOpReport cpor WHERE om.OrderNo = cpor.OrderNo and cpor.Status<>1)
	
	--update ORD_OrderMstr_4 set CloseDate = GETDATE() where Status = 4 and CloseDate is null and ProdLineType not in (1,2,3,4,9)
	--update ORD_OrderMstr_4 set CancelDate = GETDATE() where Status = 5 and CancelDate is null and ProdLineType not in (1,2,3,4,9)
		
	--INSERT INTO #Temp_OrderNo(OrderNo)
	--SELECT om.OrderNo FROM ORD_OrderMstr_4 om
	--WHERE om.ProdLineType not in (1,2,3,4,9) and om.Status = 4 AND om.CloseDate < @BakDate	
	
	--INSERT INTO #Temp_OrderNo(OrderNo)
	--SELECT om.OrderNo FROM ORD_OrderMstr_4 om
	--WHERE om.ProdLineType not in (1,2,3,4,9) and om.Status = 5 AND om.CancelDate < @BakDate	
	
	
	
	--WHILE EXISTS(SELECT 1 FROM ORD_OrderBackflushDet obd INNER JOIN #Temp_OrderNo ord ON obd.OrderNo=ord.OrderNo)
	--BEGIN
	--	DELETE TOP(10000) obd OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderBackflushDet 
	--	FROM ORD_OrderBackflushDet obd
	--	INNER JOIN #Temp_OrderNo ord 
	--		ON obd.OrderNo=ord.OrderNo
	--END
	
	--WHILE EXISTS(SELECT obd.* FROM ORD_OrderBomDet obd INNER JOIN #Temp_OrderNo ord ON obd.OrderNo=ord.OrderNo)
	--BEGIN
	--	DELETE TOP(10000) obd  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderBomDet 
	--	FROM ORD_OrderBomDet obd
	--	INNER JOIN #Temp_OrderNo ord 
	--		ON obd.OrderNo=ord.OrderNo
	--END
	
	Create table #Temp_RecNo
	(
		RecNo varchar(50) Primary Key
	)
	CREATE INDEX IX_Temp_RecNo ON #Temp_RecNo(RecNo)
	
	INSERT INTO #Temp_RecNo(RecNo) SELECT DISTINCT RecNo
	FROM ORD_RecDet_4 rd INNER JOIN #Temp_OrderNo ord ON rd.OrderNo=ord.OrderNo
	
	WHILE EXISTS(SELECT * FROM ORD_RecLocationDet_4 rld INNER JOIN #Temp_RecNo rn ON rld.RecNo=rn.RecNo)
	BEGIN
		DELETE TOP(10000) rld  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_RecLocationDet_4 
		FROM ORD_RecLocationDet_4 rld 
		INNER JOIN #Temp_RecNo rn 
			ON rld.RecNo=rn.RecNo	
	END
	
	WHILE EXISTS(SELECT * FROM ORD_RecDet_4 rd INNER JOIN #Temp_RecNo rn ON rd.RecNo=rn.RecNo)
	BEGIN
		DELETE TOP(10000) rd  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_RecDet_4 
		FROM ORD_RecDet_4 rd
		INNER JOIN #Temp_RecNo rn 
			ON rd.RecNo=rn.RecNo	
	END	
	
	WHILE EXISTS(SELECT * FROM ORD_RecMstr_4 rm INNER JOIN #Temp_RecNo rn ON rm.RecNo=rn.RecNo)
	BEGIN
		DELETE TOP(10000) rm  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_RecMstr_4 
		FROM ORD_RecMstr_4 rm
		INNER JOIN #Temp_RecNo rn 
			ON rm.RecNo=rn.RecNo	
	END		
	
	WHILE EXISTS(SELECT od.* FROM ORD_OrderDet_4 od INNER JOIN #Temp_OrderNo ord ON od.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) od  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderDet_4 
		FROM ORD_OrderDet_4 od
		INNER JOIN #Temp_OrderNo ord 
			ON od.OrderNo=ord.OrderNo
	END
	
	WHILE EXISTS(SELECT op.* FROM ORD_OrderOpReport op
	INNER JOIN #Temp_OrderNo ord ON op.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) op  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderOpReport 
		FROM ORD_OrderOpReport op
		INNER JOIN #Temp_OrderNo ord 
			ON op.OrderNo=ord.OrderNo
	END	
	
	WHILE EXISTS(SELECT op.* FROM ORD_OrderOp op
	INNER JOIN #Temp_OrderNo ord ON op.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) op  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderOp 
		FROM ORD_OrderOp op
		INNER JOIN #Temp_OrderNo ord 
			ON op.OrderNo=ord.OrderNo
	END	
	
	WHILE EXISTS(SELECT os.* FROM ORD_OrderSeq os INNER JOIN ORD_OrderMstr_4 om ON om.TraceCode=os.TraceCode 
	INNER JOIN #Temp_OrderNo ord ON om.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) os  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderDet_4 
		FROM ORD_OrderSeq os INNER JOIN ORD_OrderMstr_4 om 
			ON om.TraceCode=os.TraceCode 
		INNER JOIN #Temp_OrderNo ord 
			ON om.OrderNo=ord.OrderNo
	END	
	
	WHILE EXISTS(SELECT om.* FROM ORD_OrderMstr_4 om INNER JOIN #Temp_OrderNo ord ON om.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) om  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderMstr_4 
		FROM ORD_OrderMstr_4 om
		INNER JOIN #Temp_OrderNo ord 
			ON om.OrderNo=ord.OrderNo
	END
END 

GO