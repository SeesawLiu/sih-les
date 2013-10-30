CREATE TABLE [dbo].[INV_StockTakeLoc] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [StNo]             VARCHAR (50)  NOT NULL,
    [Loc]              VARCHAR (50)  NOT NULL,
    [LocName]          VARCHAR (50)  NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

