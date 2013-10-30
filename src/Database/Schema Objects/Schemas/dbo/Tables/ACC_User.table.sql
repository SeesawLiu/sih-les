CREATE TABLE [dbo].[ACC_User] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Code]             VARCHAR (50)  NOT NULL,
    [Password]         VARCHAR (50)  NOT NULL,
    [FirstName]        VARCHAR (50)  NOT NULL,
    [LastName]         VARCHAR (50)  NOT NULL,
    [Type]             TINYINT       NOT NULL,
    [Email]            VARCHAR (50)  NULL,
    [TelPhone]         VARCHAR (50)  NULL,
    [MobilePhone]      VARCHAR (50)  NULL,
    [Language]         VARCHAR (50)  NULL,
    [IsActive]         BIT           NOT NULL,
    [AccountExpired]   BIT           NOT NULL,
    [AccountLocked]    BIT           NOT NULL,
    [PasswordExpired]  BIT           NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

