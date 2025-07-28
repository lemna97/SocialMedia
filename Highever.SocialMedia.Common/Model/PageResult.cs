using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Common
{
    public class PageResult<T>
    {
        public int Total { get; set; } // 总记录数
        /// <summary>
        /// 
        /// </summary>
        public List<T> Items { get; set; } // 当前页数据
    }
}
