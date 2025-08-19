using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 产品信息实体，对应Excel每一行数据
    /// </summary>
    public class ProductInfo
    {
        /// <summary>编号</summary>
        public string SKU { get; set; }

        /// <summary>产品图片</summary>
        public string UPC { get; set; }

        /// <summary>ASIN</summary>
        public string ASIN { get; set; } 
    }


}
