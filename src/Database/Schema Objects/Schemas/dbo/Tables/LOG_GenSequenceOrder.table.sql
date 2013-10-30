CREATE TABLE [dbo].[LOG_GenSequenceOrder] (
    [Id]         BIGINT        IDENTITY (1, 1) NOT NULL,
    [SeqGroup]   VARCHAR (50)  NULL,
    [Lvl]        TINYINT       NULL,
    [Msg]        VARCHAR (500) NULL,
    [CreateDate] DATETIME      DEFAULT (getdate()) NULL
);

