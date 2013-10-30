CREATE TABLE [dbo].[INP_ProdScanDet] (
    [Id]               INT           IDENTITY (1, 1) NOT FOR REPLICATION NOT NULL,
    [ProdScan]         VARCHAR (50)  NOT NULL,
    [Seq]              INT           NOT NULL,
    [Op]               INT           NOT NULL,
    [OpRef]            VARCHAR (50)  NOT NULL,
    [Item]             VARCHAR (50)  NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL
);

