CREATE TABLE [dbo].[PRD_ShiftMstr] (
    [Code]             VARCHAR (50)  NOT NULL,
    [Name]             VARCHAR (100) NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [ShiftCount]       INT           NOT NULL
);

