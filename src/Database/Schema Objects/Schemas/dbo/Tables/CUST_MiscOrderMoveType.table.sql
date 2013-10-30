CREATE TABLE [dbo].[CUST_MiscOrderMoveType] (
    [Id]                    INT          IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [IOType]                TINYINT      NOT NULL,
    [MoveType]              VARCHAR (5)  NOT NULL,
    [CancelMoveType]        VARCHAR (5)  NOT NULL,
    [Desc1]                 VARCHAR (50) NOT NULL,
    [CheckRecLoc]           BIT          NOT NULL,
    [CheckNote]             BIT          NOT NULL,
    [CheckCostCenter]       BIT          NOT NULL,
    [CheckAsn]              BIT          NOT NULL,
    [CheckReserveNo]        BIT          NOT NULL,
    [CheckReserveLine]      BIT          NOT NULL,
    [CheckRefNo]            BIT          NOT NULL,
    [CheckDeliverRegion]    BIT          NOT NULL,
    [CheckWBS]              BIT          NOT NULL,
    [CheckQualityType]      TINYINT      NULL,
    [CheckEBELN]            BIT          NOT NULL,
    [CheckEBELP]            BIT          NOT NULL,
    [CheckManufactureParty] BIT          NULL,
    [CheckConsignment]      TINYINT      NULL
);

