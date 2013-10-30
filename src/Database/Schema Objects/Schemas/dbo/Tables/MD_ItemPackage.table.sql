CREATE TABLE [dbo].[MD_ItemPackage] (
    [Id]               INT             IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Item]             VARCHAR (50)    NOT NULL,
    [UC]               DECIMAL (18, 8) NOT NULL,
    [Desc1]            VARCHAR (100)   NOT NULL,
    [IsDefault]        BIT             NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL
);

