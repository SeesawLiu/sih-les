CREATE TABLE [dbo].[SAP_MapMoveTypeTCode] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [BWART]       CHAR (3)      NOT NULL,
    [SOBKZ]       CHAR (1)      NULL,
    [TCode]       VARCHAR (10)  NOT NULL,
    [Description] VARCHAR (100) NULL
);

