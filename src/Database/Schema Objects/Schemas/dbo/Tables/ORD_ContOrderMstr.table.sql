CREATE TABLE [dbo].[ORD_ContOrderMstr] (
    [OrderNo]      VARCHAR (50)  NOT NULL,
    [RefOrderNo]   VARCHAR (50)  NULL,
    [LocationFrom] VARCHAR (50)  NOT NULL,
    [LocationTo]   VARCHAR (50)  NOT NULL,
    [IsEmpty]      BIT           NOT NULL,
    [IsPrint]      BIT           NOT NULL,
    [CreateUser]   INT           NOT NULL,
    [CreateUserNm] VARCHAR (100) NOT NULL,
    [CreateDate]   DATETIME      NOT NULL
);

