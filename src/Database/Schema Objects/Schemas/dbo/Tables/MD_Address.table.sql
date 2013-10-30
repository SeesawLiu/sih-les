CREATE TABLE [dbo].[MD_Address] (
    [Code]             VARCHAR (50)  NOT NULL,
    [Type]             TINYINT       NOT NULL,
    [Address]          VARCHAR (256) NULL,
    [PostCode]         VARCHAR (50)  NULL,
    [TelPhone]         VARCHAR (50)  NULL,
    [MobilePhone]      VARCHAR (50)  NULL,
    [Fax]              VARCHAR (50)  NULL,
    [Email]            VARCHAR (50)  NULL,
    [ContactPsnNm]     VARCHAR (50)  NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

