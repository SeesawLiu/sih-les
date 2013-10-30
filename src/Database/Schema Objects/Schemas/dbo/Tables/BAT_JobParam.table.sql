CREATE TABLE [dbo].[BAT_JobParam] (
    [Id]         INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [JobId]      INT           NOT NULL,
    [ParamKey]   VARCHAR (50)  NOT NULL,
    [ParamValue] VARCHAR (256) NOT NULL
);

