insert into SYS_EntityPreference(Id, Seq, Value, Desc1, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate)
values(11057, 206, 'A', '备件生产默认SAP生产线', 2603,	'用户 超级', '2011-01-01 00:00:00.000', 2603,	'用户 超级', '2011-01-01 00:00:00.000')
go

alter table CUST_OpRefMap add Location varchar(50)
go