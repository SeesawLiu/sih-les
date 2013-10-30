SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Search_VanOrderSeqCount') 
     DROP PROCEDURE USP_Search_VanOrderSeqCount 
GO 

CREATE PROCEDURE [dbo].[USP_Search_VanOrderSeqCount]
(	
	@Flow varchar(50),
	@TraceCode varchar(50)
)
AS
BEGIN
/*
exec USP_Search_VanOrderSeqCount 'RAA00',''  
*/

	DECLARE @Statement nvarchar(4000) 
	DECLARE @Parameter nvarchar(4000) 
	DECLARE @PagePara nvarchar(4000) 
	DECLARE @SortDesc nvarchar(100) 
	DECLARE @Where nvarchar(4000) 
	DECLARE @PermissionClause nvarchar(1000) 
	SET @Where='WHERE 1=1 AND (om.Status IN (0,1,2) or om.Status is null)  AND (om.ProdLineType in(1,2,3,4,9) or om.ProdLineType is null)'
	--SET @Where='WHERE 1=1 AND om.Status IN (0,1,2)  AND om.ProdLineType in(1,2,3,4,9)'

	DECLARE @Seq bigint
	SELECT @Seq=os.Seq FROM ORD_OrderSeq os where os.TraceCode=@TraceCode AND os.ProdLine=@Flow 
	if(@Seq is null)
	begin
	SELECT @Seq=max(os.Seq) FROM ORD_OrderSeq os left join ORD_OrderMstr_4 as m on os.TraceCode=m.TraceCode AND os.ProdLine=m.Flow where os.ProdLine=@Flow  and m.Status in(3,4)
	end
	if(@Seq is not null)
		SET @Where=@Where+' AND os.Seq>=@Seq_1' 
	
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND os.ProdLine=@Flow_1' 
	END 
	
	SET @Statement=N'SELECT COUNT(1) FROM ORD_OrderSeq os LEFT JOIN 
	ORD_OrderMstr_4 om ON om.TraceCode=os.TraceCode AND om.Flow=os.ProdLine AND om.OrderNo=os.OrderNo AND om.Status IN (0,1,2) ' +@Where

	SET @Parameter=N'@Flow_1 varchar(50),@Seq_1 bigint'		 

	EXEC SP_EXECUTESQL @Statement,@Parameter, 
		@Flow_1=@Flow,@Seq_1=@Seq
END

