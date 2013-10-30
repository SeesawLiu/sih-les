CREATE TABLE [dbo].[INP_InspectMstr] (
    [InpNo]            VARCHAR (50)  NOT NULL,
    [RefNo]            VARCHAR (50)  NULL,
    [Region]           VARCHAR (50)  NOT NULL,
    [Status]           TINYINT       NOT NULL,
    [Type]             TINYINT       NOT NULL,
    [IsATP]            BIT           NOT NULL,
    [IsPrint]          BIT           NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [Version]          INT           NOT NULL,
    [ManufactureParty] VARCHAR (50)  NULL,
    [WMSNo]            VARCHAR (50)  NULL,
    [IpNo]             VARCHAR (50)  NULL,
    [RecNo]            VARCHAR (50)  NULL
);

