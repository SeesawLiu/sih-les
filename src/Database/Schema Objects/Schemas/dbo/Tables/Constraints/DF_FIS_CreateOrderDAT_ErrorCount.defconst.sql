ALTER TABLE [dbo].[FIS_CreateOrderDAT]
    ADD CONSTRAINT [DF_FIS_CreateOrderDAT_ErrorCount] DEFAULT ((0)) FOR [ErrorCount];

