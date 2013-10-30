CREATE TABLE [dbo].[SCM_SeqGroup] (
    [Code]              VARCHAR (50)  NOT NULL,
    [ProdLine]          VARCHAR (50)  NOT NULL,
    [SeqBatch]          INT           NOT NULL,
    [PrevOrderNo]       VARCHAR (50)  NULL,
    [PrevTraceCode]     VARCHAR (50)  NULL,
    [PrevSeq]           BIGINT        NULL,
    [PrevSubSeq]        INT           NULL,
    [PrevDeliveryDate]  DATE          NULL,
    [PrevDeliveryCount] INT           NULL,
    [CreateUser]        INT           NOT NULL,
    [CreateUserNm]      VARCHAR (100) NOT NULL,
    [CreateDate]        DATETIME      NOT NULL,
    [LastModifyUser]    INT           NOT NULL,
    [LastModifyUserNm]  VARCHAR (100) NOT NULL,
    [LastModifyDate]    DATETIME      NOT NULL,
    [Version]           INT           NOT NULL,
    [OpRef]             VARCHAR (50)  NOT NULL
);

