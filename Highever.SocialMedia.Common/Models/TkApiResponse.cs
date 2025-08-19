using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;


namespace Highever.SocialMedia.Common.Model
{
    public class TkApiResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("router")]
        public string Router { get; set; } = string.Empty;

        [JsonPropertyName("params")]
        public Dictionary<string, object>? Params { get; set; }

        [JsonPropertyName("data")]
        public dynamic Data { get; set; }
    }
    public record ApiErrorItem(
    IReadOnlyList<string> Loc,
    string Msg,
    string Type);

    public record ApiErrorResponse(
        IReadOnlyList<ApiErrorItem> Detail);

    /// <summary>封装非 2xx 时的详细信息</summary>
    public class ApiException : HttpRequestException
    {
        public HttpStatusCode StatusCode { get; }
        public ApiErrorResponse? Error { get; }

        public ApiException(HttpStatusCode status, string message,
                            ApiErrorResponse? error, Exception? inner = null)
            : base(message, inner) =>
            (StatusCode, Error) = (status, error);
    }
}
