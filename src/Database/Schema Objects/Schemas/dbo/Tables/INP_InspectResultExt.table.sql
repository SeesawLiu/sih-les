CREATE TABLE [dbo].[INP_InspectResultExt] (
    [id]               INT           IDENTITY (1, 1) NOT NULL,
    [inpRetId]         INT           NOT NULL,
    [QuaDesc]          VARCHAR (50)  NULL,
    [QuaType]          VARCHAR (50)  NULL,
    [ProType]          VARCHAR (50)  NULL,
    [Supplier]         VARCHAR (50)  NULL,
    [UnitCode]         VARCHAR (50)  NULL,
    [RatInspect]       VARCHAR (50)  NULL,
    [Checker]          VARCHAR (50)  NULL,
    [PurDep]           VARCHAR (50)  NULL,
    [Picture]          IMAGE         NULL,
    [StartLotNo]       VARCHAR (50)  NULL,
    [EndLotNo]         VARCHAR (50)  NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    PRIMARY KEY NONCLUSTERED ([id] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY]
);

