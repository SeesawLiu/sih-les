CREATE TABLE [dbo].[LOG_OrderTraceDet] (
    [Id]               BIGINT          IDENTITY (1, 1) NOT NULL,
    [UUID]             VARCHAR (50)    NULL,
    [Type]             VARCHAR (5)     NULL,
    [Item]             VARCHAR (50)    NULL,
    [RefItemCode]      VARCHAR (50)    NULL,
    [ItemDesc]         VARCHAR (100)   NULL,
    [ManufactureParty] VARCHAR (50)    NULL,
    [Location]         VARCHAR (50)    NULL,
    [OrderNo]          VARCHAR (50)    NULL,
    [ReqTime]          DATETIME        NOT NULL,
    [OrderQty]         DECIMAL (18, 8) NULL,
    [FinishQty]        DECIMAL (18, 8) NULL,
    [CreateDate]       DATETIME        DEFAULT (getdate()) NOT NULL
);

