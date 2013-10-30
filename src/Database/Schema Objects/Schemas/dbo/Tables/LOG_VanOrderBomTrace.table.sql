CREATE TABLE [dbo].[LOG_VanOrderBomTrace] (
    [Id]               BIGINT          IDENTITY (1, 1) NOT NULL,
    [UUID]             VARCHAR (50)    NULL,
    [ProdLine]         VARCHAR (50)    NULL,
    [VanOrderNo]       VARCHAR (50)    NULL,
    [VanOrderBomDetId] INT             NULL,
    [Item]             VARCHAR (50)    NULL,
    [RefItemCode]      VARCHAR (50)    NULL,
    [ItemDesc]         VARCHAR (100)   NULL,
    [OpRef]            VARCHAR (50)    NULL,
    [LocFrom]          VARCHAR (50)    NULL,
    [LocTo]            VARCHAR (50)    NULL,
    [OrderQty]         DECIMAL (18, 8) NULL,
    [CPTime]           DATETIME        NULL,
    [CreateDate]       DATETIME        DEFAULT (getdate()) NULL
);

