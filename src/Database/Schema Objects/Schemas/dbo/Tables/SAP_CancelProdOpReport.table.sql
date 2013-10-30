CREATE TABLE [dbo].[SAP_CancelProdOpReport] (
    [Id]             INT          IDENTITY (1, 1) NOT NULL,
    [AUFNR]          VARCHAR (50) NOT NULL,
    [TEXT]           VARCHAR (50) NOT NULL,
    [Status]         TINYINT      NOT NULL,
    [CreateDate]     DATETIME     NOT NULL,
    [LastModifyDate] DATETIME     NOT NULL,
    [ErrorCount]     INT          NOT NULL,
    [RecNo]          VARCHAR (50) NULL,
    [OrderNo]        VARCHAR (50) NULL,
    [OrderOpId]      INT          NULL
);

