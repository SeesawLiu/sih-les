ALTER TABLE [dbo].[ORD_IpDet_6]
    ADD CONSTRAINT [FK_ORD_IpDet6_ManufactureParty_REFERENCE_MD_Party_Code] FOREIGN KEY ([ManufactureParty]) REFERENCES [dbo].[MD_Party] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

