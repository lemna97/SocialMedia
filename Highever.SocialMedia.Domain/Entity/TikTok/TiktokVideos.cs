using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// TikTok视频信息表
    /// </summary>
    [SugarTable("tiktok_videos")]
    public class TiktokVideos
    {
        /// <summary>
        /// 视频ID(TikTok视频唯一标识)
        /// </summary>
        [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsNullable = false, Length = 50)]
        public string Id { get; set; }

        /// <summary>
        /// 用户ID(发布视频的用户唯一标识)
        /// </summary>
        [SugarColumn(ColumnName = "user_id", IsNullable = false, Length = 50)]
        public string UserId { get; set; }

        /// <summary>
        /// 用户名(用户自定义的唯一标识符)
        /// </summary>
        [SugarColumn(ColumnName = "unique_id", Length = 100, IsNullable = false)]
        public string UniqueId { get; set; }

        /// <summary>
        /// 用户昵称(显示名称)
        /// </summary>
        [SugarColumn(ColumnName = "nickname", Length = 200, IsNullable = true)]
        public string? Nickname { get; set; }

        /// <summary>
        /// 视频描述(视频标题或描述内容)
        /// </summary>
        [SugarColumn(ColumnName = "desc", IsNullable = true)]
        public string? Desc { get; set; }

        /// <summary>
        /// 视频创建时间戳(Unix时间戳)
        /// </summary>
        [SugarColumn(ColumnName = "create_time", IsNullable = true)]
        public long? CreateTime { get; set; }

        /// <summary>
        /// 视频时长(秒)
        /// </summary>
        [SugarColumn(ColumnName = "duration", IsNullable = true)]
        public int? Duration { get; set; }

        /// <summary>
        /// 视频宽高比(如16:9、9:16等)
        /// </summary>
        [SugarColumn(ColumnName = "ratio", Length = 20, IsNullable = true)]
        public string? Ratio { get; set; }

        /// <summary>
        /// 视频封面图URL(缩略图链接)
        /// </summary>
        [SugarColumn(ColumnName = "cover_url", Length = 500, IsNullable = true)]
        public string? CoverUrl { get; set; }

        /// <summary>
        /// 视频播放URL(视频文件链接)
        /// </summary>
        [SugarColumn(ColumnName = "play_url", Length = 500, IsNullable = true)]
        public string? PlayUrl { get; set; }

        /// <summary>
        /// 音乐ID(背景音乐唯一标识)
        /// </summary>
        [SugarColumn(ColumnName = "music_id", IsNullable = true, Length = 50)]
        public string? MusicId { get; set; }

        /// <summary>
        /// 音乐标题(背景音乐名称)
        /// </summary>
        [SugarColumn(ColumnName = "music_title", Length = 200, IsNullable = true)]
        public string? MusicTitle { get; set; }

        /// <summary>
        /// 音乐作者(背景音乐创作者)
        /// </summary>
        [SugarColumn(ColumnName = "music_author", Length = 100, IsNullable = true)]
        public string? MusicAuthor { get; set; }

        /// <summary>
        /// 是否原声(是否使用原创音频)
        /// </summary>
        [SugarColumn(ColumnName = "is_original_sound", IsNullable = false)]
        public bool IsOriginalSound { get; set; } = false;

        /// <summary>
        /// 话题标签(JSON格式存储的标签数组)
        /// </summary>
        [SugarColumn(ColumnName = "hashtags", IsNullable = true)]
        public string? Hashtags { get; set; }

        /// <summary>
        /// 地点名称(拍摄或发布地点)
        /// </summary>
        [SugarColumn(ColumnName = "poi_name", Length = 200, IsNullable = true)]
        public string? PoiName { get; set; }

        /// <summary>
        /// 详细地址(具体位置描述)
        /// </summary>
        [SugarColumn(ColumnName = "address", Length = 500, IsNullable = true)]
        public string? Address { get; set; }

        /// <summary>
        /// 纬度(地理位置纬度坐标)
        /// </summary>
        [SugarColumn(ColumnName = "latitude", Length = 50, IsNullable = true)]
        public string? Latitude { get; set; }

        /// <summary>
        /// 经度(地理位置经度坐标)
        /// </summary>
        [SugarColumn(ColumnName = "longitude", Length = 50, IsNullable = true)]
        public string? Longitude { get; set; }

        /// <summary>
        /// 地区代码(视频发布地区)
        /// </summary>
        [SugarColumn(ColumnName = "region", Length = 10, IsNullable = true)]
        public string? Region { get; set; }

        /// <summary>
        /// 播放次数(视频观看数量)
        /// </summary>
        [SugarColumn(ColumnName = "play_count", IsNullable = false)]
        public int PlayCount { get; set; } = 0;

        /// <summary>
        /// 点赞数量(用户点赞次数)
        /// </summary>
        [SugarColumn(ColumnName = "digg_count", IsNullable = false)]
        public int DiggCount { get; set; } = 0;

        /// <summary>
        /// 评论数量(用户评论次数)
        /// </summary>
        [SugarColumn(ColumnName = "comment_count", IsNullable = false)]
        public int CommentCount { get; set; } = 0;

        /// <summary>
        /// 下载次数(视频下载数量)
        /// </summary>
        [SugarColumn(ColumnName = "download_count", IsNullable = false)]
        public int DownloadCount { get; set; } = 0;

        /// <summary>
        /// 分享次数(视频分享数量)
        /// </summary>
        [SugarColumn(ColumnName = "share_count", IsNullable = false)]
        public int ShareCount { get; set; } = 0;

        /// <summary>
        /// 是否允许评论(评论权限设置)
        /// </summary>
        [SugarColumn(ColumnName = "allow_comment", IsNullable = false)]
        public bool AllowComment { get; set; } = true;

        /// <summary>
        /// 是否允许分享(分享权限设置)
        /// </summary>
        [SugarColumn(ColumnName = "allow_share", IsNullable = false)]
        public bool AllowShare { get; set; } = true;

        /// <summary>
        /// 是否允许下载(下载权限设置)
        /// </summary>
        [SugarColumn(ColumnName = "allow_download", IsNullable = false)]
        public bool AllowDownload { get; set; } = true;

        /// <summary>
        /// 是否允许合拍(合拍功能权限)
        /// </summary>
        [SugarColumn(ColumnName = "allow_duet", IsNullable = false)]
        public bool AllowDuet { get; set; } = true;

        /// <summary>
        /// 是否允许拼接(拼接功能权限)
        /// </summary>
        [SugarColumn(ColumnName = "allow_stitch", IsNullable = false)]
        public bool AllowStitch { get; set; } = true;

        /// <summary>
        /// 是否AI创作(是否由AI生成的内容)
        /// </summary>
        [SugarColumn(ColumnName = "created_by_ai", IsNullable = false)]
        public bool CreatedByAi { get; set; } = false;

        /// <summary>
        /// 分享链接(视频分享URL)
        /// </summary>
        [SugarColumn(ColumnName = "share_url", Length = 500, IsNullable = true)]
        public string? ShareUrl { get; set; }

        /// <summary>
        /// 创建时间(记录首次插入时间)
        /// </summary>
        [SugarColumn(ColumnName = "created_at", IsNullable = false)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间(记录最后修改时间)
        /// </summary>
        [SugarColumn(ColumnName = "updated_at", IsNullable = false)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 个人签名(用户简介描述)
        /// </summary>
        [SugarColumn(ColumnName = "signature", Length = 255, IsNullable = true)]
        public string? Signature { get; set; }

        /// <summary>
        /// 粉丝数量
        /// </summary>
        [SugarColumn(ColumnName = "follower_count", IsNullable = false)]
        public int FollowerCount { get; set; } = 0;

        /// <summary>
        /// 关注数量
        /// </summary>
        [SugarColumn(ColumnName = "following_count", IsNullable = false)]
        public int FollowingCount { get; set; } = 0;

        /// <summary>
        /// 作品数量
        /// </summary>
        [SugarColumn(ColumnName = "aweme_count", IsNullable = false)]
        public int AwemeCount { get; set; } = 0;

        /// <summary>
        /// 获赞总数
        /// </summary>
        [SugarColumn(ColumnName = "total_favorited", IsNullable = false)]
        public int TotalFavorited { get; set; } = 0;

        /// <summary>
        /// 点赞数量
        /// </summary>
        [SugarColumn(ColumnName = "favoriting_count", IsNullable = false)]
        public int FavoritingCount { get; set; } = 0;

        /// <summary>
        /// 内容描述
        /// </summary>
        [SugarColumn(ColumnName = "content_desc", IsNullable = true)]
        public string? ContentDesc { get; set; }

        /// <summary>
        /// 是否广告
        /// </summary>
        [SugarColumn(ColumnName = "is_ads", IsNullable = false)]
        public bool IsAds { get; set; } = false;

        /// <summary>
        /// 是否置顶
        /// </summary>
        [SugarColumn(ColumnName = "is_top", IsNullable = false)]
        public bool IsTop { get; set; } = false;

        /// <summary>
        /// 内容原创类型
        /// </summary>
        [SugarColumn(ColumnName = "content_original_type", IsNullable = false)]
        public int ContentOriginalType { get; set; } = 0;

        /// <summary>
        /// 音乐是否商业音乐
        /// </summary>
        [SugarColumn(ColumnName = "music_is_commerce_music", IsNullable = false)]
        public bool MusicIsCommerceMusic { get; set; } = false;

        /// <summary>
        /// 话题名称列表(JSON格式)
        /// </summary>
        [SugarColumn(ColumnName = "cha_names", IsNullable = true)]
        public string? ChaNames { get; set; }

        /// <summary>
        /// 话题ID列表(JSON格式)
        /// </summary>
        [SugarColumn(ColumnName = "hashtag_ids", IsNullable = true)]
        public string? HashtagIds { get; set; }

        /// <summary>
        /// 收藏数量
        /// </summary>
        [SugarColumn(ColumnName = "collect_count", IsNullable = false)]
        public int CollectCount { get; set; } = 0;

        /// <summary>
        /// 转发数量
        /// </summary>
        [SugarColumn(ColumnName = "forward_count", IsNullable = false)]
        public int ForwardCount { get; set; } = 0;

        /// <summary>
        /// WhatsApp分享数量
        /// </summary>
        [SugarColumn(ColumnName = "whatsapp_share_count", IsNullable = false)]
        public int WhatsappShareCount { get; set; } = 0;

        /// <summary>
        /// AIGC标签类型
        /// </summary>
        [SugarColumn(ColumnName = "aigc_label_type", IsNullable = false)]
        public int AigcLabelType { get; set; } = 0;

        /// <summary>
        /// 其他信息
        /// </summary>
        [SugarColumn(ColumnName = "misc_info", IsNullable = true)]
        public string? MiscInfo { get; set; }

        /// <summary>
        /// 是否有水印
        /// </summary>
        [SugarColumn(ColumnName = "has_watermark", IsNullable = false)]
        public bool HasWatermark { get; set; } = false;

        /// <summary>
        /// 推广图标文本
        /// </summary>
        [SugarColumn(ColumnName = "promote_icon_text", Length = 100, IsNullable = true)]
        public string? PromoteIconText { get; set; }

        /// <summary>
        /// 封面标签(JSON格式)
        /// </summary>
        [SugarColumn(ColumnName = "cover_labels", IsNullable = true)]
        public string? CoverLabels { get; set; }
    }
}


