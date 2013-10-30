CREATE TABLE [dbo].[SYS_ErrorLog] (
    [SP_Name]        VARCHAR (50)  NULL,
    [ErrorMessage]   VARCHAR (MAX) NULL,
    [ErrorNumber]    INT           NULL,
    [ErrorSeverity]  INT           NULL,
    [ErrorState]     INT           NULL,
    [ErrorProcedure] VARCHAR (100) NULL,
    [ErrorLine]      INT           NULL
);

