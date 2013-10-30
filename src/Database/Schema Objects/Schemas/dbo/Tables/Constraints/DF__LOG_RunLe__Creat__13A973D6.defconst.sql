ALTER TABLE [dbo].[LOG_RunLeanEngine]
    ADD CONSTRAINT [DF__LOG_RunLe__Creat__13A973D6] DEFAULT (getdate()) FOR [CreateDate];

