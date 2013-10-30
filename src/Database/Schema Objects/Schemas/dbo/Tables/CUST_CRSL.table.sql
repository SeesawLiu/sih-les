CREATE TABLE [dbo].[CUST_CRSL] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [OrderNo]          VARCHAR (50)    NULL,
    [Seq]              INT             NULL,
    [Item]             VARCHAR (50)    NULL,
    [EBELN]            VARCHAR (50)    NULL,
    [EBELP]            VARCHAR (50)    NULL,
    [Qty]              DECIMAL (18, 8) NULL,
    [Status]           TINYINT         NULL,
    [ErrorCount]       INT             NULL,
    [ErrorMessage]     VARCHAR (255)   NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL
);

