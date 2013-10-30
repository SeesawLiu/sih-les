ALTER TABLE [dbo].[INP_InspectResultExt]
    ADD CONSTRAINT [FK_InspectResultExt_inpRetId_FOREIGN_KEY] FOREIGN KEY ([inpRetId]) REFERENCES [dbo].[INP_InspectResultExt] ([id]) ON DELETE NO ACTION ON UPDATE NO ACTION;

