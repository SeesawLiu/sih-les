CREATE TABLE [dbo].[LOG_GenVanProdOrder] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [AUFNR]      VARCHAR (50)  NULL,
    [ZLINE]      VARCHAR (50)  NULL,
    [ProdLine]   VARCHAR (50)  NULL,
    [BatchNo]    VARCHAR (50)  NULL,
    [Msg]        VARCHAR (500) NULL,
    [CreateDate] DATETIME      NOT NULL
);

