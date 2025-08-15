using System.ComponentModel.DataAnnotations;

namespace Highever.SocialMedia.Application.Contracts.DTOs.System
{
    /// <summary>
    /// 获取角色列表请求
    /// </summary>
    public class GetRolesRequest
    {
        /// <summary>
        /// 角色名称（模糊查询）
        /// </summary>
        public string? Name { get; set; }
         
    }

    /// <summary>
    /// 角色响应DTO
    /// </summary>
    public class RoleResponse
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 角色代码
        /// </summary>
        public string Code { get; set; }
 

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// 创建角色请求
    /// </summary>
    public class CreateRoleRequest
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        [Required(ErrorMessage = "角色名称不能为空")]
        [StringLength(50, ErrorMessage = "角色名称长度不能超过50个字符")]
        public string Name { get; set; }

        /// <summary>
        /// 角色代码
        /// </summary>
        [Required(ErrorMessage = "角色代码不能为空")]
        [StringLength(100, ErrorMessage = "角色代码长度不能超过100个字符")]
        public string Code { get; set; }
         

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 排序
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "排序值必须大于等于0")]
        public int Sort { get; set; } = 0;
    }

    /// <summary>
    /// 更新角色请求
    /// </summary>
    public class UpdateRoleRequest
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Required(ErrorMessage = "角色ID不能为空")]
        public int Id { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        [Required(ErrorMessage = "角色名称不能为空")]
        [StringLength(50, ErrorMessage = "角色名称长度不能超过50个字符")]
        public string Name { get; set; }

        /// <summary>
        /// 角色代码
        /// </summary>
        [Required(ErrorMessage = "角色代码不能为空")]
        [StringLength(100, ErrorMessage = "角色代码长度不能超过100个字符")]
        public string Code { get; set; }
         

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "排序值必须大于等于0")]
        public int Sort { get; set; }
    }

    /// <summary>
    /// 删除角色请求
    /// </summary>
    public class DeleteRoleRequest
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Required(ErrorMessage = "角色ID不能为空")]
        public int Id { get; set; }

        /// <summary>
        /// 是否强制删除（删除关联的用户角色关系）
        /// </summary>
        public bool ForceDelete { get; set; } = false;
    }

    /// <summary>
    /// 批量删除角色请求
    /// </summary>
    public class BatchDeleteRoleRequest
    {
        /// <summary>
        /// 角色ID列表
        /// </summary>
        [Required(ErrorMessage = "角色ID列表不能为空")]
        public List<int> Ids { get; set; }

        /// <summary>
        /// 是否强制删除
        /// </summary>
        public bool ForceDelete { get; set; } = false;
    }
}