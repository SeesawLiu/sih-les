CREATE TABLE [dbo].[CUST_ProdLineLocationMap] (
    [Id]          INT          IDENTITY (1, 1) NOT NULL,
    [ProdLine]    VARCHAR (50) NOT NULL,
    [SapLocation] VARCHAR (50) NOT NULL,
    [Location]    VARCHAR (50) NOT NULL
);

