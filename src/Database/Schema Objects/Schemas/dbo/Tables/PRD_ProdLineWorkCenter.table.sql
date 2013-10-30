CREATE TABLE [dbo].[PRD_ProdLineWorkCenter] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [Flow]             VARCHAR (50)  NOT NULL,
    [WorkCenter]       VARCHAR (50)  NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

