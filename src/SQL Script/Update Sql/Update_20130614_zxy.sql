
IF EXISTS(select * from sysobjects where name ='VIEW_VanOrderSeq')
	drop view VIEW_VanOrderSeq
go
CREATE VIEW [dbo].[VIEW_VanOrderSeq]
AS
SELECT ROW_NUMBER() OVER (ORDER BY os.Seq ASC,os.SubSeq ASC) AS RowId, os.Id,
			 om.Flow,om.OrderNo ,om.TraceCode,om.ExtOrderNo,om.Status, om.PauseStatus, om.CurtOp,os.SubSeq,os.Seq
			 FROM ORD_OrderSeq os LEFT JOIN ORD_OrderMstr_4 om
			ON om.TraceCode=os.TraceCode AND om.Flow=os.ProdLine AND om.OrderNo=os.OrderNo AND om.Status IN (2) 
GO


insert into sys_menu values
('Url_DeferredFeedCounter_Cancel','空车上线取消','Url_OrderMstr_Production',231,'空车上线取消','~/DeferredFeedCounter/CancelIndex','~/Content/Images/Nav/Default.png',1)
go

insert into acc_permission values('Url_DeferredFeedCounter_Cancel','空车上线取消','Production')
go
