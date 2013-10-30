CREATE TABLE [dbo].[SYS_CodeDet] (
    [Id]        INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Code]      VARCHAR (50)  NOT NULL,
    [Value]     VARCHAR (100) NOT NULL,
    [Desc1]     VARCHAR (100) NOT NULL,
    [IsDefault] BIT           NOT NULL,
    [Seq]       INT           NOT NULL
);

