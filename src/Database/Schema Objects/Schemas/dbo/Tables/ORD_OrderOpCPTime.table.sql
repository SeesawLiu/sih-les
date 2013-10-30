CREATE TABLE [dbo].[ORD_OrderOpCPTime] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [OrderNo]     VARCHAR (50) NULL,
    [VanProdLine] VARCHAR (50) NULL,
    [AssProdLine] VARCHAR (50) NULL,
    [Seq]         BIGINT       NULL,
    [SubSeq]      INT          NULL,
    [Op]          INT          NULL,
    [OpTaktTime]  INT          NULL,
    [CPTime]      DATETIME     NULL,
    [CreateDate]  DATETIME     NOT NULL,
    [VanOp]       INT          NULL,
    [AssOp]       INT          NULL
);

