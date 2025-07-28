using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Common
{
    /// <summary>
    /// 上传响应模型
    /// </summary>
    public class UploadResult
    {
        /// <summary>
        /// code
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        public string Msg { get; set; }
        /// <summary> 
        /// 图片地址
        /// </summary>
        public string data { get; set; }
        /// <summary>
        /// 图片名称
        /// </summary>
        public string imgname { get; set; }
    }

}
