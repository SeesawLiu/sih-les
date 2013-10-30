CREATE TABLE [dbo].[SYS_SNRuleExt] (
    [Id]        INT          IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Code]      INT          NOT NULL,
    [Field]     VARCHAR (20) NOT NULL,
    [FieldSeq]  INT          NOT NULL,
    [IsChoosed] BIT          NOT NULL
);

