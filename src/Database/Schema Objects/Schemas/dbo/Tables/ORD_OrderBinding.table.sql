CREATE TABLE [dbo].[ORD_OrderBinding] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [OrderNo]          VARCHAR (50)  NOT NULL,
    [BindFlow]         VARCHAR (50)  NOT NULL,
    [BindFlowStrategy] TINYINT       NOT NULL,
    [BindOrderNo]      VARCHAR (50)  NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [Version]          INT           NOT NULL,
    [BindType]         TINYINT       NOT NULL
);

