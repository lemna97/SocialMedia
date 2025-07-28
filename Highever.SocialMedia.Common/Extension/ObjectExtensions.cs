using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Common
{
    public static class ObjectExtensions
    {
        public static T ToModel<T>(this object source) where T : class, new()
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "Source object cannot be null.");

            try
            {
                // 序列化源对象为 JSON 字符串
                var json = System.Text.Json.JsonSerializer.Serialize(source);

                // 反序列化为目标类型对象
                var result = System.Text.Json.JsonSerializer.Deserialize<T>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // 忽略属性大小写
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull // 忽略空值
                });

                // 检查结果是否为 null
                if (result == null)
                {
                    throw new InvalidOperationException($"Failed to map object of type {source.GetType().Name} to {typeof(T).Name}.");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error mapping object of type {source.GetType().Name} to {typeof(T).Name}: {ex.Message}", ex);
            }
        }

    }

}
