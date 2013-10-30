CREATE TABLE [dbo].[SYS_EntityPreference] (
    [Id]               INT           NOT NULL,
    [Seq]              INT           NOT NULL,
    [Value]            VARCHAR (100) NOT NULL,
    [Desc1]            VARCHAR (100) NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

