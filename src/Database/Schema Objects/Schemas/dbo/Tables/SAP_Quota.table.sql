CREATE TABLE [dbo].[SAP_Quota] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [Code]           VARCHAR (20)    NOT NULL,
    [Sequence]       VARCHAR (20)    NULL,
    [Supplier]       VARCHAR (20)    NULL,
    [Plant]          VARCHAR (4)     NULL,
    [PlantFrom]      VARCHAR (4)     NULL,
    [Item]           VARCHAR (20)    NOT NULL,
    [StartDate]      DATETIME        NULL,
    [EndDate]        DATETIME        NULL,
    [PoType]         CHAR (1)        NULL,
    [SubPoType]      CHAR (1)        NULL,
    [Weight]         DECIMAL (18, 8) NULL,
    [Status]         TINYINT         NULL,
    [CreateDate]     DATETIME        NOT NULL,
    [LastModifyDate] DATETIME        NULL,
    [ErrorCount]     INT             NOT NULL
);

