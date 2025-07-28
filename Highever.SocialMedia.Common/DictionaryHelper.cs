using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 
    /// </summary>
    public static class DictionaryHelper
    {
        /// <summary>
        /// 根据指定值从字典中批量删除键值对。
        /// </summary>
        /// <typeparam name="TKey">字典键的类型</typeparam>
        /// <typeparam name="TValue">字典值的类型</typeparam>
        /// <param name="dictionary">需要操作的字典</param>
        /// <param name="valueToMatch">用于匹配的值</param>
        public static ConcurrentDictionary<TKey, TValue> RemoveKeysByValue<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dictionary, TValue valueToMatch)
        {
            var result = dictionary;
            // 获取符合条件的键列表
            var tempKeys = result.Where(t => t.Value.Equals(valueToMatch))?.Select(t => t.Key).ToList();

            if (tempKeys != null)
            {
                // 移除这些键
                foreach (var key in tempKeys)
                {
                    result.TryRemove(key, out _);
                }
            }
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>

        public static ( double, double) EstimateMemoryUsage<T>(List<T> list)
        {
            // 进行垃圾回收以确保我们得到了当前已分配的内存
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // 获取初始内存使用
            long initialMemory = GC.GetTotalMemory(true);

            // 创建副本以便测量内存增量
            var temp = new List<T>(list);

            // 获取新的内存使用
            long finalMemory = GC.GetTotalMemory(true);

            // 返回差值（字节）
            var tempNumber = finalMemory - initialMemory;
             
            double memoryMB = tempNumber / (1024.0 * 1024.0);
            double memoryGB = tempNumber / (1024.0 * 1024.0 * 1024.0);

            return (memoryMB,memoryGB);
        }

    }
}
