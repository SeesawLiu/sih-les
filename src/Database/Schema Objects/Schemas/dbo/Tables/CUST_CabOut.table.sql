CREATE TABLE [dbo].[CUST_CabOut] (
    [OrderNo]          VARCHAR (50)  NOT NULL,
    [ProdLine]         VARCHAR (50)  NOT NULL,
    [CabType]          TINYINT       NOT NULL,
    [CabItem]          VARCHAR (50)  NOT NULL,
    [QulityBarcode]    VARCHAR (50)  NULL,
    [Status]           TINYINT       NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [OutUser]          INT           NULL,
    [OutUserNm]        VARCHAR (100) NULL,
    [OutDate]          DATETIME      NULL,
    [TransferUser]     INT           NULL,
    [TransferUserNm]   VARCHAR (100) NULL,
    [TransferDate]     DATETIME      NULL
);

