
alter table PRD_MultiSupplyGroup add [KBEffSupplier] varchar(50) NULL
go
alter table PRD_MultiSupplyGroup add KBTargetCycleQty int null
go
update PRD_MultiSupplyGroup set KBTargetCycleQty =0 
go
alter table PRD_MultiSupplyGroup alter column KBTargetCycleQty int not null
go
alter table PRD_MultiSupplyGroup add KBAccumulateQty int null
go
update PRD_MultiSupplyGroup set KBAccumulateQty =0 
go
alter table PRD_MultiSupplyGroup alter column KBAccumulateQty int not null
go


alter table PRD_MultiSupplyItem drop column SupplierNm 
go
alter table PRD_MultiSupplyItem add [Proportion] varchar(100) NULL
go


alter table PRD_MultiSupplyItem add ItemDesc varchar(100) null
go
update PRD_MultiSupplyItem set ItemDesc=(select Desc1 from MD_Item where code =item)
go
alter table PRD_MultiSupplyItem alter column ItemDesc varchar(100) not null
go
alter table PRD_MultiSupplyItem add SubstituteGroup varchar(50) null
go


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
if exists(select 1 from sysobjects where name ='VIEW_MultiSupplyGroup')
drop view [VIEW_MultiSupplyGroup]
go
CREATE VIEW [dbo].[VIEW_MultiSupplyGroup]
AS
SELECT     ROW_NUMBER() OVER (ORDER BY grp.groupno) AS RowId, grp.GroupNo, grp.Desc1 as Description, grp.EffSupplier, grp.TargetCycleQty, grp.AccumulateQty, sup.Supplier, 
sup.Seq, sup.CycleQty, sup.SpillQty, sup.Proportion, sup.IsActive, it.Item, it.ItemDesc, it.SubstituteGroup,it.Id
FROM         dbo.PRD_MultiSupplyGroup AS grp LEFT JOIN
                      dbo.PRD_MultiSupplySupplier AS sup ON grp.GroupNo = sup.GroupNo LEFT JOIN
                      dbo.PRD_MultiSupplyItem AS it ON it.GroupNo = sup.GroupNo AND it.Supplier = sup.Supplier


GO

insert into sys_menu  values
('Url_MultiSupplyGroup_View','多轨组','Menu.Production.Setup',50500,'多轨组','~/MultiSupplyGroup/Index','~/Content/Images/Nav/Default.png',1)
insert into acc_permission values('Url_MultiSupplyGroup_View','查看多轨组','Production')
insert into acc_permission values('Url_MultiSupplyGroup_Edit','编辑多轨组','Production')
go


insert into SYS_EntityPreference values (10123,21,'9000000','ExportMaxRows',3088,'jey',getdate(),3088,'jey',getdate())
go
