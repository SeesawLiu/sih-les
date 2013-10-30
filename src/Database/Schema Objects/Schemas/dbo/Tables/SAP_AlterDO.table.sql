CREATE TABLE [dbo].[SAP_AlterDO] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [OrderNo]         VARCHAR (10)    NULL,
    [Sequence]        INT             NULL,
    [Item]            VARCHAR (18)    NULL,
    [Plant]           CHAR (4)        NULL,
    [Location]        CHAR (4)        NULL,
    [Qty]             DECIMAL (18, 8) NULL,
    [Uom]             CHAR (3)        NULL,
    [KUNAG]           VARCHAR (20)    NULL,
    [KUNNR]           VARCHAR (20)    NULL,
    [Action]          CHAR (1)        NULL,
    [Status]          TINYINT         NOT NULL,
    [CreateDate]      DATETIME        NOT NULL,
    [LastModifyDate]  DATETIME        NOT NULL,
    [ErrorCount]      INT             NOT NULL,
    [WindowTime]      DATETIME        NULL,
    [ExternalOrderno] VARCHAR (50)    NULL
);

