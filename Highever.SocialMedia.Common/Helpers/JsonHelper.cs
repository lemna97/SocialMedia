using System.Text.Json;
using System.Text.Json.Serialization;

namespace Highever.SocialMedia.Common
{
    public static class JsonHelper
    {
        public static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = {
                new FlexibleBooleanConverter(),
                new FlexibleDecimalConverter()
            }
        };
    }
}