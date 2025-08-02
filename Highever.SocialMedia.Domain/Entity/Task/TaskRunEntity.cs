using SqlSugar;

namespace Highever.SocialMedia.Domain.Entity
{

    [SugarTable("task_runs")]
    public class TaskRunEntity
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public long Id { get; set; }

        [SugarColumn(ColumnName = "task_id", IsNullable = false)]
        public long TaskId { get; set; }  // 关联任务ID

        [SugarColumn(ColumnName = "status", DefaultValue = "0")]
        public int Status { get; set; }  // 任务执行状态（0：失败，1：成功）

        [SugarColumn(ColumnName = "start_time", IsOnlyIgnoreInsert = true)]
        public DateTime StartTime { get; set; }  // 任务开始时间

        [SugarColumn(ColumnName = "end_time", IsOnlyIgnoreInsert = true)]
        public DateTime EndTime { get; set; }  // 任务结束时间

        [SugarColumn(ColumnName = "error_message")]
        public string ErrorMessage { get; set; }  // 错误信息（任务失败时）
         
    }

}
