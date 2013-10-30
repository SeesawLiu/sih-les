CREATE TABLE [dbo].[PRD_ShiftDet] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [Shift]            VARCHAR (50)  NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [StartTime]        VARCHAR (5)   NULL,
    [EndTime]          VARCHAR (5)   NULL,
    [IsOvernight]      INT           NOT NULL,
    [Seq]              INT           NOT NULL
);

