CREATE TABLE [dbo].[CUST_DefectCode] (
    [Code]                VARCHAR (50)  NOT NULL,
    [Desc1]               VARCHAR (50)  NULL,
    [ProductCode]         VARCHAR (50)  NULL,
    [Assemblies]          VARCHAR (50)  NULL,
    [ComponentDefectCode] VARCHAR (50)  NULL,
    [CreateUser]          INT           NOT NULL,
    [CreateUserNm]        VARCHAR (100) NOT NULL,
    [CreateDate]          DATETIME      NOT NULL,
    [LastModifyUser]      INT           NOT NULL,
    [LastModifyUserNm]    VARCHAR (100) NOT NULL,
    [LastModifyDate]      DATETIME      NOT NULL
);

