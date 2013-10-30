CREATE TABLE [dbo].[PRD_MultiSupplySupplier] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [GroupNo]          VARCHAR (50)    NOT NULL,
    [Supplier]         VARCHAR (50)    NOT NULL,
    [SupplierNm]       VARCHAR (100)   NULL,
    [Seq]              INT             NOT NULL,
    [CycleQty]         INT             NOT NULL,
    [SpillQty]         DECIMAL (18, 8) NOT NULL,
    [AccumulateQty]    DECIMAL (18, 8) NOT NULL,
    [IsActive]         BIT             NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL,
    [Version]          INT             NOT NULL,
    [Proportion]       VARCHAR (100)   NULL
);

