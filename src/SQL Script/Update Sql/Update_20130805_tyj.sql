go
alter table INV_LocationDetPref add ItemDesc varchar(100) null
go
insert into SYS_Menu values('Url_LocationDetailPref_View','安全库存','Menu.MasterData',600,'安全库存','~/LocationDetailPref/Index','~/Content/Images/Nav/Default.png',1)
go
insert into ACC_Permission values('Url_LocationDetailPref_View','安全库存','MasterData')
go

insert into SYS_Menu values('Url_LocationDetailPref_View','安全库存','Menu.MasterData',600,'安全库存','~/LocationDetailPref/Index','~/Content/Images/Nav/Default.png',1)
go
insert into ACC_Permission values('Url_LocationDetailPref_View','安全库存','MasterData')