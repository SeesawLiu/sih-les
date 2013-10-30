

alter table FIS_WMSDatFile add ReceiveTotal decimal(18,8) default(0) null 
go

alter table FIS_WMSDatFile add CancelQty decimal(18,8) default(0)  null 
go
alter table FIS_WMSDatFile add Version int default(1) not null
go
update  FIS_WMSDatFile set ReceiveTotal=Qty,CancelQty=0,Version=1 
go
truncate table FIS_CreateProcurementOrderDAT
go
truncate table LOG_SeqOrderChange
go
alter table FIS_CancelReceiptMaster add constraint CreateDate_Default DEFAULT GETDATE() for CreateDate 
go
select Id, ASN_NO, ASN_ITEM, WH_CODE, WH_DOCK, WH_LOCATION, ITEM_CODE, SUPPLIER_CODE, QTY, UOM, BASE_UNIT_QTY, BASE_UNIT_UOM, QC_FLAG, DELIVERY_DATE, TIME_WINDOW, PO, FINANCE_FLAG, COMPONENT_FLAG, TRACKID, PO_LINE, F80XBJ, F80X_LOCATION, FactoryInfo, ErrorCount, IsCreateDat, CreateUserNm, TIME_STAMP1, FileName  into  FIS_CreateIpDAT2 from FIS_CreateIpDAT 
go
truncate table FIS_CreateIpDAT
go
alter table FIS_CreateIpDAT alter column DELIVERY_DATE datetime not null 
go
go
alter table FIS_CreateIpDAT alter column TIME_STAMP1 datetime null 
go
go
select  Id, Type, MoveType, Sequense, PO, POLine, WMSNo, WMSLine, HandTime, Item, HandResult, ErrorCause, IsCreateDat, FileName, ASNNo, ExtNo, Qty, QtyMark into  FIS_LesINLog2 from FIS_LesINLog 
go
truncate table FIS_LesINLog
go
alter table FIS_LesINLog alter column HandTime datetime not null 
go
