CREATE TABLE [dbo].[SYS_SNRule] (
    [Code]        INT           NOT NULL,
    [Desc1]       VARCHAR (100) NOT NULL,
    [PreFixed]    VARCHAR (10)  NULL,
    [YearCode]    VARCHAR (6)   NULL,
    [MonthCode]   VARCHAR (4)   NULL,
    [DayCode]     VARCHAR (4)   NULL,
    [BlockSeq]    VARCHAR (10)  NULL,
    [SeqLength]   TINYINT       NOT NULL,
    [SeqBaseType] VARCHAR (10)  NULL,
    [SeqMin]      INT           NOT NULL
);

