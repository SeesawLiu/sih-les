CREATE TABLE [dbo].[BIL_PriceListMstr] (
    [Code]             VARCHAR (50)  NOT NULL,
    [Type]             TINYINT       NOT NULL,
    [Party]            VARCHAR (50)  NOT NULL,
    [Currency]         VARCHAR (50)  NOT NULL,
    [IsIncludeTax]     BIT           NOT NULL,
    [Tax]              VARCHAR (50)  NULL,
    [IsActive]         BIT           NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

