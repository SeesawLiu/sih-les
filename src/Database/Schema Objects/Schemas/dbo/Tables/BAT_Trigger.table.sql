CREATE TABLE [dbo].[BAT_Trigger] (
    [Id]             INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [JobId]          INT           NOT NULL,
    [Name]           VARCHAR (100) NOT NULL,
    [Desc1]          VARCHAR (256) NOT NULL,
    [PrevFireTime]   DATETIME      NULL,
    [NextFireTime]   DATETIME      NULL,
    [RepeatCount]    INT           NOT NULL,
    [Interval]       INT           NOT NULL,
    [IntervalType]   TINYINT       NOT NULL,
    [TimesTriggered] INT           NOT NULL,
    [Status]         TINYINT       NOT NULL
);

