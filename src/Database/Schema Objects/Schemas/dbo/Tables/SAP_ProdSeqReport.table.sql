CREATE TABLE [dbo].[SAP_ProdSeqReport] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [AUFNR]          VARCHAR (50)    NOT NULL,
    [WORKCENTER]     VARCHAR (50)    NOT NULL,
    [GAMNG]          DECIMAL (18, 8) NOT NULL,
    [Status]         TINYINT         NOT NULL,
    [CreateDate]     DATETIME        NOT NULL,
    [LastModifyDate] DATETIME        NOT NULL,
    [ErrorCount]     INT             NOT NULL,
    [SCRAP]          DECIMAL (18, 8) NOT NULL,
    [TEXT]           VARCHAR (50)    NOT NULL
);

