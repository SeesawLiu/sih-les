ALTER TABLE [dbo].[SAP_TransCallBack]
    ADD CONSTRAINT [DF_SAP_TransCallBack_CreateDate] DEFAULT (getdate()) FOR [CreateDate];

