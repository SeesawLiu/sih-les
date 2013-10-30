CREATE TABLE [dbo].[SAP_Item] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [Code]          VARCHAR (20)  NULL,
    [ReferenceCode] VARCHAR (20)  NULL,
    [Description]   NVARCHAR (60) NULL,
    [Uom]           VARCHAR (5)   NULL,
    [Plant]         VARCHAR (4)   NULL,
    [IOStatus]      INT           NOT NULL,
    [InboundDate]   DATETIME      NOT NULL,
    [OutboundDate]  DATETIME      NULL,
    [ShortCode]     VARCHAR (10)  NULL
);

