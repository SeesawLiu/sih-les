CREATE TABLE [dbo].[ISS_IssueLevel] (
    [Code]             VARCHAR (50)  NOT NULL,
    [Desc1]            VARCHAR (100) NOT NULL,
    [IsActive]         BIT           NOT NULL,
    [IsSubmit]         BIT           NOT NULL,
    [IsInProcess]      BIT           NOT NULL,
    [Seq]              INT           NOT NULL,
    [IsDefault]        BIT           NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

