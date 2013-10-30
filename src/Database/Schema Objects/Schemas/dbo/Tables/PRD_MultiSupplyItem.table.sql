CREATE TABLE [dbo].[PRD_MultiSupplyItem] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [GroupNo]          VARCHAR (50)  NOT NULL,
    [Supplier]         VARCHAR (50)  NOT NULL,
    [Item]             VARCHAR (50)  NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [ItemDesc]         VARCHAR (100) NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [SubstituteGroup]  VARCHAR (50)  NULL
);

