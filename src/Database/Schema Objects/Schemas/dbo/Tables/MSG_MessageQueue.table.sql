CREATE TABLE [dbo].[MSG_MessageQueue] (
    [Id]             INT          IDENTITY (1, 1) NOT NULL,
    [MethodName]     VARCHAR (50) NOT NULL,
    [ParamValue]     VARCHAR (50) NOT NULL,
    [CreateTime]     DATETIME     NULL,
    [Status]         TINYINT      NOT NULL,
    [LastModifyDate] DATETIME     NOT NULL,
    [ErrorCount]     INT          NOT NULL
);

