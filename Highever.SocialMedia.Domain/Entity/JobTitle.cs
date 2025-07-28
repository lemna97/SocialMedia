using System;
using Newtonsoft.Json.Bson;
using SqlSugar;
using Highever.SocialMedia.MongoDB;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Highever.SocialMedia.Domain.Entity;

namespace Highever.SocialMedia.Domain
{
    /// <summary>
    /// Boss 直聘职位详情
    /// </summary> 
    public class JobTitle
    {
        public JobTitle() { }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        /// <summary>
        /// 职位唯一加密 ID（主键）
        /// </summary> 
        public string EncryptJobId { get; set; } = string.Empty;

        /// <summary>
        /// 采集时间
        /// </summary> 
        public DateTime CollectedAt { get; set; } = DateTime.Now;

        /* ---------- 数字类字段 ---------- */

        /// <summary>审核 ID</summary> 
        public int? AuditId { get; set; }

        /// <summary>职位审核状态</summary> 
        public int? JobAuditStatus { get; set; }

        /// <summary>最低薪资</summary> 
        public double? LowSalary { get; set; }

        /// <summary>最高薪资</summary> 
        public double? HighSalary { get; set; }

        /// <summary>薪资类型</summary> 
        public int? SalaryType { get; set; }

        /// <summary>薪资发放月数</summary> 
        public int? SalaryMonth { get; set; }

        /// <summary>经验要求（枚举码）</summary> 
        public int? Experience { get; set; }

        /// <summary>学历要求（枚举码）</summary> 
        public int? Degree { get; set; }

        /// <summary>职位类型</summary> 
        public int? JobType { get; set; }

        /// <summary>最低招聘人数</summary> 
        public int? LowRecruitNum { get; set; }

        /// <summary>最高招聘人数</summary>
        [SugarColumn(ColumnName = "highRecruitNum", IsNullable = true)]
        public int? HighRecruitNum { get; set; }

        /// <summary>毕业年限</summary>
        [SugarColumn(ColumnName = "graduateYear", IsNullable = true)]
        public int? GraduateYear { get; set; }

        /// <summary>最低毕业年限</summary>
        [SugarColumn(ColumnName = "lowGraduateYear", IsNullable = true)]
        public int? LowGraduateYear { get; set; }

        /// <summary>兼职最低薪资</summary>
        [SugarColumn(ColumnName = "partTimeLowSalary", DecimalDigits = 2, IsNullable = true)]
        public decimal? PartTimeLowSalary { get; set; }

        /// <summary>兼职最高薪资</summary>
        [SugarColumn(ColumnName = "partTimeHighSalary", DecimalDigits = 2, IsNullable = true)]
        public decimal? PartTimeHighSalary { get; set; }

        /// <summary>兼职薪资类型</summary>
        [SugarColumn(ColumnName = "partTimeSalaryType", IsNullable = true)]
        public int? PartTimeSalaryType { get; set; }

        /// <summary>兼职开关状态</summary> 
        public int? PartTimeSwitchStatus { get; set; }

        /// <summary>最短在岗月数</summary> 
        public int? LeastMonth { get; set; }

        /// <summary>每周工作天数</summary> 
        public int? DaysPerWeek { get; set; }

        /// <summary>账户类型</summary> 
        public int? AcType { get; set; }

        /// <summary>周期类型</summary> 
        public int? PeriodType { get; set; }

        /// <summary>代理类型</summary> 
        public int? ProxyType { get; set; }

        /// <summary>是否接受海外</summary> 
        public int? AcceptOverseas { get; set; }

        /* ---------- 文本类字段 ---------- */

        /// <summary>薪资范围描述</summary> 
        public string? SalaryDesc { get; set; }

        /// <summary>职位名称</summary> 
        public string? PositionName { get; set; }

        /// <summary>职位类别</summary> 
        public string? PositionCategory { get; set; }

        /// <summary>技能标签</summary> 
        public string? SkillRequire { get; set; }

        /// <summary>职位描述</summary> 
        public string? PostDescription { get; set; }

        /// <summary>绩效描述</summary> 
        public string? Performance { get; set; }

        /// <summary>工作地点 JSON</summary> 
        public string? RelationIdJson { get; set; }

        /* ---------- 公司信息 ---------- */

        /// <summary>品牌名称</summary> 
        public string? BrandName { get; set; }

        /// <summary>公司名称</summary> 
        public string? ComName { get; set; }

        /// <summary>行业描述</summary> 
        public string? IndustryDesc { get; set; }

        /* ---------- 兼职 / 弹性 ---------- */

        /// <summary>兼职职位描述</summary> 
        public string? PartTimeJobDesc { get; set; }

        /// <summary>兼职薪资描述</summary> 
        public string? PartTimeSalaryDesc { get; set; }

        /* ---------- 日期 / 时间 ---------- */

        /// <summary>周期时间</summary> 
        public DateTime? Period { get; set; }

        /// <summary>截止日期</summary> 
        public DateTime? Deadline { get; set; }

        /// <summary>招聘截止时间</summary> 
        public DateTime? RecruitEndTime { get; set; }

        /* ---------- 其它杂项 ---------- */

        /// <summary>
        /// 工作日信息
        /// </summary> 
        public string? Workday { get; set; }

        /// <summary>匿名描述</summary> 
        public string? AnonymousDesc { get; set; }

        /// <summary>所属部门</summary> 
        public string? Department { get; set; }

        /// <summary>高亮信息</summary> 
        public string? Highlights { get; set; }

        /// <summary>补贴信息</summary> 
        public string? Subsidy { get; set; }

        /// <summary>海外地址</summary> 
        public string? OverseasAddress { get; set; }

        /// <summary>海外描述</summary> 
        public string? OverseasDetail { get; set; }

        /// <summary>添加时间字符串</summary> 
        public string? AddTime { get; set; }

        /* ---------- 元信息 ---------- */

        /// <summary>来源页面 URL</summary> 
        public string? SourcePage { get; set; }

        /// <summary>
        /// 强制初始化主键
        /// </summary>
        public JobTitle(string encryptJobId)
        {
            EncryptJobId = !string.IsNullOrWhiteSpace(encryptJobId)
                ? encryptJobId
                : throw new ArgumentException("EncryptJobId 不能为空");

            CollectedAt = DateTime.Now;
            _id = Guid.NewGuid().ToString("N");
        }
    }
}
