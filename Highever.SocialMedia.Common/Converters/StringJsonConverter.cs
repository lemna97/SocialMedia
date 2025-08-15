using System;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Highever.SocialMedia.Common
{
    public class StringJsonConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // 将所有 JSON 数据都读取为字符串
            return reader.GetString();
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            // 所有数据都作为字符串写入
            writer.WriteStringValue(value?.ToString());
        }
    }
}
