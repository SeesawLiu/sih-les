CREATE TABLE [dbo].[ISS_IssueTypeToRoleDet] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [IssueTypeTo]      VARCHAR (50)  NOT NULL,
    [RoleId]           INT           NOT NULL,
    [IsEmail]          BIT           NOT NULL,
    [IsSMS]            BIT           NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

