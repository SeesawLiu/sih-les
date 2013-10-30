CREATE TABLE [dbo].[INV_ContLocationDet] (
    [Id]               INT             IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Location]         VARCHAR (50)    NOT NULL,
    [Container]        VARCHAR (50)    NOT NULL,
    [ContId]           VARCHAR (50)    NULL,
    [Qty]              DECIMAL (18, 8) NOT NULL,
    [IsEmpty]          BIT             NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL,
    [Version]          INT             NOT NULL
);

