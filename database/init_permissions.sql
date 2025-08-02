-- 首先插入资源数据（如果system_resources表还没有数据）
INSERT INTO `system_resources` (`id`, `name`, `display_name`, `created_at`) VALUES
(1, 'dashboard', '视图统计', NOW()),
(2, 'user_info', '用户信息', NOW()),
(3, 'user_update_log', '用户更新记录', NOW()),
(4, 'video_info', '视频信息', NOW()),
(5, 'video_update_log', '视频更新记录', NOW()),
(6, 'tk_config', 'TK账号配置', NOW()),
(7, 'system_users', '系统用户', NOW()),
(8, 'system_roles', '角色信息', NOW()),
(9, 'system_menus', '菜单设置', NOW()),
(10, 'system_tasks', '任务管理', NOW()),
(11, 'system_logs', '系统日志', NOW());

-- 插入权限数据
INSERT INTO `system_permissions` (`resource_id`, `operation`, `perm_code`, `created_at`) VALUES

-- 视图统计权限
(1, 'Read', 'dashboard:read', NOW()),

-- 用户信息权限
(2, 'Read', 'user_info:read', NOW()),
(2, 'Create', 'user_info:create', NOW()),
(2, 'Update', 'user_info:update', NOW()),
(2, 'Delete', 'user_info:delete', NOW()),

-- 用户更新记录权限
(3, 'Read', 'user_update_log:read', NOW()),

-- 视频信息权限
(4, 'Read', 'video_info:read', NOW()),
(4, 'Create', 'video_info:create', NOW()),
(4, 'Update', 'video_info:update', NOW()),
(4, 'Delete', 'video_info:delete', NOW()),

-- 视频更新记录权限
(5, 'Read', 'video_update_log:read', NOW()),

-- TK账号配置权限
(6, 'Read', 'tk_config:read', NOW()),
(6, 'Create', 'tk_config:create', NOW()),
(6, 'Update', 'tk_config:update', NOW()),
(6, 'Delete', 'tk_config:delete', NOW()),

-- 系统用户权限
(7, 'Read', 'system_users:read', NOW()),
(7, 'Create', 'system_users:create', NOW()),
(7, 'Update', 'system_users:update', NOW()),
(7, 'Delete', 'system_users:delete', NOW()),

-- 角色信息权限
(8, 'Read', 'system_roles:read', NOW()),
(8, 'Create', 'system_roles:create', NOW()),
(8, 'Update', 'system_roles:update', NOW()),
(8, 'Delete', 'system_roles:delete', NOW()),

-- 菜单设置权限
(9, 'Read', 'system_menus:read', NOW()),
(9, 'Create', 'system_menus:create', NOW()),
(9, 'Update', 'system_menus:update', NOW()),
(9, 'Delete', 'system_menus:delete', NOW()),

-- 任务管理权限
(10, 'Read', 'system_tasks:read', NOW()),
(10, 'Create', 'system_tasks:create', NOW()),
(10, 'Update', 'system_tasks:update', NOW()),
(10, 'Delete', 'system_tasks:delete', NOW()),

-- 系统日志权限
(11, 'Read', 'system_logs:read', NOW());