IF EXISTS(SELECT * FROM SYS.objects WHERE type='V' AND name='VIEW_LocationDet')
	DROP VIEW VIEW_LocationDet
GO

CREATE VIEW VIEW_LocationDet
AS
/*GROUP BY Location, Item*/ SELECT max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_0 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_1 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_2 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_3 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_4 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_5 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_6 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_7 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_8 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_9 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_10 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_11 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_12 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_13 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_14 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_15 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_16 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_17 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_18 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
UNION all
SELECT      max(det.Id) as Id, det.Location, det.Item, hu.ManufactureParty, sum(det.Qty) AS Qty, sum(CASE WHEN det.IsCs = 1 THEN det.Qty ELSE 0 END) AS CsQty, 
                      sum(CASE WHEN det.QualityType = 0 THEN det.Qty ELSE 0 END) AS QualifyQty, sum(CASE WHEN det.QualityType = 1 THEN det.Qty ELSE 0 END) AS InspectQty, 
                      sum(CASE WHEN det.QualityType = 2 THEN det.Qty ELSE 0 END) AS RejectQty, sum(CASE WHEN det.IsATP = 1 THEN det.Qty ELSE 0 END) AS ATPQty, 
                      sum(CASE WHEN det.IsFreeze = 1 THEN det.Qty ELSE 0 END) AS FreezeQty
FROM         inv_locationlotdet_19 AS det LEFT JOIN
                      dbo.INV_Hu AS hu ON det.HuId = hu.HuId
WHERE     (det.Qty <> 0)
GROUP BY det.Location, det.Item, hu.ManufactureParty
GO
