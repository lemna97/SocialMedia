-- 插入菜单数据
INSERT INTO `system_menus` (`id`, `parent_id`, `name`, `code`, `url`, `icon`, `sort`, `is_active`, `created_at`) VALUES

-- 一级菜单
(1, 0, '视图统计', 'dashboard', '/dashboard', 'chart-bar', 1, 1, NOW()),
(2, 0, '用户管理', 'user_management', '/user', 'users', 2, 1, NOW()),
(3, 0, '视频管理', 'video_management', '/video', 'video', 3, 1, NOW()),
(4, 0, '系统设置', 'system_settings', '/system', 'cog', 4, 1, NOW()),

-- 视图统计 - 二级菜单
(11, 1, '用户数据', 'dashboard_user_data', '/dashboard/user-data', 'user-chart', 11, 1, NOW()),
(12, 1, '视频数据', 'dashboard_video_data', '/dashboard/video-data', 'video-chart', 12, 1, NOW()),

-- 用户管理 - 二级菜单
(21, 2, '用户信息', 'user_info', '/user/info', 'user-info', 21, 1, NOW()),
(22, 2, '更新记录', 'user_update_log', '/user/update-log', 'history', 22, 1, NOW()),

-- 视频管理 - 二级菜单
(31, 3, '视频信息', 'video_info', '/video/info', 'video-info', 31, 1, NOW()),
(32, 3, '更新记录', 'video_update_log', '/video/update-log', 'history', 32, 1, NOW()),

-- 系统设置 - 二级菜单
(41, 4, 'TK账号配置', 'system_tk_config', '/system/tk-config', 'key', 41, 1, NOW()),
(42, 4, '系统用户', 'system_users', '/system/users', 'user-cog', 42, 1, NOW()),
(43, 4, '角色信息', 'system_roles', '/system/roles', 'user-tag', 43, 1, NOW()),
(44, 4, '菜单设置', 'system_menus', '/system/menus', 'menu', 44, 1, NOW()),
(45, 4, '任务管理', 'system_tasks', '/system/tasks', 'tasks', 45, 1, NOW()),
(46, 4, '系统日志', 'system_logs', '/system/logs', 'file-text', 46, 1, NOW());