CREATE TABLE [dbo].[CUST_StockTakeLocationLotDet](
	[Id] [int] IDENTITY(1,1) NOT FOR REPLICATION NOT NULL,
	[Location] [varchar](50) NOT NULL,
	[Item] [varchar](50) NOT NULL,
	[ItemDesc] [varchar](100) NOT NULL,
	[Qty] [decimal](18, 8) NOT NULL,
	[QualityType] [tinyint] NOT NULL,
	[CSSupplier] [varchar](50) NULL,
	[IsConsigement] [bit] NOT NULL,
	[RefNo] [varchar](50) NULL,
	[Uom] [varchar](50) NULL,
	[CreateUser] [int] NOT NULL,
	[CreateUserNm] [varchar](100) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastModifyUser] [int] NOT NULL,
	[LastModifyUserNm] [varchar](100) NOT NULL,
	[LastModifyDate] [datetime] NOT NULL,
 CONSTRAINT [PK_CUST_StockTakeLocationLotDet] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
go
insert into SYS_Menu values('Url_StockTakeLocationLotDet_View','盘点库存查询','Url_Inventory_StockTake',30,'盘点库存查询','~/StockTake/BackUpInvIndex','~/Content/Images/Nav/Default.png',1)
go
insert into ACC_Permission values('Url_StockTakeLocationLotDet_View','盘点库存查询','Inventory')
go
alter table INV_StockTakeMstr add RefNo varchar(50) null
go
alter table INV_StockTakeDet add CSSupplier varchar(50) null
go
alter table INV_StockTakeDet add IsConsigement bit 
go
go
alter table INV_StockTakeResult add CSSupplier varchar(50) null
go
alter table INV_StockTakeResult add IsConsigement bit 
go
alter table INV_StockTakeResult add RefItemCode varchar(50) null 
select * from SYS_Menu where Desc1='供应商配额调整量'
go
alter table INV_StockTakeDet add RefItemCode varchar(50) null 
go
alter table CUST_StockTakeLocationLotDet add RefItemCode varchar(50) null
go
