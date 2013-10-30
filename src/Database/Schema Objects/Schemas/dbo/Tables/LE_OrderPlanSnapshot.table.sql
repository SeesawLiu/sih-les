CREATE TABLE [dbo].[LE_OrderPlanSnapshot] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [Item]             VARCHAR (50)    NULL,
    [ManufactureParty] VARCHAR (50)    NULL,
    [Location]         VARCHAR (50)    NULL,
    [ReqTime]          DATETIME        NULL,
    [OrderNo]          VARCHAR (50)    NULL,
    [IRType]           TINYINT         NULL,
    [OrderType]        TINYINT         NULL,
    [OrderQty]         DECIMAL (18, 8) NULL,
    [FinishQty]        DECIMAL (18, 8) NULL
);

