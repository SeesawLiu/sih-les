SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

IF EXISTS(SELECT * FROM sys.objects WHERE type='P' AND name='USP_Search_VanOrderSeq') 
     DROP PROCEDURE USP_Search_VanOrderSeq 
GO 
CREATE PROCEDURE [dbo].[USP_Search_VanOrderSeq]
(	
	@Flow varchar(50),
	@TraceCode varchar(50),
	@SortCloumn varchar(50)=null, 
	@SortRule varchar(50)=null, 
	@PageSize int, 
	@Page int 
)
AS
BEGIN
/*

exec USP_Search_VanOrderSeq 'RAC00','0070015662111',' ExternalOrderNo',null,20,1
exec USP_Search_VanOrderSeq 'RAA00',null,' ExternalOrderNo',null,100,1

*/
	DECLARE @Statement nvarchar(4000) 
	DECLARE @Parameter nvarchar(4000) 
	DECLARE @PagePara nvarchar(4000) 
	DECLARE @SortDesc nvarchar(100) 
	DECLARE @Where nvarchar(4000) 
	DECLARE @PermissionClause nvarchar(1000) 
	SET @Where='WHERE 1=1 AND (om.Status IN (0,1,2) or om.Status is null)  AND (om.ProdLineType in(1,2,3,4,9) or om.ProdLineType is null)'
	
	DECLARE @Seq bigint
	SELECT @Seq=os.Seq FROM ORD_OrderSeq os where os.TraceCode=@TraceCode AND os.ProdLine=@Flow 
	if(@Seq is null)
	begin
	SELECT @Seq=max(os.Seq) FROM ORD_OrderSeq os left join ORD_OrderMstr_4 as m on os.TraceCode=m.TraceCode AND os.ProdLine=m.Flow where os.ProdLine=@Flow  and m.Status in(3,4)
	end
	if(@Seq is not null)
	begin
		SET @Where=@Where+' AND os.Seq>=@Seq_1'
	end
	
	IF(ISNULL(@Flow,'')<>'') 
	BEGIN 
		SET @Where=@Where+' AND os.ProdLine=@Flow_1' 
	END 
	
	IF ISNULL(@SortCloumn,'')='' 
	BEGIN 
		SET @SortDesc=' ORDER BY os.Seq ASC,os.SubSeq ASC' 
	END 
	ELSE 
	BEGIN 
		IF(ISNULL(@SortRule,'')='') 
		BEGIN 
			SET @SortRule=' ASC' 
		END 
		IF(CHARINDEX('ExternalOrderNo',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'ExternalOrderNo','ExtOrderNo') 
		END 
		ELSE IF(CHARINDEX('OrderNo',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'OrderNo','os.OrderNo') 
		END 
		IF(CHARINDEX('TraceCode',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'TraceCode','os.TraceCode') 
		END 
		IF(CHARINDEX('Flow',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'Flow','os.ProdLine') 
		END 
		IF(CHARINDEX('Sequence',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'Sequence','Seq') 
		END 
		IF(CHARINDEX('SubSequence',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'SubSequence','SubSeq') 
		END		
		IF(CHARINDEX('OrderStatusDescription',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'OrderStatusDescription','Status') 
		END 		
		IF(CHARINDEX('CurrentOperation',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'CurrentOperation','CurtOp') 
		END 	
		IF(CHARINDEX('PauseStatusDescription',@SortCloumn)>0) 
		BEGIN 
			SET @SortCloumn=REPLACE(@SortCloumn,'PauseStatusDescription','PauseStatus') 
		END 		 
		SET @SortDesc=' ORDER BY '+@SortCloumn+' '+@SortRule 
	END 
	IF @Page>0 
	BEGIN 
		SET @PagePara='WHERE RowId BETWEEN '+cast(((@PageSize*(@Page-1))+1) as varchar(50))+' AND '++cast(@PageSize*(@Page) as varchar(50)) 
	END 
	print @SortCloumn
	SET @Statement=N'SELECT TOP('+CAST(@PageSize AS VARCHAR(10))+') * FROM 
			(SELECT RowId=ROW_NUMBER()OVER('+@SortDesc+'),os.Id,
			 om.Flow,om.OrderNo ,om.TraceCode,om.ExtOrderNo,om.Status, om.PauseStatus, om.CurtOp,os.SubSeq,os.Seq
			FROM ORD_OrderSeq os LEFT JOIN ORD_OrderMstr_4 om
			ON om.TraceCode=os.TraceCode AND om.Flow=os.ProdLine AND om.OrderNo=os.OrderNo AND om.Status IN (0,1,2) '+@Where+') AS T '+@PagePara 
	PRINT @Statement 
	PRINT LEN(@Statement) 
	SET @Parameter=N'@Flow_1 varchar(50),@Seq_1 bigint'		 
	EXEC SP_EXECUTESQL @Statement,@Parameter, 
		@Flow_1=@Flow,@Seq_1=@Seq
END
