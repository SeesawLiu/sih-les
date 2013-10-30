CREATE TABLE [dbo].[ORD_ContOrderDet] (
    [Id]           INT             NOT NULL,
    [OrderNo]      VARCHAR (50)    NOT NULL,
    [Container]    VARCHAR (50)    NOT NULL,
    [ContId]       VARCHAR (50)    NULL,
    [Qty]          DECIMAL (18, 8) NOT NULL,
    [CreateUser]   INT             NOT NULL,
    [CreateUserNm] VARCHAR (100)   NOT NULL,
    [CreateDate]   DATETIME        NOT NULL
);

