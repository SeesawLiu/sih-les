CREATE TABLE [dbo].[MD_ItemRef] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Item]             VARCHAR (50)  NOT NULL,
    [Party]            VARCHAR (50)  NULL,
    [RefCode]          VARCHAR (50)  NOT NULL,
    [IsActive]         BIT           NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

