-- 清理 Hangfire 所有表数据
-- 注意：执行前请确保没有正在运行的任务

-- 禁用外键约束检查
SET FOREIGN_KEY_CHECKS = 0;

-- 清理所有 Hangfire 表
TRUNCATE TABLE task_aggregatedcounter;
TRUNCATE TABLE task_counter;
TRUNCATE TABLE task_hash;
TRUNCATE TABLE task_job;
TRUNCATE TABLE task_jobparameter;
TRUNCATE TABLE task_jobqueue;
TRUNCATE TABLE task_jobstate;
TRUNCATE TABLE task_list;
TRUNCATE TABLE task_lock;
TRUNCATE TABLE task_server;
TRUNCATE TABLE task_set;
TRUNCATE TABLE task_state;

-- 重新启用外键约束检查
SET FOREIGN_KEY_CHECKS = 1;

-- 重置自增ID
ALTER TABLE task_job AUTO_INCREMENT = 1;
ALTER TABLE task_jobstate AUTO_INCREMENT = 1;
ALTER TABLE task_jobparameter AUTO_INCREMENT = 1;

SELECT 'Hangfire 数据已清理完成' as Result;