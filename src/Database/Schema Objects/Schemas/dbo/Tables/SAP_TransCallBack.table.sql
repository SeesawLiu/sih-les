CREATE TABLE [dbo].[SAP_TransCallBack] (
    [Id]         BIGINT         IDENTITY (1, 1) NOT NULL,
    [FRBNR]      VARCHAR (16)   NULL,
    [SGTXT]      VARCHAR (50)   NULL,
    [MBLNR]      VARCHAR (10)   NULL,
    [ZEILE]      VARCHAR (4)    NULL,
    [BUDAT]      CHAR (14)      NULL,
    [CPUDT]      CHAR (14)      NULL,
    [MTYPE]      VARCHAR (4)    NULL,
    [MSTXT]      NVARCHAR (220) NULL,
    [CreateDate] DATETIME       NOT NULL
);

