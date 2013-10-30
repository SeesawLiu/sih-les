CREATE TABLE [dbo].[INV_HuMapping] (
    [Id]               INT              IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [HuId]             VARCHAR (50)     NOT NULL,
    [OldHus]           VARCHAR (500)    NOT NULL,
    [Item]             VARCHAR (50)     NOT NULL,
    [Qty]              DECIMAL (18, 10) NOT NULL,
    [IsEffective]      BIT              NOT NULL,
    [IsRepack]         BIT              NOT NULL,
    [OrderNo]          VARCHAR (50)     NOT NULL,
    [OrderDetId]       BIGINT           NOT NULL,
    [CreateUser]       INT              NOT NULL,
    [CreateUserNm]     VARCHAR (100)    NOT NULL,
    [CreateDate]       DATETIME         NOT NULL,
    [LastModifyUser]   INT              NOT NULL,
    [LastModifyUserNm] VARCHAR (100)    NOT NULL,
    [LastModifyDate]   DATETIME         NOT NULL
);

