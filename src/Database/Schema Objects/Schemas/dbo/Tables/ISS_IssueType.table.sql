CREATE TABLE [dbo].[ISS_IssueType] (
    [Code]                 VARCHAR (50)    NOT NULL,
    [Desc1]                VARCHAR (100)   NOT NULL,
    [InProcessWaitingTime] DECIMAL (18, 8) NULL,
    [CompleteWaitingTime]  DECIMAL (18, 8) NULL,
    [IsActive]             BIT             NOT NULL,
    [CreateUser]           INT             NOT NULL,
    [CreateUserNm]         VARCHAR (100)   NOT NULL,
    [CreateDate]           DATETIME        NOT NULL,
    [LastModifyUser]       INT             NOT NULL,
    [LastModifyUserNm]     VARCHAR (100)   NOT NULL,
    [LastModifyDate]       DATETIME        NOT NULL
);

