using ICSharpCode.SharpZipLib.GZip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// 递归去除 JSON 字符串的多重转义，并返回标准 JSON。
        /// </summary>
        /// <param name="input">需要处理的输入字符串</param>
        /// <returns>去除转义后的标准 JSON 字符串</returns>
        public static string UnescapeJson(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("输入字符串不能为空或仅包含空白字符！");
            }

            try
            {
                // 递归解析直到不再需要去转义
                while (IsWrappedWithQuotes(input))
                {
                    input = JsonConvert.DeserializeObject<string>(input); // 去掉一层转义
                }
                return input;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"去转义过程中发生错误：{ex.Message}");
            }
        }

        /// <summary>
        /// 判断输入是否被多重引号包装（即是否需要进一步去转义）
        /// </summary>
        private static bool IsWrappedWithQuotes(string input)
        {
            return input.StartsWith("\"") && input.EndsWith("\"");
        }

        /// <summary>
        /// 根据三个字符串参数生成一个 16 位符号 + 数字组合的唯一 ID。
        /// </summary>
        /// <param name="param1">第一个字符串</param>
        /// <param name="param2">第二个字符串</param>
        /// <param name="param3">第三个字符串</param>
        /// <returns>返回 16 位符号 + 数字的加密 ID</returns>
        public static string GenerateId(this string param1, string param2="", string param3="")
        {
            // 1. 将三个参数合并成一个字符串
            string combinedString = $"{param1}{param2}{param3}";

            // 2. 使用 MD5 对字符串进行加密
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(combinedString));

                // 3. 转换为 32 个字符的十六进制字符串
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // 转换为小写十六进制字符串
                }

                // 4. 截取前 16 个字符作为 ID。如果需要符号，可以额外处理。
                return sb.ToString().Substring(0, 16);
            }
        }
        /// <summary>
        /// 从字符串中提取第一个有效的数字（整数或小数）。
        /// </summary>
        /// <typeparam name="T">返回的数值类型，可为 string, double, decimal 等。</typeparam>
        /// <param name="input">包含数字的字符串。</param>
        /// <returns>提取的数字，如果无法提取则返回默认值。</returns>
        public static T ExtractNumber<T>(string input) where T : IConvertible
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("输入不能为空", nameof(input));
            }

            // 使用正则表达式提取第一个数字（包括小数）
            var match = Regex.Match(input, @"\d+(\.\d+)?");

            // 如果匹配成功
            if (match.Success)
            {
                string numericString = match.Value;

                try
                {
                    // 如果目标类型是 string，直接返回
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)numericString; // 强制转换为 string
                    }

                    // 将匹配到的字符串转换为 T 类型
                    if (typeof(T) == typeof(double))
                    {
                        return (T)(object)double.Parse(numericString);
                    }
                    else if (typeof(T) == typeof(decimal))
                    {
                        return (T)(object)decimal.Parse(numericString);
                    }

                    throw new InvalidCastException($"不支持的转换类型：{typeof(T).Name}");
                }
                catch (Exception ex)
                {
                    throw new FormatException($"无法将数字 {numericString} 转换为类型 {typeof(T).Name}: {ex.Message}");
                }
            }

            // 如果没有数字，返回默认值或直接抛出异常
            throw new InvalidOperationException("字符串中未找到有效的数字");
        }
        /// <summary>
        /// 批次号
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="dateFormat"></param>
        /// <returns></returns>
        public static string GenerateBatchNumber(string prefix = "BATCH", string dateFormat = "yyyyMMdd")
        {
            string currentDate = DateTime.Now.ToString(dateFormat);
            Random random = new Random();
            int randomNumber = random.Next(1000, 9999);
            return $"{prefix}-{currentDate}-{randomNumber}";
        } 
        /// <summary>
        /// 对比两个字符串
        /// LEMNA
        /// </summary>
        /// <param name="json1"></param>
        /// <param name="json2"></param>
        /// <returns></returns>
        public static bool JsonStringsEqual(this string json1, string json2)
        {
            var jObject1 = JObject.Parse(json1);
            var jObject2 = JObject.Parse(json2);

            return JToken.DeepEquals(jObject1, jObject2);
        }
        /// <summary>
        /// 对比两个对象
        /// LEMNA
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool JsonStringsEqual(this object obj1, object obj2)
        {
            // 序列化对象为JSON字符串
            string json1 = JsonConvert.SerializeObject(obj1);
            string json2 = JsonConvert.SerializeObject(obj2);

            return string.Equals(json1, json2, StringComparison.Ordinal);
        } 
        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="length"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string GenerateRandomString(int length)
        {
            Random random = new Random();

            // 字符集包含大写字母和数字
            const string charPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = charPool[random.Next(charPool.Length)];
            }
            return new string(chars);
        }

        /// <summary>
        /// 判断时间是否有效：小于2000年视为无效
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsValid(this DateTime time)
        {
            var t = DateTime.Parse("2000-01-01");
            if (time < t) return false;
            return true;

        }

        /// <summary> 
        /// 判断某个日期是否在某段日期范围内，返回布尔值
        /// </summary> 
        /// <param name="dt">要判断的日期</param> 
        /// <param name="dt1">开始日期</param> 
        /// <param name="dt2">结束日期</param> 
        /// <returns></returns>  
        public static bool IsInDate(DateTime dt, DateTime? dt1, DateTime? dt2)
        {
            return DateTime.Compare(dt, (DateTime)dt1) >= 0 && DateTime.Compare(dt, (DateTime)dt2) <= 0;
        }

        /// <summary>
        /// List分页处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_list"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public static List<T> SplitList<T>(List<T> _list, int PageIndex, int PageSize)
        {
            int _PageIndex = PageIndex == 0 ? 1 : PageIndex;
            int _PageSize = PageSize == 0 ? 20 : PageSize;
            int PageConut = (int)Math.Ceiling(Convert.ToDecimal(_list.Count) / _PageSize);
            if (PageConut >= _PageIndex)
            {
                List<T> list = new List<T>();
                list = _list.Skip((_PageIndex - 1) * _PageSize).Take(_PageSize).ToList();
                return list;
            }
            else
                return _list;
        }
         
        public static string DataTimeFormatToString(this DateTime time, string forMat = "yyyy-MM-dd HH:mm:ss")
        {
            return time.ToString(forMat);
        }
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        } 
        public static int TryParseToInt(this string str, int value = 0)
        {
            var flag = int.TryParse(str, out int strValue);
            if (flag) return strValue;
            else return value;
        }

        public static long TryParseToLong(this string str, long value = 0)
        {
            var flag = long.TryParse(str, out long strValue);
            if (flag) return strValue;
            else return value;
        }

        public static decimal? TryParseToDecimal(this string str)
        {
            var flag = decimal.TryParse(str, out decimal strValue);
            if (flag) return Math.Round(strValue, 2);
            else return null;
        }

        public static decimal TryParseToDecimalZero2(this string str)
        {
            var flag = decimal.TryParse(str, out decimal strValue);
            if (flag) return Math.Round(strValue, 2);
            else return 0;
        }

        public static decimal TryParseToDecimalZero(this string str)
        {
            var flag = decimal.TryParse(str, out decimal strValue);
            if (flag) return strValue;
            else return 0;
        }
        public static double? TryParseToDouble(this string str)
        {
            var flag = double.TryParse(str, out double strValue);
            if (flag) return strValue;
            else return null;
        } 
        public static DateTime? TryParseToDateTime(this string str)
        {
            var flag = DateTime.TryParse(str, out DateTime strValue);
            if (flag) return strValue;
            else return null;
        }

        public static bool TryParseToBool(this string str)
        {
            var flag = bool.TryParse(str, out bool strValue);
            if (flag) return strValue;
            else return flag;
        }

        public static bool IsExistDicAndNotNullOrWhiteSpace(this Dictionary<string, object> paramDic, string str)
        {
            return paramDic.ContainsKey(str) && !string.IsNullOrWhiteSpace(paramDic[str].ToString());
        }

         
        public static string Md5File(Stream stream = null, byte[] bytes = null)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = null;
            if (stream != null)
            {
                retVal = md5.ComputeHash(stream);
            }
            else if (bytes != null)
            {
                retVal = md5.ComputeHash(bytes);
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        } 
        /// <summary>
        /// 检查字符串是否是纯数字和字母
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool CheckStringIsNumberOrAbc(string str)
        {
            Regex regex = new Regex(@"^[A-Za-z0-9]+$");
            var r = regex.IsMatch(str);
            return r;
        }
        
        public static string ToISO8601Time(this DateTime time)
        {
            return time.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        /// <summary>
        /// decimal金额 转为三位','分割 (1.保留两位 2.自动四舍五入)
        /// </summary>
        /// <param name="thisPrice"></param>
        /// <returns></returns>
        public static string decimalToPrice(this decimal? thisPrice)
        {
            var strPrice = thisPrice ?? 0;
            return string.Format("{0:N}", strPrice);
        }
        /// <summary>
        /// 时间转字符串
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="format">字符串格式 默认yyyy-MM-dd</param>
        /// <param name="defaultValue">时间为空时，默认返回</param>
        /// <returns></returns>
        public static string TimeToString(DateTime? time, string format = "yyyy-MM-dd", bool isThrow = true, string defaultValue = "")
        {
            if (time == null && isThrow) throw new Exception("转换时值不可为空");
            else if (time == null) return defaultValue;
            return ((DateTime)time).ToString(format);
        }
        /// <summary>
        /// 整数值转字符串
        /// </summary>
        /// <param name="number">需要转换的值</param>
        /// <param name="format">格式 默认无</param>
        /// <param name="isThrow">若为空值 是否抛出异常，该值为true时，defaultValue无效</param>
        /// <param name="defaultValue">若为空值，默认返回</param>
        /// <returns></returns>
        public static string IntToString(int? number, string format = "", bool isThrow = true, string defaultValue = "")
        {
            if (number == null && isThrow) throw new Exception("转换时值不可为空");
            else if (number == null) return defaultValue;
            return ((decimal)number).ToString(format);
        }
        /// <summary>
        /// （小）数值转字符串
        /// </summary>
        /// <param name="number">需要转换的值</param>
        /// <param name="format">格式 默认无</param>
        /// <param name="isThrow">若为空值 是否抛出异常，该值为true时，defaultValue无效</param>
        /// <param name="defaultValue">若为空值，默认返回</param>
        /// <returns></returns>
        public static string DecimalToString(decimal? number, string format = "", bool isThrow = true, string defaultValue = "")
        {
            if (number == null && isThrow) throw new Exception("转换时值不可为空");
            else if (number == null) return defaultValue;
            return ((decimal)number).ToString(format);
        }

        /// <summary>
        /// decimal金额 转为三位','分割 (1.保留两位 2.自动四舍五入)
        /// </summary>
        /// <param name="thisPrice"></param>
        /// <returns></returns>
        public static string decimalToPrice(this decimal thisPrice)
        {
            return string.Format("{0:N}", thisPrice);
        }

        public static DateTime ToDate(this string str)
        {
            return Convert.ToDateTime(str);
        }
        /// <summary>
        /// DATETIME?
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DateTime ToDate(this DateTime? str)
        {
            return Convert.ToDateTime(str);
        }
        public static long ToLong(this string str)
        {
            return Convert.ToInt64(str);
        }

        public static decimal? ToDecimal(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            else
            {
                return Convert.ToDecimal(str);
            }
        }
        public static int? ToInt32(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }
            else
            {
                return Convert.ToInt32(str);
            }
        }

        public static int DecimalToInt(this decimal? str)
        {
            return str == null ? 0 : (int)str;
        }

        public static string ToStringByChar(this List<string> str, string ca = "|")
        {
            if (str == null || str.Count == 0)
            {
                return null;
            }
            else
            {
                return string.Join(ca, str);
            }
        }
        /// <summary>
        /// 通过当前时间获取唯一名
        /// </summary>
        /// <returns></returns>
        public static string GetUnionNameByDate(int num = 3)
        {
            var str = "";
            if (num > 0)
            {
                var nums = Enumerable.Range(0, 9).OrderBy(x => Guid.NewGuid()).Take(num);
                str = string.Join("", nums);
            }
            return DateTime.Now.ToString("yyyyMMddHHmmss") + str;
        }
   
        /// <summary>
        /// C# 序列化json不显示null的字段
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJsonSerializeObject(this object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }


        //清除不固定多个不同类型的List占用内存
        public static void ListClear<T>(this List<T> list, params List<T>[] lists)
        {
            if (list != null)
            {
                list?.Clear();
                list?.TrimExcess();
                list = null;
            }
            if (lists != null)
            {
                foreach (var item in lists)
                {
                    item?.Clear();
                    item?.TrimExcess();
                }
            }
        }
        //清除不固定多个不同类型的List占用内存
        public static void ListClear<T>(this Dictionary<T, T> list, params Dictionary<T, T>[] lists)
        {
            if (list != null)
            {
                list?.Clear();
                list?.TrimExcess();
                list = null;
            }
            if (lists != null)
            {
                foreach (var item in lists)
                {
                    item?.Clear();
                    item?.TrimExcess();
                }
            }
        }


        /// <summary>
        /// 保留小数点2位
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static decimal ToDecimal2(this decimal a)
        {
            return Math.Round(a, 2);
        }


        public static decimal ToDecimal(this decimal? a)
        {
            return a == null ? 0m : (decimal)a;
        }


        #region 金额阿拉伯数字转换为大写
        /// <summary>
        /// 金额阿拉伯数字转换为大写
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string ToMoneyText(this decimal value)
        {
            string result = "";         //←定义结果
            int unitPointer = 0;        //←定义单位位置
            //↓格式化金额字符串
            string valueStr = value.ToString("#0.00");
            //↓判断是否超出万亿的限制
            if (valueStr.Length > 16)
            {
                throw new Exception("暂不支持超过万亿级别的数字！");
            }
            //↓遍历字符串，获取金额大写
            for (int i = valueStr.Length - 1; i >= 0; i--)
            {
                //↓判断是否小数点
                if (valueStr[i] != '.')
                {
                    //↓后推方式增加内容
                    result = GetDaXie(valueStr[i]) + moneyUnit[unitPointer] + result;
                    //↓设置单位位置
                    unitPointer++;
                }
            }
            return result;
        }

        public static Dictionary<string, string> GetAllCountry()
        {
            return new Dictionary<string, string>() {
                {"AE","阿联酋" },
                {"AU","澳大利亚" },
                {"BR","巴西" },
                {"CA","加拿大" },
                {"CN","中国" },
                {"DE","德国" },
                {"ES","西班牙" },
                {"FR","法国" },
                {"GB","英国" },
                {"IN","印度" },
                {"IT","意大利" },
                {"JP","日本" },
                {"MX","墨西哥" },
                {"NL","荷兰" },
                {"PL","波兰" },
                {"SA","沙特阿拉伯" },
                {"SE","瑞典" },
                {"SG","新加坡" },
                {"TR","土耳其" },
                {"US","美国" },
            };
        }

        private static string[] moneyUnit = { "分", "角", "元", "拾", "佰", "仟", "萬", "拾", "佰", "仟", "亿", "拾", "佰", "仟", "萬" };
        /// <summary>
        /// 获取大写信息
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static string GetDaXie(char c)
        {
            string result = "";
            switch (c)
            {
                case '0':
                    result = "零";
                    break;
                case '1':
                    result = "壹";
                    break;
                case '2':
                    result = "贰";
                    break;
                case '3':
                    result = "叁";
                    break;
                case '4':
                    result = "肆";
                    break;
                case '5':
                    result = "伍";
                    break;
                case '6':
                    result = "陆";
                    break;
                case '7':
                    result = "柒";
                    break;
                case '8':
                    result = "捌";
                    break;
                case '9':
                    result = "玖";
                    break;
            }
            return result;
        }

        #region DateTime To 时间戳

        public static long GetTimeStamp(DateTime date)
        {
            return (date.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public static long ToTimeStamp(this DateTime date)
        {
            return (date.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }
        public static DateTime FromTimeStamp(this long timestamp)
        {
            return new DateTime(timestamp * 10000000 + 621355968000000000, DateTimeKind.Utc).ToLocalTime();
        }

        public static long? ToTimeStamp(this DateTime? date)
        {
            if (date == null) return null;
            return (date?.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        #endregion


        /// <summary>
        /// 根据时间戳获取时间
        /// </summary>
        /// <param name="createTime"></param>
        /// <returns></returns>
        public static DateTime GetDateByStamp(long timeStamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timeStamp).ToLocalTime();
        }
        public static DateTime ToDateByStamp(this long timeStamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timeStamp).ToLocalTime();
        }
        public static DateTime? ToDateByStamp(this long? timeStamp)
        {
            if (timeStamp == null) return null;
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timeStamp ?? 0).ToLocalTime();
        }

        //public static DateTime ToDateByStampToDate(this long? timestamp)
        //{
        //    // 假定这个方法处理以秒为单位的时间戳
        //    var dateTime = timestamp.ToDateByStamp();
        //    return dateTime.ToLocalTime().Date; // 转换为当地时间并只取日期部分
        //}

        //某日期是本月的第几周
        //public static int WeekOfMonth(DateTime dtSel, bool sundayStart)
        //{
        //    //如果要判断的日期为1号，则肯定是第一周了 
        //    if (dtSel.Day == 1) return 1;
        //    else
        //    {
        //        //得到本月第一天 
        //        DateTime dtStart = new DateTime(dtSel.Year, dtSel.Month, 1);
        //        //得到本月第一天是周几 
        //        int dayofweek = (int)dtStart.DayOfWeek;
        //        //如果不是以周日开始，需要重新计算一下dayofweek，详细风DayOfWeek枚举的定义 
        //        if (!sundayStart)
        //        {
        //            dayofweek = dayofweek - 1;
        //            if (dayofweek < 0) dayofweek = 7;
        //        }
        //        //得到本月的第一周一共有几天 
        //        int startWeekDays = 7 - dayofweek;
        //        //如果要判断的日期在第一周范围内，返回1 
        //        if (dtSel.Day <= startWeekDays) return 1;
        //        else
        //        {
        //            int aday = dtSel.Day + 7 - startWeekDays;
        //            return aday / 7 + (aday % 7 > 0 ? 1 : 0);
        //        }
        //    }
        //}


        public static int WeekOfMonth(DateTime day, int WeekStart)
        {
            //WeekStart
            //1表示 周一至周日 为一周
            //2表示 周日至周六 为一周
            DateTime FirstofMonth;
            FirstofMonth = Convert.ToDateTime(day.Date.Year + "-" + day.Date.Month + "-" + 1);

            int i = (int)FirstofMonth.Date.DayOfWeek;
            if (i == 0)
            {
                i = 7;
            }

            if (WeekStart == 1)
            {
                return (day.Date.Day + i - 2) / 7 + 1;
            }
            if (WeekStart == 2)
            {
                return (day.Date.Day + i - 1) / 7;

            }
            return 0;
            //错误返回值0
        }

        public static void SetValue(object inputObject, string propertyName, object propertyVal)
        {
            //find out the type
            Type type = inputObject.GetType();

            //get the property information based on the type
            System.Reflection.PropertyInfo propertyInfo = type.GetProperty(propertyName);

            //find the property type
            Type propertyType = propertyInfo.PropertyType;

            //Convert.ChangeType does not handle conversion to nullable types
            //if the property type is nullable, we need to get the underlying type of the property
            var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;

            //Returns an System.Object with the specified System.Type and whose value is
            //equivalent to the specified object.
            propertyVal = Convert.ChangeType(propertyVal, targetType);

            //Set the value of the property
            propertyInfo.SetValue(inputObject, propertyVal, null);

        }
        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        #endregion
        /// <summary>
        /// 或判断  判断是否存在空(至少一个)
        /// </summary>
        /// <param name="paramArr"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpaceOr(params object[] paramArr)
        {
            var flag = false;
            foreach (var item in paramArr)
            {
                flag = flag || string.IsNullOrWhiteSpace(item?.ToString());
                if (flag) break;
            }
            return flag;
        }
        /// <summary>
        /// 与判断  判断是否都为空
        /// </summary>
        /// <param name="paramArr"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpaceAnd(params object[] paramArr)
        {
            var flag = true;
            foreach (var item in paramArr)
            {
                flag = flag && string.IsNullOrWhiteSpace(item?.ToString());
            }
            return flag;
        }
        /// <summary>
        /// 是否包含  或判断
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="paramArr"></param>
        /// <returns></returns>
        public static bool IsContainsOr(this string origin, params string[] paramArr)
        {
            var flag = false;
            for (int i = 0; i < paramArr.Length; i++)
            {
                flag = flag || origin.Contains(paramArr[i]);
                if (flag) break;
            }
            return flag;
        }
        /// <summary>
        /// 是否包含  与判断
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="paramArr"></param>
        /// <returns></returns>
        public static bool IsContainsAnd(this string origin, params string[] paramArr)
        {
            var flag = true;
            for (int i = 0; i < paramArr.Length; i++)
            {
                flag = flag && origin.Contains(paramArr[i]);
            }
            return flag;
        }


        /// <summary>
        /// 字符串 特殊字符串 ??? 的替换 ' '
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string HandleSpecialSpace(this string value)
        {
            byte[] space = new byte[] { 0xc2, 0xa0 };
            string UTFSpace = Encoding.GetEncoding("UTF-8").GetString(space);
            return value.Replace(UTFSpace, " ");//&nbsp;
        }

        /// <summary>
        /// 首字母转小写
        /// </summary>
        /// <param name="ptteType"></param>
        /// <returns></returns>
        public static string GetTitleLowerCase(string ptteType)
        {
            if (string.IsNullOrWhiteSpace(ptteType)) return "";
            if (ptteType.Length <= 1) return ptteType.ToLower();
            return ptteType.Substring(0, 1).ToLower() + ptteType.Substring(1);
        }

        /// <summary>
        /// 首字母转大写
        /// </summary>
        /// <param name="ptteType"></param>
        /// <returns></returns>
        public static string GetTitleUpperCase(string ptteType)
        {
            if (string.IsNullOrWhiteSpace(ptteType)) return "";
            if (ptteType.Length <= 1) return ptteType.ToUpper();
            return ptteType.Substring(0, 1).ToUpper() + ptteType.Substring(1);
        }

        #region 解压缩
        public static string CompressStr(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            MemoryStream ms = new MemoryStream();
            using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
            {
                zip.Write(buffer, 0, buffer.Length);
            }

            ms.Position = 0;
            MemoryStream outStream = new MemoryStream();

            byte[] compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);

            byte[] gzBuffer = new byte[compressed.Length + 4];
            System.Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }

        /// <summary>
        /// 将传入字符串以GZip算法压缩后，返回Base64编码字符
        /// </summary>
        /// <param name="rawString">需要压缩的字符串</param>
        /// <returns>压缩后的Base64编码的字符串</returns>
        public static string GZipCompressString(string rawString)
        {
            if (string.IsNullOrEmpty(rawString) || rawString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] rawData = System.Text.Encoding.UTF8.GetBytes(rawString.ToString());
                byte[] zippedData = Compress(rawData);
                return (string)(Convert.ToBase64String(zippedData));
            }

        }
        /// <summary>
        /// GZip压缩
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] rawData)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
            compressedzipStream.Write(rawData, 0, rawData.Length);
            compressedzipStream.Close();
            return ms.ToArray();
        }
        /// <summary>
        /// 将传入的二进制字符串资料以GZip算法解压缩
        /// </summary>
        /// <param name="zippedString">经GZip压缩后的二进制字符串</param>
        /// <returns>原始未压缩字符串</returns>
        public static string GZipDecompressString(string zippedString)
        {
            if (string.IsNullOrEmpty(zippedString) || zippedString.Length == 0)
            {
                return "";
            }
            else
            {
                byte[] zippedData = Convert.FromBase64String(zippedString.ToString());
                return (string)(System.Text.Encoding.UTF8.GetString(Decompress(zippedData)));
            }
        }
        /// <summary>
        /// ZIP解压
        /// </summary>
        /// <param name="zippedData"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] zippedData)
        {
            MemoryStream ms = new MemoryStream(zippedData);
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Decompress);
            MemoryStream outBuffer = new MemoryStream();
            byte[] block = new byte[1024];
            while (true)
            {
                int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                else
                    outBuffer.Write(block, 0, bytesRead);
            }
            compressedzipStream.Close();
            return outBuffer.ToArray();
        }

        /// <summary>
        /// 将gzip文件转成json
        /// </summary>
        /// <param name="fileToDecompress"></param>
        public static void Decompress(FileInfo fileToDecompress)
        {
            using (FileStream originalFileStream = fileToDecompress.OpenRead())
            {
                string currentFileName = fileToDecompress.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

                using (FileStream decompressedFileStream = File.Create(newFileName))
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(decompressedFileStream);
                        Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
                    }
                }
            }
        }


        /// <summary>
        /// C#使用GZIP解压缩完整读取网页内容
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetHtmlWithUtf(string url)
        {
            if (!(url.Contains("http://") || url.Contains("https://")))
            {
                url = "http://" + url;
            }
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.UserAgent = "User-Agent: Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
            req.Accept = "*/*";
            req.Headers.Add("Accept-Language", "zh-cn,en-us;q=0.5");
            req.ContentType = "text/xml";
            req.Headers["Accept-Encoding"] = "gzip,deflate";
            req.AutomaticDecompression = DecompressionMethods.GZip;
            string sHTML = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                if (response.ContentEncoding.ToLower().Contains("gzip"))
                {
                    using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            sHTML = reader.ReadToEnd();
                        }
                    }
                }
                else if (response.ContentEncoding.ToLower().Contains("deflate"))
                {
                    using (DeflateStream stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            sHTML = reader.ReadToEnd();
                        }
                    }
                }
                else
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            sHTML = reader.ReadToEnd();
                        }
                    }
                }
            }
            return sHTML;
        }
        /// <summary>
        /// http下载文件
        /// </summary>
        /// <param name="url">下载文件地址</param>
        /// <returns></returns>
        //public static string HttpDownload(string url)
        //{
        //    using (var client = new WebClient())
        //    {
        //        string tempFile = Path.GetTempFileName();
        //         client.DownloadData(url);//下载临时文件
        //        Console.WriteLine("Using " + tempFile);
        //        return FileToStream(tempFile, true);
        //    }
        //} 
        /// <summary>
        /// url文件转流
        /// </summary>
        /// <param name="url">网页路径</param> 
        /// <returns></returns>
        public static string HttpDownload(string url)
        {
            //打开文件
            using (var client = new WebClient())
            {
                byte[] bytes = client.DownloadData(url);//下载临时文件

                // 把 byte[] 转换成 Stream
                Stream stream = new MemoryStream(bytes);
                //解压后的流
                Stream outStream = new MemoryStream();
                //文件流解压
                GZipDeCompress(stream, outStream);
                //解压后的文件流读取数据
                var text = StreamToString(outStream, Encoding.Default);

                return text;
            }
        }
        /// <summary>
        ///  GZip解压
        /// </summary>
        /// <param name="zipStream"></param>
        /// <param name="outputStream"></param>
        public static void GZipDeCompress(Stream zipStream, Stream outputStream)
        {
            GZipInputStream gZipInputStream = new GZipInputStream(zipStream);
            try
            {
                int i = 4096;
                byte[] buffer = new byte[i];
                while (i > 0)
                {
                    i = gZipInputStream.Read(buffer, 0, i);
                    outputStream.Write(buffer, 0, i);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GZip解压缩出错:" + ex.Message);
            }
            zipStream.Close();
            gZipInputStream.Close();
        }

        //public static async Task<Stream> HttpDownloadAsync(string url)
        //{
        //    //using (var httpClient = new HttpClient())
        //    //{
        //    //    var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        //    //    response.EnsureSuccessStatusCode();

        //    //    // 获取响应流
        //    //    var stream = await response.Content.ReadAsStreamAsync();

        //    //    // 如果内容已经是gzip压缩的，则先解压缩
        //    //    if (response.Content.Headers.ContentEncoding.Contains("gzip", StringComparer.OrdinalIgnoreCase))
        //    //    {
        //    //        using (var decompressionStream = new GZipStream(stream, CompressionMode.Decompress))
        //    //        using (var decompressedStreamReader = new StreamReader(decompressionStream, Encoding.UTF8))
        //    //        {
        //    //            return await decompressedStreamReader.ReadToEndAsync();
        //    //        }
        //    //    }

        //    //    // 如果没有压缩，则直接读取
        //    //    using (var streamReader = new StreamReader(stream, Encoding.UTF8))
        //    //    {
        //    //        return await streamReader.ReadToEndAsync();
        //    //    }
        //    //} 
        //    using (var httpClient = new HttpClient())
        //    {
        //        HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead); // 异步GET请求
        //        response.EnsureSuccessStatusCode(); // 确保响应状态码表示成功

        //        Stream contentStream = await response.Content.ReadAsStreamAsync(); // 获取响应内容流

        //        if (response.Content.Headers.ContentEncoding.Contains("gzip")) // 检查是否有gzip内容编码头
        //        {
        //            // 如果是gzip压缩，则创建 GZipStream 以解压缩
        //            return new GZipStream(contentStream, CompressionMode.Decompress);
        //        }

        //        // 如果不是gzip压缩，直接返回原始流
        //        return contentStream;
        //    }
        //}
        //public static async Task<string> GetStreamPreviewAsync(Stream stream)
        //{
        //    byte[] buffer = new byte[256];
        //    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        //    // 只读取字节流的前 256 个字节来预览
        //    return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        //}



        /// <summary>
        /// 流转化成字符串
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="encode"></param>
        /// <returns>字符串</returns>
        public static string StreamToString(Stream stream, Encoding encode)
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            StreamReader streamReader = new StreamReader(stream, encode);
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            return text;
        }

        #endregion

        #region unicode转换
        /// <summary>
        /// <summary>
        /// 字符串转Unicode
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>Unicode编码后的字符串</returns>
        public static string String2Unicode(string source)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(source);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("\\u{0}{1}", bytes[i + 1].ToString("x").PadLeft(2, '0'), bytes[i].ToString("x").PadLeft(2, '0'));
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Unicode转字符串
        /// </summary>
        /// <param name="source">经过Unicode编码的字符串</param>
        /// <returns>正常字符串</returns>
        public static string Unicode2String(string source)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                source, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }
        #endregion

        static Dictionary<string, string> SponsoredTypeDic = new Dictionary<string, string>();
        public static string GetReportSponsoredTypeApi(string sponsoredType)
        {
            if (SponsoredTypeDic == null || !SponsoredTypeDic.Any())
            {
                SponsoredTypeDic = new Dictionary<string, string>() {
                    {"SPONSORED_BRANDS", "SB-Video"},
                    {"SPONSORED_PRODUCTS", "SP"},
                    {"SPONSORED_DISPLAY",  "SD"},
                };
            }
            if (SponsoredTypeDic.ContainsKey(sponsoredType)) return SponsoredTypeDic[sponsoredType];
            return "SP";
        }
        /// <summary>
        /// 示例方法，判断字符串是否为ASIN
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsAsin(string name)
        {
            var regex = new Regex(@"B[A-Z0-9]{9}");
            var match = regex.Match(name);
            // 示例逻辑，根据实际情况调整
            return match.Success;
        }
    }
}
