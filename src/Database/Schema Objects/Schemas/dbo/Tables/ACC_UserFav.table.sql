CREATE TABLE [dbo].[ACC_UserFav] (
    [Id]     INT          IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [UserId] INT          NOT NULL,
    [Menu]   VARCHAR (50) NULL
);

