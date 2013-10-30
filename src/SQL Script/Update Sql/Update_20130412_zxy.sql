
CREATE table ORD_AliquotStartTask
(
	Id int identity(1,1) primary key,
	Flow varchar(50) not null,
	VanNo varchar(50) null,
	IsStart bit not null,
	StartTime datetime null,
	CreateUser int not null,
	CreateUserNm varchar(100) not null,
	CreateDate datetime not null,
	LastModifyUser int not null,
	LastModifyUserNm varchar(100) not null,
	LastModifyDate datetime not null
)

go