CREATE TABLE [dbo].[SAP_ProdOrder] (
    [Id]         INT             IDENTITY (1, 1) NOT NULL,
    [BatchNo]    INT             NOT NULL,
    [CreateDate] DATETIME        DEFAULT (getdate()) NOT NULL,
    [AUFNR]      VARCHAR (50)    NULL,
    [WERKS]      VARCHAR (50)    NULL,
    [DAUAT]      VARCHAR (50)    NULL,
    [MATNR]      VARCHAR (50)    NULL,
    [MAKTX]      VARCHAR (50)    NULL,
    [DISPO]      VARCHAR (50)    NULL,
    [CHARG]      VARCHAR (50)    NULL,
    [GSTRS]      DATETIME        NULL,
    [CY_SEQNR]   BIGINT          NULL,
    [GMEIN]      VARCHAR (50)    NULL,
    [GAMNG]      DECIMAL (18, 8) NULL,
    [LGORT]      VARCHAR (50)    NULL,
    [LTEXT]      VARCHAR (50)    NULL,
    [ZLINE]      VARCHAR (50)    NULL,
    [RSNUM]      INT             NULL,
    [AUFPL]      INT             NULL
);

