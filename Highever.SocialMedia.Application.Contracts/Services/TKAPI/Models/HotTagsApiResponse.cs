using System.Text.Json.Serialization;

namespace Highever.SocialMedia.Application.Contracts
{
    /// <summary>
    /// 热门标签API响应数据模型
    /// </summary>
    public class HotTagsApiResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("router")]
        public string Router { get; set; }

        [JsonPropertyName("params")]
        public Dictionary<string, string> Params { get; set; }

        [JsonPropertyName("data")]
        public HotTagsResponseData Data { get; set; }
    }

    public class HotTagsResponseData
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("data")]
        public HotTagsListData Data { get; set; }
    }

    public class HotTagsListData
    {
        [JsonPropertyName("list")]
        public List<HotTagItem> List { get; set; }

        [JsonPropertyName("pagination")]
        public PaginationInfo Pagination { get; set; }
    }

    public class PaginationInfo
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("has_more")]
        public bool HasMore { get; set; }
    }

    public class HotTagItem
    {
        [JsonPropertyName("hashtag_id")]
        public string HashtagId { get; set; }

        [JsonPropertyName("hashtag_name")]
        public string HashtagName { get; set; }

        [JsonPropertyName("country_info")]
        public CountryInfo CountryInfo { get; set; }

        [JsonPropertyName("industry_info")]
        public IndustryInfo IndustryInfo { get; set; }

        [JsonPropertyName("is_promoted")]
        public bool IsPromoted { get; set; }

        [JsonPropertyName("trend")]
        public List<TrendItem> Trend { get; set; }

        [JsonPropertyName("creators")]
        public List<CreatorItem> Creators { get; set; }

        [JsonPropertyName("publish_cnt")]
        public long PublishCnt { get; set; }

        [JsonPropertyName("video_views")]
        public long VideoViews { get; set; }

        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        [JsonPropertyName("rank_diff")]
        public int RankDiff { get; set; }

        [JsonPropertyName("rank_diff_type")]
        public int RankDiffType { get; set; }
    }

    public class CountryInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }
    }

    public class IndustryInfo
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }
    }

    public class TrendItem
    {
        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("value")]
        public decimal Value { get; set; }
    }

    public class CreatorItem
    {
        [JsonPropertyName("nick_name")]
        public string NickName { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }
    }
}