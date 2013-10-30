IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'VIEW_VanOrderSeq'))
	DROP VIEW VIEW_VanOrderSeq
GO

	Create VIEW [dbo].[VIEW_VanOrderSeq]
AS 
select  ROW_NUMBER() OVER (ORDER BY result.Seq ASC,result.SubSeq ASC) AS RowId ,* from (
select os.Id,
os.ProdLine as Flow,om.OrderNo ,om.TraceCode,om.ExtOrderNo,om.Status, om.PauseStatus, om.CurtOp,os.SubSeq,os.Seq
FROM ORD_OrderSeq os LEFT JOIN ORD_OrderMstr_4 om
ON om.TraceCode=os.TraceCode AND om.Flow=os.ProdLine AND om.OrderNo=os.OrderNo 
 WHERE (om.Status IN (0,1,2) AND om.ProdLineType in(1,2,3,4,9)) or om.Status is null) result 
 where seq>=isnull((select max(seq) from ( 
 select s.Seq,s.ProdLine from ORD_OrderSeq as s left join ORD_OrderMstr_4 as mstr ON mstr.TraceCode=s.TraceCode AND mstr.Flow=s.ProdLine AND mstr.OrderNo=s.OrderNo where mstr.Status in (3,4)  and  mstr.ProdLineType in(1,2,3,4,9) ) as t where t.ProdLine=result.Flow
 ),0) or result.Status  IN (0,1,2)