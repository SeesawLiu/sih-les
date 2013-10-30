ALTER TABLE [dbo].[FIS_CreateIpDAT]
    ADD CONSTRAINT [DF_FIS_CreateIpDAT_ErrorCount] DEFAULT ((0)) FOR [ErrorCount];

