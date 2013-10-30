CREATE TABLE [dbo].[CUST_VanJob] (
    [JobId]                INT          IDENTITY (1, 1) NOT NULL,
    [FlowCode]             VARCHAR (50) NOT NULL,
    [CreateDate]           DATETIME     NOT NULL,
    [CompleteDate]         DATETIME     NULL,
    [Result_SapVanOrderNo] VARCHAR (50) NULL
);

