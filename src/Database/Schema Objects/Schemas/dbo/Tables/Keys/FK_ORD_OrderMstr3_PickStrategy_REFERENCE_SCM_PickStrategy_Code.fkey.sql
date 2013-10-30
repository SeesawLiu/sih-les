ALTER TABLE [dbo].[ORD_OrderMstr_3]
    ADD CONSTRAINT [FK_ORD_OrderMstr3_PickStrategy_REFERENCE_SCM_PickStrategy_Code] FOREIGN KEY ([PickStrategy]) REFERENCES [dbo].[SCM_PickStrategy] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

