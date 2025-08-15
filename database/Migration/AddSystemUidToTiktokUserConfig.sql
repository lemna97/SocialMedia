-- 为 tiktok_user_config 表添加 system_uid 字段
ALTER TABLE `tiktok_user_config` 
ADD COLUMN `system_uid` BIGINT NOT NULL COMMENT '系统用户ID(数据权限关联)' AFTER `sec_uid`;

-- 创建索引提高查询性能
CREATE INDEX `idx_tiktok_user_config_system_uid` ON `tiktok_user_config`(`system_uid`);

-- 创建复合索引（系统用户ID + 是否启用）
CREATE INDEX `idx_tiktok_user_config_system_uid_active` ON `tiktok_user_config`(`system_uid`, `is_active`);
