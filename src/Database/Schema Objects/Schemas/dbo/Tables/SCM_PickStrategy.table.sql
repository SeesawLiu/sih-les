CREATE TABLE [dbo].[SCM_PickStrategy] (
    [Code]             VARCHAR (50)  NOT NULL,
    [IsPickFromBin]    BIT           NOT NULL,
    [OddOption]        TINYINT       NOT NULL,
    [IsOddOccupy]      BIT           NOT NULL,
    [IsDevan]          BIT           NOT NULL,
    [IsFulfillUC]      BIT           NOT NULL,
    [ShipStrategy]     TINYINT       NOT NULL,
    [CreateUser]       INT           NOT NULL,
    [CreateUserNm]     VARCHAR (100) NOT NULL,
    [CreateDate]       DATETIME      NOT NULL,
    [LastModifyUser]   INT           NOT NULL,
    [LastModifyUserNm] VARCHAR (100) NOT NULL,
    [LastModifyDate]   DATETIME      NOT NULL,
    [IsSimple]         BIT           NOT NULL
);

