ALTER TABLE [dbo].[MSG_MessageQueue]
    ADD CONSTRAINT [DF_MSG_MessageQueue_CreateTime] DEFAULT (getdate()) FOR [CreateTime];

