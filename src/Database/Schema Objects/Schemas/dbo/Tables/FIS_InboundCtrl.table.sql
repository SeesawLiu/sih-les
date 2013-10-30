CREATE TABLE [dbo].[FIS_InboundCtrl] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [SystemCode]    VARCHAR (50)  NOT NULL,
    [InFloder]      VARCHAR (255) NOT NULL,
    [FtpFolder]     VARCHAR (255) NOT NULL,
    [FilePattern]   VARCHAR (255) NULL,
    [ServiceName]   VARCHAR (255) NOT NULL,
    [ArchiveFloder] VARCHAR (255) NOT NULL,
    [ErrorFloder]   VARCHAR (255) NOT NULL,
    [SeqNo]         INT           NOT NULL,
    [FileEncoding]  VARCHAR (50)  NULL
);

