
create table LOG_ProdOrderPauseResume(
	Id			    BigInt IDENTITY(1,1) primary key,
	ProdLine	    Varchar(50) not null,
	ProdLineDesc    Varchar(50) not null,
	OrderNo         varchar(50) not null,
	BeforeOrderNo   varchar(50)  null,
	VanCode         varchar(50) not null,
	CurrentOperation  varchar(50)  null,
	OprateType       tinyint not null,
	CreateDate       datetime default(getdate())   not null,
	CreateUserName   varchar(50) not null
)
go
insert into SYS_CodeMstr values('OrderPauseType','生产单暂停恢复选项',0)
go
insert into SYS_CodeDet values('OrderPauseType',0,'CodeDetail_OrderPauseType_NormalPause',1,1)
go
insert into SYS_CodeDet values('OrderPauseType',1,'CodeDetail_OrderPauseType_OperationPause',0,2)
go
insert into SYS_CodeDet values('OrderPauseType',2,'CodeDetail_OrderPauseType_MovePause',0,3)
go
insert into SYS_CodeDet values('OrderPauseType',3,'CodeDetail_OrderPauseType_Resume',0,4)
go
insert into SYS_Menu values('Url_ProdOrderPauseResume_View','生产单暂停恢复日志','Url_OrderMstr_Production',218,'生产单暂停恢复日志','~/ProdOrderPauseResume/Index','~/Content/Images/Nav/Default.png',1)
go
insert into ACC_Permission values('Url_ProdOrderPauseResume_View','生产单暂停恢复日志','Production')