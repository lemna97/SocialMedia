-- 添加刷新令牌相关字段
ALTER TABLE users 
ADD COLUMN refresh_token VARCHAR(500) NULL COMMENT '刷新令牌',
ADD COLUMN refresh_token_expiry DATETIME NULL COMMENT '刷新令牌过期时间',
ADD COLUMN last_activity_time DATETIME NULL COMMENT '最后活动时间';

-- 创建索引提高查询性能
CREATE INDEX idx_users_refresh_token ON users(refresh_token);
CREATE INDEX idx_users_last_activity ON users(last_activity_time);