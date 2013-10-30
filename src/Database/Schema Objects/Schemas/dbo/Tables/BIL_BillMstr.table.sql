CREATE TABLE [dbo].[BIL_BillMstr] (
    [BillNo]           VARCHAR (50)  NOT NULL,
    [ExtBillNo]        VARCHAR (50)  NULL,
    [RefBillNo]        VARCHAR (50)  NULL,
    [Type]             TINYINT       NOT NULL,
    [SubType]          TINYINT       NOT NULL,
    [Status]           TINYINT       NOT NULL,
    [BillAddr]         VARCHAR (50)  NOT NULL,
    [BillAddrDesc]     VARCHAR (256) NULL,
    [Party]            VARCHAR (50)  NOT NULL,
    [PartyNm]          VARCHAR (100) NULL,
    [Currency]         VARCHAR (50)  NOT NULL,
    [IsIncludeTax]     BIT           NOT NULL,
    [Tax]              VARCHAR (50)  NULL,
    [EffDate]          DATETIME      NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [Version]          INT           NOT NULL
);

