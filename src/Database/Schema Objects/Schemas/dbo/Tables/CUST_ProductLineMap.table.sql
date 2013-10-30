CREATE TABLE [dbo].[CUST_ProductLineMap] (
    [SAPProdLine]      VARCHAR (50) NOT NULL,
    [ProdLine]         VARCHAR (50) NULL,
    [TransFlow]        VARCHAR (50) NULL,
    [SaddleFlow]       VARCHAR (50) NULL,
    [CabProdLine]      VARCHAR (50) NULL,
    [ChassisProdLine]  VARCHAR (50) NULL,
    [AssemblyProdLine] VARCHAR (50) NULL,
    [SpecialProdLine]  VARCHAR (50) NULL,
    [MaxOrderCount]    INT          NULL,
    [InitVanOrder]     VARCHAR (50) NULL,
    [Plant]            VARCHAR (50) NULL,
    [IsActive]         BIT          NOT NULL,
    [Type]             TINYINT      NOT NULL
);

