CREATE PROCEDURE [dbo].[USP_Busi_GetHuHistoryInv]
(
	@Location varchar(50),
	@Item varchar(4000),
	@HistoryDate datetime
)
AS
BEGIN
	
	select * from View_LocationLotDet
END
