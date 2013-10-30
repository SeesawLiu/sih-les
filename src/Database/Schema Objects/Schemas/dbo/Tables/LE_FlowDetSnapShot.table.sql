CREATE TABLE [dbo].[LE_FlowDetSnapShot] (
    [Id]               INT             IDENTITY (1, 1) NOT NULL,
    [Flow]             VARCHAR (50)    NULL,
    [FlowDetId]        INT             NULL,
    [Item]             VARCHAR (50)    NOT NULL,
    [Uom]              VARCHAR (5)     NULL,
    [UC]               DECIMAL (18, 8) NULL,
    [ManufactureParty] VARCHAR (50)    NULL,
    [LocFrom]          VARCHAR (50)    NULL,
    [LocTo]            VARCHAR (50)    NOT NULL,
    [IsRefFlow]        BIT             NULL,
    [SafeStock]        DECIMAL (18, 8) NULL,
    [MaxStock]         DECIMAL (18, 8) NULL,
    [MinLotSize]       DECIMAL (18, 8) NULL,
    [RoundUpOpt]       TINYINT         NULL,
    [Strategy]         TINYINT         NULL
);

