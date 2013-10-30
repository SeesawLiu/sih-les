ALTER TABLE [dbo].[ORD_OrderDet_2]
    ADD CONSTRAINT [FK_ORD_OrderDet2_PickStrategy_REFERENCE_SCM_PickStrategy_Code] FOREIGN KEY ([PickStrategy]) REFERENCES [dbo].[SCM_PickStrategy] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

