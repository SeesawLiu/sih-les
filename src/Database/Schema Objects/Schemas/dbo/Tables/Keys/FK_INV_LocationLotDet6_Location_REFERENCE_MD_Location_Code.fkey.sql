ALTER TABLE [dbo].[INV_LocationLotDet_6]
    ADD CONSTRAINT [FK_INV_LocationLotDet6_Location_REFERENCE_MD_Location_Code] FOREIGN KEY ([Location]) REFERENCES [dbo].[MD_Location] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

