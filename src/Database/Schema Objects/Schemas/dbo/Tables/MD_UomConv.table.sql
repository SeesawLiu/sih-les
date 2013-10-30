CREATE TABLE [dbo].[MD_UomConv] (
    [Id]               INT             IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Item]             VARCHAR (50)    NULL,
    [BaseUom]          VARCHAR (5)     NOT NULL,
    [AltUom]           VARCHAR (5)     NOT NULL,
    [BaseQty]          DECIMAL (18, 8) NOT NULL,
    [AltQty]           DECIMAL (18, 8) NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL
);

