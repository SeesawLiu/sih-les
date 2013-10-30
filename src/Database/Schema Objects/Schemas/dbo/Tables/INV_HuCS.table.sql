CREATE TABLE [dbo].[INV_HuCS] (
    [Id]               NUMERIC (18)    IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [HuId]             VARCHAR (50)    NOT NULL,
    [PlannBillId]      INT             NOT NULL,
    [PlanQty]          DECIMAL (18, 8) NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL
);

