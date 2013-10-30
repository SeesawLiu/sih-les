update SYS_Menu set Seq=300 where Code='Url_SI_SAP'
go
update SYS_Menu set IsActive =0 where Parent like 'Url_SI_%'
go
update SYS_Menu set Code='Url_SI_SAP_CancelProdOpReport_View',PageUrl='~/CancelProdOpReport/Index' where Code='Url_SI_SAP_CancelProdSeqReport_View'
go
update SYS_Menu set Code='Url_SI_SAP_ProdOpReport_View',PageUrl='~/SAPProdOpReport/Index' where Code='Url_SI_SAP_ProdSeqReport_View'
go
update ACC_Permission set Code='Url_SI_SAP_CancelProdOpReport_View' where Code='Url_SI_SAP_CancelProdSeqReport_View'
go
update ACC_Permission set code='Url_SI_SAP_ProdOpReport_View' where Code='Url_SI_SAP_ProdSeqReport_View'

update SYS_Menu set IsActive =1 where Code  in( 'Url_SI_SAP_InvTrans_View'
,'Url_SI_SAP_CancelProdOpReport_View','Url_SI_SAP_ProdOpReport_View')
go