CREATE TABLE [dbo].[ORD_OrderBindingDet] (
    [Id]               INT           NOT NULL,
    [OrderBindingId]   INT           NOT NULL,
    [OrderNo]          VARCHAR (50)  NOT NULL,
    [BindOrderNo]      VARCHAR (50)  NOT NULL,
    [OrderDetId]       INT           NOT NULL,
    [BindOrderDetId]   INT           NOT NULL,
    [OrderBomDetId]    INT           NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

