CREATE TABLE [dbo].[PRD_BFStrategy] (
    [Code]        VARCHAR (50)  NOT NULL,
    [Description] VARCHAR (100) NULL,
    [BFMethod]    TINYINT       NOT NULL,
    [FeedMethod]  TINYINT       NOT NULL,
    [IsAutoFeed]  BIT           NOT NULL
);

