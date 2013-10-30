alter table ORD_OrderMstr drop column IsPlanPause
alter table ORD_OrderMstr_0 drop column IsPlanPause
alter table ORD_OrderMstr_1 drop column IsPlanPause
alter table ORD_OrderMstr_2 drop column IsPlanPause
alter table ORD_OrderMstr_3 drop column IsPlanPause
alter table ORD_OrderMstr_4 drop column IsPlanPause
alter table ORD_OrderMstr_5 drop column IsPlanPause
alter table ORD_OrderMstr_6 drop column IsPlanPause
alter table ORD_OrderMstr_7 drop column IsPlanPause
alter table ORD_OrderMstr_8 drop column IsPlanPause
go
alter table ORD_OrderMstr drop column IsPause
alter table ORD_OrderMstr_0 drop column IsPause
alter table ORD_OrderMstr_1 drop column IsPause
alter table ORD_OrderMstr_2 drop column IsPause
alter table ORD_OrderMstr_3 drop column IsPause
alter table ORD_OrderMstr_4 drop column IsPause
alter table ORD_OrderMstr_5 drop column IsPause
alter table ORD_OrderMstr_6 drop column IsPause
alter table ORD_OrderMstr_7 drop column IsPause
alter table ORD_OrderMstr_8 drop column IsPause
go
alter table ORD_OrderMstr drop column IsPLPause
alter table ORD_OrderMstr_0 drop column IsPLPause
alter table ORD_OrderMstr_1 drop column IsPLPause
alter table ORD_OrderMstr_2 drop column IsPLPause
alter table ORD_OrderMstr_3 drop column IsPLPause
alter table ORD_OrderMstr_4 drop column IsPLPause
alter table ORD_OrderMstr_5 drop column IsPLPause
alter table ORD_OrderMstr_6 drop column IsPLPause
alter table ORD_OrderMstr_7 drop column IsPLPause
alter table ORD_OrderMstr_8 drop column IsPLPause
go
alter table ORD_OrderMstr add PauseStatus tinyint
alter table ORD_OrderMstr_0 add PauseStatus tinyint
alter table ORD_OrderMstr_1 add PauseStatus tinyint
alter table ORD_OrderMstr_2 add PauseStatus tinyint
alter table ORD_OrderMstr_3 add PauseStatus tinyint
alter table ORD_OrderMstr_4 add PauseStatus tinyint
alter table ORD_OrderMstr_5 add PauseStatus tinyint
alter table ORD_OrderMstr_6 add PauseStatus tinyint
alter table ORD_OrderMstr_7 add PauseStatus tinyint
alter table ORD_OrderMstr_8 add PauseStatus tinyint
go
update ORD_OrderMstr set PauseStatus = 0
update ORD_OrderMstr_0 set PauseStatus = 0
update ORD_OrderMstr_1 set PauseStatus = 0
update ORD_OrderMstr_2 set PauseStatus = 0
update ORD_OrderMstr_3 set PauseStatus = 0
update ORD_OrderMstr_4 set PauseStatus = 0
update ORD_OrderMstr_5 set PauseStatus = 0
update ORD_OrderMstr_6 set PauseStatus = 0
update ORD_OrderMstr_7 set PauseStatus = 0
update ORD_OrderMstr_8 set PauseStatus = 0
go
alter table ORD_OrderMstr alter column PauseStatus tinyint not null
alter table ORD_OrderMstr_0 alter column PauseStatus tinyint not null
alter table ORD_OrderMstr_1 alter column PauseStatus tinyint not null
alter table ORD_OrderMstr_2 alter column PauseStatus tinyint not null
alter table ORD_OrderMstr_3 alter column PauseStatus tinyint not null
alter table ORD_OrderMstr_4 alter column PauseStatus tinyint not null
alter table ORD_OrderMstr_5 alter column PauseStatus tinyint not null
alter table ORD_OrderMstr_6 alter column PauseStatus tinyint not null
alter table ORD_OrderMstr_7 alter column PauseStatus tinyint not null
alter table ORD_OrderMstr_8 alter column PauseStatus tinyint not null
go
insert into SYS_CodeMstr(Code, Desc1, Type) values('PauseStatus', 'ÔÝÍ£×´Ì¬', 0)
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('PauseStatus', 0, 'CodeDetail_PauseStatus_None', 1, 1)
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('PauseStatus', 1, 'CodeDetail_PauseStatus_PlanPause', 0, 2)
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('PauseStatus', 2, 'CodeDetail_PauseStatus_Paused', 0, 3)
go
alter table ORD_OrderMstr drop column Seq
alter table ORD_OrderMstr_0 drop column Seq
alter table ORD_OrderMstr_1 drop column Seq
alter table ORD_OrderMstr_2 drop column Seq
alter table ORD_OrderMstr_3 drop column Seq
alter table ORD_OrderMstr_4 drop column Seq
alter table ORD_OrderMstr_5 drop column Seq
alter table ORD_OrderMstr_6 drop column Seq
alter table ORD_OrderMstr_7 drop column Seq
alter table ORD_OrderMstr_8 drop column Seq
go
alter table ORD_OrderMstr drop column SapSeq
alter table ORD_OrderMstr_0 drop column SapSeq
alter table ORD_OrderMstr_1 drop column SapSeq
alter table ORD_OrderMstr_2 drop column SapSeq
alter table ORD_OrderMstr_3 drop column SapSeq
alter table ORD_OrderMstr_4 drop column SapSeq
alter table ORD_OrderMstr_5 drop column SapSeq
alter table ORD_OrderMstr_6 drop column SapSeq
alter table ORD_OrderMstr_7 drop column SapSeq
alter table ORD_OrderMstr_8 drop column SapSeq
go
alter table ORD_OrderMstr drop column IsCheckPartyFromAuth
alter table ORD_OrderMstr_0 drop column IsCheckPartyFromAuth
alter table ORD_OrderMstr_1 drop column IsCheckPartyFromAuth
alter table ORD_OrderMstr_2 drop column IsCheckPartyFromAuth
alter table ORD_OrderMstr_3 drop column IsCheckPartyFromAuth
alter table ORD_OrderMstr_4 drop column IsCheckPartyFromAuth
alter table ORD_OrderMstr_5 drop column IsCheckPartyFromAuth
alter table ORD_OrderMstr_6 drop column IsCheckPartyFromAuth
alter table ORD_OrderMstr_7 drop column IsCheckPartyFromAuth
alter table ORD_OrderMstr_8 drop column IsCheckPartyFromAuth
go
alter table ORD_OrderMstr drop column IsCheckPartyToAuth
alter table ORD_OrderMstr_0 drop column IsCheckPartyToAuth
alter table ORD_OrderMstr_1 drop column IsCheckPartyToAuth
alter table ORD_OrderMstr_2 drop column IsCheckPartyToAuth
alter table ORD_OrderMstr_3 drop column IsCheckPartyToAuth
alter table ORD_OrderMstr_4 drop column IsCheckPartyToAuth
alter table ORD_OrderMstr_5 drop column IsCheckPartyToAuth
alter table ORD_OrderMstr_6 drop column IsCheckPartyToAuth
alter table ORD_OrderMstr_7 drop column IsCheckPartyToAuth
alter table ORD_OrderMstr_8 drop column IsCheckPartyToAuth
go