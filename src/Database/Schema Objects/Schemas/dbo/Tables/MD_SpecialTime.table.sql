CREATE TABLE [dbo].[MD_SpecialTime] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Region]           VARCHAR (50)  NULL,
    [StartTime]        DATETIME      NOT NULL,
    [EndTime]          DATETIME      NOT NULL,
    [Desc1]            VARCHAR (100) NULL,
    [Type]             TINYINT       NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [ProdLine]         VARCHAR (50)  NULL
);

