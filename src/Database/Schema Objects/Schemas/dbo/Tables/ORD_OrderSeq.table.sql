CREATE TABLE [dbo].[ORD_OrderSeq] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [ProdLine]         VARCHAR (50)  NOT NULL,
    [TraceCode]        VARCHAR (50)  NULL,
    [Seq]              BIGINT        NOT NULL,
    [SapSeq]           BIGINT        NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [Version]          INT           NOT NULL,
    [OrderNo]          VARCHAR (50)  NULL,
    [SubSeq]           INT           NOT NULL
);

