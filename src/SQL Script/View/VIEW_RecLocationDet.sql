IF EXISTS(SELECT * FROM SYS.objects WHERE type='V' AND name='VIEW_RecLocationDet')
	DROP VIEW VIEW_RecLocationDet
GO

CREATE VIEW VIEW_RecLocationDet
AS
SELECT a.Id,a.RecNo,a.RecDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.WMSSeq FROM ORD_RecLocationDet_0 AS a
UNION ALL
SELECT a.Id,a.RecNo,a.RecDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.WMSSeq FROM ORD_RecLocationDet_1 AS a
UNION ALL
SELECT a.Id,a.RecNo,a.RecDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.WMSSeq FROM ORD_RecLocationDet_2 AS a
UNION ALL
SELECT a.Id,a.RecNo,a.RecDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.WMSSeq FROM ORD_RecLocationDet_3 AS a
UNION ALL
SELECT a.Id,a.RecNo,a.RecDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.WMSSeq FROM ORD_RecLocationDet_4 AS a
UNION ALL
SELECT a.Id,a.RecNo,a.RecDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.WMSSeq FROM ORD_RecLocationDet_5 AS a
UNION ALL
SELECT a.Id,a.RecNo,a.RecDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.WMSSeq FROM ORD_RecLocationDet_6 AS a
UNION ALL
SELECT a.Id,a.RecNo,a.RecDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.WMSSeq FROM ORD_RecLocationDet_7 AS a
UNION ALL
SELECT a.Id,a.RecNo,a.RecDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.WMSSeq FROM ORD_RecLocationDet_8 AS a
GO

