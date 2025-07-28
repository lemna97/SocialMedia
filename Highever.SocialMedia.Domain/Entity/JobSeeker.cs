using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Highever.SocialMedia.Domain.Entity
{
    /// <summary>
    /// Boss直聘求职人详情
    /// </summary>
    public class JobSeeker
    {
        public JobSeeker() { }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        /// <summary>用户唯一加密ID（主键）</summary>
        public string EncryptGeekId { get; set; } = string.Empty;

        /// <summary>数据采集时间</summary>
        public DateTime CollectedAt { get; set; } = DateTime.Now;

        /* ---------- 基本信息 ---------- */

        /// <summary>牛人ID</summary>
        public long GeekId { get; set; }

        /// <summary>姓名</summary>
        public string GeekName { get; set; }

        /// <summary>头像URL</summary>
        public string GeekAvatar { get; set; }

        /// <summary>性别：1-男 2-女 0-未知</summary>
        public int GeekGender { get; set; }

        /// <summary>出生日期（yyyymmdd）</summary>
        public string Birthday { get; set; }

        /// <summary>年龄描述，如"24岁"</summary>
        public string AgeDesc { get; set; }

        /* ---------- 教育经历 ---------- */

        /// <summary>最高学历毕业院校</summary>
        public string School { get; set; }

        /// <summary>专业</summary>
        public string Major { get; set; }

        /// <summary>学历名称，如“本科”</summary>
        public string DegreeName { get; set; }

        /// <summary>入学时间（yyyy.MM）</summary>
        public string EduStartDate { get; set; }

        /// <summary>毕业时间（yyyy.MM）</summary>
        public string EduEndDate { get; set; }

        /* ---------- 工作经历 ---------- */

        /// <summary>工作经历列表</summary>
        public List<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();

        /* ---------- 求职意向 ---------- */

        /// <summary>期望工作城市（中文）</summary>
        public string ExpectLocationName { get; set; }

        /// <summary>期望岗位（中文）</summary>
        public string ExpectPositionName { get; set; }

        /// <summary>期望薪资区间文本，如“15-20K”</summary>
        public string Salary { get; set; }

        /// <summary>到岗/求职状态描述</summary>
        public string ApplyStatusDesc { get; set; }

        /// <summary>求职意向补充信息数组</summary>
        public string[] ExpectInfos { get; set; }

        /* ---------- 个性与亮点 ---------- */

        /// <summary>个人简介</summary>
        public string Description { get; set; }

        /// <summary>亮点标签列表</summary>
        public List<string> Matches { get; set; }

        /* ---------- 活跃度 ---------- */

        /// <summary>最近活跃时间描述</summary>
        public string ActiveTimeDesc { get; set; }

        /* ---------- GPT分析结果与分数 ---------- */

        /// <summary>GPT智能分析文本</summary> 
        public string GptText { get; set; }

        /// <summary>GPT分析得分（如“80%”）</summary> 
        public int Score { get; set; }


        /* ======================== 新增字段（2025-07-11） ======================== */

        /// <summary>
        /// 发起打招呼时所选的 **职位 EncryptId**。  
        /// 便于排查「深圳职位却招呼到成都候选人」等问题。
        /// </summary> 
        public string? JobId { get; set; }

        /// <summary>
        /// 列表检索时的筛选条件键值对，前端直接透传。  
        /// 示例：{ "cityCode":"101280600", "age":"16,-1", … }
        /// </summary> 
        public string SearchFilters { get; set; }

        /// <summary>
        /// 列表接口（Geek/List 或 NewList）的查询字符串原文，便于重放。
        /// </summary> 
        public string? ListRequest { get; set; }

        /// <summary>
        /// 打招呼接口 <c>/chat/start</c> 的 payload（form-data → Dictionary）。  
        /// 用于追溯 jid/gid/expectId 等真实发送参数。
        /// </summary> 
        public string GreetRequest { get; set; }

        /* ====================================================================== */

        /// <summary>
        /// 强制初始化主键
        /// </summary>
        public JobSeeker(string encryptGeekId)
        {
            EncryptGeekId = !string.IsNullOrWhiteSpace(encryptGeekId)
                ? encryptGeekId
                : throw new ArgumentException("EncryptGeekId 不能为空");

            CollectedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// 工作经历
    /// </summary>
    public class WorkExperience
    {
        /// <summary>公司名称</summary>
        public string Company { get; set; }

        /// <summary>职位名称</summary>
        public string PositionName { get; set; }

        /// <summary>开始时间（yyyy.MM）</summary>
        public string StartDate { get; set; }

        /// <summary>结束时间（yyyy.MM），至今为空</summary>
        public string EndDate { get; set; }

        /// <summary>是否在职</summary>
        public bool StillWork { get; set; }
    }

}
