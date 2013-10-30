CREATE TABLE [dbo].[SAP_Supplier] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [Code]            VARCHAR (20)  NULL,
    [OldSupplierCode] VARCHAR (20)  NULL,
    [Name]            NVARCHAR (60) NULL,
    [IOStatus]        INT           NOT NULL,
    [InboundDate]     DATETIME      NULL,
    [OutBoundDate]    DATETIME      NULL
);

