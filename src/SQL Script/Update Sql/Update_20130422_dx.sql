alter table PRD_WorkingCalendar drop column FlowStrategy
go
alter table PRD_StandardWorkingCalendar drop column FlowStrategy
go
alter table PRD_WorkingCalendar add Category tinyint
go
update PRD_WorkingCalendar set Category = 0
go
alter table PRD_WorkingCalendar alter column Category tinyint not null
go
alter table PRD_StandardWorkingCalendar add Category tinyint
go
update PRD_StandardWorkingCalendar set Category = 0
go
alter table PRD_StandardWorkingCalendar alter column Category tinyint not null
go
alter table PRD_RoutingDet drop column JointOpRef
go
alter table PRD_StandardWorkingCalendar add ProdLine varchar(50)
go
alter table PRD_WorkingCalendar add ProdLine varchar(50)
go
alter table PRD_WorkingCalendar alter column Region varchar(50) null
go
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('ProdLineType', 5, 'CodeDetail_ProdLineType_SparePart', 0, 6)
go
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('ProdLineType', 6, 'CodeDetail_ProdLineType_Repair', 0, 7)
go
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('ProdLineType', 7, 'CodeDetail_ProdLineType_Trial', 0, 8)
go
update SCM_FlowMstr set ProdLineType = 1 where Code in ('RAC00', 'RAD00')
update SCM_FlowMstr set ProdLineType = 2 where Code in ('RCA00', 'RCB00')
update SCM_FlowMstr set ProdLineType = 3 where Code in ('RAA00', 'RAB00')
go
alter table MD_SpecialTime add ProdLine varchar(50)
go
alter table MD_SpecialTime alter column Region varchar(50) null
go