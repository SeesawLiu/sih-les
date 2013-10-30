CREATE PROCEDURE [dbo].[USP_Split_LocTransDet_INSERT]
(
	@LocTransId bigint,
	@OrderNo varchar(50),
	@OrderType tinyint,
	@OrderSubType tinyint,
	@OrderDetSeq int,
	@OrderDetId int,
	@OrderBomDetId int,
	@OrderBomDetSeq int,
	@IpNo varchar(50),
	@IpDetId int,
	@IpDetSeq int,
	@RecNo varchar(50),
	@RecDetId int,
	@RecDetSeq int,
	@SeqNo varchar(50),
	@BillTransId int,
	@LocLotDetId int,
	@TraceCode varchar(50),
	@Item varchar(50),
	@Qty decimal(18,8),
	@IsCS bit,
	@PlanBill int,
	@PlanBillQty decimal(18,8),
	@ActBill int,
	@ActBillQty decimal(18,8),
	@QualityType tinyint,
	@HuId varchar(50),
	@LotNo varchar(50),
	@TransType int,
	@IOType tinyint,
	@PartyFrom varchar(50),
	@PartyTo varchar(50),
	@LocFrom varchar(50),
	@LocTo varchar(50),
	@Bin varchar(50),
	@PlanBackflushId int,
	@LocIOReason varchar(50),
	@EffDate datetime,
	@CreateUser int,
	@CreateDate datetime
)
AS
BEGIN
	DECLARE @Id bigint
	BEGIN TRAN
		IF EXISTS (SELECT * FROM SYS_TabIdSeq WITH (UPDLOCK,SERIALIZABLE) WHERE TabNm='INV_LocTransDet')
		BEGIN
			SELECT @Id=Id+1 FROM SYS_TabIdSeq WHERE TabNm='INV_LocTransDet'
			UPDATE SYS_TabIdSeq SET Id=Id+1 WHERE TabNm='INV_LocTransDet'
		END
		ELSE
		BEGIN
			INSERT INTO SYS_TabIdSeq(TabNm,Id)
			VALUES('INV_LocTransDet',1)
			SET @Id=1
		END
	COMMIT TRAN
	
	IF @TransType='301'
	BEGIN
		INSERT INTO INV_LocTransDet_1(Id,LocTransId,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,OrderBomDetId,OrderBomDetSeq,IpNo,IpDetId,IpDetSeq,RecNo,RecDetId,RecDetSeq,SeqNo,BillTransId,LocLotDetId,TraceCode,Item,Qty,IsCS,PlanBill,PlanBillQty,ActBill,ActBillQty,QualityType,HuId,LotNo,TransType,IOType,PartyFrom,PartyTo,LocFrom,LocTo,Bin,PlanBackflushId,LocIOReason,EffDate,CreateUser,CreateDate)
		VALUES(@Id,@LocTransId,@OrderNo,@OrderType,@OrderSubType,@OrderDetSeq,@OrderDetId,@OrderBomDetId,@OrderBomDetSeq,@IpNo,@IpDetId,@IpDetSeq,@RecNo,@RecDetId,@RecDetSeq,@SeqNo,@BillTransId,@LocLotDetId,@TraceCode,@Item,@Qty,@IsCS,@PlanBill,@PlanBillQty,@ActBill,@ActBillQty,@QualityType,@HuId,@LotNo,@TransType,@IOType,@PartyFrom,@PartyTo,@LocFrom,@LocTo,@Bin,@PlanBackflushId,@LocIOReason,@EffDate,@CreateUser,@CreateDate)
	END
	ELSE
	BEGIN
		INSERT INTO INV_LocTransDet_0(Id,LocTransId,OrderNo,OrderType,OrderSubType,OrderDetSeq,OrderDetId,OrderBomDetId,OrderBomDetSeq,IpNo,IpDetId,IpDetSeq,RecNo,RecDetId,RecDetSeq,SeqNo,BillTransId,LocLotDetId,TraceCode,Item,Qty,IsCS,PlanBill,PlanBillQty,ActBill,ActBillQty,QualityType,HuId,LotNo,TransType,IOType,PartyFrom,PartyTo,LocFrom,LocTo,Bin,PlanBackflushId,LocIOReason,EffDate,CreateUser,CreateDate)
		VALUES(@Id,@LocTransId,@OrderNo,@OrderType,@OrderSubType,@OrderDetSeq,@OrderDetId,@OrderBomDetId,@OrderBomDetSeq,@IpNo,@IpDetId,@IpDetSeq,@RecNo,@RecDetId,@RecDetSeq,@SeqNo,@BillTransId,@LocLotDetId,@TraceCode,@Item,@Qty,@IsCS,@PlanBill,@PlanBillQty,@ActBill,@ActBillQty,@QualityType,@HuId,@LotNo,@TransType,@IOType,@PartyFrom,@PartyTo,@LocFrom,@LocTo,@Bin,@PlanBackflushId,@LocIOReason,@EffDate,@CreateUser,@CreateDate)
	END
	SELECT @Id
END
