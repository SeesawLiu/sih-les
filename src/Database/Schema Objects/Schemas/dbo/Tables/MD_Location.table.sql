CREATE TABLE [dbo].[MD_Location] (
    [Code]             VARCHAR (50)  NOT NULL,
    [Name]             VARCHAR (50)  NOT NULL,
    [Region]           VARCHAR (50)  NOT NULL,
    [IsActive]         BIT           NOT NULL,
    [AllowNegaInv]     BIT           NOT NULL,
    [EnableAdvWM]      BIT           NOT NULL,
    [IsCS]             BIT           NOT NULL,
    [IsMRP]            BIT           NOT NULL,
    [IsInvFreeze]      BIT           NOT NULL,
    [SAPLocation]      VARCHAR (50)  NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [PartSuffix]       VARCHAR (10)  NULL
);

