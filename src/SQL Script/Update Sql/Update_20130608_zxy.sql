

insert into sys_menu values
('Url_OrderMstr_Production_VanView','整车生产单查询','Url_OrderMstr_Production',104,'整车生产单查询','~/ProductionOrder/VanIndex','~/Content/Images/Nav/Default.png',1)
go

insert into acc_permission values('Url_OrderMstr_Production_VanView','整车生产单查询','Production')
go

update sys_menu set name='非整车生产单查询',desc1='非整车生产单查询' where code ='Url_OrderMstr_Production_View'
go

