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
        public List<T> Items { get; set; } // 当前页数据
        
        // 新增属性
        public int PageIndex { get; set; } // 当前页码
        public int PageSize { get; set; } // 每页大小
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0; // 总页数（计算属性）
        
        // 便利属性
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
