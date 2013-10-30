CREATE TABLE [dbo].[CUST_FailCode] (
    [Code]             VARCHAR (50)  NOT NULL,
    [CHNDesc]          VARCHAR (50)  NULL,
    [ENGDesc]          VARCHAR (50)  NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

