CREATE TABLE [dbo].[FIS_YieldReturn] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [IpNo]             VARCHAR (50)    NOT NULL,
    [ArriveTime]       DATETIME        NOT NULL,
    [PartyFrom]        VARCHAR (50)    NOT NULL,
    [PartyTo]          VARCHAR (50)    NOT NULL,
    [Dock]             VARCHAR (100)   NULL,
    [IpCreateDate]     DATETIME        NOT NULL,
    [Seq]              VARCHAR (50)    NOT NULL,
    [Item]             VARCHAR (50)    NOT NULL,
    [ManufactureParty] VARCHAR (50)    NULL,
    [Qty]              DECIMAL (18, 8) NOT NULL,
    [IsConsignment]    BIT             NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [CreateDATDate]    DATETIME        NULL,
    [DATFileName]      VARCHAR (255)   NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);

