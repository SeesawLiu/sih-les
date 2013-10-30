 Create table [SCM_FlowShiftDet](
	[Id] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Flow] [varchar](50) NOT NULL,
	[Shift] [varchar](50) NOT NULL,
	[WinTime] [varchar](256) NOT NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL
)
