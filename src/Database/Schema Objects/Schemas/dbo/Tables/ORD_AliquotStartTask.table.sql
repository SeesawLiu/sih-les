CREATE TABLE [dbo].[ORD_AliquotStartTask] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [Flow]             VARCHAR (50)  NOT NULL,
    [VanNo]            VARCHAR (50)  NULL,
    [IsStart]          BIT           NOT NULL,
    [StartTime]        DATETIME      NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [OrderNo]          VARCHAR (50)  NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF)
);

