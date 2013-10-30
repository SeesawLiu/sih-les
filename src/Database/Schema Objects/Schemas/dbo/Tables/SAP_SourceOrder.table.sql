CREATE TABLE [dbo].[SAP_SourceOrder] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [Code]           VARCHAR (50)    NULL,
    [Sequence]       VARCHAR (50)    NULL,
    [Item]           VARCHAR (50)    NULL,
    [Plant]          VARCHAR (50)    NULL,
    [ZEORD]          VARCHAR (50)    NULL,
    [Supplier]       VARCHAR (50)    NULL,
    [PlantFrom]      VARCHAR (50)    NULL,
    [StartDate]      DATETIME        NULL,
    [EndDate]        DATETIME        NULL,
    [Status]         TINYINT         NOT NULL,
    [CreateDate]     DATETIME        NOT NULL,
    [LastModifyDate] DATETIME        NOT NULL,
    [ErrorCount]     INT             NOT NULL,
    [Uom]            VARCHAR (50)    NULL,
    [BaseUom]        VARCHAR (50)    NULL,
    [UomQty]         DECIMAL (18, 8) NULL,
    [BaseUomQty]     DECIMAL (18, 8) NULL
);

