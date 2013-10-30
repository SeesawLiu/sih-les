CREATE TABLE [dbo].[ORD_OrderTracer] (
    [Id]                 INT             IDENTITY (1, 1) NOT NULL,
    [Code]               VARCHAR (50)    NULL,
    [ReqTime]            DATETIME        NULL,
    [Item]               VARCHAR (50)    NULL,
    [OrderedQty]         DECIMAL (18, 8) NULL,
    [FinishedQty]        DECIMAL (18, 8) NULL,
    [Qty]                DECIMAL (18, 8) NULL,
    [RefOrderLocTransId] INT             NULL,
    [OrderDetailId]      INT             NULL
);

