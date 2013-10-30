CREATE TABLE [dbo].[PRD_MultiSupplyGroup] (
    [GroupNo]          VARCHAR (50)    NOT NULL,
    [Desc1]            VARCHAR (100)   NULL,
    [EffSupplier]      VARCHAR (50)    NULL,
    [TargetCycleQty]   INT             NOT NULL,
    [AccumulateQty]    DECIMAL (18, 8) NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL,
    [Version]          INT             NOT NULL,
    [KBEffSupplier]    VARCHAR (50)    NULL,
    [KBTargetCycleQty] INT             NOT NULL,
    [KBAccumulateQty]  INT             NOT NULL
);

