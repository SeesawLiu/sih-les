
CREATE PROCEDURE [dbo].[USP_Busi_PauseVanOrder]
AS
BEGIN
	--暂停计划暂停的工单，当前工位等于计划暂停工位
	update ORD_OrderMstr_4 set IsPause = 1, IsPlanPause = 0	where CurtOp = PauseSeq and IsPlanPause = 1
END
