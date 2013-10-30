CREATE TABLE [dbo].[LE_FlowMstrSnapShot] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [Flow]           VARCHAR (50)  NOT NULL,
    [Type]           TINYINT       NULL,
    [Strategy]       TINYINT       NULL,
    [PartyFrom]      VARCHAR (50)  NULL,
    [PartyTo]        VARCHAR (50)  NULL,
    [LocFrom]        VARCHAR (50)  NULL,
    [LocTo]          VARCHAR (50)  NULL,
    [Dock]           VARCHAR (50)  NULL,
    [ExtraDmdSource] VARCHAR (255) NULL,
    [OrderTime]      DATETIME      NULL,
    [ReqTimeFrom]    DATETIME      NULL,
    [ReqTimeTo]      DATETIME      NULL,
    [WindowTime]     DATETIME      NULL,
    [EMWindowTime]   DATETIME      NULL
);

