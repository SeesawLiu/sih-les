ALTER TABLE [dbo].[ORD_OrderOpCPTime]
    ADD CONSTRAINT [DF_ORD_OrderOpCPTime_CreateDate] DEFAULT (getdate()) FOR [CreateDate];

