insert into FIS_FtpCtrl values
('10.86.128.128',21,'infuser','infuser','INTERFACE/TNT/FILEOUT/BarCode/OutTemp','INTERFACE/TNT/FILEOUT/BarCode','*.DAT','C:\\DAT\\BarCode\\OutTemp','C:\\DAT\\BarCode','OUT',null,null,null)
go

insert into FIS_OutboundCtrl values
('PPC005','C:\DAT\BarCode','com.Sconit.Service.FIS.Impl.CreateBarCodeDATMgrImpl','C:\DAT\BarCodeFolder',4,'C:\Temp','UTF-8',null,null,1,0,null,null,null,null,null)
go