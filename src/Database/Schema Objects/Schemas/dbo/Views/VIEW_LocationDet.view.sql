

CREATE VIEW [dbo].[VIEW_LocationDet]
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
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VIEW_LocationDet';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 1, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'VIEW_LocationDet';

