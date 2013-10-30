CREATE TABLE [dbo].[INP_RejectMstr] (
    [RejNo]            VARCHAR (50)  NOT NULL,
    [RefNo]            VARCHAR (50)  NULL,
    [Region]           VARCHAR (50)  NOT NULL,
    [Status]           TINYINT       NOT NULL,
    [IsPrint]          BIT           NOT NULL,
    [HandleResult]     TINYINT       NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [Version]          INT           NOT NULL,
    [InpType]          TINYINT       NOT NULL
);

