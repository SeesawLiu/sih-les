CREATE TABLE [dbo].[INV_DailyTransBalance] (
    [Id]           BIGINT          IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Item]         VARCHAR (50)    NOT NULL,
    [TransType]    INT             NOT NULL,
    [IOType]       TINYINT         NOT NULL,
    [Region]       VARCHAR (50)    NOT NULL,
    [Location]     VARCHAR (50)    NOT NULL,
    [QualifyQty]   DECIMAL (18, 8) NOT NULL,
    [InspectQty]   DECIMAL (18, 8) NOT NULL,
    [RejectQty]    DECIMAL (18, 8) NOT NULL,
    [FinanceYear]  INT             NOT NULL,
    [FinanceMonth] INT             NOT NULL,
    [InvDate]      DATETIME        NOT NULL,
    [CreateUser]   INT             NOT NULL,
    [CreateUserNm] VARCHAR (100)   NOT NULL,
    [CreateDate]   DATETIME        NOT NULL
);

