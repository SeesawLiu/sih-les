delete from ord_orderdet_2 where orderno in (select orderno from ord_ordermstr_2 where orderstrategy = 4);

delete from ord_ordermstr_2 where orderstrategy = 4;

truncate table dbo.LOG_GenSequenceOrder

update scm_seqgroup set PrevOrderNo = null,prevseq = null,prevsubseq = null,prevtracecode = null,prevdeliverydate = null,prevdeliverycount = null

delete from dbo.ORD_SeqOrderTrace

select * from scm_seqgroup

exec dbo.USP_LE_SnapshotOrderBomCPTime4LeanEngine

exec dbo.USP_LE_GenSequenceOrder 3088,'su'

select * from LOG_GenSequenceOrder


select * from ord_ordermstr_2 where orderno = 'd200026005'

select * from ord_orderdet_2 where orderno = 'd200026009'
select * from scm_flowmstr where type = 2

select count(*) as col_0_0_ from VIEW_VanOrderSeq vanorderse0_ where (vanorderse0_.Status='In-Process' or vanorderse0_.Status is null) and vanorderse0_.Flow='RAA00' ]\r\nPositional parameters:  #0>InProcess #1>RAD00\r\n[SQL: select count(*) as col_0_0_ from VIEW_VanOrderSeq vanorderse0_ where (vanorderse0_.Status=@p0 or vanorderse0_.Status is null) and vanorderse0_.Flow=@p1