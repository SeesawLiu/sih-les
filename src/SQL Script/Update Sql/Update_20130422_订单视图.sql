USE [sconit5_New]
GO

/****** Object:  View [dbo].[VIEW_OrderMstr]    Script Date: 04/22/2013 10:25:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[VIEW_OrderMstr]
AS
SELECT     OrderNo, Flow,ProdLineFact, TraceCode, OrderStrategy, RefOrderNo, ExtOrderNo, Type, SubType, QualityType, StartTime, WindowTime,  
                      PauseSeq, PauseTime, EffDate,IsQuick,Priority, Status,PartyFrom, PartyFromNm, PartyTo, PartyToNm, ShipFrom, ShipFromAddr, 
                      ShipFromTel, ShipFromCell, ShipFromFax, ShipFromContact, ShipToAddr, ShipTo, ShipToTel, ShipToCell, ShipToFax, ShipToContact, Shift, LocFrom, LocFromNm, 
                      LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, Currency, Dock, Routing, CurtOp, IsAutoRelease, IsAutoStart, IsAutoShip, IsAutoReceive, IsAutoBill, 
                      IsManualCreateDet, IsListPrice, IsPrintOrder, IsOrderPrinted, IsPrintAsn, IsPrintRec, IsShipExceed, IsRecExceed, IsOrderFulfillUC, IsShipFulfillUC, IsRecFulfillUC, 
                      IsShipScanHu, IsRecScanHu, IsCreatePL, IsPLCreate, IsShipFifo, IsRecFifo, IsShipByOrder, IsOpenOrder, IsAsnUniqueRec,                       
                      RecGapTo, RecTemplate, OrderTemplate, AsnTemplate, HuTemplate, BillTerm, CreateHuOpt, ReCalculatePriceOpt, PickStrategy,  ExtraDmdSource,CreateUser,
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, ReleaseDate, ReleaseUser, ReleaseUserNm, StartDate, StartUser, StartUserNm, 
                      CompleteDate, CompleteUser, CompleteUserNm, CloseDate, CloseUser, CloseUserNm, CancelDate, CancelUser, CancelUserNm, CancelReason, Version, FlowDesc, 
                      WMSNo,ProdLineType,PauseStatus,SeqGroup
FROM         dbo.ORD_OrderMstr_1
UNION ALL
SELECT     OrderNo, Flow,ProdLineFact, TraceCode, OrderStrategy, RefOrderNo, ExtOrderNo, Type, SubType, QualityType, StartTime, WindowTime,  
                      PauseSeq, PauseTime, EffDate,IsQuick,Priority, Status,PartyFrom, PartyFromNm, PartyTo, PartyToNm, ShipFrom, ShipFromAddr, 
                      ShipFromTel, ShipFromCell, ShipFromFax, ShipFromContact, ShipToAddr, ShipTo, ShipToTel, ShipToCell, ShipToFax, ShipToContact, Shift, LocFrom, LocFromNm, 
                      LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, Currency, Dock, Routing, CurtOp, IsAutoRelease, IsAutoStart, IsAutoShip, IsAutoReceive, IsAutoBill, 
                      IsManualCreateDet, IsListPrice, IsPrintOrder, IsOrderPrinted, IsPrintAsn, IsPrintRec, IsShipExceed, IsRecExceed, IsOrderFulfillUC, IsShipFulfillUC, IsRecFulfillUC, 
                      IsShipScanHu, IsRecScanHu, IsCreatePL, IsPLCreate, IsShipFifo, IsRecFifo, IsShipByOrder, IsOpenOrder, IsAsnUniqueRec,                       
                      RecGapTo, RecTemplate, OrderTemplate, AsnTemplate, HuTemplate, BillTerm, CreateHuOpt, ReCalculatePriceOpt, PickStrategy,  ExtraDmdSource,CreateUser,
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, ReleaseDate, ReleaseUser, ReleaseUserNm, StartDate, StartUser, StartUserNm, 
                      CompleteDate, CompleteUser, CompleteUserNm, CloseDate, CloseUser, CloseUserNm, CancelDate, CancelUser, CancelUserNm, CancelReason, Version, FlowDesc, 
                      WMSNo,ProdLineType,PauseStatus,SeqGroup
FROM         dbo.ORD_OrderMstr_2
UNION ALL
SELECT     OrderNo, Flow,ProdLineFact, TraceCode, OrderStrategy, RefOrderNo, ExtOrderNo, Type, SubType, QualityType, StartTime, WindowTime,  
                      PauseSeq, PauseTime, EffDate,IsQuick,Priority, Status,PartyFrom, PartyFromNm, PartyTo, PartyToNm, ShipFrom, ShipFromAddr, 
                      ShipFromTel, ShipFromCell, ShipFromFax, ShipFromContact, ShipToAddr, ShipTo, ShipToTel, ShipToCell, ShipToFax, ShipToContact, Shift, LocFrom, LocFromNm, 
                      LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, Currency, Dock, Routing, CurtOp, IsAutoRelease, IsAutoStart, IsAutoShip, IsAutoReceive, IsAutoBill, 
                      IsManualCreateDet, IsListPrice, IsPrintOrder, IsOrderPrinted, IsPrintAsn, IsPrintRec, IsShipExceed, IsRecExceed, IsOrderFulfillUC, IsShipFulfillUC, IsRecFulfillUC, 
                      IsShipScanHu, IsRecScanHu, IsCreatePL, IsPLCreate, IsShipFifo, IsRecFifo, IsShipByOrder, IsOpenOrder, IsAsnUniqueRec,                       
                      RecGapTo, RecTemplate, OrderTemplate, AsnTemplate, HuTemplate, BillTerm, CreateHuOpt, ReCalculatePriceOpt, PickStrategy,  ExtraDmdSource,CreateUser,
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, ReleaseDate, ReleaseUser, ReleaseUserNm, StartDate, StartUser, StartUserNm, 
                      CompleteDate, CompleteUser, CompleteUserNm, CloseDate, CloseUser, CloseUserNm, CancelDate, CancelUser, CancelUserNm, CancelReason, Version, FlowDesc, 
                      WMSNo,ProdLineType,PauseStatus,SeqGroup
FROM         dbo.ORD_OrderMstr_3
UNION ALL
SELECT     OrderNo, Flow,ProdLineFact, TraceCode, OrderStrategy, RefOrderNo, ExtOrderNo, Type, SubType, QualityType, StartTime, WindowTime,  
                      PauseSeq, PauseTime, EffDate,IsQuick,Priority, Status,PartyFrom, PartyFromNm, PartyTo, PartyToNm, ShipFrom, ShipFromAddr, 
                      ShipFromTel, ShipFromCell, ShipFromFax, ShipFromContact, ShipToAddr, ShipTo, ShipToTel, ShipToCell, ShipToFax, ShipToContact, Shift, LocFrom, LocFromNm, 
                      LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, Currency, Dock, Routing, CurtOp, IsAutoRelease, IsAutoStart, IsAutoShip, IsAutoReceive, IsAutoBill, 
                      IsManualCreateDet, IsListPrice, IsPrintOrder, IsOrderPrinted, IsPrintAsn, IsPrintRec, IsShipExceed, IsRecExceed, IsOrderFulfillUC, IsShipFulfillUC, IsRecFulfillUC, 
                      IsShipScanHu, IsRecScanHu, IsCreatePL, IsPLCreate, IsShipFifo, IsRecFifo, IsShipByOrder, IsOpenOrder, IsAsnUniqueRec,                       
                      RecGapTo, RecTemplate, OrderTemplate, AsnTemplate, HuTemplate, BillTerm, CreateHuOpt, ReCalculatePriceOpt, PickStrategy,  ExtraDmdSource,CreateUser,
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, ReleaseDate, ReleaseUser, ReleaseUserNm, StartDate, StartUser, StartUserNm, 
                      CompleteDate, CompleteUser, CompleteUserNm, CloseDate, CloseUser, CloseUserNm, CancelDate, CancelUser, CancelUserNm, CancelReason, Version, FlowDesc, 
                      WMSNo,ProdLineType,PauseStatus,SeqGroup
FROM         dbo.ORD_OrderMstr_4
UNION ALL
SELECT     OrderNo, Flow,ProdLineFact, TraceCode, OrderStrategy, RefOrderNo, ExtOrderNo, Type, SubType, QualityType, StartTime, WindowTime,  
                      PauseSeq, PauseTime, EffDate,IsQuick,Priority, Status,PartyFrom, PartyFromNm, PartyTo, PartyToNm, ShipFrom, ShipFromAddr, 
                      ShipFromTel, ShipFromCell, ShipFromFax, ShipFromContact, ShipToAddr, ShipTo, ShipToTel, ShipToCell, ShipToFax, ShipToContact, Shift, LocFrom, LocFromNm, 
                      LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, Currency, Dock, Routing, CurtOp, IsAutoRelease, IsAutoStart, IsAutoShip, IsAutoReceive, IsAutoBill, 
                      IsManualCreateDet, IsListPrice, IsPrintOrder, IsOrderPrinted, IsPrintAsn, IsPrintRec, IsShipExceed, IsRecExceed, IsOrderFulfillUC, IsShipFulfillUC, IsRecFulfillUC, 
                      IsShipScanHu, IsRecScanHu, IsCreatePL, IsPLCreate, IsShipFifo, IsRecFifo, IsShipByOrder, IsOpenOrder, IsAsnUniqueRec,                       
                      RecGapTo, RecTemplate, OrderTemplate, AsnTemplate, HuTemplate, BillTerm, CreateHuOpt, ReCalculatePriceOpt, PickStrategy,  ExtraDmdSource,CreateUser,
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, ReleaseDate, ReleaseUser, ReleaseUserNm, StartDate, StartUser, StartUserNm, 
                      CompleteDate, CompleteUser, CompleteUserNm, CloseDate, CloseUser, CloseUserNm, CancelDate, CancelUser, CancelUserNm, CancelReason, Version, FlowDesc, 
                      WMSNo,ProdLineType,PauseStatus,SeqGroup
FROM         dbo.ORD_OrderMstr_5
UNION ALL
SELECT     OrderNo, Flow,ProdLineFact, TraceCode, OrderStrategy, RefOrderNo, ExtOrderNo, Type, SubType, QualityType, StartTime, WindowTime,  
                      PauseSeq, PauseTime, EffDate,IsQuick,Priority, Status,PartyFrom, PartyFromNm, PartyTo, PartyToNm, ShipFrom, ShipFromAddr, 
                      ShipFromTel, ShipFromCell, ShipFromFax, ShipFromContact, ShipToAddr, ShipTo, ShipToTel, ShipToCell, ShipToFax, ShipToContact, Shift, LocFrom, LocFromNm, 
                      LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, Currency, Dock, Routing, CurtOp, IsAutoRelease, IsAutoStart, IsAutoShip, IsAutoReceive, IsAutoBill, 
                      IsManualCreateDet, IsListPrice, IsPrintOrder, IsOrderPrinted, IsPrintAsn, IsPrintRec, IsShipExceed, IsRecExceed, IsOrderFulfillUC, IsShipFulfillUC, IsRecFulfillUC, 
                      IsShipScanHu, IsRecScanHu, IsCreatePL, IsPLCreate, IsShipFifo, IsRecFifo, IsShipByOrder, IsOpenOrder, IsAsnUniqueRec,                       
                      RecGapTo, RecTemplate, OrderTemplate, AsnTemplate, HuTemplate, BillTerm, CreateHuOpt, ReCalculatePriceOpt, PickStrategy,  ExtraDmdSource,CreateUser,
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, ReleaseDate, ReleaseUser, ReleaseUserNm, StartDate, StartUser, StartUserNm, 
                      CompleteDate, CompleteUser, CompleteUserNm, CloseDate, CloseUser, CloseUserNm, CancelDate, CancelUser, CancelUserNm, CancelReason, Version, FlowDesc, 
                      WMSNo,ProdLineType,PauseStatus,SeqGroup
FROM         dbo.ORD_OrderMstr_6
UNION ALL
SELECT     OrderNo, Flow,ProdLineFact, TraceCode, OrderStrategy, RefOrderNo, ExtOrderNo, Type, SubType, QualityType, StartTime, WindowTime,  
                      PauseSeq, PauseTime, EffDate,IsQuick,Priority, Status,PartyFrom, PartyFromNm, PartyTo, PartyToNm, ShipFrom, ShipFromAddr, 
                      ShipFromTel, ShipFromCell, ShipFromFax, ShipFromContact, ShipToAddr, ShipTo, ShipToTel, ShipToCell, ShipToFax, ShipToContact, Shift, LocFrom, LocFromNm, 
                      LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, Currency, Dock, Routing, CurtOp, IsAutoRelease, IsAutoStart, IsAutoShip, IsAutoReceive, IsAutoBill, 
                      IsManualCreateDet, IsListPrice, IsPrintOrder, IsOrderPrinted, IsPrintAsn, IsPrintRec, IsShipExceed, IsRecExceed, IsOrderFulfillUC, IsShipFulfillUC, IsRecFulfillUC, 
                      IsShipScanHu, IsRecScanHu, IsCreatePL, IsPLCreate, IsShipFifo, IsRecFifo, IsShipByOrder, IsOpenOrder, IsAsnUniqueRec,                       
                      RecGapTo, RecTemplate, OrderTemplate, AsnTemplate, HuTemplate, BillTerm, CreateHuOpt, ReCalculatePriceOpt, PickStrategy,  ExtraDmdSource,CreateUser,
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, ReleaseDate, ReleaseUser, ReleaseUserNm, StartDate, StartUser, StartUserNm, 
                      CompleteDate, CompleteUser, CompleteUserNm, CloseDate, CloseUser, CloseUserNm, CancelDate, CancelUser, CancelUserNm, CancelReason, Version, FlowDesc, 
                      WMSNo,ProdLineType,PauseStatus,SeqGroup
FROM         dbo.ORD_OrderMstr_7
UNION ALL
SELECT     OrderNo, Flow,ProdLineFact, TraceCode, OrderStrategy, RefOrderNo, ExtOrderNo, Type, SubType, QualityType, StartTime, WindowTime,  
                      PauseSeq, PauseTime, EffDate,IsQuick,Priority, Status,PartyFrom, PartyFromNm, PartyTo, PartyToNm, ShipFrom, ShipFromAddr, 
                      ShipFromTel, ShipFromCell, ShipFromFax, ShipFromContact, ShipToAddr, ShipTo, ShipToTel, ShipToCell, ShipToFax, ShipToContact, Shift, LocFrom, LocFromNm, 
                      LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, Currency, Dock, Routing, CurtOp, IsAutoRelease, IsAutoStart, IsAutoShip, IsAutoReceive, IsAutoBill, 
                      IsManualCreateDet, IsListPrice, IsPrintOrder, IsOrderPrinted, IsPrintAsn, IsPrintRec, IsShipExceed, IsRecExceed, IsOrderFulfillUC, IsShipFulfillUC, IsRecFulfillUC, 
                      IsShipScanHu, IsRecScanHu, IsCreatePL, IsPLCreate, IsShipFifo, IsRecFifo, IsShipByOrder, IsOpenOrder, IsAsnUniqueRec,                       
                      RecGapTo, RecTemplate, OrderTemplate, AsnTemplate, HuTemplate, BillTerm, CreateHuOpt, ReCalculatePriceOpt, PickStrategy,  ExtraDmdSource,CreateUser,
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, ReleaseDate, ReleaseUser, ReleaseUserNm, StartDate, StartUser, StartUserNm, 
                      CompleteDate, CompleteUser, CompleteUserNm, CloseDate, CloseUser, CloseUserNm, CancelDate, CancelUser, CancelUserNm, CancelReason, Version, FlowDesc, 
                      WMSNo,ProdLineType,PauseStatus,SeqGroup
FROM         dbo.ORD_OrderMstr_8
UNION ALL
SELECT     OrderNo, Flow,ProdLineFact, TraceCode, OrderStrategy, RefOrderNo, ExtOrderNo, Type, SubType, QualityType, StartTime, WindowTime,  
                      PauseSeq, PauseTime, EffDate,IsQuick,Priority, Status,PartyFrom, PartyFromNm, PartyTo, PartyToNm, ShipFrom, ShipFromAddr, 
                      ShipFromTel, ShipFromCell, ShipFromFax, ShipFromContact, ShipToAddr, ShipTo, ShipToTel, ShipToCell, ShipToFax, ShipToContact, Shift, LocFrom, LocFromNm, 
                      LocTo, LocToNm, IsInspect, BillAddr, BillAddrDesc, PriceList, Currency, Dock, Routing, CurtOp, IsAutoRelease, IsAutoStart, IsAutoShip, IsAutoReceive, IsAutoBill, 
                      IsManualCreateDet, IsListPrice, IsPrintOrder, IsOrderPrinted, IsPrintAsn, IsPrintRec, IsShipExceed, IsRecExceed, IsOrderFulfillUC, IsShipFulfillUC, IsRecFulfillUC, 
                      IsShipScanHu, IsRecScanHu, IsCreatePL, IsPLCreate, IsShipFifo, IsRecFifo, IsShipByOrder, IsOpenOrder, IsAsnUniqueRec,                       
                      RecGapTo, RecTemplate, OrderTemplate, AsnTemplate, HuTemplate, BillTerm, CreateHuOpt, ReCalculatePriceOpt, PickStrategy,  ExtraDmdSource,CreateUser,
                      CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate, ReleaseDate, ReleaseUser, ReleaseUserNm, StartDate, StartUser, StartUserNm, 
                      CompleteDate, CompleteUser, CompleteUserNm, CloseDate, CloseUser, CloseUserNm, CancelDate, CancelUser, CancelUserNm, CancelReason, Version, FlowDesc, 
                      WMSNo,ProdLineType,PauseStatus,SeqGroup
FROM         dbo.ORD_OrderMstr_0

GO


