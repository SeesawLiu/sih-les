insert into ACC_Permission values('Client_ForceScanMaterialIn','关键件强制扫描','Terminal')
go 
insert into SYS_Menu values('Url_OrderItemTrace_FrameBarCode','车架扫描','Menu.Quality.Trans',700,'车架扫描','~/OrderItemTrace/FrameBarCode','~/Content/Images/Nav/Default.png',1)
go
insert into ACC_Permission values('Url_OrderItemTrace_FrameBarCode','车架扫描','Quality')
go
alter table SAP_ProdOrder add [VERSION] varchar(50)
go