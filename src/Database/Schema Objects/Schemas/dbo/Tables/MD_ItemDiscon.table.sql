CREATE TABLE [dbo].[MD_ItemDiscon] (
    [Id]               INT             IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Item]             VARCHAR (50)    NOT NULL,
    [DisconItem]       VARCHAR (50)    NOT NULL,
    [Bom]              VARCHAR (50)    NULL,
    [UnitQty]          DECIMAL (18, 8) NOT NULL,
    [Priority]         INT             NOT NULL,
    [StartDate]        DATETIME        NOT NULL,
    [EndDate]          DATETIME        NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL
);

