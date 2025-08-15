-- =====================================================
-- 将 BaseEntity 继承表的 ID 字段从 BIGINT 改为 INT
-- 执行前请备份数据库！
-- =====================================================

-- 开始事务
START TRANSACTION;

-- 1. 修改 system_menus 表
-- 检查是否有超出 INT 范围的 ID
SELECT COUNT(*) as count_over_int_limit FROM system_menus WHERE id > 2147483647;

-- 如果上述查询返回 0，则可以安全执行以下语句
ALTER TABLE `system_menus` 
MODIFY COLUMN `id` INT NOT NULL AUTO_INCREMENT COMMENT '主键ID（自增ID）';

-- 2. 修改 system_roles 表
SELECT COUNT(*) as count_over_int_limit FROM system_roles WHERE id > 2147483647;

ALTER TABLE `system_roles` 
MODIFY COLUMN `id` INT NOT NULL AUTO_INCREMENT COMMENT '主键ID（自增ID）';

-- 3. 修改 system_users 表
SELECT COUNT(*) as count_over_int_limit FROM system_users WHERE id > 2147483647;

ALTER TABLE `system_users` 
MODIFY COLUMN `id` INT NOT NULL AUTO_INCREMENT COMMENT '主键ID（自增ID）';

-- 4. 修改 system_permissions 表
SELECT COUNT(*) as count_over_int_limit FROM system_permissions WHERE id > 2147483647;

ALTER TABLE `system_permissions` 
MODIFY COLUMN `id` INT NOT NULL AUTO_INCREMENT COMMENT '主键ID（自增ID）';

-- 5. 修改相关外键字段
-- system_permissions 表的 resource_id 字段（如果 system_resources 表也需要修改）
-- 注意：需要先检查 system_resources 表是否存在以及其结构

-- 检查 system_resources 表是否存在
SELECT COUNT(*) FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'system_resources';

-- 如果 system_resources 表存在且也继承 BaseEntity，则修改它
-- ALTER TABLE `system_resources` 
-- MODIFY COLUMN `id` INT NOT NULL AUTO_INCREMENT COMMENT '主键ID（自增ID）';

-- 6. 修改用户角色关联表（如果存在）
-- 检查是否存在用户角色关联表
SELECT COUNT(*) FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'system_user_roles';

-- 如果存在，修改外键字段
-- ALTER TABLE `system_user_roles` 
-- MODIFY COLUMN `user_id` INT NOT NULL COMMENT '用户ID',
-- MODIFY COLUMN `role_id` INT NOT NULL COMMENT '角色ID';

-- 7. 修改角色权限关联表（如果存在）
SELECT COUNT(*) FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'system_role_permissions';

-- 如果存在，修改外键字段
-- ALTER TABLE `system_role_permissions` 
-- MODIFY COLUMN `role_id` INT NOT NULL COMMENT '角色ID',
-- MODIFY COLUMN `permission_id` INT NOT NULL COMMENT '权限ID';

-- 8. 修改菜单权限关联表（如果存在）
SELECT COUNT(*) FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'system_menu_permissions';

-- 如果存在，修改外键字段
-- ALTER TABLE `system_menu_permissions` 
-- MODIFY COLUMN `menu_id` INT NOT NULL COMMENT '菜单ID',
-- MODIFY COLUMN `permission_id` INT NOT NULL COMMENT '权限ID';

-- 9. 修改其他可能的关联字段
-- 检查 account_config 表的 system_uid 字段
SELECT COUNT(*) FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'account_config';

-- 修改 account_config 表的 system_uid 字段
ALTER TABLE `account_config` 
MODIFY COLUMN `system_uid` INT NOT NULL COMMENT '系统用户ID(数据权限关联)';

-- 更新相关索引
DROP INDEX IF EXISTS `idx_account_config_system_uid` ON `account_config`;
DROP INDEX IF EXISTS `idx_account_config_system_uid_active` ON `account_config`;

CREATE INDEX `idx_account_config_system_uid` ON `account_config`(`system_uid`);
CREATE INDEX `idx_account_config_system_uid_active` ON `account_config`(`system_uid`, `is_active`);

-- 提交事务
COMMIT;

-- 验证修改结果
SELECT 
    table_name,
    column_name,
    data_type,
    is_nullable,
    extra
FROM information_schema.columns 
WHERE table_schema = DATABASE() 
    AND table_name IN ('system_menus', 'system_roles', 'system_users', 'system_permissions', 'account_config')
    AND column_name IN ('id', 'system_uid')
ORDER BY table_name, column_name;