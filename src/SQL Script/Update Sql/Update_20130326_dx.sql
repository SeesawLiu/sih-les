if exists (select 1
            from  sysobjects
           where  id = object_id('PRD_MultiSupplyItem')
            and   type = 'U')
   drop table PRD_MultiSupplyItem
go

if exists (select 1
            from  sysobjects
           where  id = object_id('PRD_MultiSupplySupplier')
            and   type = 'U')
   drop table PRD_MultiSupplySupplier
go

if exists (select 1
            from  sysobjects
           where  id = object_id('PRD_MultiSupplyGroup')
            and   type = 'U')
   drop table PRD_MultiSupplyGroup
go

/*==============================================================*/
/* Table: PRD_MultiSupplyGroup                                  */
/*==============================================================*/
create table PRD_MultiSupplyGroup (
   GroupNo              varchar(50)          not null,
   Desc1                varchar(100)         null,
   EffSupplier          varchar(50)          null,
   TargetCycleQty       int                  not null,
   AccumulateQty        decimal(18, 8)       not null,
   CreateUser           int                  not null,
   CreateUserNm         varchar(100)         not null,
   CreateDate           datetime             not null,
   LastModifyUser       int                  not null,
   LastModifyUserNm     varchar(100)         not null,
   LastModifyDate       datetime             not null,
   Version              int                  not null
)
go

alter table PRD_MultiSupplyGroup
   add constraint PK_PRD_MULTISUPPLYGROUP primary key (GroupNo)
go

alter table PRD_MultiSupplyGroup
   add constraint FK_PRD_MULT_REFERENCE_MD_SUPPL foreign key (EffSupplier)
      references MD_Supplier (Code)
go

/*==============================================================*/
/* Table: PRD_MultiSupplySupplier                               */
/*==============================================================*/
create table PRD_MultiSupplySupplier (
   Id                   int                  identity,
   GroupNo              varchar(50)          not null,
   Supplier             varchar(50)          not null,
   SupplierNm           varchar(100)         null,
   Seq                  int                  not null,
   CycleQty             int                  not null,
   SpillQty             decimal(18,8)        not null,
   AccumulateQty        decimal(18,8)        not null,
   IsActive             bit                  not null,
   CreateUser           int                  not null,
   CreateUserNm         varchar(100)         not null,
   CreateDate           datetime             not null,
   LastModifyUser       int                  not null,
   LastModifyUserNm     varchar(100)         not null,
   LastModifyDate       datetime             not null,
   Version              int                  not null
)
go

alter table PRD_MultiSupplySupplier
   add constraint PK_PRD_MULTISUPPLYSUPPLIER primary key (Id)
go

alter table PRD_MultiSupplySupplier
   add constraint FK_PRD_MULT_REFERENCE_PRD_MULT foreign key (GroupNo)
      references PRD_MultiSupplyGroup (GroupNo)
go

alter table PRD_MultiSupplySupplier
   add constraint FK_PRD_MULT_REFERENCE_MD_SUPPL2 foreign key (Supplier)
      references MD_Supplier (Code)
go

/*==============================================================*/
/* Table: PRD_MultiSupplyItem                                   */
/*==============================================================*/
create table PRD_MultiSupplyItem (
   Id                   int                  identity,
   GroupNo              varchar(50)          not null,
   Supplier             varchar(50)          not null,
   Item                 varchar(50)          not null,
   LastModifyDate       datetime             not null,
   ItemDesc             varchar(100)         null,
   CreateUser           int                  not null,
   CreateUserNm         varchar(100)         not null,
   CreateDate           datetime             not null,
   LastModifyUser       int                  not null,
   LastModifyUserNm     varchar(100)         not null
)
go

alter table PRD_MultiSupplyItem
   add constraint PK_PRD_MULTISUPPLYITEM primary key (Id)
go

alter table PRD_MultiSupplyItem
   add constraint FK_PRD_MULT_REFERENCE_PRD_MULT2 foreign key (GroupNo)
      references PRD_MultiSupplyGroup (GroupNo)
go

alter table PRD_MultiSupplyItem
   add constraint FK_PRD_MULT_REFERENCE_MD_SUPPL3 foreign key (Supplier)
      references MD_Supplier (Code)
go

alter table PRD_MultiSupplyItem
   add constraint FK_PRD_MULT_REFERENCE_MD_ITEM foreign key (Item)
      references MD_Item (Code)
go