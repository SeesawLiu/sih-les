ALTER TABLE [dbo].[ORD_OrderDet_7]
    ADD CONSTRAINT [FK_ORD_OrderDet7_ManufactureParty_REFERENCE_MD_Party_Code] FOREIGN KEY ([ManufactureParty]) REFERENCES [dbo].[MD_Party] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

