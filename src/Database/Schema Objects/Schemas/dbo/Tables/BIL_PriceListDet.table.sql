CREATE TABLE [dbo].[BIL_PriceListDet] (
    [Id]               INT             IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [PriceList]        VARCHAR (50)    NOT NULL,
    [Item]             VARCHAR (50)    NOT NULL,
    [StartDate]        DATETIME        NOT NULL,
    [Uom]              VARCHAR (5)     NOT NULL,
    [EndDate]          DATETIME        NULL,
    [UnitPrice]        DECIMAL (18, 8) NOT NULL,
    [IsProvEst]        BIT             NOT NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL
);

