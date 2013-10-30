CREATE TABLE [dbo].[SAP_PostDO] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [OrderNo]        VARCHAR (50)  NULL,
    [ReceiptNo]      VARCHAR (50)  NULL,
    [Result]         VARCHAR (500) NULL,
    [ZTCODE]         VARCHAR (50)  NULL,
    [Success]        VARCHAR (50)  NULL,
    [LastModifyDate] DATETIME      NULL,
    [Status]         INT           NULL,
    [CreateDate]     DATETIME      NULL,
    [ErrorCount]     INT           NULL
);

