using System.ComponentModel.DataAnnotations;

namespace Highever.SocialMedia.API.Areas.TikTok.DTO
{
    /// <summary>
    /// 账户配置查询请求
    /// </summary>
    public class AccountConfigQueryRequest
    {
        /// <summary>
        /// 关键词（UniqueId搜索）
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 是否只查询启用状态
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string? OrderBy { get; set; } = "CreatedAt";

        /// <summary>
        /// 是否升序
        /// </summary>
        public bool Ascending { get; set; } = false;
    }

    /// <summary>
    /// 创建账户配置请求
    /// </summary>
    public class CreateAccountConfigRequest
    {
        /// <summary>
        /// 用户名(用户自定义的唯一标识符)
        /// </summary>
        [Required(ErrorMessage = "UniqueId不能为空")]
        [StringLength(100, ErrorMessage = "UniqueId长度不能超过100个字符")]
        public string UniqueId { get; set; }

        /// <summary>
        /// 安全用户ID(TikTok内部安全标识)
        /// </summary>
        [StringLength(200, ErrorMessage = "SecUid长度不能超过200个字符")]
        public string? SecUid { get; set; }

        /// <summary>
        /// 系统用户ID(数据权限关联)
        /// 
        /// </summary>  
        public int? SystemUid { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 更新账户配置请求
    /// </summary>
    public class UpdateAccountConfigRequest
    {
        /// <summary>
        /// 配置ID
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "Id必须大于0")]
        public int Id { get; set; }

        /// <summary>
        /// 用户名(用户自定义的唯一标识符)
        /// </summary>
        [Required(ErrorMessage = "UniqueId不能为空")]
        [StringLength(100, ErrorMessage = "UniqueId长度不能超过100个字符")]
        public string UniqueId { get; set; }

        /// <summary>
        /// 安全用户ID(TikTok内部安全标识)
        /// </summary>
        [StringLength(200, ErrorMessage = "SecUid长度不能超过200个字符")]
        public string? SecUid { get; set; }

        /// <summary>
        /// 系统用户ID(数据权限关联)
        /// </summary> 
        public int? SystemUid { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 删除账户配置请求
    /// </summary>
    public class DeleteAccountConfigRequest
    {
        /// <summary>
        /// 配置ID
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "Id必须大于0")]
        public int Id { get; set; }
    }

    /// <summary>
    /// 更改账户配置状态请求
    /// </summary>
    public class ChangeAccountConfigStatusRequest
    {
        /// <summary>
        /// 配置ID
        /// </summary>
        [Required(ErrorMessage = "Id不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "Id必须大于0")]
        public int Id { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 账户配置响应DTO
    /// </summary>
    public class AccountConfigResponse
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// 安全用户ID
        /// </summary>
        public string? SecUid { get; set; }

        /// <summary>
        /// 系统用户ID
        /// </summary>
        public int? SystemUid { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}