Create PROCEDURE [dbo].[USP_Busi_PauseProductLine]
(
	@ProductLineCode varchar(50),
	@UserId int,
	@UserNm varchar(100)
)
AS
BEGIN
	declare @DateTimeNow as datetime;
	set @DateTimeNow = GETDATE();
	
	--查询该生产线上所有未关闭生产单的Van号
	select TraceCode into #temp1 from ORD_OrderMstr_4 where Flow = @ProductLineCode and Status in (0, 1, 2);
	
	begin tran	
		--生产线暂停
		update SCM_FlowMstr set IsPause = 1, PauseTime = @DateTimeNow, LastModifyDate = @DateTimeNow, LastModifyUser = @UserId, LastModifyUserNm = @UserNm where Code = @ProductLineCode;
		
		--根据Van号暂停采购单（Kit单/排序单）
		update ORD_OrderMstr_1 set IsPLPause = 1, PauseTime = @DateTimeNow, Version = Version + 1, LastModifyDate = GETDATE(), LastModifyUser = @UserId, LastModifyUserNm = @UserNm
		from ORD_OrderMstr_1 as m1 inner join #temp1 as t on m1.TraceCode = t.TraceCode
		
		--根据Van号暂停移库单（Kit单/排序单）
		update ORD_OrderMstr_2 set IsPLPause = 1, PauseTime = @DateTimeNow, Version = Version + 1, LastModifyDate = GETDATE(), LastModifyUser = @UserId, LastModifyUserNm = @UserNm
		from ORD_OrderMstr_2 as m1 inner join #temp1 as t on m1.TraceCode = t.TraceCode
		
		--根据Van号暂停生产单（驾驶室/底盘）
		update ORD_OrderMstr_4 set IsPLPause = 1, PauseTime = @DateTimeNow, Version = Version + 1, LastModifyDate = GETDATE(), LastModifyUser = @UserId, LastModifyUserNm = @UserNm
		from ORD_OrderMstr_4 as m1 inner join #temp1 as t on m1.TraceCode = t.TraceCode
		
		--根据Van号暂停计划协议单（Kit单/排序单）
		update ORD_OrderMstr_8 set IsPLPause = 1, PauseTime = @DateTimeNow, Version = Version + 1, LastModifyDate = GETDATE(), LastModifyUser = @UserId, LastModifyUserNm = @UserNm
		from ORD_OrderMstr_8 as m1 inner join #temp1 as t on m1.TraceCode = t.TraceCode
	commit

END
