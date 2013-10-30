IF EXISTS(SELECT * FROM SYS.objects WHERE type='V' AND name='VIEW_UserPermission')
	DROP VIEW VIEW_UserPermission
GO

CREATE VIEW VIEW_UserPermission
AS
select ACC_UserPermission.UserId, ACC_Permission.Code as PermissionCode, ACC_Permission.Category, ACC_PermissionCategory.Type As CategoryType 
from dbo.ACC_UserPermission
inner join dbo.ACC_Permission on ACC_UserPermission.PermissionId = ACC_Permission.Id
inner join dbo.ACC_PermissionCategory on ACC_Permission.Category = ACC_PermissionCategory.Code
union
select ACC_UserPermissionGroup.UserId, ACC_Permission.Code as PermissionCode, ACC_Permission.Category, ACC_PermissionCategory.Type As CategoryType 
from dbo.ACC_UserPermissionGroup 
inner join dbo.ACC_PermissionGroupPermission on ACC_UserPermissionGroup.GroupId = ACC_PermissionGroupPermission.GroupId
inner join dbo.ACC_Permission on ACC_PermissionGroupPermission.PermissionId = ACC_Permission.Id
inner join dbo.ACC_PermissionCategory on ACC_Permission.Category = ACC_PermissionCategory.Code
union
select ACC_UserRole.UserId, ACC_Permission.Code as PermissionCode, ACC_Permission.Category, ACC_PermissionCategory.Type As CategoryType 
from dbo.ACC_UserRole 
inner join dbo.ACC_RolePermission on ACC_UserRole.RoleId = ACC_RolePermission.RoleId
inner join dbo.ACC_Permission on ACC_RolePermission.PermissionId = ACC_Permission.Id
inner join dbo.ACC_PermissionCategory on ACC_Permission.Category = ACC_PermissionCategory.Code
union
select ACC_UserRole.UserId, ACC_Permission.Code as PermissionCode, ACC_Permission.Category, ACC_PermissionCategory.Type As CategoryType 
from dbo.ACC_UserRole 
inner join dbo.ACC_RolePermissionGroup on ACC_UserRole.RoleId = ACC_RolePermissionGroup.RoleId
inner join dbo.ACC_PermissionGroupPermission on ACC_RolePermissionGroup.GroupId = ACC_PermissionGroupPermission.GroupId
inner join dbo.ACC_Permission on ACC_PermissionGroupPermission.PermissionId = ACC_Permission.Id
inner join dbo.ACC_PermissionCategory on ACC_Permission.Category = ACC_PermissionCategory.Code
GO
