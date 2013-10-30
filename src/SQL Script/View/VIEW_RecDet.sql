IF EXISTS(SELECT * FROM SYS.objects WHERE type='V' AND name='VIEW_RecDet')
	DROP VIEW VIEW_RecDet
GO

CREATE VIEW VIEW_RecDet
AS
SELECT     Id, RecNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, RecQty, ScrapQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, CreateUser, CreateUserNm, 
                      CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, OrderNo, OrderDetSeq, Seq, IpNo, IpDetId, IpDetSeq, IpDetType, OrderType, OrderSubType, Version, IsInspect, 
                      BillAddr, PriceList, UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, BaseUom, ManufactureParty, ExtNo, ExtSeq, IpGapAdjOpt, Flow
FROM         dbo.ORD_RecDet_1
UNION ALL
SELECT     Id, RecNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, RecQty, ScrapQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, CreateUser, CreateUserNm, 
                      CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, OrderNo, OrderDetSeq, Seq, IpNo, IpDetId, IpDetSeq, IpDetType, OrderType, OrderSubType, Version, IsInspect, 
                      BillAddr, PriceList, UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, BaseUom, ManufactureParty, ExtNo, ExtSeq, IpGapAdjOpt, Flow
FROM         dbo.ORD_RecDet_2
UNION ALL
SELECT     Id, RecNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, RecQty, ScrapQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, CreateUser, CreateUserNm, 
                      CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, OrderNo, OrderDetSeq, Seq, IpNo, IpDetId, IpDetSeq, IpDetType, OrderType, OrderSubType, Version, IsInspect, 
                      BillAddr, PriceList, UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, BaseUom, ManufactureParty, ExtNo, ExtSeq, IpGapAdjOpt, Flow
FROM         dbo.ORD_RecDet_3
UNION ALL
SELECT     Id, RecNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, RecQty, ScrapQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, CreateUser, CreateUserNm, 
                      CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, OrderNo, OrderDetSeq, Seq, IpNo, IpDetId, IpDetSeq, IpDetType, OrderType, OrderSubType, Version, IsInspect, 
                      BillAddr, PriceList, UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, BaseUom, ManufactureParty, ExtNo, ExtSeq, IpGapAdjOpt, Flow
FROM         dbo.ORD_RecDet_4
UNION ALL
SELECT     Id, RecNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, RecQty, ScrapQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, CreateUser, CreateUserNm, 
                      CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, OrderNo, OrderDetSeq, Seq, IpNo, IpDetId, IpDetSeq, IpDetType, OrderType, OrderSubType, Version, IsInspect, 
                      BillAddr, PriceList, UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, BaseUom, ManufactureParty, ExtNo, ExtSeq, IpGapAdjOpt, Flow
FROM         dbo.ORD_RecDet_5
UNION ALL
SELECT     Id, RecNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, RecQty, ScrapQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, CreateUser, CreateUserNm, 
                      CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, OrderNo, OrderDetSeq, Seq, IpNo, IpDetId, IpDetSeq, IpDetType, OrderType, OrderSubType, Version, IsInspect, 
                      BillAddr, PriceList, UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, BaseUom, ManufactureParty, ExtNo, ExtSeq, IpGapAdjOpt, Flow
FROM         dbo.ORD_RecDet_6
UNION ALL
SELECT     Id, RecNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, RecQty, ScrapQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, CreateUser, CreateUserNm, 
                      CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, OrderNo, OrderDetSeq, Seq, IpNo, IpDetId, IpDetSeq, IpDetType, OrderType, OrderSubType, Version, IsInspect, 
                      BillAddr, PriceList, UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, BaseUom, ManufactureParty, ExtNo, ExtSeq, IpGapAdjOpt, Flow
FROM         dbo.ORD_RecDet_7
UNION ALL
SELECT     Id, RecNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, RecQty, ScrapQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, CreateUser, CreateUserNm, 
                      CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, OrderNo, OrderDetSeq, Seq, IpNo, IpDetId, IpDetSeq, IpDetType, OrderType, OrderSubType, Version, IsInspect, 
                      BillAddr, PriceList, UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, BaseUom, ManufactureParty, ExtNo, ExtSeq, IpGapAdjOpt, Flow
FROM         dbo.ORD_RecDet_8
UNION ALL
SELECT     Id, RecNo, OrderDetId, Item, ItemDesc, RefItemCode, Uom, UC, QualityType, RecQty, ScrapQty, UnitQty, LocFrom, LocFromNm, LocTo, LocToNm, CreateUser, CreateUserNm, 
                      CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, OrderNo, OrderDetSeq, Seq, IpNo, IpDetId, IpDetSeq, IpDetType, OrderType, OrderSubType, Version, IsInspect, 
                      BillAddr, PriceList, UnitPrice, Currency, IsProvEst, Tax, IsIncludeTax, BillTerm, BaseUom, ManufactureParty, ExtNo, ExtSeq, IpGapAdjOpt, Flow
FROM         dbo.ORD_RecDet_0




GO


