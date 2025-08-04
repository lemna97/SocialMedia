using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{

    [SugarTable("tasks")]
    public class TaskEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }
        /// <summary>
        /// 任务名称（对应TaskService中的Job结尾的文件）
        /// </summary>

        [SugarColumn(ColumnName = "task_name", Length = 100, IsNullable = false)]
        public string TaskName { get; set; }
        /// <summary>
        /// 描述
        /// </summary>

        [SugarColumn(ColumnName = "description", Length = 255)]
        public string Description { get; set; }
        /// <summary>
        /// Cron 表达式
        /// </summary>

        [SugarColumn(ColumnName = "cron_expression", Length = 100)]
        public string CronExpression { get; set; } = "0 30 9 * * *";

        /// <summary>
        /// IP
        /// </summary>

        [SugarColumn(ColumnName = "created_ip", Length = 100)]
        public string? CreatedIP { get; set; }
        /// <summary>
        /// 任务状态（1：启用，0：禁用）
        /// </summary>

        [SugarColumn(ColumnName = "status", DefaultValue = "1")]
        public int Status { get; set; }
        /// <summary>
        /// 当前时间
        /// </summary>

        [SugarColumn(ColumnName = "created_at", IsOnlyIgnoreInsert = true)]
        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [SugarColumn(ColumnName = "updated_at", IsOnlyIgnoreInsert = true)]
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }

}
