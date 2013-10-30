IF EXISTS(SELECT * FROM SYS.objects WHERE type='V' AND name='VIEW_MultiSupplyGroup')
	DROP VIEW VIEW_MultiSupplyGroup
GO

CREATE VIEW VIEW_MultiSupplyGroup
AS
SELECT     ROW_NUMBER() OVER (ORDER BY grp.groupno) AS RowId, grp.GroupNo, grp.Desc1 as Description, grp.EffSupplier, grp.TargetCycleQty, grp.AccumulateQty, sup.Supplier, 
sup.Seq, sup.CycleQty, sup.SpillQty, sup.Proportion, sup.IsActive, it.Item, it.ItemDesc, it.SubstituteGroup,it.Id
FROM         dbo.PRD_MultiSupplyGroup AS grp LEFT JOIN
                      dbo.PRD_MultiSupplySupplier AS sup ON grp.GroupNo = sup.GroupNo LEFT JOIN
                      dbo.PRD_MultiSupplyItem AS it ON it.GroupNo = sup.GroupNo AND it.Supplier = sup.Supplier

GO
