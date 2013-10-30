CREATE TABLE [dbo].[MSG_Email] (
    [Id]             INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Subject]        VARCHAR (256) NOT NULL,
    [Body]           TEXT          NOT NULL,
    [MailTo]         VARCHAR (256) NOT NULL,
    [ReplayTo]       VARCHAR (256) NULL,
    [Priority]       TINYINT       NOT NULL,
    [Status]         TINYINT       NOT NULL,
    [CreateDate]     DATETIME      NOT NULL,
    [LastModifyDate] DATETIME      NOT NULL
);

