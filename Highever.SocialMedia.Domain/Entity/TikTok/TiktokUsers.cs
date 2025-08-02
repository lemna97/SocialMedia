using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// TikTok用户信息表
    /// </summary>
    [SugarTable("tiktok_users")]
    public class TiktokUsers
    {
        /// <summary>
        /// 用户ID(TikTok用户唯一标识)
        /// </summary>
        [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsNullable = false)]
        public long Id { get; set; }

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
        /// 个人签名(用户简介描述)
        /// </summary>
        [SugarColumn(ColumnName = "signature", IsNullable = true)]
        public string? Signature { get; set; }

        /// <summary>
        /// 大头像URL(高清头像链接)
        /// </summary>
        [SugarColumn(ColumnName = "avatar_large", Length = 500, IsNullable = true)]
        public string? AvatarLarge { get; set; }

        /// <summary>
        /// 中头像URL(中等尺寸头像链接)
        /// </summary>
        [SugarColumn(ColumnName = "avatar_medium", Length = 500, IsNullable = true)]
        public string? AvatarMedium { get; set; }

        /// <summary>
        /// 小头像URL(缩略图头像链接)
        /// </summary>
        [SugarColumn(ColumnName = "avatar_thumb", Length = 500, IsNullable = true)]
        public string? AvatarThumb { get; set; }

        /// <summary>
        /// 是否认证用户(官方认证标识)
        /// </summary>
        [SugarColumn(ColumnName = "verified", IsNullable = false)]
        public bool Verified { get; set; } = false;

        /// <summary>
        /// 是否私密账户(隐私设置)
        /// </summary>
        [SugarColumn(ColumnName = "private_account", IsNullable = false)]
        public bool PrivateAccount { get; set; } = false;

        /// <summary>
        /// 地区代码(用户所在地区)
        /// </summary>
        [SugarColumn(ColumnName = "region", Length = 10, IsNullable = true)]
        public string? Region { get; set; }

        /// <summary>
        /// 语言代码(用户使用语言)
        /// </summary>
        [SugarColumn(ColumnName = "language", Length = 10, IsNullable = true)]
        public string? Language { get; set; }

        /// <summary>
        /// 账户创建时间戳(Unix时间戳)
        /// </summary>
        [SugarColumn(ColumnName = "create_time", IsNullable = true)]
        public long? CreateTime { get; set; }

        /// <summary>
        /// 安全用户ID(TikTok内部安全标识)
        /// </summary>
        [SugarColumn(ColumnName = "sec_uid", Length = 200, IsNullable = true)]
        public string? SecUid { get; set; }

        /// <summary>
        /// 粉丝数量(关注该用户的人数)
        /// </summary>
        [SugarColumn(ColumnName = "follower_count", IsNullable = false)]
        public int FollowerCount { get; set; } = 0;

        /// <summary>
        /// 关注数量(该用户关注的人数)
        /// </summary>
        [SugarColumn(ColumnName = "following_count", IsNullable = false)]
        public int FollowingCount { get; set; } = 0;

        /// <summary>
        /// 视频总数(用户发布的视频数量)
        /// </summary>
        [SugarColumn(ColumnName = "video_count", IsNullable = false)]
        public int VideoCount { get; set; } = 0;

        /// <summary>
        /// 获赞总数(用户所有视频获得的点赞总数)
        /// </summary>
        [SugarColumn(ColumnName = "heart_count", IsNullable = false)]
        public int HeartCount { get; set; } = 0;

        /// <summary>
        /// 点赞数(用户点赞他人的次数)
        /// </summary>
        [SugarColumn(ColumnName = "digg_count", IsNullable = false)]
        public int DiggCount { get; set; } = 0;

        /// <summary>
        /// 朋友数量(互相关注的好友数)
        /// </summary>
        [SugarColumn(ColumnName = "friend_count", IsNullable = false)]
        public int FriendCount { get; set; } = 0;

        /// <summary>
        /// 记录日期(数据采集日期)
        /// </summary>
        [SugarColumn(ColumnName = "record_date", IsPrimaryKey = true, IsNullable = false)]
        public DateTime RecordDate { get; set; } = DateTime.Today;

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
    }
}