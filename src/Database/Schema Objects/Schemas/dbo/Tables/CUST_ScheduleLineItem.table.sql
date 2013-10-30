CREATE TABLE [dbo].[CUST_ScheduleLineItem] (
    [Id]               INT           IDENTITY (1, 1) NOT NULL,
    [Supplier]         VARCHAR (50)  NOT NULL,
    [Item]             VARCHAR (50)  NOT NULL,
    [IsClose]          BIT           NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NULL,
    [LastModifyUserNm] VARCHAR (100) NULL,
    [LastModifyDate]   DATETIME      NULL
);

