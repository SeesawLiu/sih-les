CREATE TABLE [dbo].[ORD_ShipList] (
    [ShipNo]           VARCHAR (50)  NOT NULL,
    [Vehicle]          VARCHAR (50)  NULL,
    [Shipper]          VARCHAR (50)  NULL,
    [Status]           TINYINT       NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [CloseDate]        DATETIME      NULL,
    [CloseUser]        INT           NULL,
    [CloseUserNm]      VARCHAR (100) NULL,
    [CancelDate]       DATETIME      NULL,
    [CancelUser]       INT           NULL,
    [CancelUserNm]     VARCHAR (100) NULL
);

