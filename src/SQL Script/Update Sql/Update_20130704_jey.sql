update acc_permission set desc1 = '非整车生产单批量处理' where code = 'Url_OrderMstr_Production_BatchProcess'

update sys_menu set name = '非整车生产单批量处理',desc1 = '非整车生产单批量处理' where code = 'Url_OrderMstr_Production_BatchProcess'

update acc_permission set desc1 = '整车生产单收货' where code = 'Url_OrderMstr_Production_Receive'

update sys_menu set name = '整车生产单收货',desc1 = '整车生产单收货' where code = 'Url_OrderMstr_Production_Receive'
go
insert into ACC_Permission values('Client_ForceScanMaterialIn','强制扫描','Terminal')
go	
insert into SYS_Menu values('Url_OrderItemTrace_FrameBarCode','车架扫描','Menu.Quality.Trans',700,'车架扫描','~/OrderItemTrace/FrameBarCode','~/Content/Images/Nav/Default.png',1)
go
insert into ACC_Permission values('Url_OrderItemTrace_FrameBarCode','车架扫描','Quality')