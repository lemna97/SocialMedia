using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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


        public static string GetJsonStringSerializeObject(object ob)
        {
            string result = JsonConvert.SerializeObject(ob);
            return result;

        }

        public static IEnumerable<T> GetJsonIEnumerableDeserialize<T>(string jsonString)
        {
            IEnumerable<T> result = JsonConvert.DeserializeObject<IEnumerable<T>>(jsonString);
            return result;
        }

        public static JArray GetJArrayDeserialize(string jsonStr)
        {
            var resObj = (JArray)JsonConvert.DeserializeObject(jsonStr);
            return resObj;
        }

        public static JObject GetJObjectDeserialize(string jsonStr)
        {
            var resObj = (JObject)JsonConvert.DeserializeObject(jsonStr);
            return resObj;
        }
        public static string GetJTokenStringDeserialize(string obj, params object[] keys)
        {
            var jObject = (JObject)JsonConvert.DeserializeObject(obj);
            JToken token = jObject;

            foreach (var i in keys)  // 实现效果 token = jObject[keys1][keys2][keys3]......
            {
                token = token[i];
            }

            string result = token?.ToString();
            return result;

        }
        public static object GetJTokenStringDeserializeBeContains(string obj, string key, string source, string findkey)
        {
            object ob = null;
            var ajaxData = (JObject)JsonConvert.DeserializeObject(obj);

            JToken token = ajaxData[key]?.FirstOrDefault(x => source.Contains(x[findkey]?.ToString()));
            if (token != null)
            {
                ob = token["content"];
            }
            else
            {
                ob = ajaxData["data"]?.Select(x => $"\"{x["oid"].ToString()}\"");
            }

            return ob;

        }
        public static string GetJArrayStringDeserialize(string value, Dictionary<string, string> cookieDic)
        {
            //var cookieJson = (JArray)GetJsonIEnumerableDeserializeObject(value);
            var cookieJson = (JArray)JsonConvert.DeserializeObject(value);
            foreach (var json in cookieJson)
            {
                foreach (var dic in cookieDic)
                {
                    if (json["Name"].ToString().Equals(dic.Key))
                    {
                        json["Value"] = dic.Value;
                        break;
                    }
                }
            }
            string cookieStr = JsonConvert.SerializeObject(cookieJson);
            return cookieStr;
        }


        public static T GetJObjectDeserialize<T>(string jsonStr)
        {
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }

    }
}