CREATE VIEW [dbo].[VIEW_IpMstr]
AS
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_1
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_2
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_3
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_4
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_5
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_6
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_7
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_8
UNION ALL
SELECT     IpNo, EffDate, IsShipScanHu, Version, CloseReason, CloseUserNm, CloseUser, CloseDate, LastModifyDate, LastModifyUserNm, CreateDate, LastModifyUser, 
                      CreateUserNm, CreateUser, HuTemplate, RecTemplate, RecGapTo, AsnTemplate, IsCheckPartyToAuth, IsCheckPartyFromAuth, IsRecScanHu, IsAsnUniqueRec, 
                      IsRecFifo, IsRecFulfillUC, IsRecExceed, IsPrintRec, IsAsnPrinted, IsPrintAsn, CreateHuOpt, IsAutoReceive, Dock, ShipToContact, ShipToFax, ShipToCell, ShipToTel, 
                      ShipToAddr, ShipTo, ShipFromContact, ShipFromFax, ShipFromCell, ShipFromTel, ShipFromAddr, ShipFrom, PartyToNm, PartyTo, PartyFromNm, PartyFrom, 
                      ArriveTime, DepartTime, Status, QualityType, OrderType, OrderSubType, Type, GapIpNo, ExtIpNo, SeqNo, WMSNo, Flow, ShipNo, Vehicle
FROM         dbo.ORD_IpMstr_0
