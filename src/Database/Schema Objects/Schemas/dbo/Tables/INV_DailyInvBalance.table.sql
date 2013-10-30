CREATE TABLE [dbo].[INV_DailyInvBalance] (
    [Id]               BIGINT          IDENTITY (1, 1) NOT NULL,
    [Item]             VARCHAR (50)    NOT NULL,
    [Location]         VARCHAR (50)    NULL,
    [SAPLocation]      VARCHAR (50)    NOT NULL,
    [ManufactureParty] VARCHAR (50)    NULL,
    [LotNo]            VARCHAR (50)    NULL,
    [IsCs]             BIT             NOT NULL,
    [QualifyQty]       DECIMAL (18, 8) NOT NULL,
    [InspectQty]       DECIMAL (18, 8) NOT NULL,
    [RejectQty]        DECIMAL (18, 8) NOT NULL,
    [TobeQualifyQty]   DECIMAL (18, 8) NOT NULL,
    [TobeInspectQty]   DECIMAL (18, 8) NOT NULL,
    [TobeRejectQty]    DECIMAL (18, 8) NOT NULL,
    [FinanceYear]      INT             NOT NULL,
    [FinanceMonth]     INT             NOT NULL,
    [InvDate]          DATETIME        NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    PRIMARY KEY NONCLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY]
);

