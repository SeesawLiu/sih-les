update fis_ftpctrl set ftptempfolder = replace(ftptempfolder,'INTERFACE','INTERFACE_TEST'),ftpfolder = replace(ftpfolder,'INTERFACE','INTERFACE_TEST')
update fis_inboundctrl set ftpfolder = replace(ftpfolder,'INTERFACE','INTERFACE_TEST')
update sys_entitypreference set value = replace(value,'les.sih.cq.cn','localhost') where id = 1111
update sys_entitypreference set value = '10.86.128.31' where id = 11005
update sys_entitypreference set value = 'localhost' where id = 11051
update sys_entitypreference set value = '1' where id = 11040

update sys_entitypreference set value = 'dms_wangjun' where id = 11001
update sys_entitypreference set value = '12345678' where id = 11002

update bat_trigger set status = 1 