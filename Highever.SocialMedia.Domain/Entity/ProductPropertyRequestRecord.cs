using SqlSugar;
using System;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// 商品属性处理请求表
    /// </summary>
    [SugarTable("product_property_request")]
    public class ProductPropertyRequestRecord
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [SugarColumn(ColumnName = "id", IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 请求唯一标识
        /// </summary>
        [SugarColumn(ColumnName = "request_id", Length = 255, IsNullable = false)]
        public string RequestId { get; set; }

        /// <summary>
        /// 商品ID
        /// </summary>
        [SugarColumn(ColumnName = "product_id", IsNullable = false)]
        public long ProductId { get; set; }

        /// <summary>
        /// Mallid
        /// </summary>
        [SugarColumn(ColumnName = "mallid", IsNullable = false)]
        public long Mallid { get; set; }

        /// <summary>
        /// 编辑场景
        /// </summary>
        [SugarColumn(ColumnName = "edit_scene", IsNullable = false)]
        public int EditScene { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(ColumnName = "create_date", IsNullable = false)]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 商品标题
        /// </summary>
        [SugarColumn(ColumnName = "product_name", Length = 255, IsNullable = false)]
        public string ProductName { get; set; }

        /// <summary>
        /// 商品的问题（JSON字符串）
        /// </summary>
        [SugarColumn(ColumnName = "product_questions", IsJson = true, IsNullable = true)]
        public string ProductQuestions { get; set; }
        /// <summary>
        /// 构建的提问文本
        /// </summary>
        [SugarColumn(ColumnName = "gpt_textContent", IsNullable = true)]
        public string TextContent { get; set; }
        /// <summary>
        /// 商品图片（Base64或URL）
        /// </summary>
        [SugarColumn(ColumnName = "product_picture", IsNullable = true)]
        public string ProductPicture { get; set; }

        /// <summary>
        /// 商品属性结果（JSON字符串）
        /// </summary>
        [SugarColumn(ColumnName = "product_properties", IsJson = true, IsNullable = true)]
        public string ProductProperties { get; set; }

        /// <summary>
        /// GPT返回的内容（JSON字符串）
        /// </summary>
        [SugarColumn(ColumnName = "gpt_responses", IsJson = true, IsNullable = true)]
        public string GptResponses { get; set; }

        /// <summary>
        /// 属性是否补充成功
        /// </summary>
        [SugarColumn(ColumnName = "isSuccess", IsNullable = false)]
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 返回的消息
        /// </summary>
        [SugarColumn(ColumnName = "resultText", IsNullable = false)]
        public string ResultText { get; set; }
    }
}
