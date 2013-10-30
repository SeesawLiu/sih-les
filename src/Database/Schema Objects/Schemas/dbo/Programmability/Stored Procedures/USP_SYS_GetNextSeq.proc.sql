CREATE PROCEDURE [dbo].[USP_SYS_GetNextSeq]
	@CodePrefix varchar(50),
	@NextSequence int OUTPUT
AS
Begin Tran
	Declare @invValue int;
	select  @invValue = IntValue FROM SYS_NumCtrl WITH (UPDLOCK, ROWLOCK) where Code = @CodePrefix;
	if @invValue is null
	begin
		if @NextSequence is not null
		begin 
			insert into SYS_NumCtrl(Code, IntValue) values (@CodePrefix, @NextSequence + 1);
		end	
		else
		begin
			set @NextSequence = 1;
			insert into SYS_NumCtrl(Code, IntValue) values (@CodePrefix, 2);
		end
	end 
	else
	begin
		if @NextSequence is not null
		begin 
			if @invValue <= @NextSequence
			begin
				update SYS_NumCtrl set IntValue = @NextSequence + 1 where Code = @CodePrefix;
			end
		end
		else
		begin
			set @NextSequence = @invValue;
			update SYS_NumCtrl set IntValue = @NextSequence + 1 where Code = @CodePrefix;
		end
	end	
Commit tran
