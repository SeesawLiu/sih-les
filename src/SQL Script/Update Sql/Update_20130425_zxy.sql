delete from acc_userpermission where permissionid = (select id from acc_permission where code ='Url_InSequenceGroup_View')
delete from acc_Rolepermission where permissionid = (select id from acc_permission where code ='Url_InSequenceGroup_View')
delete from acc_permission where code ='Url_InSequenceGroup_View'
delete from sys_menu where code ='Url_InSequenceGroup_View'
go

update sys_menu set name ='序列组',desc1='序列组' where code ='Url_SequenceGroup_View'
update acc_permission set desc1='序列组' where code ='Url_SequenceGroup_View'
go

insert into sys_menu values
('Url_OrderSeq_View','车序查询','Menu.Production.Info',205,'车序查询','~/OrderSeq/Index','~/Content/Images/Nav/Default.png',1)
go
insert into acc_permission values('Url_OrderSeq_View','车序查询','Production')
go







