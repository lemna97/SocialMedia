using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// 每次GPT请求的记录
    /// </summary>
    [SugarTable("product_chatgpt_prompt")]
    public class ProductChatGptPrompt
    {
        /// <summary>
        /// 上下文ID
        /// </summary>
        [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 提示ID
        /// </summary>
        [SugarColumn(ColumnName = "promptId", Length = 255, IsNullable = false)]
        public string PromptId { get; set; }

        /// <summary>
        /// 提问消息
        /// </summary>
        [SugarColumn(ColumnName = "chatMessage", IsJson = true, IsNullable = true)]
        public string ChatMessage { get; set; }

        /// <summary>
        /// 返回的消息
        /// </summary>
        [SugarColumn(ColumnName = "choicesMessage", IsNullable = true)]
        public string ChoicesMessage { get; set; }

        /// <summary>
        /// 产品ID
        /// </summary>
        [SugarColumn(ColumnName = "productId", IsNullable = false)]
        public int ProductId { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [SugarColumn(ColumnName = "status", IsNullable = true)]
        public int Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "createDate", IsNullable = false)]
        public DateTime CreateDate { get; set; }
    }
}
