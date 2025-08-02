CREATE TABLE IF NOT EXISTS `loginfo` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT COMMENT '主键ID',
  `application` varchar(100) NOT NULL COMMENT '应用程序名称',
  `machineName` varchar(100) DEFAULT NULL COMMENT '机器名称',
  `logged` datetime NOT NULL COMMENT '记录时间',
  `level` varchar(10) NOT NULL COMMENT '日志级别',
  `message` text NOT NULL COMMENT '日志消息',
  `logger` varchar(255) DEFAULT NULL COMMENT '日志记录器名称',
  `callsite` varchar(500) DEFAULT NULL COMMENT '调用位置',
  `exception` text DEFAULT NULL COMMENT '异常信息',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  PRIMARY KEY (`id`),
  KEY `idx_application` (`application`),
  KEY `idx_level` (`level`),
  KEY `idx_logged` (`logged`),
  KEY `idx_logger` (`logger`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='NLog日志信息表';