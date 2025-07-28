using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// 对应MongoDB的CSVTest集合
    /// </summary>
    public class CSVTest
    {
        /// <summary>
        /// MongoDB文档主键
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// 匹配分数，如 "70%"
        /// </summary>
        public string? Match { get; set; }

        /// <summary>
        /// 候选人姓名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 能力评分或优势描述
        /// </summary>
        public string? Score { get; set; }

        /// <summary>
        /// 扣分项或不足描述
        /// </summary>
        public string? Deducte { get; set; }

        /// <summary>
        /// 详细评价/说明
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// 图片，支持多张（通常存储图片URL数组）
        /// </summary>
        public List<string>? Image { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string? UserId { get; set; }
    }
}
