-- 创建用户Token会话表 - 支持多设备登录
CREATE TABLE IF NOT EXISTS `user_token_sessions` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '主键ID',
  `user_id` bigint(20) NOT NULL COMMENT '用户ID',
  `refresh_token` varchar(500) NOT NULL COMMENT '刷新令牌',
  `refresh_token_expiry` datetime NOT NULL COMMENT '刷新令牌过期时间',
  `device_id` varchar(200) DEFAULT NULL COMMENT '设备标识（浏览器指纹、设备ID等）',
  `device_info` varchar(500) DEFAULT NULL COMMENT '设备信息（User-Agent等）',
  `ip_address` varchar(50) DEFAULT NULL COMMENT 'IP地址',
  `last_activity_time` datetime NOT NULL COMMENT '最后活动时间',
  `is_active` tinyint(1) NOT NULL DEFAULT 1 COMMENT '是否活跃',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_refresh_token` (`refresh_token`),
  KEY `idx_user_id` (`user_id`),
  KEY `idx_user_id_active` (`user_id`, `is_active`),
  KEY `idx_refresh_token_expiry` (`refresh_token_expiry`),
  KEY `idx_last_activity_time` (`last_activity_time`),
  KEY `idx_device_id` (`device_id`),
  KEY `idx_ip_address` (`ip_address`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='用户Token会话表 - 支持多设备登录';

-- 添加外键约束（如果需要）
-- ALTER TABLE `user_token_sessions` 
-- ADD CONSTRAINT `fk_user_token_sessions_user_id` 
-- FOREIGN KEY (`user_id`) REFERENCES `system_users` (`id`) ON DELETE CASCADE;