CREATE TABLE [dbo].[SYS_Menu] (
    [Code]     VARCHAR (50)  NOT NULL,
    [Name]     VARCHAR (50)  NOT NULL,
    [Parent]   VARCHAR (50)  NULL,
    [Seq]      INT           NOT NULL,
    [Desc1]    VARCHAR (50)  NOT NULL,
    [PageUrl]  VARCHAR (256) NULL,
    [ImageUrl] VARCHAR (256) NULL,
    [IsActive] BIT           NOT NULL
);

