CREATE TABLE [dbo].[ACC_Permission] (
    [Id]       INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Code]     VARCHAR (50)  NOT NULL,
    [Desc1]    VARCHAR (100) NOT NULL,
    [Category] VARCHAR (50)  NOT NULL
);

