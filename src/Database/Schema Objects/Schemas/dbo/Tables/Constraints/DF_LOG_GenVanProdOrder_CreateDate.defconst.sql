ALTER TABLE [dbo].[LOG_GenVanProdOrder]
    ADD CONSTRAINT [DF_LOG_GenVanProdOrder_CreateDate] DEFAULT (getdate()) FOR [CreateDate];

