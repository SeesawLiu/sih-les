CREATE TABLE [dbo].[FIS_LesINLog] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [Type]        VARCHAR (50)    NULL,
    [MoveType]    VARCHAR (50)    NULL,
    [Sequense]    VARCHAR (50)    NULL,
    [PO]          VARCHAR (50)    NULL,
    [POLine]      VARCHAR (50)    NULL,
    [WMSNo]       VARCHAR (50)    NULL,
    [WMSLine]     VARCHAR (50)    NULL,
    [HandTime]    VARCHAR (50)    NULL,
    [Item]        VARCHAR (50)    NULL,
    [HandResult]  VARCHAR (50)    NULL,
    [ErrorCause]  VARCHAR (500)   NULL,
    [IsCreateDat] BIT             NULL,
    [FileName]    VARCHAR (50)    NULL,
    [ASNNo]       VARCHAR (50)    NULL,
    [ExtNo]       VARCHAR (50)    NULL,
    [Qty]         DECIMAL (18, 8) NULL,
    [QtyMark]     BIT             NULL
);

