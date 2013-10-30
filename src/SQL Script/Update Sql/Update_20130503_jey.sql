/**取消原拣货单相关权限**/

delete from acc_permission where code = 'Url_PickListDetail_View';
delete from acc_permission where code = 'Url_PickList_Cancel';
delete from acc_permission where code = 'Url_PickList_New';
delete from acc_permission where code = 'Url_PickList_Ship';
delete from acc_permission where code = 'Url_PickList_Start';
delete from acc_permission where code = 'Url_PickList_View';