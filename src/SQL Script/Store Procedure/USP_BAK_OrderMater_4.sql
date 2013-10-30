IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_BAK_OrderMater_4') 
     DROP PROCEDURE USP_BAK_OrderMater_4 
GO 
CREATE PROCEDURE [dbo].[USP_BAK_OrderMater_4] 
AS 
BEGIN
	DECLARE @BakDate datetime
	SET @BakDate=DATEADD(DAY,-7,GETDATE())
	SELECT OrderNo INTO #Temp_OrderNo FROM ORD_OrderMstr_4 om WHERE om.ProdLineType in (1,2,3,4,9) AND om.CreateDate<@BakDate
	AND NOT EXISTS(SELECT * FROM ORD_OrderMstr_4 b WHERE om.TraceCode=b.TraceCode AND b.Status<>4)
	
	INSERT INTO #Temp_OrderNo(OrderNo)
	SELECT om.OrderNo FROM ORD_OrderMstr_4 om
	WHERE om.ProdLineType in (0,6,7,8) AND om.CreateDate<@BakDate	
	
	WHILE EXISTS(SELECT 1 FROM ORD_OrderBackflushDet WHERE CreateDate<@BakDate AND OrderNo IS NULL)
	BEGIN
		DELETE TOP(10000) FROM ORD_OrderBackflushDet  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderBackflushDet 
			WHERE CreateDate<@BakDate AND OrderNo IS NULL
	END
	
	WHILE EXISTS(SELECT 1 FROM ORD_OrderBackflushDet obd INNER JOIN #Temp_OrderNo ord ON obd.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) obd OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderBackflushDet 
		FROM ORD_OrderBackflushDet obd
		INNER JOIN #Temp_OrderNo ord 
			ON obd.OrderNo=ord.OrderNo
	END
	
	WHILE EXISTS(SELECT obd.* FROM ORD_OrderBomDet obd INNER JOIN #Temp_OrderNo ord ON obd.OrderNo=ord.OrderNo)
	BEGIN
		DELETE TOP(10000) obd  OUTPUT deleted.* INTO [Sconit5_Arch].[DBO].ORD_OrderBomDet 
		FROM ORD_OrderBomDet obd
		INNER JOIN #Temp_OrderNo ord 
			ON obd.OrderNo=ord.OrderNo
	END
	
	SELECT DISTINCT RecNo INTO #Temp_RecNo FROM ORD_RecDet_4 rd INNER JOIN #Temp_OrderNo ord ON rd.OrderNo=ord.OrderNo
	
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
	
	WHILE EXISTS(SELECT os.* FROM ORD_OrderSeq os INNER JOIN ORD_OrderMstr_4 om ON om.TraceCode=os.TraceCode INNER JOIN #Temp_OrderNo ord ON om.OrderNo=ord.OrderNo)
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
