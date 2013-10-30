CREATE TABLE [dbo].[ORD_OrderItemTraceResult] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [OrderItemTraceId] INT           NULL,
    [BarCode]          VARCHAR (50)  NOT NULL,
    [Supplier]         VARCHAR (50)  NULL,
    [LotNo]            VARCHAR (50)  NULL,
    [OpRef]            VARCHAR (50)  NULL,
    [Item]             VARCHAR (50)  NULL,
    [ItemDesc]         VARCHAR (100) NULL,
    [RefItemCode]      VARCHAR (50)  NULL,
    [BomId]            INT           NULL,
    [OrderNo]          VARCHAR (50)  NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [IsWithdraw]       BIT           NOT NULL
);

