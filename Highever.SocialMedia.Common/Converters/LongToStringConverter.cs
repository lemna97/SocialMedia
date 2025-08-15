using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Common
{
    public class LongToStringConverter : System.Text.Json.Serialization.JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                // 如果是字符串类型，尝试解析为 long
                if (long.TryParse(reader.GetString(), out var result))
                {
                    return result;
                }
                throw new System.Text.Json.JsonException($"无法将值 '{reader.GetString()}' 转换为 long");
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                // 正常处理数字类型
                return reader.GetInt64();
            }

            throw new System.Text.Json.JsonException($"意外的 Token 类型: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString()); // 将 long 写为字符串
        }
    }

}
