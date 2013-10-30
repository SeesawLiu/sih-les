CREATE TABLE [dbo].[MD_Container] (
    [Code]             VARCHAR (50)  NOT NULL,
    [Desc1]            VARCHAR (100) NOT NULL,
    [IsActive]         BIT           NOT NULL,
    [InvType]          TINYINT       NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

