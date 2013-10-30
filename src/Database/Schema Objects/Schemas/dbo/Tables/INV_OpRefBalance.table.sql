CREATE TABLE [dbo].[INV_OpRefBalance] (
    [Id]               INT             IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Item]             VARCHAR (50)    NOT NULL,
    [OpRef]            VARCHAR (50)    NOT NULL,
    [Qty]              DECIMAL (18, 8) NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL,
    [Version]          INT             NULL
);

