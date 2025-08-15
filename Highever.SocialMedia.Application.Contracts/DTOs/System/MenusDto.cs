using System.ComponentModel.DataAnnotations;

namespace Highever.SocialMedia.Application.Contracts.DTOs.System
{
    /// <summary>
    /// 获取菜单列表请求
    /// </summary>
    public class GetMenusRequest
    {
        /// <summary>
        /// 角色ID（可选，不传则查询所有菜单）
        /// </summary>
        public int? RoleId { get; set; }
    }

    /// <summary>
    /// 菜单响应DTO
    /// </summary>
    public class MenuResponse
    {
        /// <summary>
        /// 菜单ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 父级菜单ID
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// 菜单名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 菜单代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 菜单URL
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// 菜单图标
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 关联的角色ID（如果查询时指定了角色ID）
        /// </summary>
        public int? RoleId { get; set; }

        /// <summary>
        /// 是否已分配给该角色
        /// </summary>
        public bool IsAssigned { get; set; }

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
    /// 新增菜单请求
    /// </summary>
    public class CreateMenuRequest
    {
        /// <summary>
        /// 父级菜单ID
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// 菜单名称
        /// </summary>
        [Required(ErrorMessage = "菜单名称不能为空")]
        [StringLength(50, ErrorMessage = "菜单名称长度不能超过50个字符")]
        public string Name { get; set; }

        /// <summary>
        /// 菜单代码
        /// </summary>
        [Required(ErrorMessage = "菜单代码不能为空")]
        [StringLength(100, ErrorMessage = "菜单代码长度不能超过100个字符")]
        public string Code { get; set; }

        /// <summary>
        /// 菜单URL
        /// </summary>
        [StringLength(200, ErrorMessage = "菜单URL长度不能超过200个字符")]
        public string? Url { get; set; }

        /// <summary>
        /// 菜单图标
        /// </summary>
        [StringLength(50, ErrorMessage = "菜单图标长度不能超过50个字符")]
        public string? Icon { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "排序值必须大于等于0")]
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 修改菜单请求
    /// </summary>
    public class UpdateMenuRequest
    {
        /// <summary>
        /// 菜单ID
        /// </summary>
        [Required(ErrorMessage = "菜单ID不能为空")]
        public int Id { get; set; }

        /// <summary>
        /// 父级菜单ID
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// 菜单名称
        /// </summary>
        [Required(ErrorMessage = "菜单名称不能为空")]
        [StringLength(50, ErrorMessage = "菜单名称长度不能超过50个字符")]
        public string Name { get; set; }

        /// <summary>
        /// 菜单代码
        /// </summary>
        [Required(ErrorMessage = "菜单代码不能为空")]
        [StringLength(100, ErrorMessage = "菜单代码长度不能超过100个字符")]
        public string Code { get; set; }

        /// <summary>
        /// 菜单URL
        /// </summary>
        [StringLength(200, ErrorMessage = "菜单URL长度不能超过200个字符")]
        public string? Url { get; set; }

        /// <summary>
        /// 菜单图标
        /// </summary>
        [StringLength(50, ErrorMessage = "菜单图标长度不能超过50个字符")]
        public string? Icon { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "排序值必须大于等于0")]
        public int Sort { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// 删除菜单请求
    /// </summary>
    public class DeleteMenuRequest
    {
        /// <summary>
        /// 菜单ID
        /// </summary>
        [Required(ErrorMessage = "菜单ID不能为空")]
        public int Id { get; set; }

        /// <summary>
        /// 是否强制删除（删除子菜单）
        /// </summary>
        public bool ForceDelete { get; set; } = false;
    }

    /// <summary>
    /// 批量删除菜单请求
    /// </summary>
    public class BatchDeleteMenuRequest
    {
        /// <summary>
        /// 菜单ID列表
        /// </summary>
        [Required(ErrorMessage = "菜单ID列表不能为空")]
        public List<int> Ids { get; set; }

        /// <summary>
        /// 是否强制删除（删除子菜单）
        /// </summary>
        public bool ForceDelete { get; set; } = false;
    }
}
