ALTER TABLE [dbo].[ORD_OrderMstr_1]
    ADD CONSTRAINT [FK_ORD_OrderMstr1_Currency_REFERENCE_MD_Currency_Code] FOREIGN KEY ([Currency]) REFERENCES [dbo].[MD_Currency] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

