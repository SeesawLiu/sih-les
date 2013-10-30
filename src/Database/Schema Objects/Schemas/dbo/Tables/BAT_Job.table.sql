CREATE TABLE [dbo].[BAT_Job] (
    [Id]          INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Name]        VARCHAR (100) NOT NULL,
    [Desc1]       VARCHAR (256) NOT NULL,
    [ServiceType] VARCHAR (256) NOT NULL
);

