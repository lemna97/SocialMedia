-- 第一步：添加临时字段
ALTER TABLE tiktok_videos 
ADD COLUMN latitude_temp VARCHAR(50) NULL COMMENT '临时纬度字段',
ADD COLUMN longitude_temp VARCHAR(50) NULL COMMENT '临时经度字段';

-- 第二步：复制数据到临时字段
UPDATE tiktok_videos 
SET latitude_temp = CAST(latitude AS CHAR),
    longitude_temp = CAST(longitude AS CHAR)
WHERE latitude IS NOT NULL OR longitude IS NOT NULL;

-- 第三步：删除原字段
ALTER TABLE tiktok_videos 
DROP COLUMN latitude,
DROP COLUMN longitude;

-- 第四步：重命名临时字段
ALTER TABLE tiktok_videos 
CHANGE COLUMN latitude_temp latitude VARCHAR(50) NULL COMMENT '纬度(地理位置纬度坐标)',
CHANGE COLUMN longitude_temp longitude VARCHAR(50) NULL COMMENT '经度(地理位置经度坐标)';