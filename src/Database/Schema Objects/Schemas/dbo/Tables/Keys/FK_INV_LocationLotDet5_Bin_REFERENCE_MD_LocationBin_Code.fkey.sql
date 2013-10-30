ALTER TABLE [dbo].[INV_LocationLotDet_5]
    ADD CONSTRAINT [FK_INV_LocationLotDet5_Bin_REFERENCE_MD_LocationBin_Code] FOREIGN KEY ([Bin]) REFERENCES [dbo].[MD_LocationBin] ([Code]) ON DELETE NO ACTION ON UPDATE NO ACTION;

