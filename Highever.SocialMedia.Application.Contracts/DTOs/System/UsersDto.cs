using System.ComponentModel.DataAnnotations;

namespace Highever.SocialMedia.Application.Contracts.DTOs.System
{
    /// <summary>
    /// 获取用户列表请求
    /// </summary>
    public class GetUsersRequest
    {
        /// <summary>
        /// 关键词（用户名或昵称，可为null或空字符串）
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int? PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int? PageSize { get; set; } = 20;

        /// <summary>
        /// 用户名（模糊查询）
        /// </summary>
        public string? Username { get; set; } 

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        public int? RoleId { get; set; }
    }

    /// <summary>
    /// 用户响应DTO
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Account { get; set; }
          
        /// <summary>
        /// 昵称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 用户角色列表
        /// </summary>
        public List<RoleResponse> Roles { get; set; } = new List<RoleResponse>();
    }

    /// <summary>
    /// 创建用户请求
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100个字符之间")]
        public string Password { get; set; } 

        /// <summary>
        /// 昵称
        /// </summary>
        [StringLength(50, ErrorMessage = "昵称长度不能超过50个字符")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [StringLength(200, ErrorMessage = "头像URL长度不能超过200个字符")]
        public string? Avatar { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 角色ID列表
        /// </summary>
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// 更新用户请求
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required(ErrorMessage = "用户ID不能为空")]
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
        public string Account { get; set; }
         
        /// <summary>
        /// 昵称
        /// </summary>
        [StringLength(50, ErrorMessage = "昵称长度不能超过50个字符")]
        public string? DisplayName { get; set; } 

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 角色ID列表
        /// </summary>
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// 删除用户请求
    /// </summary>
    public class DeleteUserRequest
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required(ErrorMessage = "用户ID不能为空")]
        public int Id { get; set; }
    }

    /// <summary>
    /// 批量删除用户请求
    /// </summary>
    public class BatchDeleteUserRequest
    {
        /// <summary>
        /// 用户ID列表
        /// </summary>
        [Required(ErrorMessage = "用户ID列表不能为空")]
        public List<int> Ids { get; set; }
    }

    /// <summary>
    /// 修改密码请求
    /// </summary>
    public class ChangePasswordRequest
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required(ErrorMessage = "用户ID不能为空")]
        public int Id { get; set; }

        /// <summary>
        /// 新密码
        /// </summary>
        [Required(ErrorMessage = "新密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100个字符之间")]
        public string NewPassword { get; set; }
    }
}
