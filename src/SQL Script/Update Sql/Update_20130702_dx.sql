alter table SAP_ProdBomDet add MDMNG_ORG decimal(18, 8)
go
alter table CUST_CabOut add CabItemDesc varchar(100)
go
update co set CabItemDesc = i.Desc1
from CUST_CabOut as co inner join MD_Item as i on co.CabItem = i.Code
go