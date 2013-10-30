CREATE TABLE [dbo].[MD_Currency] (
    [Code]             VARCHAR (50)  NOT NULL,
    [Name]             VARCHAR (50)  NOT NULL,
    [IsBase]           BIT           NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

