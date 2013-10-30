ALTER TABLE [dbo].[ORD_OrderDet_7]
    ADD CONSTRAINT [FK_ORD_OrderDet7_Currency_REFERENCE_MD_Currency_Code] FOREIGN KEY ([Currency]) REFERENCES [dbo].[MD_Currency] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

