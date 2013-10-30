CREATE TABLE [dbo].[SAP_InvLoc] (
    [Id]         BIGINT       IDENTITY (1, 1) NOT NULL,
    [SourceType] INT          NOT NULL,
    [SourceId]   BIGINT       NOT NULL,
    [FRBNR]      VARCHAR (16) NOT NULL,
    [SGTXT]      VARCHAR (50) NOT NULL,
    [CreateUser] VARCHAR (50) NOT NULL,
    [CreateDate] DATETIME     NOT NULL
);

