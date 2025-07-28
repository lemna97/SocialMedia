using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Highever.SocialMedia.Common
{
    public static class StringExtension
    {
        #region string[]
        /// <summary>
        /// 删除数组中的重复项
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string[] RemoveDup(this string[] values)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < values.Length; i++)//遍历数组成员
            {
                if (!list.Contains(values[i]))
                {
                    list.Add(values[i]);
                };
            }
            return list.ToArray();
        }
        #endregion

        #region String
        /// <summary>
        /// 自动生成编号 例： 201008251145409865
        /// </summary>
        /// <returns></returns>
        public static string NewNo()
        {
            Random random = new Random();
            string strRandom = random.Next(1000, 10000).ToString(); //生成编号 
            string code = DateTime.Now.ToString("yyyyMMddHHmmss") + strRandom;//形如
            return code;
        } 
        /// <summary>
        /// 获得Guid，不包含-；例：ece4f4a60b764339b94a07c84e338a27
        /// </summary> 
        /// <returns></returns>
        public static string NewGuid()
        {
            string pasCode = Guid.NewGuid().ToString("N"); 
            return pasCode;
        }
        #endregion

        /// <summary>
        ///  
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            } 
            return true;
        }
        public static bool IsNullOrEmpty(this object value)
        {
            if (value==null)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        ///  
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// string转化为int, false:返回 0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int TryGetInt(this string value)
        {
            int n = 0;
            int result = 0;
            if (int.TryParse(value, out n) && n <= Int32.MaxValue)
            {
                result = Convert.ToInt32(n);
            }
            return result;
        }
        /// <summary>
        /// 获取int类型 返回object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object? TryGetIntObject(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                int n = 0;
                int result = 0;
                if (int.TryParse(value, out n) && n <= Int32.MaxValue)
                {
                    result = Convert.ToInt32(n);
                }
                return result;
            }
            else
            {
                return null;
            }
        }


        public static short TryGetShort(this string value)
        {
            short n = 0;
            short result = 0;
            if (short.TryParse(value, out n) && n <= Int16.MaxValue)
            {
                result = Convert.ToInt16(n);
            }
            return result;
        }

        /// <summary>
        /// 获取short类型返回object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object? TryGetShortObject(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                short n = 0;
                short result = 0;
                if (short.TryParse(value, out n) && n <= Int16.MaxValue)
                {
                    result = Convert.ToInt16(n);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        public static byte TryGetByte(this string value)
        {
            byte result = 0;
            byte.TryParse(value, out result);
            return result;
        }

        public static bool TryGetBool(this string value)
        {
            bool result = false;
            if (value == "1") return true;
            bool.TryParse(value, out result);
            return result;
        }

        /// <summary>
        /// 转换布尔
        /// </summary>
        /// <param name="value">判断的值</param>
        /// <param name="defaultValue">默认值</param>
        /// <remarks>侯湘岳</remarks>
        /// <returns></returns>
        public static bool TryGetBool(this string value, bool defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            value = value.ToUpper();
            return (value == "YES" || value == "TRUE" || value == "1");
        }

        public static long TryGetLong(this string value)
        {
            long n = 0;
            long result = 0;
            if (long.TryParse(value, out n))
            {
                result = Convert.ToInt64(n);
            }
            return result;
        }

        public static object? TryGetLongObject(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                long n = 0;
                long result = 0;
                if (long.TryParse(value, out n))
                {
                    result = Convert.ToInt64(n);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        public static float TryGetFloat(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace(",", "");
            }

            float result = 0;
            float.TryParse(value, out result);
            return result;
        }

        /// <summary>
        /// 获取Float类型返回Object类型
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object? TryGetFloatObject(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace(",", "");
                float result = 0;
                float.TryParse(value, out result);
                return result;
            }
            else
            {
                return null;
            }
        }

        public static double TryGetDouble(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace(",", "");
            }
            double result = 0;
            double.TryParse(value, out result);
            return result;
        }
        /// <summary>
        /// 获取double类型返回object类型
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object? TryGetDoubleObject(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace(",", "");
                double result = 0;
                double.TryParse(value, out result);
                return result;
            }
            else
            {
                return null;
            }
        }

        public static DateTime TryGetDateTime(this string value)
        {
            DateTime result = default(DateTime);
            DateTime.TryParse(value, out result);
            return result;
        }
        /// <summary>
        /// 将时间转换成字符串
        /// </summary>
        /// <param name="date"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string TryGetDateTimeFormat(this DateTime? date, string format)
        {
            if (date == null || date == default(DateTime) || date <= DateTime.MinValue)
            {
                return "";
            }
            return Convert.ToDateTime(date).ToString(format);
        }

        /// <summary>
        /// 保留小数位数
        /// byte
        /// </summary>
        /// <param name="d">数值</param>
        /// <param name="decimals">保留几位小数位数</param>
        /// <param name="roundOff">是否四舍五入</param>
        /// <returns></returns>
        public static string RoundCorrect(this double d, int decimals, bool roundOff)
        {
            double multiplier = Math.Pow(10, decimals), result;

            if (d < 0)
                multiplier *= -1;
            if (roundOff)
                result = Math.Floor((d * multiplier) + 0.51) / multiplier;
            else
                result = Math.Floor((d * multiplier)) / multiplier;
            return result.ToString("f" + decimals.ToString());
        }

        public class DateTimeFormat
        {
            public static string yyyyMMdd = "yyyyMMdd";
            public static string yyyy_MM_dd = "yyyy-MM-dd";
            public static string yyyyMM = "yyyyMM";
            public static string yyyy_MM_dd_hh_mm_ss = "yyyy-MM-dd HH:mm:ss";
        }

        /// <summary>
        /// 将datetime转化为string,格式为：yyyy-MM-dd
        /// </summary>
        public static string TryGetDateTimeFormat(this object value, DateTime dt)
        {
            string format = DateTimeFormat.yyyy_MM_dd;
            return TryGetDateTimeFormat(value, format, dt);
        }
        /// <summary>
        /// 获取当前天的最后时间
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DateTime GetCurDayLastTime(this DateTime dt)
        {
            string str = " 23:59:59";
            str = dt.ToString("yyyy-MM-dd") + str;
            return Convert.ToDateTime(str);
        }
        /// <summary>
        /// 将datetime转化为string可以自定义格,格式类：DateTimeFormat
        /// </summary>
        public static string TryGetDateTimeFormat(this object value, string format, DateTime dt)
        {
            string result;
            if (value == null || Convert.ToString(value) == "")
            {
                result = dt.ToString(format);
            }
            else
            {
                try
                {
                    result = Convert.ToDateTime(value).ToString(format);
                }
                catch { result = ""; }
            }
            return result;
        }

        public static decimal TryGetDecimal(this string value)
        {
            decimal result = default(decimal);
            decimal.TryParse(value, out result);
            return result;
        }
        /// <summary>
        /// 获取Decimal类型 
        /// </summary>
        /// <param name="value">输入值</param>
        /// <returns>如果没有值返回null</returns>
        public static object? TryGetDecimalObject(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            else
            {
                decimal result = default(decimal);
                decimal.TryParse(value, out result);
                return result;
            }
        }

        /// <summary>
        /// 获得第几周：0-当前日期是第几周。n-相隔N周是第几周
        /// </summary>
        public static string TryGetFewWeeks(this object value, DateTime dt, int apart)
        {
            string result =string.Empty;
            if (value == null)
            {
                DateTime time = dt.AddDays(-(apart * 7));
                string year = time.Year.ToString();
                int weeknow = Convert.ToInt32(time.DayOfWeek);//今天星期几
                int daydiff = (-1) * (weeknow + 1);//今日与上周末的天数差
                int days = time.AddDays(daydiff).DayOfYear;//上周末是本年第几天
                int weeks = days / 7;
                if (days % 7 != 0)
                {
                    weeks++;
                }
                string week = Convert.ToString(weeks + 1);
                if (week.Length == 1)
                {
                    week = "0" + week;
                }
                result = year + week;
            }
            else
            {
                result = value.ToString();
            }
            return result;
        }

        /// <summary>
        /// 获得处于第几季：0-上季是第几季。n-相隔N季
        /// </summary>
        public static string TryGetFewQuarter(this object value, DateTime dt)
        {
            string result;
            if (value == null)
            {
                string quarter = "";
                int spare = Convert.ToInt32(dt.Month) % 3;
                int divide = Convert.ToInt32(dt.Month) / 3;
                if ((spare != 0 && divide == 0) || (spare == 0 && divide == 1))
                {
                    quarter = dt.AddYears(-1).Year + "04";
                }
                else if (spare == 0 && divide > 1)
                {
                    quarter = dt.Year + "0" + Convert.ToString(divide - 1);
                }
                else if (spare != 0 && divide > 1)
                {
                    quarter = dt.Year + "0" + Convert.ToString(divide);
                }
                result = quarter;
            }
            else
            {
                result = value.ToString();
            }

            return result;
        }

        /// <summary>
        /// 获取转码的文字信息
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string TryGetDecodeStr(this object o)
        {
            if (o == null)
            {
                return "";
            }
            else
            {
                return HttpUtility.UrlDecode(o.ToString());
            }
        }
        /// <summary>
        /// 进行MD5效验
        /// </summary>
        /// <param name="strmd5"></param>
        /// <returns></returns>
        public static string GetMd5(this string strmd5)
        {
            byte[] md5Bytes = ASCIIEncoding.Default.GetBytes(strmd5);
            byte[] encodedBytes;
            MD5 md5;
            md5 = new MD5CryptoServiceProvider();
            //FileStream fs= new FileStream(filepath,FileMode.Open,FileAccess.Read);
            encodedBytes = md5.ComputeHash(md5Bytes);
            string nn = BitConverter.ToString(encodedBytes);
            nn = Regex.Replace(nn, "-", "");//因为转化完的都是34-2d这样的，所以替换掉- 
            nn = nn.ToLower();//根据需要转化成小写
            //fs.Close();
            return nn;
        }
        /// <summary>
        /// Unicode编码(汉字转换为\uxxx)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EnUnicode(this string str)
        {
            StringBuilder strResult = new StringBuilder();
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 0; i < str.Length; i++)
                {
                    strResult.Append("\\u");
                    strResult.Append(((int)str[i]).ToString("x"));
                }
            }
            return strResult.ToString();
        }

        /// <summary>
        /// Unicode解码（\uxxxx转换为汉字）
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DeUnicode(this string str)
        {
            //最直接的方法Regex.Unescape(str);
            Regex reg = new Regex(@"(?i)\\[uU]([0-9a-f]{4})");
            return reg.Replace(str, delegate (Match m) { return ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString(); });
        }
        /// <summary>
        /// 递归 20201117 witt
        /// </summary>
        /// <typeparam name="T">入参对象</typeparam>
        /// <typeparam name="TResult">返回对象</typeparam>
        /// <param name="f"></param>
        /// <returns></returns>
        public static Func<T, TResult> Fix<T, TResult>(Func<Func<T, TResult>, Func<T, TResult>> f)
        {
            return x => f(Fix(f))(x);
        }
        ///编码
        public static string EncodeBase64(this string code_type, string code)
        {
            string encode = "";
            byte[] bytes = Encoding.GetEncoding(code_type).GetBytes(code);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = code;
            }
            return encode;
        }
        ///解码
        public static string DecodeBase64(this string code_type, string code)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(code);
            try
            {
                decode = Encoding.GetEncoding(code_type).GetString(bytes);
            }
            catch
            {
                decode = code;
            }
            return decode;
        } 
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp(DateTime? dateTime = null, string type = "ms")
        {
            dateTime = dateTime == null ? DateTime.Now : dateTime;
            TimeSpan ts = Convert.ToDateTime(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            if (type == "s")
                return (long)ts.TotalSeconds;
            else
                return (long)ts.TotalMilliseconds; //精确到毫秒
        }
        /// <summary>
        /// 转换时间戳为C#时间
        /// </summary>
        /// <param name="timeStamp">时间戳 单位：毫秒</param>
        /// <returns>C#时间</returns>
        public static DateTime ConvertTimeStampToDateTime(long timeStamp, string type = "ms")
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            if (type == "s")
                return start.AddSeconds(timeStamp);
            else
                return start.AddMilliseconds(timeStamp);
        }
        /// <summary>
        /// 获取6位数数字验证码
        /// </summary>
        /// <returns></returns>
        public static string GetRandomNum()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        /// <summary>
        /// 获取指定时间内的随机时间redis缓存过期 (秒) 默认：每晚1点到5点之间随机
        /// </summary>
        /// <returns></returns>
        public static int GetRedisExpirationTime(DateTime? minTime = null, DateTime? maxTime = null)
        {
            if (minTime == null)
                minTime = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + " 1:00:00");
            if (maxTime == null)
                maxTime = Convert.ToDateTime(DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") + " 5:00:00");
            DateTime dt = GetRandomTime(Convert.ToDateTime(minTime), Convert.ToDateTime(maxTime));
            return (int)new TimeSpan(dt.Ticks - DateTime.Now.Ticks).TotalSeconds;
        }
        public static DateTime GetRandomTime(DateTime minTime, DateTime maxTime)
        {
            Random random = new Random();
            TimeSpan ts = new TimeSpan(maxTime.Ticks - minTime.Ticks);
            int dTotalSecontds = (int)ts.TotalSeconds;
            int i = random.Next(System.Math.Abs(dTotalSecontds));
            return minTime.AddSeconds(i);
        }
        /// <summary>
        /// 产生随机字符串，用于客户端随机命名
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GetRndString(int len)
        {
            string s = Guid.NewGuid().ToString().Replace("-", "");
            return s.Substring(0, len > s.Length ? s.Length : len);
        }

        /// <summary>
        /// url解码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlDecode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return HttpUtility.UrlDecode(str).Replace("%20", "+");
        }
        /// <summary>
        /// url编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlEncode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return HttpUtility.UrlEncode(str).Replace("+", "%20");
        }
    }
}
