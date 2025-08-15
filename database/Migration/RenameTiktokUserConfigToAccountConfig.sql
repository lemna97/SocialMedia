-- 重命名表：tiktok_user_config -> account_config
RENAME TABLE `tiktok_user_config` TO `account_config`;

-- 更新索引名称（如果需要保持一致性）
DROP INDEX `idx_tiktok_user_config_system_uid` ON `account_config`;
DROP INDEX `idx_tiktok_user_config_system_uid_active` ON `account_config`;

-- 重新创建索引
CREATE INDEX `idx_account_config_system_uid` ON `account_config`(`system_uid`);
CREATE INDEX `idx_account_config_system_uid_active` ON `account_config`(`system_uid`, `is_active`);

-- 添加注释说明表的新用途
ALTER TABLE `account_config` COMMENT = '账户配置表（原TikTok用户配置表）';