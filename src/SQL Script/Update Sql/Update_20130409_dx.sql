while exists(select Code from ACC_Permission group by Code having COUNT(1) > 1)
begin
	delete from ACC_PermissionGroupPermission where PermissionId in (select MAX(Id) from ACC_Permission group by Code having COUNT(1) > 1)
	delete from ACC_RolePermission where PermissionId in (select MAX(Id) from ACC_Permission group by Code having COUNT(1) > 1)
	delete from ACC_UserPermission where PermissionId in (select MAX(Id) from ACC_Permission group by Code having COUNT(1) > 1)
	delete from ACC_Permission where Id in (select MAX(Id) from ACC_Permission group by Code having COUNT(1) > 1)
end
go

CREATE UNIQUE NONCLUSTERED INDEX [IX_Permission] ON [dbo].[ACC_Permission] 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

while exists(select Code from SYS_CodeDet group by Code, Value having COUNT(1) > 1)
begin
	delete from SYS_CodeDet where Id in (select MAX(Id) from SYS_CodeDet group by Code, Value having COUNT(1) > 1)
end
go

CREATE UNIQUE NONCLUSTERED INDEX [IX_CodeDet] ON [dbo].[SYS_CodeDet] 
(
	[Code] ASC,
	[Value] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

alter table PRD_RoutingMstr add VirtualOpRef varchar(50)
go

CREATE TABLE [dbo].[CUST_WMSKitOrder](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Flow] [varchar](50) NULL,
	[SapProdLine] [varchar](50) NULL,
	[SapOrderNo] [varchar](50) NULL,
	[Van] [varchar](50) NULL,
	[Item] [varchar](50) NULL,
	[RefItemCode] [varchar](50) NULL,
	[ItemDesc] [varchar](100) NULL,
	[UOM] [varchar](5) NULL,
	[ManufactureParty] [varchar](50) NULL,
	[OpRef] [varchar](50) NULL,
	[OrderQty] [decimal](18, 8) NULL,
	[ReserveNo] [varchar](50) NULL,
	[ReserveLine] [varchar](50) NULL,
	[ZOPWZ] [varchar](50) NULL,
	[ZOPID] [varchar](50) NULL,
	[ZOPDS] [varchar](50) NULL,
	[AUFNR] [varchar](50) NULL,
	[ICHARG] [varchar](50) NULL,
	[BWART] [varchar](50) NULL,
	[WorkCenter] [varchar](50) NULL,
	[IsClose] [bit] NULL,
	[CreateUser] [int] NULL,
	[CreateUserNm] [varchar](100) NULL,
	[CreateDate] [datetime] NULL,
	[LastModifyUser] [int] NULL,
	[LastModifyUserNm] [varchar](100) NULL,
	[LastModifyDate] [datetime] NULL,
	[Version] [int] NULL,
 CONSTRAINT [PK_CUST_WMSKitOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO