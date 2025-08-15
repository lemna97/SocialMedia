using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Common.Extension
{ 
    public class TextJsonExtension
    {
        public class DateTimeConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return DateTime.Parse(reader.GetString());
            }
            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
        public class DateTimeNullableConverter : JsonConverter<DateTime?>
        {
            public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return string.IsNullOrEmpty(reader.GetString()) ? default(DateTime?) : DateTime.Parse(reader.GetString());
            }

            public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value?.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
        public class DatetimeJsonConverter : JsonConverter<DateTime>
        {
            public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    if (DateTime.TryParse(reader.GetString(), out DateTime date))
                        return date;
                }
                return reader.GetDateTime();
            }

            public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
        /// <summary>
        /// 灵活的可空DateTime转换器，支持空字符串转null
        /// </summary>
        public class FlexibleDateTimeNullableConverter : JsonConverter<DateTime?>
        {
            public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    var stringValue = reader.GetString();
                    
                    // 处理空字符串、null字符串、undefined等情况
                    if (string.IsNullOrWhiteSpace(stringValue) || 
                        stringValue.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                        stringValue.Equals("undefined", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    // 尝试解析日期时间
                    if (DateTime.TryParse(stringValue, out DateTime dateTime))
                    {
                        return dateTime;
                    }

                    // 如果解析失败，返回null而不是抛出异常
                    return null;
                }

                // 处理其他类型的token
                if (reader.TokenType == JsonTokenType.StartObject || reader.TokenType == JsonTokenType.StartArray)
                {
                    return null;
                }

                try
                {
                    return reader.GetDateTime();
                }
                catch
                {
                    return null;
                }
            }

            public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
            {
                if (value.HasValue)
                {
                    writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    writer.WriteNullValue();
                }
            }
        }
        /// <summary>
        /// 灵活的可空long转换器，支持空字符串转null
        /// </summary>
        public class FlexibleLongNullableConverter : JsonConverter<long?>
        {
            public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    var stringValue = reader.GetString();
                    
                    // 处理空字符串、null字符串等情况
                    if (string.IsNullOrWhiteSpace(stringValue) || 
                        stringValue.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                        stringValue.Equals("undefined", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    // 尝试解析long
                    if (long.TryParse(stringValue, out long longValue))
                    {
                        return longValue;
                    }

                    return null;
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetInt64();
                }

                return null;
            }

            public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
            {
                if (value.HasValue)
                {
                    writer.WriteStringValue(value.Value.ToString());
                }
                else
                {
                    writer.WriteNullValue();
                }
            }
        }
        /// <summary>
        /// 灵活的可空int转换器，支持空字符串转null
        /// </summary>
        public class FlexibleIntNullableConverter : JsonConverter<int?>
        {
            public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    var stringValue = reader.GetString();
                    
                    if (string.IsNullOrWhiteSpace(stringValue) || 
                        stringValue.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                        stringValue.Equals("undefined", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                    if (int.TryParse(stringValue, out int intValue))
                    {
                        return intValue;
                    }

                    return null;
                }

                if (reader.TokenType == JsonTokenType.Number)
                {
                    return reader.GetInt32();
                }

                return null;
            }

            public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
            {
                if (value.HasValue)
                {
                    writer.WriteNumberValue(value.Value);
                }
                else
                {
                    writer.WriteNullValue();
                }
            }
        }
    }
}
