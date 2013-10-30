CREATE TABLE [dbo].[SCM_ProdLineFact] (
    [Code]             VARCHAR (50)  NOT NULL,
    [ProdLine]         VARCHAR (50)  NOT NULL,
    [IsActive]         BIT           NOT NULL,
    [LocFrom]          VARCHAR (50)  NULL,
    [LocTo]            VARCHAR (50)  NULL,
    [InspLoc]          VARCHAR (50)  NULL,
    [RejLoc]           VARCHAR (50)  NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

