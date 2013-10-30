ALTER TABLE [dbo].[SAP_InvTrans]
    ADD CONSTRAINT [DF_SAP_InvTrans_CreateDate] DEFAULT (getdate()) FOR [CreateDate];

