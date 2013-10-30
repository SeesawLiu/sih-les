CREATE TABLE [dbo].[CUST_VehicleInFactoryMstr] (
    [OrderNo]          VARCHAR (50)  NOT NULL,
    [VehicleNo]        VARCHAR (50)  NULL,
    [Plant]            VARCHAR (50)  NOT NULL,
    [Status]           TINYINT       NOT NULL,
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

