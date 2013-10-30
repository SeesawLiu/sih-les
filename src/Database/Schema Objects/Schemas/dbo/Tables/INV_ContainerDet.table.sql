CREATE TABLE [dbo].[INV_ContainerDet] (
    [ContId]           VARCHAR (50)  NOT NULL,
    [Container]        VARCHAR (50)  NOT NULL,
    [Location]         VARCHAR (50)  NOT NULL,
    [IsEmpty]          BIT           NOT NULL,
    [ActiveDate]       DATETIME      NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [Version]          INT           NOT NULL
);

