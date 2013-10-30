CREATE TABLE [dbo].[PRD_RoutingDet] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Routing]          VARCHAR (50)  NOT NULL,
    [Op]               INT           NOT NULL,
    [OpRef]            VARCHAR (50)  NOT NULL,
    [Location]         VARCHAR (50)  NULL,
    [WorkCenter]       VARCHAR (50)  NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [IsReport]         BIT           NOT NULL,
    [BackFlushMethod]  TINYINT       NOT NULL,
    [Capacity]         INT           NULL
);

