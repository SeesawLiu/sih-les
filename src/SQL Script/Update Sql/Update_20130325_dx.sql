if exists (select 1
            from  sysobjects
           where  id = object_id('SAP_ProdOrder')
            and   type = 'U')
   drop table SAP_ProdOrder
go

/*==============================================================*/
/* Table: SAP_ProdOrder                                         */
/*==============================================================*/
create table SAP_ProdOrder (
   Id                   int                  identity,
   BatchNo              int                  not null,
   CreateDate           datetime             not null default GETDATE(),
   AUFNR                varchar(12)          null,
   WERKS                varchar(4)           null,
   DAUAT                varchar(4)           null,
   MATNR                varchar(18)          null,
   MAKTX                varchar(40)          null,
   DISPO                varchar(3)           null,
   CHARG                varchar(10)          null,
   GSTRS                datetime             null,
   CY_SEQNR             bigint               null,
   GMEIN                varchar(3)           null,
   GAMNG                decimal(13)          null,
   LGORT                varchar(4)           null,
   LTEXT                varchar(40)          null,
   ZLINE                varchar(1)           null,
   RSNUM                int                  null,
   AUFPL                int                  null
)
go

alter table SAP_ProdOrder
   add constraint PK_SAP_PRODORDER primary key (Id)
go

if exists (select 1
            from  sysobjects
           where  id = object_id('SAP_ProdBomDet')
            and   type = 'U')
   drop table SAP_ProdBomDet
go

/*==============================================================*/
/* Table: SAP_ProdBomDet                                        */
/*==============================================================*/
create table SAP_ProdBomDet (
   Id                   bigint               identity,
   BatchNo              int                  not null,
   CreateDate           datetime             not null default GETDATE(),
   AUFNR                varchar(12)          null,
   RSNUM                int                  null,
   RSPOS                int                  null,
   WEAKS                varchar(4)           null,
   MATERIAL             varchar(18)          null,
   BISMT                varchar(18)          null,
   MAKTX                varchar(40)          null,
   DISPO                varchar(3)           null,
   BESKZ                varchar(1)           null,
   SOBSL                varchar(2)           null,
   MEINS                varchar(3)           null,
   MDMNG                decimal(13)          null,
   LGORT                varchar(4)           null,
   BWART                varchar(3)           null,
   AUFPL                int                  null,
   PLNFL                varchar(6)           null,
   VORNR                varchar(4)           null,
   GW                   varchar(6)           null,
   WZ                   varchar(8)           null,
   ZOPID                varchar(20)          null,
   ZOPDS                varchar(40)          null,
   LIFNR                varchar(10)          null,
   ICHARG               varchar(10)          null
)
go

alter table SAP_ProdBomDet
   add constraint PK_SAP_PRODBOMDET primary key (Id)
go

if exists (select 1
            from  sysobjects
           where  id = object_id('SAP_ProdRoutingDet')
            and   type = 'U')
   drop table SAP_ProdRoutingDet
go

/*==============================================================*/
/* Table: SAP_ProdRoutingDet                                    */
/*==============================================================*/
create table SAP_ProdRoutingDet (
   Id                   int                  identity,
   BatchNo              int                  not null,
   CreateDate           datetime             not null default GETDATE(),
   AUFNR                varchar(12)          null,
   WERKS                varchar(4)           null,
   AUFPL                int                  null,
   APLZL                int                  null,
   PLNTY                varchar(6)           null,
   PLNNR                varchar(3)           null,
   PLNAL                varchar(8)           null,
   PLNFL                varchar(6)           null,
   VORNR                varchar(3)           null,
   ARBPL                varchar(8)           null,
   RUEK                 varchar(1)           null,
   AUTWE                varchar(1)           null
)
go

alter table SAP_ProdRoutingDet
   add constraint PK_SAP_PRODROUTINGDET primary key (Id)
go

alter table SCM_FlowMstr add ProdLineType tinyint
go
update SCM_FlowMstr set ProdLineType = 0
go
alter table SCM_FlowMstr alter column ProdLineType tinyint not null
go

if exists (select 1
   from sys.sysreferences r join sys.sysobjects o on (o.id = r.constid and o.type = 'F')
   where r.fkeyid = object_id('MD_WorkCenter') and o.name = 'FK_MD_WORKC_REFERENCE_MD_LOCAT')
alter table MD_WorkCenter
   drop constraint FK_MD_WORKC_REFERENCE_MD_LOCAT
go
if exists (select 1
            from  sysobjects
           where  id = object_id('MD_WorkCenter')
            and   type = 'U')
   drop table MD_WorkCenter
go

/*==============================================================*/
/* Table: MD_WorkCenter                                         */
/*==============================================================*/
create table MD_WorkCenter (
   Code                 varchar(50)          not null,
   Location             varchar(50)          null,
   CreateUser           int                  not null,
   CreateUserNm         varchar(100)         not null,
   CreateDate           datetime             not null,
   LastModifyUser       int                  not null,
   LastModifyUserNm     varchar(100)         not null,
   LastModifyDate       datetime             not null
)
go

alter table MD_WorkCenter
   add constraint PK_MD_WORKCENTER primary key (Code)
go

alter table MD_WorkCenter
   add constraint FK_MD_WORKC_REFERENCE_MD_LOCAT foreign key (Location)
      references MD_Location (Code)
go

insert into MD_WorkCenter(Code, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate)
select distinct WorkCenter, 1, '用户超级', GetDate(), 1, '用户超级', GetDate() from PRD_RoutingDet where WorkCenter is not null
go

alter table PRD_RoutingDet
   add constraint FK_PRD_ROUT_REFERENCE_MD_WORKC foreign key (WorkCenter)
      references MD_WorkCenter (Code)
go

insert into MD_WorkCenter(Code, CreateUser, CreateUserNm, CreateDate, LastModifyUser, LastModifyUserNm, LastModifyDate)
select distinct WorkCenter, 1, '用户超级', GetDate(), 1, '用户超级', GetDate() from ORD_OrderOp 
where WorkCenter is not null and WorkCenter not in (select Code from MD_WorkCenter)
go

alter table ORD_OrderOp
   add constraint FK_ORD_ORDE_REFERENCE_MD_WORKC foreign key (WorkCenter)
      references MD_WorkCenter (Code)
go

alter table ORD_OrderMstr add ProdLineType tinyint
alter table ORD_OrderMstr_0 add ProdLineType tinyint
alter table ORD_OrderMstr_1 add ProdLineType tinyint
alter table ORD_OrderMstr_2 add ProdLineType tinyint
alter table ORD_OrderMstr_3 add ProdLineType tinyint
alter table ORD_OrderMstr_4 add ProdLineType tinyint
alter table ORD_OrderMstr_5 add ProdLineType tinyint
alter table ORD_OrderMstr_6 add ProdLineType tinyint
alter table ORD_OrderMstr_7 add ProdLineType tinyint
alter table ORD_OrderMstr_8 add ProdLineType tinyint
go
update ORD_OrderMstr set ProdLineType = 0
update ORD_OrderMstr_0 set ProdLineType = 0
update ORD_OrderMstr_1 set ProdLineType = 0
update ORD_OrderMstr_2 set ProdLineType = 0
update ORD_OrderMstr_3 set ProdLineType = 0
update ORD_OrderMstr_4 set ProdLineType = 0
update ORD_OrderMstr_5 set ProdLineType = 0
update ORD_OrderMstr_6 set ProdLineType = 0
update ORD_OrderMstr_7 set ProdLineType = 0
update ORD_OrderMstr_8 set ProdLineType = 0
go
alter table ORD_OrderMstr alter column ProdLineType tinyint not null
alter table ORD_OrderMstr_0 alter column ProdLineType tinyint not null
alter table ORD_OrderMstr_1 alter column ProdLineType tinyint not null
alter table ORD_OrderMstr_2 alter column ProdLineType tinyint not null
alter table ORD_OrderMstr_3 alter column ProdLineType tinyint not null
alter table ORD_OrderMstr_4 alter column ProdLineType tinyint not null
alter table ORD_OrderMstr_5 alter column ProdLineType tinyint not null
alter table ORD_OrderMstr_6 alter column ProdLineType tinyint not null
alter table ORD_OrderMstr_7 alter column ProdLineType tinyint not null
alter table ORD_OrderMstr_8 alter column ProdLineType tinyint not null
go

if exists (select 1
            from  sysobjects
           where  id = object_id('PRD_ProdLineWorkCenter')
            and   type = 'U')
   drop table PRD_ProdLineWorkCenter
go

/*==============================================================*/
/* Table: PRD_ProdLineWorkCenter                                */
/*==============================================================*/
create table PRD_ProdLineWorkCenter (
   Id                   int                  not null,
   Flow                 varchar(50)          not null,
   WorkCenter           varchar(50)          not null,
   CreateUser           int                  not null,
   CreateUserNm         varchar(100)         not null,
   CreateDate           datetime             not null,
   LastModifyUser       int                  not null,
   LastModifyUserNm     varchar(100)         not null,
   LastModifyDate       datetime             not null
)
go

alter table PRD_ProdLineWorkCenter
   add constraint PK_PRD_PRODLINEWORKCENTER primary key (Id)
go

alter table PRD_ProdLineWorkCenter
   add constraint FK_PRD_PROD_REFERENCE_SCM_FLOW2 foreign key (Flow)
      references SCM_FlowMstr (Code)
go

alter table PRD_ProdLineWorkCenter
   add constraint FK_PRD_PROD_REFERENCE_MD_WORKC foreign key (WorkCenter)
      references MD_WorkCenter (Code)
go

alter table CUST_ProductLineMap add CabProdLine varchar(50)
go
alter table CUST_ProductLineMap add ChassisProdLine varchar(50)
go
alter table CUST_ProductLineMap add AssemblyProdLine varchar(50)
go
alter table CUST_ProductLineMap add SpecialProdLine varchar(50)
go

insert into SYS_CodeMstr(Code, Desc1, Type) values('ProdLineType', '生产线类型', 0)
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('ProdLineType', 0, 'CodeDetail_ProdLineType_Semi', 1, 1)
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('ProdLineType', 1, 'CodeDetail_ProdLineType_Cab', 0, 2)
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('ProdLineType', 2, 'CodeDetail_ProdLineType_Chassis', 0, 3)
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('ProdLineType', 3, 'CodeDetail_ProdLineType_Assembly', 0, 4)
insert into SYS_CodeDet(Code, Value, Desc1, IsDefault, Seq) values('ProdLineType', 4, 'CodeDetail_ProdLineType_Special', 0, 5)
go

if exists (select 1
            from  sysobjects
           where  id = object_id('ORD_OrderSeq')
            and   type = 'U')
   drop table ORD_OrderSeq
go

/*==============================================================*/
/* Table: ORD_OrderSeq                                          */
/*==============================================================*/
create table ORD_OrderSeq (
   Id                   int                  identity,
   ProdLine             varchar(50)          not null,
   TraceCode            varchar(50)          not null,
   Seq                  bigint               not null,
   SapSeq               bigint               not null,
   CreateUser           int                  not null,
   CreateUserNm         varchar(100)         not null,
   CreateDate           datetime             not null,
   LastModifyUser       int                  not null,
   LastModifyUserNm     varchar(100)         not null,
   LastModifyDate       datetime             not null,
   Version              int                  not null
)
go

alter table ORD_OrderSeq
   add constraint PK_ORD_ORDERSEQ primary key (Id)
go
