insert into sys_menu values
('Url_AliquotStartTask_View','分装上线任务','Menu.Production.Info',202,'分装上线任务','~/AliquotStartTask/Index','~/Content/Images/Nav/Default.png',1)

insert into acc_permission values('Url_AliquotStartTask_View','分装上线任务','Production')

go


update sys_menu set isactive =0 where code ='Url_OrderMstr_Distribution_ReturnNew'
go

 
insert into sys_menu values
('Url_OrderMstr_Distribution_ReturnShip','发货','Url_OrderMstr_Distribution_Return',21,'发货','~/DistributionOrder/ReturnShipIndex','~/Content/Images/Nav/Default.png',1)


insert into acc_permission values
('Url_OrderMstr_Distribution_ReturnShip','发货退货单发货','Distribution')
go