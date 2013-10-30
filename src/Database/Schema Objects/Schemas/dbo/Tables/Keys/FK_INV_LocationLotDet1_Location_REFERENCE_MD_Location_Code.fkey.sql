ALTER TABLE [dbo].[INV_LocationLotDet_1]
    ADD CONSTRAINT [FK_INV_LocationLotDet1_Location_REFERENCE_MD_Location_Code] FOREIGN KEY ([Location]) REFERENCES [dbo].[MD_Location] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

