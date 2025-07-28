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
    public class SystemTextJsonExtension
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
    }
}
