SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM SYS.OBJECTS WHERE TYPE='P' AND name='USP_LE_RunLeanEngine')
	DROP PROCEDURE USP_LE_RunLeanEngine	
GO

CREATE PROCEDURE [dbo].[USP_LE_RunLeanEngine]
(
	@CreateUserId int,
	@CreateUserNm varchar(100)
) --WITH ENCRYPTION
AS
BEGIN 
	set nocount on
	declare @BatchNo int
	declare @LERunTime datetime = GetDate()
	declare @Msg nvarchar(Max)
	
	exec USP_SYS_GetNextSeq 'LeanEngineBatchNo', @BatchNo output
	
	--记录日志
	set @Msg = N'精益引擎运行开始'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	-------------------↓快照OrderBomCPTime-----------------------
	exec USP_LE_SnapshotOrderBomCPTime4LeanEngine @BatchNo, @CreateUserId, @CreateUserNm
	-------------------↑快照OrderBomCPTime-----------------------
	
	
	-------------------↓快照配额-----------------------
	exec USP_LE_SnapshotQuota4LeanEngine @BatchNo
	-------------------↑快照配额-----------------------
	
	
	-------------------↓生成排序单-----------------------
	set @Msg = N'生成排序单开始'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	exec USP_LE_GenSequenceOrder @BatchNo, @CreateUserId, @CreateUserNm
	
	set @Msg = N'生成排序单结束'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	-------------------↑生成排序单-----------------------
	

	-------------------↓获取路线明细-----------------------
	set @Msg = N'缓存路线明细开始'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	exec USP_LE_SnapshotFlowDet4LeanEngine @BatchNo, @LERunTime
	
	set @Msg = N'缓存路线明细结束'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	-------------------↑获取路线明细-----------------------
	
	
	-------------------↓计算整车线边物料拉动-----------------------
	set @Msg = N'生成整车线边物料要货单开始'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	exec USP_LE_GenOrder4VanProdLine4SpecifyManufactureParty @BatchNo, @LERunTime, @CreateUserId, @CreateUserNm
	exec USP_LE_GenOrder4VanProdLine @BatchNo, @LERunTime, @CreateUserId, @CreateUserNm
	
	set @Msg = N'生成整车线边物料要货单结束'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	-------------------↑计算整车线边物料拉动-----------------------

	
	-------------------↓获取计划订单-----------------------
	exec USP_LE_SnapshotOrderPlan4LeanEngine @BatchNo
	-------------------↑获取计划订单-----------------------
	
	
	
	-------------------↓获取库存，没有考虑投料-----------------------
	exec USP_LE_SnapshotLocationDet4LeanEngine @BatchNo
	-------------------↑获取库存，没有考虑投料-----------------------
	
	
	
	-------------------↓生成要货单-----------------------
	exec USP_LE_GenOrder @BatchNo, @LERunTime, @CreateUserId, @CreateUserNm
	-------------------↑生成要货单-----------------------
	
	--记录日志
	set @Msg = N'精益引擎运行结束'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	
	
	-------------------↓发送引擎运行错误-----------------------
	set @Msg = ''

	if exists(select top 1 1 from LOG_RunLeanEngine where Lvl = 1 and BatchNo = @BatchNo)
	begin
		set @Msg = @Msg + 'select * from LOG_RunLeanEngine where Lvl = 1 and BatchNo = ' + CONVERT(varchar,  @BatchNo) + CHAR(10) + CHAR(13)
	end
	
	if exists(select top 1 1 from LOG_GenOrder4VanProdLine where Lvl = 1 and BatchNo = @BatchNo)
	begin
		set @Msg = @Msg + 'select * from LOG_GenOrder4VanProdLine where Lvl = 1 and BatchNo = ' + CONVERT(varchar,  @BatchNo) + CHAR(10) + CHAR(13)
	end
	
	if exists(select top 1 1 from LOG_GenSequenceOrder where Lvl = 1 and BatchNo = @BatchNo)
	begin
		set @Msg = @Msg + 'select * from LOG_GenSequenceOrder where Lvl = 1 and BatchNo = ' + CONVERT(varchar,  @BatchNo) + CHAR(10) + CHAR(13)
	end
	
	if exists(select top 1 1 from LOG_GenJITOrder where Lvl = 1 and BatchNo = @BatchNo)
	begin
		set @Msg = @Msg + 'select * from LOG_GenJITOrder where Lvl = 1 and BatchNo = ' + CONVERT(varchar,  @BatchNo) + CHAR(10) + CHAR(13)
	end
	
	if exists(select top 1 1 from LOG_GenKBOrder where Lvl = 1 and BatchNo = @BatchNo)
	begin
		set @Msg = @Msg + 'select * from LOG_GenKBOrder where Lvl = 1 and BatchNo = ' + CONVERT(varchar,  @BatchNo) + CHAR(10) + CHAR(13)
	end
	
	if @Msg <> ''
	begin
		declare @EmailRecipients varchar(max)
		select @EmailRecipients = Emails from SYS_MsgSubscirber where Id = 12105
		set @Msg = '请用下列SQL查询' + CHAR(10) + CHAR(13) + @Msg
		exec msdb..sp_send_dbmail @profile_name =  'mail_profile'               -- profile 名称 
									 ,@recipients   =  @EmailRecipients				-- 收件人邮箱 
									 ,@subject      =  '精益引擎运行异常'			-- 邮件标题 
									 ,@body         =  @Msg							-- 邮件内容 
									 ,@body_format  =  'TEXT'                       -- 邮件格式 TEXT/HTML
	end
	
	-------------------↑生成要货单-----------------------
END 