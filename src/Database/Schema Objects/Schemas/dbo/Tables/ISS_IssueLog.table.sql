CREATE TABLE [dbo].[ISS_IssueLog] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Issue]            VARCHAR (50)  NULL,
    [IssueDet]         INT           NULL,
    [Level_]           VARCHAR (50)  NULL,
    [Content_]         VARCHAR (255) NULL,
    [UserId]           VARCHAR (50)  NULL,
    [EmailStatus]      VARCHAR (50)  NULL,
    [SMSStatus]        VARCHAR (50)  NULL,
    [Email]            VARCHAR (50)  NULL,
    [MPhone]           VARCHAR (50)  NULL,
    [IsEmail]          BIT           NULL,
    [IsSMS]            BIT           NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

