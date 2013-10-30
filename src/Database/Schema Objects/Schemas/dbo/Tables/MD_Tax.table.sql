CREATE TABLE [dbo].[MD_Tax] (
    [Code]             VARCHAR (50)    NOT NULL,
    [Name]             VARCHAR (50)    NOT NULL,
    [Rate]             DECIMAL (18, 8) NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL
);

