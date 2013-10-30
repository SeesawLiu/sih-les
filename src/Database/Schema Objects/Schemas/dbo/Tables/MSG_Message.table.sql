CREATE TABLE [dbo].[MSG_Message] (
    [Id]             INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [SendTo]         VARCHAR (256) NOT NULL,
    [Content]        VARCHAR (256) NOT NULL,
    [Priority]       TINYINT       NOT NULL,
    [Status]         TINYINT       NOT NULL,
    [CreateDate]     DATETIME      NOT NULL,
    [LastModifyDate] DATETIME      NOT NULL
);

