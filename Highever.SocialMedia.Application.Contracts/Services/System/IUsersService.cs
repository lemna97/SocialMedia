using Highever.SocialMedia.Application.Contracts.DTOs.System;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Contracts.Services
{
    /// <summary>
    /// 用户管理服务接口
    /// </summary>
    public interface IUsersService : ITransientDependency
    {
        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="input">用户信息</param>
        /// <returns>用户ID</returns>
        Task<int> CreateAsync(Users input);

        /// <summary>
        /// 批量创建用户
        /// </summary>
        /// <param name="input">用户列表</param>
        /// <returns>影响行数</returns>
        Task<int> CreateAsync(List<Users> input);

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="input">用户信息</param>
        /// <returns>影响行数</returns>
        Task<int> UpdateAsync(Users input);

        /// <summary>
        /// 批量更新用户
        /// </summary>
        /// <param name="input">用户列表</param>
        /// <returns>影响行数</returns>
        Task<int> UpdateAsync(List<Users> input);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="predicate">删除条件</param>
        /// <returns>影响行数</returns>
        Task<int> DeleteAsync(Expression<Func<Users, bool>> predicate);

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户信息</returns>
        Task<Users?> GetByIdAsync(int id);

        /// <summary>
        /// 根据条件查询单个用户
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>用户信息</returns>
        Task<Users?> FirstOrDefaultAsync(Expression<Func<Users, bool>> predicate);

        /// <summary>
        /// 获取所有用户
        /// </summary>
        /// <returns>用户列表</returns>
        Task<List<Users>> GetAllAsync();

        /// <summary>
        /// 根据条件查询用户列表
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <returns>用户列表</returns>
        Task<List<Users>> GetListAsync(Expression<Func<Users, bool>> predicate);

        /// <summary>
        /// 获取用户列表（带查询条件和角色信息）
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>分页用户列表</returns>
        Task<List<UserResponse>> GetUsersAsyncAll(GetUsersRequest? request = null);

        /// <summary>
        /// 获取用户列表（带查询条件和角色信息）
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>分页用户列表</returns>
        Task<PagedResult<UserResponse>> GetUsersAsync(GetUsersRequest? request = null);

        /// <summary>
        /// 创建用户（包含角色分配）
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建结果</returns>
        Task<int> CreateUserAsync(CreateUserRequest request);

        /// <summary>
        /// 更新用户（包含角色分配）
        /// </summary>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        Task<bool> UpdateUserAsync(UpdateUserRequest request);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="request">删除请求</param>
        /// <returns>删除结果</returns>
        Task<bool> DeleteUserAsync(DeleteUserRequest request);

        /// <summary>
        /// 批量删除用户
        /// </summary>
        /// <param name="request">批量删除请求</param>
        /// <returns>删除结果</returns>
        Task<bool> BatchDeleteUserAsync(BatchDeleteUserRequest request);

        /// <summary>
        /// 获取用户详情（包含角色信息）
        /// </summary>
        /// <param name="id">用户ID</param>
        /// <returns>用户详情</returns>
        Task<UserResponse?> GetUserByIdAsync(int id);

        /// <summary>
        /// 检查用户名是否存在
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="excludeId">排除的用户ID</param>
        /// <returns>是否存在</returns>
        Task<bool> IsUsernameExistAsync(string username, int? excludeId = null);
         
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="request">修改密码请求</param>
        /// <returns>修改结果</returns>
        Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    }
}




