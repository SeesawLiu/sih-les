CREATE TABLE [dbo].[FIS_ItemStandardPack] (
    [Id]            INT             IDENTITY (1, 1) NOT NULL,
    [FlowDetId]     INT             NOT NULL,
    [Item]          VARCHAR (50)    NOT NULL,
    [Pack]          VARCHAR (50)    NULL,
    [UC]            DECIMAL (18, 8) NOT NULL,
    [IOType]        VARCHAR (50)    NOT NULL,
    [Location]      CHAR (4)        NOT NULL,
    [Plant]         CHAR (4)        NOT NULL,
    [CreateDate]    DATETIME        NOT NULL,
    [CreateDATDate] DATETIME        NULL,
    [DATFileName]   VARCHAR (255)   NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);

