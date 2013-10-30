alter table dbo.FIS_CreateSeqOrderDAT add OrderDetId int null

go
  update SYS_Menu set IsActive =0 where Code='Url_Sort_List'

go
insert SYS_Menu values('Url_SequenceOrder_MakeupOrder','手工排序要货','Url_SequenceOrder',400,'手工排序要货','~/SequenceMaster/MakeupIndex','~/Content/Images/Nav/Default.png',1)
go
insert into ACC_Permission values('Url_SequenceOrder_MakeupOrder','手工排序要货','Procurement')
