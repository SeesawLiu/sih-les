IF EXISTS(select * from sysobjects where name ='VIEW_VanOrderSeq')
	drop view VIEW_VanOrderSeq
go
CREATE VIEW [dbo].[VIEW_VanOrderSeq]
AS
SELECT ROW_NUMBER() OVER (ORDER BY os.Seq ASC,os.SubSeq ASC) AS RowId, os.Id,
			 om.OrderNo ,om.TraceCode,om.ExtOrderNo,om.Status, om.PauseStatus, om.CurtOp,os.SubSeq,os.Seq,os.ProdLine as Flow
			 FROM ORD_OrderSeq os LEFT JOIN ORD_OrderMstr_4 om
			ON om.TraceCode=os.TraceCode AND om.Flow=os.ProdLine AND om.OrderNo=os.OrderNo AND om.Status IN (2) 
GO
