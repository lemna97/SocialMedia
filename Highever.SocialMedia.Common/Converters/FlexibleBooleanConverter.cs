using System.Text.Json;
using System.Text.Json.Serialization;

namespace Highever.SocialMedia.Common
{
    // 可空布尔值转换器
    public class FlexibleBooleanConverter : JsonConverter<bool?>
    {
        public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.Number:
                    var number = reader.GetInt32();
                    return number != 0;
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (bool.TryParse(stringValue, out var boolValue))
                        return boolValue;
                    if (int.TryParse(stringValue, out var intValue))
                        return intValue != 0;
                    return null;
                case JsonTokenType.Null:
                    return null;
                default:
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteBooleanValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }

    // 非可空布尔值转换器
    public class FlexibleBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.Number:
                    var number = reader.GetInt32();
                    return number != 0;
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (bool.TryParse(stringValue, out var boolValue))
                        return boolValue;
                    if (int.TryParse(stringValue, out var intValue))
                        return intValue != 0;
                    return false;
                case JsonTokenType.Null:
                    return false;
                default:
                    return false;
            }
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}


