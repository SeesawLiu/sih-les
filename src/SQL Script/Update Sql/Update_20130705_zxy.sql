
delete from sys_codedet 
where desc1 in ('CodeDetail_FlowStrategy_KIT','CodeDetail_FlowStrategy_MRP','CodeDetail_FlowStrategy_JIT2')
go
update sys_codedet set value='75',seq=6 where desc1 ='CodeDetail_FlowStrategy_ANDON'

go
