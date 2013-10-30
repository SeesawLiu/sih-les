CREATE TABLE [dbo].[ORD_OrderItemTrace] (
    [Id]               INT             IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [OrderNo]          VARCHAR (50)    NOT NULL,
    [OrderBomId]       INT             NOT NULL,
    [Item]             VARCHAR (50)    NOT NULL,
    [ItemDesc]         VARCHAR (100)   NULL,
    [RefItemCode]      VARCHAR (50)    NULL,
    [Op]               INT             NOT NULL,
    [OpRef]            VARCHAR (50)    NOT NULL,
    [Qty]              DECIMAL (18, 8) NOT NULL,
    [ScanQty]          DECIMAL (18, 8) NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL,
    [Version]          INT             NOT NULL
);

