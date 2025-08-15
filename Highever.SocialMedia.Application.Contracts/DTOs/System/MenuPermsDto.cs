using System.ComponentModel.DataAnnotations;

namespace Highever.SocialMedia.Application.Contracts.DTOs.System
{
    /// <summary>
    /// 角色菜单分配请求
    /// </summary>
    public class AssignMenusToRoleRequest
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Required(ErrorMessage = "角色ID不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "角色ID必须大于0")]
        public int RoleId { get; set; }

        /// <summary>
        /// 菜单ID列表
        /// </summary>
        [Required(ErrorMessage = "菜单ID列表不能为空")]
        public List<int> MenuIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// 删除角色菜单关联请求
    /// </summary>
    public class DeleteMenuPermRequest
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Required(ErrorMessage = "角色ID不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "角色ID必须大于0")]
        public int RoleId { get; set; }

        /// <summary>
        /// 菜单ID
        /// </summary>
        [Required(ErrorMessage = "菜单ID不能为空")] 
        public List<int> MenuIds { get; set; } = new List<int>();
    }
     

    /// <summary>
    /// 角色菜单查询请求
    /// </summary>
    public class GetMenusByRoleRequest
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Required(ErrorMessage = "角色ID不能为空")]
        [Range(1, int.MaxValue, ErrorMessage = "角色ID必须大于0")]
        public int? RoleId { get; set; } = 0;
    }
}