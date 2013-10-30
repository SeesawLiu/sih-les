ALTER TABLE [dbo].[SAP_ProdOpReport]
    ADD CONSTRAINT [DF_SAP_ProdOpReport_Status] DEFAULT ((0)) FOR [Status];

