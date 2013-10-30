CREATE TABLE [dbo].[MD_ItemKit] (
    [Id]               INT             IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [KitItem]          VARCHAR (50)    NOT NULL,
    [ChildItem]        VARCHAR (50)    NOT NULL,
    [Qty]              DECIMAL (18, 8) NOT NULL,
    [IsActive]         BIT             NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL
);

