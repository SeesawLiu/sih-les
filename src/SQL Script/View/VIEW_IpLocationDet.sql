IF EXISTS(SELECT * FROM SYS.objects WHERE type='V' AND name='VIEW_IpLocationDet')
	DROP VIEW VIEW_IpLocationDet
GO

CREATE VIEW VIEW_IpLocationDet
AS
SELECT a.Id,a.IpNo,a.IpDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.RecQty,a.IsClose,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.Version,a.WMSSeq FROM ORD_IpLocationDet_0 AS a
UNION ALL
SELECT a.Id,a.IpNo,a.IpDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.RecQty,a.IsClose,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.Version,a.WMSSeq FROM ORD_IpLocationDet_1 AS a
UNION ALL
SELECT a.Id,a.IpNo,a.IpDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.RecQty,a.IsClose,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.Version,a.WMSSeq FROM ORD_IpLocationDet_2 AS a
UNION ALL
SELECT a.Id,a.IpNo,a.IpDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.RecQty,a.IsClose,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.Version,a.WMSSeq FROM ORD_IpLocationDet_3 AS a
UNION ALL
SELECT a.Id,a.IpNo,a.IpDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.RecQty,a.IsClose,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.Version,a.WMSSeq FROM ORD_IpLocationDet_4 AS a
UNION ALL
SELECT a.Id,a.IpNo,a.IpDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.RecQty,a.IsClose,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.Version,a.WMSSeq FROM ORD_IpLocationDet_5 AS a
UNION ALL
SELECT a.Id,a.IpNo,a.IpDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.RecQty,a.IsClose,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.Version,a.WMSSeq FROM ORD_IpLocationDet_6 AS a
UNION ALL
SELECT a.Id,a.IpNo,a.IpDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.RecQty,a.IsClose,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.Version,a.WMSSeq FROM ORD_IpLocationDet_7 AS a
UNION ALL
SELECT a.Id,a.IpNo,a.IpDetId,a.OrderType,a.OrderDetId,a.Item,a.HuId,a.LotNo,a.IsCreatePlanBill,a.IsCS,a.PlanBill,a.ActBill,a.QualityType,a.IsFreeze,a.IsATP,a.OccupyType,a.OccupyRefNo,a.Qty,a.RecQty,a.IsClose,a.CreateUser,a.CreateUserNm,a.CreateDate,a.LastModifyUser,a.LastModifyUserNm,a.LastModifyDate,a.Version,a.WMSSeq FROM ORD_IpLocationDet_8 AS a
GO