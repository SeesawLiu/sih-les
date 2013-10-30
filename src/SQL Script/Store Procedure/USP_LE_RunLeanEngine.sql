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
	
	--��¼��־
	set @Msg = N'�����������п�ʼ'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	-------------------������OrderBomCPTime-----------------------
	exec USP_LE_SnapshotOrderBomCPTime4LeanEngine @BatchNo, @CreateUserId, @CreateUserNm
	-------------------������OrderBomCPTime-----------------------
	
	
	-------------------���������-----------------------
	exec USP_LE_SnapshotQuota4LeanEngine @BatchNo
	-------------------���������-----------------------
	
	
	-------------------����������-----------------------
	set @Msg = N'�������򵥿�ʼ'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	exec USP_LE_GenSequenceOrder @BatchNo, @CreateUserId, @CreateUserNm
	
	set @Msg = N'�������򵥽���'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	-------------------����������-----------------------
	

	-------------------����ȡ·����ϸ-----------------------
	set @Msg = N'����·����ϸ��ʼ'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	exec USP_LE_SnapshotFlowDet4LeanEngine @BatchNo, @LERunTime
	
	set @Msg = N'����·����ϸ����'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	-------------------����ȡ·����ϸ-----------------------
	
	
	-------------------�����������߱���������-----------------------
	set @Msg = N'���������߱�����Ҫ������ʼ'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	
	exec USP_LE_GenOrder4VanProdLine4SpecifyManufactureParty @BatchNo, @LERunTime, @CreateUserId, @CreateUserNm
	exec USP_LE_GenOrder4VanProdLine @BatchNo, @LERunTime, @CreateUserId, @CreateUserNm
	
	set @Msg = N'���������߱�����Ҫ��������'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
	-------------------�����������߱���������-----------------------

	
	-------------------����ȡ�ƻ�����-----------------------
	exec USP_LE_SnapshotOrderPlan4LeanEngine @BatchNo
	-------------------����ȡ�ƻ�����-----------------------
	
	
	
	-------------------����ȡ��棬û�п���Ͷ��-----------------------
	exec USP_LE_SnapshotLocationDet4LeanEngine @BatchNo
	-------------------����ȡ��棬û�п���Ͷ��-----------------------
	
	
	
	-------------------������Ҫ����-----------------------
	exec USP_LE_GenOrder @BatchNo, @LERunTime, @CreateUserId, @CreateUserNm
	-------------------������Ҫ����-----------------------
	
	--��¼��־
	set @Msg = N'�����������н���'
	insert into LOG_RunLeanEngine(Lvl, Msg, BatchNo) values(0, @Msg, @BatchNo)
END 