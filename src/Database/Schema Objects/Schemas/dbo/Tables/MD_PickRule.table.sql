CREATE TABLE [dbo].[MD_PickRule] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [Item]             VARCHAR (50)  NOT NULL,
    [ItemDesc]         VARCHAR (100) NULL,
    [Location]         VARCHAR (50)  NOT NULL,
    [Picker]           VARCHAR (50)  NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);

