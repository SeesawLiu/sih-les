ALTER TABLE [dbo].[SAP_InvLoc]
    ADD CONSTRAINT [DF_SAP_InvLoc_CreateDate] DEFAULT (getdate()) FOR [CreateDate];

