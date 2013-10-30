CREATE TABLE [dbo].[ORD_PickResult] (
    [ResultId]         VARCHAR (50)    NOT NULL,
    [PickId]           VARCHAR (50)    NOT NULL,
    [PickedHu]         VARCHAR (50)    NULL,
    [HuQty]            DECIMAL (18, 8) NULL,
    [PickedQty]        DECIMAL (18, 8) NULL,
    [Picker]           VARCHAR (50)    NULL,
    [PickDate]         DATETIME        NULL,
    [AsnNo]            VARCHAR (50)    NULL,
    [Memo]             VARCHAR (256)   NULL,
    [CreateUser]       INT             NOT NULL,
    [CreateUserNm]     VARCHAR (100)   NOT NULL,
    [CreateDate]       DATETIME        NOT NULL,
    [LastModifyUser]   INT             NOT NULL,
    [LastModifyUserNm] VARCHAR (100)   NOT NULL,
    [LastModifyDate]   DATETIME        NOT NULL
);

