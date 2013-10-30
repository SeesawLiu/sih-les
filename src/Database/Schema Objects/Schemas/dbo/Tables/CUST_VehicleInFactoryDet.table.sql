CREATE TABLE [dbo].[CUST_VehicleInFactoryDet] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [OrderNo]          VARCHAR (50)  NOT NULL,
    [IpNo]             VARCHAR (50)  NOT NULL,
    [IsClose]          BIT           NOT NULL,
    [Dock]             VARCHAR (50)  NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [CloseUser]        INT           NULL,
    [CloseUserNm]      VARCHAR (100) NULL,
    [CloseDate]        DATETIME      NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

