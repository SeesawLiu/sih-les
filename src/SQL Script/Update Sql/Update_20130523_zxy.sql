
update sys_menu set parent ='Menu.Production.Trans',seq =50 where code ='Url_AliquotStartTask_View'
go

 
 insert into sys_menu values('Url_WorkCenter_View','工作中心','Menu.MasterData',205,'工作中心','~/WorkCenter/Index','~/Content/Images/Nav/Default.png',1)
 go
 insert into acc_permission values('Url_WorkCenter_View','工作中心','MasterData')
 go
 