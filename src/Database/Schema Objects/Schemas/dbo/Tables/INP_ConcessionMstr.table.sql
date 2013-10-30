CREATE TABLE [dbo].[INP_ConcessionMstr] (
    [CCSNo]            VARCHAR (50)  NOT NULL,
    [RejNo]            VARCHAR (50)  NULL,
    [RefNo]            VARCHAR (50)  NULL,
    [Status]           TINYINT       NOT NULL,
    [Region]           VARCHAR (50)  NOT NULL,
    [IsPrint]          BIT           NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [Version]          INT           NOT NULL,
    [WMSNo]            VARCHAR (50)  NULL
);

