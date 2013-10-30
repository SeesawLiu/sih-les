CREATE TABLE [dbo].[PRD_StandardWorkingCalendar] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Region]           VARCHAR (50)  NULL,
    [Shift]            VARCHAR (50)  NOT NULL,
    [DayOfWeek]        TINYINT       NOT NULL,
    [Type]             TINYINT       NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [RegionName]       VARCHAR (50)  NULL,
    [Category]         TINYINT       NOT NULL,
    [ProdLine]         VARCHAR (50)  NULL
);

