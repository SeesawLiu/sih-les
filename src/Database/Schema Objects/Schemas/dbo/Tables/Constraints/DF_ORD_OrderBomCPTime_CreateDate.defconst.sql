ALTER TABLE [dbo].[ORD_OrderBomCPTime]
    ADD CONSTRAINT [DF_ORD_OrderBomCPTime_CreateDate] DEFAULT (getdate()) FOR [CreateDate];

