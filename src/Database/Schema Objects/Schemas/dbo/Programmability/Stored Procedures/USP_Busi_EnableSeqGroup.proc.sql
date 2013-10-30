
CREATE PROCEDURE [dbo].[USP_Busi_EnableSeqGroup]
	@SeqGroup varchar(50),
	@IgnoreError bit,
	@CreateUserId int,
	@CreateUserNm varchar(100)
AS
BEGIN
	set nocount on
	begin try
		begin tran EnableSeqGroup
		save tran EnableSeqGroup_Point
		
		declare @DateTimeNow datetime = GetDate()
		declare @ErrorMsg nvarchar(MAX)
		
		Create table #tempMsg
		(
			[Level] tinyint,
			Msg varchar(500)
		)
		
		select * from SCM_FlowDet as det 
		inner join SCM_FlowStrategy as stra on det.Flow = stra.Flow
		where stra.SeqGroup = @SeqGroup
		
		
		commit tran	EnableSeqGroup
	end try
	begin catch
		rollback tran EnableSeqGroup_Point
		commit tran EnableSeqGroup
		set @ErrorMsg = Error_Message()
		RAISERROR(@ErrorMsg, 16, 1)
	end catch
END
