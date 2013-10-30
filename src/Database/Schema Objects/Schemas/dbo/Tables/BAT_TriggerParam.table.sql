CREATE TABLE [dbo].[BAT_TriggerParam] (
    [Id]         INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [TriggerId]  INT           NOT NULL,
    [ParamKey]   VARCHAR (50)  NOT NULL,
    [ParamValue] VARCHAR (256) NOT NULL
);

