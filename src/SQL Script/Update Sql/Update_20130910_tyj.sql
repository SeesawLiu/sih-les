update SYS_Menu set Code='Menu_Application_DocumentCodeRule' where Code='Menu.Application.DocumentCodeRule'
go
update ACC_Permission set Code='Menu_Application_DocumentCodeRule' where Code='Menu.Application.DocumentCodeRule'
go
update SYS_Menu set Code='Menu_History_Inventory' where Code='Menu.History.Inventory'
go
update ACC_Permission set Code='Menu_History_Inventory' where Code='Menu.History.Inventory'
go
update SYS_Menu set Code='Menu_Hu_Inventory' where Code='Menu.Hu.Inventory'
go
update ACC_Permission set Code='Menu_Hu_Inventory' where Code='Menu.Hu.Inventory'
go
update SYS_Menu set Code='Menu_Inventory_InventoryTrans' where Code='Menu.Inventory.InventoryTrans'
go
update ACC_Permission set Code='Menu_Inventory_InventoryTrans' where Code='Menu.Inventory.InventoryTrans'
go
update SYS_Menu set Code='Menu_Inventory_ViewInventory' where Code='Menu.Inventory.ViewInventory'
go
update ACC_Permission set Code='Menu_Inventory_ViewInventory' where Code='Menu.Inventory.ViewInventory'
go
update SYS_Menu set Code='Menu_LibraryAge_Statements' where Code='Menu.LibraryAge.Statements'
go
update ACC_Permission set Code='Menu_LibraryAge_Statements' where Code='Menu.LibraryAge.Statements'
go
update SYS_Menu set Code='Menu_Transceivers_Statements' where Code='Menu.Transceivers.Statements'
go
update ACC_Permission set Code='Menu_Transceivers_Statements' where Code='Menu.Transceivers.Statements'
go
 alter table  FIS_CreateBarCode add IsCreateDat bit default(0) null
 go
  update  FIS_CreateBarCode set IsCreateDat=0
 go
 alter table FIS_CancelReceiptMaster add IsCreateDat bit default(0) null
 go
 update  FIS_CancelReceiptMaster set IsCreateDat=0
 go
 alter table FIS_ItemStandardPack add IsCreateDat bit default(0) null
 go
 update  FIS_ItemStandardPack set IsCreateDat=0
 go
 alter table FIS_YieldReturn add IsCreateDat bit default(0) null
 go
 update  FIS_YieldReturn set IsCreateDat=0