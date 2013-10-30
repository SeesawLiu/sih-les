CREATE TABLE [dbo].[LOG_RunLeanEngine] (
    [Id]         BIGINT        IDENTITY (1, 1) NOT NULL,
    [Flow]       VARCHAR (50)  NULL,
    [Item]       VARCHAR (50)  NULL,
    [LocFrom]    VARCHAR (50)  NULL,
    [LocTo]      VARCHAR (50)  NULL,
    [OrderNo]    VARCHAR (50)  NULL,
    [Lvl]        TINYINT       NULL,
    [ErrorId]    TINYINT       NULL,
    [Msg]        VARCHAR (500) NULL,
    [CreateDate] DATETIME      NULL
);

