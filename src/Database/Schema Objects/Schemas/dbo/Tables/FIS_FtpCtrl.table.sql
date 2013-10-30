CREATE TABLE [dbo].[FIS_FtpCtrl] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [FtpServer]        VARCHAR (255) NOT NULL,
    [FtpPort]          INT           NULL,
    [FtpUser]          VARCHAR (255) NOT NULL,
    [FtpPass]          VARCHAR (255) NOT NULL,
    [FtpTempFolder]    VARCHAR (255) NULL,
    [FtpFolder]        VARCHAR (255) NULL,
    [FilePattern]      VARCHAR (255) NULL,
    [LocalTempFolder]  VARCHAR (255) NULL,
    [LocalFolder]      VARCHAR (255) NULL,
    [IOType]           VARCHAR (50)  NULL,
    [VaildFilePattern] VARCHAR (255) NULL,
    [FtpErrorFolder]   VARCHAR (255) NULL,
    [FtpBackUp]        VARCHAR (255) NULL
);

