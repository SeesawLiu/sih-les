CREATE TABLE [dbo].[FIS_OutboundCtrl] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [SystemCode]    VARCHAR (50)  NOT NULL,
    [OutFolder]     VARCHAR (255) NOT NULL,
    [ServiceName]   VARCHAR (255) NOT NULL,
    [ArchiveFolder] VARCHAR (255) NOT NULL,
    [SeqNo]         INT           NOT NULL,
    [TempFolder]    VARCHAR (255) NOT NULL,
    [FileEncoding]  VARCHAR (50)  NULL,
    [FilePrefix]    VARCHAR (50)  NULL,
    [FileSuffix]    VARCHAR (50)  NULL,
    [IsActive]      BIT           NOT NULL,
    [Mark]          INT           NOT NULL,
    [UndefStr1]     VARCHAR (255) NULL,
    [UndefStr2]     VARCHAR (255) NULL,
    [UndefStr3]     VARCHAR (255) NULL,
    [UndefStr4]     VARCHAR (255) NULL,
    [UndefStr5]     VARCHAR (255) NULL
);

