CREATE TABLE [dbo].[SAP_ProdRoutingDet] (
    [Id]         INT          IDENTITY (1, 1) NOT NULL,
    [BatchNo]    INT          NOT NULL,
    [CreateDate] DATETIME     DEFAULT (getdate()) NOT NULL,
    [AUFNR]      VARCHAR (50) NULL,
    [WERKS]      VARCHAR (50) NULL,
    [AUFPL]      INT          NULL,
    [APLZL]      INT          NULL,
    [PLNTY]      VARCHAR (50) NULL,
    [PLNNR]      VARCHAR (50) NULL,
    [PLNAL]      VARCHAR (50) NULL,
    [PLNFL]      VARCHAR (50) NULL,
    [VORNR]      VARCHAR (50) NULL,
    [ARBPL]      VARCHAR (50) NULL,
    [RUEK]       VARCHAR (50) NULL,
    [AUTWE]      VARCHAR (50) NULL
);

