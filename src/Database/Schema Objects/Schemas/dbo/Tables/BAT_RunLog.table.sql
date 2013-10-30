CREATE TABLE [dbo].[BAT_RunLog] (
    [Id]        INT            IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [JobId]     INT            NOT NULL,
    [TriggerId] INT            NOT NULL,
    [StartTime] DATETIME       NOT NULL,
    [EndTime]   DATETIME       NULL,
    [Status]    TINYINT        NOT NULL,
    [Message]   VARCHAR (2000) NULL
);

