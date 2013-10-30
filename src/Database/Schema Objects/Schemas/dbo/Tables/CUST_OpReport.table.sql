CREATE TABLE [dbo].[CUST_OpReport] (
    [OrderNo]          VARCHAR (50)  NOT NULL,
    [Op]               INT           NOT NULL,
    [WorkCenter]       VARCHAR (50)  NOT NULL,
    [ExtOrderNo]       VARCHAR (50)  NULL,
    [VAN]              VARCHAR (50)  NULL,
    [IsReport]         BIT           NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

