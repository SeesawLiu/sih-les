CREATE TABLE [dbo].[MD_Shipper] (
    [Code]             VARCHAR (50)  NOT NULL,
    [Desc1]            VARCHAR (100) NOT NULL,
    [Location]         VARCHAR (50)  NULL,
    [Address]          VARCHAR (50)  NULL,
    [Contact]          VARCHAR (50)  NULL,
    [Tel]              VARCHAR (50)  NULL,
    [Email]            VARCHAR (50)  NULL,
    [IsActive]         BIT           NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    PRIMARY KEY CLUSTERED ([Code] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);

