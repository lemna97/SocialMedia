using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace Highever.SocialMedia.Common
{
    // 可空 decimal 转换器
    public class FlexibleDecimalConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    return reader.GetDecimal();
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (string.IsNullOrWhiteSpace(stringValue))
                        return null;
                    if (decimal.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                        return result;
                    return null;
                case JsonTokenType.Null:
                    return null;
                default:
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }

    // 非可空 decimal 转换器
    public class FlexibleDecimalNonNullableConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    return reader.GetDecimal();
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (decimal.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                        return result;
                    return 0;
                case JsonTokenType.Null:
                    return 0;
                default:
                    return 0;
            }
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}