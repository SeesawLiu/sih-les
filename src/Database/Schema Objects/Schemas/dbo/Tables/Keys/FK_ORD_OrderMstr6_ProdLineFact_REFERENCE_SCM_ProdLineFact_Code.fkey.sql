ALTER TABLE [dbo].[ORD_OrderMstr_6]
    ADD CONSTRAINT [FK_ORD_OrderMstr6_ProdLineFact_REFERENCE_SCM_ProdLineFact_Code] FOREIGN KEY ([ProdLineFact]) REFERENCES [dbo].[SCM_ProdLineFact] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

