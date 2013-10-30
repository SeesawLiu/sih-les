CREATE TABLE [dbo].[INV_StockTakeDet] (
    [Id]               INT             IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [StNo]             VARCHAR (50)    NOT NULL,
    [Item]             VARCHAR (50)    NOT NULL,
    [ItemDesc]         VARCHAR (100)   NOT NULL,
    [QualityType]      TINYINT         NOT NULL,
    [Uom]              VARCHAR (5)     NOT NULL,
    [BaseUom]          VARCHAR (5)     NOT NULL,
    [UnitQty]          DECIMAL (18, 8) NOT NULL,
    [HuId]             VARCHAR (50)    NULL,
    [LotNo]            VARCHAR (50)    NULL,
    [Qty]              DECIMAL (18, 8) NOT NULL,
    [Location]         VARCHAR (50)    NOT NULL,
    [Bin]              VARCHAR (50)    NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL
);

