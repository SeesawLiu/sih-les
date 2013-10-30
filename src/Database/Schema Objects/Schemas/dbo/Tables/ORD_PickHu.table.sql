CREATE TABLE [dbo].[ORD_PickHu] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [PickId]           VARCHAR (50)  NOT NULL,
    [RepackHu]         VARCHAR (50)  NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

