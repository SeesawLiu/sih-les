ALTER TABLE [dbo].[ORD_OrderDet_2]
    ADD CONSTRAINT [FK_ORD_OrderDet2_Currency_REFERENCE_MD_Currency_Code] FOREIGN KEY ([Currency]) REFERENCES [dbo].[MD_Currency] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

