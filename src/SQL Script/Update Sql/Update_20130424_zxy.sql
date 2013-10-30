
alter table dbo.MD_SpecialTime add Category tinyint null
go
update  MD_SpecialTime set Category =0 
go
alter table dbo.MD_SpecialTime alter column Category tinyint not null
go

update sys_menu set name ='区域停线时间',desc1 ='区域停线时间' where code ='Url_SpecialTime_RestView'
update sys_menu set name ='区域加班时间',desc1 ='区域加班时间' where code ='Url_SpecialTime_WorkView'
go
update acc_permission set desc1= '查看区域停线时间' where code ='Url_SpecialTime_RestView' 
update acc_permission set desc1= '编辑区域停线时间' where code ='Url_SpecialTime_RestEdit' 
update acc_permission set desc1= '删除区域停线时间' where code ='Url_SpecialTime_RestDelete' 
update acc_permission set desc1= '查看区域加班时间' where code ='Url_SpecialTime_WorkView' 
update acc_permission set desc1= '编辑区域加班时间' where code ='Url_SpecialTime_WorkEdit' 
update acc_permission set desc1= '删除区域加班时间' where code ='Url_SpecialTime_WorkDelete' 
go

insert into sys_menu values
('Url_ProdLineSpecialTime_RestView','生产线停线时间','Menu.Production.Setup',50404,'生产线停线时间','~/ProdLineSpecialTime/Index','~/Content/Images/Nav/Default.png',1)
go
insert into sys_menu values
('Url_ProdLineSpecialTime_WorkView','生产线加班时间','Menu.Production.Setup',50405,'生产线加班时间','~/ProdLineSpecialTime/WorkIndex','~/Content/Images/Nav/Default.png',1)
go

insert into acc_permission values('Url_ProdLineSpecialTime_RestView','查看生产线停线时间','Production')
insert into acc_permission values('Url_ProdLineSpecialTime_RestEdit','编辑生产线停线时间','Production')
insert into acc_permission values('Url_ProdLineSpecialTime_RestDelete','删除生产线停线时间','Production')
go
insert into acc_permission values('Url_ProdLineSpecialTime_WorkView','查看生产线加班时间','Production')
insert into acc_permission values('Url_ProdLineSpecialTime_WorkEdit','编辑生产线加班时间','Production')
insert into acc_permission values('Url_ProdLineSpecialTime_WorkDelete','删除生产线加班时间','Production')
go

