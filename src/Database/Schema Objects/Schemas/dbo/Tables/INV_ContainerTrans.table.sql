CREATE TABLE [dbo].[INV_ContainerTrans] (
    [Id]               INT             IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Container]        VARCHAR (50)    NOT NULL,
    [ContainerDesc]    VARCHAR (100)   NULL,
    [ContId]           VARCHAR (50)    NOT NULL,
    [RefOrderNo]       VARCHAR (50)    NULL,
    [LocationFrom]     VARCHAR (50)    NOT NULL,
    [LocationFromDesc] VARCHAR (100)   NULL,
    [LocationTo]       VARCHAR (50)    NOT NULL,
    [LocationToDesc]   VARCHAR (100)   NULL,
    [Qty]              DECIMAL (18, 8) NOT NULL,
    [IsEmpty]          BIT             NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL
);

