CREATE TABLE [dbo].[SAP_ProdOpReport] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [AUFNR]            VARCHAR (50)    NOT NULL,
    [WORKCENTER]       VARCHAR (50)    NOT NULL,
    [GAMNG]            DECIMAL (18, 8) NOT NULL,
    [Status]           TINYINT         NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL,
    [ErrorCount]       INT             NOT NULL,
    [SCRAP]            DECIMAL (18, 8) NOT NULL,
    [TEXT]             VARCHAR (50)    NULL,
    [IsCancel]         BIT             NOT NULL,
    [OrderNo]          VARCHAR (50)    NULL,
    [OrderOpId]        INT             NULL,
    [RecNo]            VARCHAR (50)    NULL,
    [EffDate]          DATETIME        NOT NULL,
    [CreateUser]       INT             NULL,
    [CreateUserNm]     VARCHAR (50)    NULL,
    [LastModifyUser]   INT             NULL,
    [LastModifyUserNm] VARCHAR (50)    NULL,
    [Version]          INT             NOT NULL,
    [ProdLine]         VARCHAR (50)    NULL
);

