CREATE TABLE [dbo].[ORD_KBOrderBomDet] (
    [Id]               INT          NOT NULL,
    [OrderBomDetId]    INT          NOT NULL,
    [Flow]             VARCHAR (50) NOT NULL,
    [CreateUser]       INT          NOT NULL,
    [CreateUserNm]     VARCHAR (50) NOT NULL,
    [CreateDate]       DATETIME     NOT NULL,
    [LastModifyUser]   INT          NULL,
    [LastModifyUserNm] VARCHAR (50) NULL,
    [LastModifyDate]   DATETIME     NULL
);

