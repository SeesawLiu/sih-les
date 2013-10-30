ALTER TABLE [dbo].[ORD_OrderMstr_0]
    ADD CONSTRAINT [FK_ORD_OrderMstr0_ProdLineFact_REFERENCE_SCM_ProdLineFact_Code] FOREIGN KEY ([ProdLineFact]) REFERENCES [dbo].[SCM_ProdLineFact] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

