insert into sys_codemstr values ('SapOrderType','Sap生产单类型',0)

insert into sys_codedet values ('SapOrderType','Z902','面向库存',0,1)
insert into sys_codedet values ('SapOrderType','ZP01','试制',0,2)
insert into sys_codedet values ('SapOrderType','Z90R','返工',0,3)
insert into sys_codedet values ('SapOrderType','Z903','备件',0,4)
insert into sys_codedet values ('SapOrderType','Z901','整车',0,5)

update sys_menu set name = '整车入库' where code = 'Url_OrderMstr_Production_Receive'

update sys_menu set name = '非整车生产线维护' where code = 'Url_ProductionFlow_View'

insert into acc_permission values ('Client_VanOrderReceive','整车下线','Terminal')
