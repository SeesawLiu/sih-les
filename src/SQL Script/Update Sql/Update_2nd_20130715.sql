update PRD_RoutingDet set IsReport = 0 where Routing = 'RAA00' and WorkCenter = '940-05'
delete from PRD_RoutingDet where Routing = 'RAA00' and Op > 1230
update PRD_RoutingDet set IsReport = 0 where Routing = 'RAB00' and WorkCenter = '940-05'
delete from PRD_RoutingDet where Routing = 'RAB00' and Op > 1230
delete from PRD_ProdLineWorkCenter where Flow = 'RAA00' and WorkCenter = '940-06'
delete from PRD_ProdLineWorkCenter where Flow = 'RAA00' and WorkCenter = '940-07'
delete from PRD_ProdLineWorkCenter where Flow = 'RAA00' and WorkCenter = '940-08'
delete from PRD_ProdLineWorkCenter where Flow = 'RAB00' and WorkCenter = '940-06'
delete from PRD_ProdLineWorkCenter where Flow = 'RAB00' and WorkCenter = '940-07'
delete from PRD_ProdLineWorkCenter where Flow = 'RAB00' and WorkCenter = '940-08'
go

ALTER TABLE ORD_IpDet_8 ADD EBELN_EBELP AS (ExtNO + '&' + ExtSeq) PERSISTED 
go
CREATE NONCLUSTERED INDEX [IX_IpDet_8_0_1] ON [dbo].[ORD_IpDet_8] 
(
	[EBELN_EBELP] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
ALTER TABLE ORD_OrderDet_8 ADD EBELN_EBELP AS (ExtNO + '&' + ExtSeq) PERSISTED 
go
CREATE NONCLUSTERED INDEX [IX_OrderDet_8_0_1] ON [dbo].[ORD_OrderDet_8] 
(
	[EBELN_EBELP] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

