using System.Text.Json.Serialization;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// TikTok 视频 API 响应数据模型
    /// </summary>
    public class TikTokVideoApiResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public VideoResponseData Data { get; set; }
    }

    public class VideoResponseData
    {
        [JsonPropertyName("aweme_list")]
        public List<VideoItem> AwemeList { get; set; }

        [JsonPropertyName("has_more")]
        public int HasMore { get; set; }

        [JsonPropertyName("max_cursor")]
        public long MaxCursor { get; set; }
    }

    /// <summary>
    /// 视频项目信息
    /// </summary>
    public class VideoItem
    {
        /// <summary>
        /// 视频ID
        /// </summary>
        [JsonPropertyName("aweme_id")]
        public string AwemeId { get; set; }

        /// <summary>
        /// 视频描述
        /// </summary>
        [JsonPropertyName("desc")]
        public string Desc { get; set; }

        /// <summary>
        /// 创建时间戳
        /// </summary>
        [JsonPropertyName("create_time")]
        public long CreateTime { get; set; }

        /// <summary>
        /// 作者信息
        /// </summary>
        [JsonPropertyName("author")]
        public VideoAuthor Author { get; set; }

        /// <summary>
        /// 视频信息
        /// </summary>
        [JsonPropertyName("video")]
        public VideoInfo Video { get; set; }

        /// <summary>
        /// 音乐信息
        /// </summary>
        [JsonPropertyName("music")]
        public MusicInfo Music { get; set; }

        /// <summary>
        /// 统计数据
        /// </summary>
        [JsonPropertyName("statistics")]
        public VideoStatistics Statistics { get; set; }

        /// <summary>
        /// 状态信息
        /// </summary>
        [JsonPropertyName("status")]
        public VideoStatus Status { get; set; }

        /// <summary>
        /// 视频控制设置
        /// </summary>
        [JsonPropertyName("video_control")]
        public VideoControl VideoControl { get; set; }

        /// <summary>
        /// 位置信息
        /// </summary>
        [JsonPropertyName("poi_data")]
        public PoiData PoiData { get; set; }

        /// <summary>
        /// 话题标签列表
        /// </summary>
        [JsonPropertyName("cha_list")]
        public List<HashtagInfo> ChaList { get; set; }

        /// <summary>
        /// 地区信息
        /// </summary>
        [JsonPropertyName("region")]
        public string Region { get; set; }

        /// <summary>
        /// AI生成内容信息
        /// </summary>
        [JsonPropertyName("aigc_info")]
        public AigcInfo AigcInfo { get; set; }

        /// <summary>
        /// 分享信息
        /// </summary>
        [JsonPropertyName("share_info")]
        public ShareInfo ShareInfo { get; set; }

        /// <summary>
        /// 内容描述
        /// </summary>
        [JsonPropertyName("content_desc")]
        public string? ContentDesc { get; set; }

        /// <summary>
        /// 是否为广告
        /// </summary>
        [JsonPropertyName("is_ads")]
        public bool? IsAds { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        [JsonPropertyName("is_top")]
        public bool? IsTop { get; set; }

        /// <summary>
        /// 内容原创类型
        /// </summary>
        [JsonPropertyName("content_original_type")]
        public int? ContentOriginalType { get; set; }

        /// <summary>
        /// 其他信息
        /// </summary>
        [JsonPropertyName("misc_info")]
        public string? MiscInfo { get; set; }

        /// <summary>
        /// 推广图标文本
        /// </summary>
        [JsonPropertyName("promote_icon_text")]
        public string? PromoteIconText { get; set; }

        /// <summary>
        /// 封面标签
        /// </summary>
        [JsonPropertyName("cover_labels")]
        public object? CoverLabels { get; set; }
    }

    /// <summary>
    /// 视频作者信息
    /// </summary>
    public class VideoAuthor
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [JsonPropertyName("uid")]
        public string Uid { get; set; }

        /// <summary>
        /// 唯一标识符
        /// </summary>
        [JsonPropertyName("unique_id")]
        public string UniqueId { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        /// <summary>
        /// 个人签名
        /// </summary>
        [JsonPropertyName("signature")]
        public string? Signature { get; set; }

        /// <summary>
        /// 粉丝数量
        /// </summary>
        [JsonPropertyName("follower_count")]
        public int? FollowerCount { get; set; }

        /// <summary>
        /// 关注数量
        /// </summary>
        [JsonPropertyName("following_count")]
        public int? FollowingCount { get; set; }

        /// <summary>
        /// 作品数量
        /// </summary>
        [JsonPropertyName("aweme_count")]
        public int? AwemeCount { get; set; }

        /// <summary>
        /// 获赞总数
        /// </summary>
        [JsonPropertyName("total_favorited")]
        public int? TotalFavorited { get; set; }

        /// <summary>
        /// 点赞数量
        /// </summary>
        [JsonPropertyName("favoriting_count")]
        public int? FavoritingCount { get; set; }
    }

    /// <summary>
    /// 视频信息
    /// </summary>
    public class VideoInfo
    {
        /// <summary>
        /// 视频时长（秒）
        /// </summary>
        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        /// <summary>
        /// 视频比例
        /// </summary>
        [JsonPropertyName("ratio")]
        public string Ratio { get; set; }

        /// <summary>
        /// 封面图片
        /// </summary>
        [JsonPropertyName("cover")]
        public UrlList Cover { get; set; }

        /// <summary>
        /// 播放地址
        /// </summary>
        [JsonPropertyName("play_addr")]
        public UrlList PlayAddr { get; set; }

        /// <summary>
        /// 是否有水印
        /// </summary>
        [JsonPropertyName("has_watermark")]
        public bool? HasWatermark { get; set; }
    }

    /// <summary>
    /// URL列表
    /// </summary>
    public class UrlList
    {
        /// <summary>
        /// URL列表数据
        /// </summary>
        [JsonPropertyName("url_list")]
        public List<string> UrlListData { get; set; }
    }

    /// <summary>
    /// 音乐信息
    /// </summary>
    public class MusicInfo
    {
        /// <summary>
        /// 音乐ID字符串
        /// </summary>
        [JsonPropertyName("id_str")]
        public string IdStr { get; set; }

        /// <summary>
        /// 音乐标题
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// 音乐作者
        /// </summary>
        [JsonPropertyName("author")]
        public string Author { get; set; }

        /// <summary>
        /// 是否为原声
        /// </summary>
        [JsonPropertyName("is_original_sound")]
        public bool IsOriginalSound { get; set; }

        /// <summary>
        /// 是否为商业音乐
        /// </summary>
        [JsonPropertyName("is_commerce_music")]
        public bool? IsCommerceMusic { get; set; }
    }

    /// <summary>
    /// 视频统计数据
    /// </summary>
    public class VideoStatistics
    {
        /// <summary>
        /// 播放次数
        /// </summary>
        [JsonPropertyName("play_count")]
        public int PlayCount { get; set; }

        /// <summary>
        /// 点赞次数
        /// </summary>
        [JsonPropertyName("digg_count")]
        public int DiggCount { get; set; }

        /// <summary>
        /// 评论次数
        /// </summary>
        [JsonPropertyName("comment_count")]
        public int CommentCount { get; set; }

        /// <summary>
        /// 下载次数
        /// </summary>
        [JsonPropertyName("download_count")]
        public int DownloadCount { get; set; }

        /// <summary>
        /// 分享次数
        /// </summary>
        [JsonPropertyName("share_count")]
        public int ShareCount { get; set; }

        /// <summary>
        /// 收藏次数
        /// </summary>
        [JsonPropertyName("collect_count")]
        public int? CollectCount { get; set; }

        /// <summary>
        /// 转发次数
        /// </summary>
        [JsonPropertyName("forward_count")]
        public int? ForwardCount { get; set; }

        /// <summary>
        /// WhatsApp分享次数
        /// </summary>
        [JsonPropertyName("whatsapp_share_count")]
        public int? WhatsappShareCount { get; set; }
    }

    /// <summary>
    /// 视频状态信息
    /// </summary>
    public class VideoStatus
    {
        /// <summary>
        /// 是否允许评论
        /// </summary>
        [JsonPropertyName("allow_comment")]
        public bool AllowComment { get; set; }

        /// <summary>
        /// 是否允许分享
        /// </summary>
        [JsonPropertyName("allow_share")]
        public bool AllowShare { get; set; }
    }

    /// <summary>
    /// 视频控制设置
    /// </summary>
    public class VideoControl
    {
        /// <summary>
        /// 是否允许下载
        /// </summary>
        [JsonPropertyName("allow_download")]
        public bool AllowDownload { get; set; }

        /// <summary>
        /// 是否允许合拍
        /// </summary>
        [JsonPropertyName("allow_duet")]
        public bool AllowDuet { get; set; }

        /// <summary>
        /// 是否允许拼接
        /// </summary>
        [JsonPropertyName("allow_stitch")]
        public bool AllowStitch { get; set; }
    }

    /// <summary>
    /// 位置数据
    /// </summary>
    public class PoiData
    {
        /// <summary>
        /// 位置名称
        /// </summary>
        [JsonPropertyName("poi_name")]
        public string PoiName { get; set; }

        /// <summary>
        /// 地址信息
        /// </summary>
        [JsonPropertyName("address_info")]
        public AddressInfo AddressInfo { get; set; }
    }

    /// <summary>
    /// 地址信息
    /// </summary>
    public class AddressInfo
    {
        /// <summary>
        /// 详细地址
        /// </summary>
        [JsonPropertyName("address")]
        public string Address { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        [JsonPropertyName("lat")]
        public decimal? Lat { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        [JsonPropertyName("lng")]
        public decimal? Lng { get; set; }
    }

    /// <summary>
    /// 话题标签信息
    /// </summary>
    public class HashtagInfo
    {
        /// <summary>
        /// 话题名称
        /// </summary>
        [JsonPropertyName("cha_name")]
        public string ChaName { get; set; }

        /// <summary>
        /// 话题ID
        /// </summary>
        [JsonPropertyName("cid")]
        public string? Cid { get; set; }
    }

    /// <summary>
    /// AI生成内容信息
    /// </summary>
    public class AigcInfo
    {
        /// <summary>
        /// 是否由AI创建
        /// </summary>
        [JsonPropertyName("created_by_ai")]
        public bool CreatedByAi { get; set; }

        /// <summary>
        /// AIGC标签类型
        /// </summary>
        [JsonPropertyName("aigc_label_type")]
        public int? AigcLabelType { get; set; }
    }

    /// <summary>
    /// 分享信息
    /// </summary>
    public class ShareInfo
    {
        /// <summary>
        /// 分享链接
        /// </summary>
        [JsonPropertyName("share_url")]
        public string ShareUrl { get; set; }
    }
}

