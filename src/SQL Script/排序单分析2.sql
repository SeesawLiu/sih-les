select cp.*,i.desc1 from ord_orderbomcptime cp inner join md_item i on cp.item = i.code where cp.vanprodline = 'RAA00' and cp.assprodline = 'raa00' and cp.orderno = 'D400004683'

select * from dbo.LOG_GenSequenceOrder

select * from ord_orderseq where prodline = 'raa00'

select * from ord_ordermstr_4 where tracecode = '0070125390'

select * from dbo.LE_OrderBomCPTimeSnapshot where item = '5801271863'

select * from ord_orderbomcptime where orderno = 'D400004767' and item = '5801271932' order by cptime asc 

select * from ord_orderbomcptime where orderno = 'D400004755' order by cptime asc 

select * from ord_ordermstr_4 where orderno = 'D400004683'
select * from md_item where code = '17315297'

select * from ord_orderbomdet obd inner join scm_flowdet fd on obd.item = fd.item where obd.orderno = 'D400004767' and fd.flow = 'tyre-a'