using Highever.SocialMedia.Application.Contracts.DTOs.System;
using Highever.SocialMedia.Application.Contracts.Services;
using Highever.SocialMedia.Common;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Highever.SocialMedia.Application.Services.System
{
    public class UsersService : IUsersService
    {
        private readonly IRepository<Users> _repository;
        private readonly IRepository<Roles> _rolesRepository;
        private readonly IRepository<UserRoles> _userRolesRepository;

        public UsersService(
            IRepository<Users> repository,
            IRepository<Roles> rolesRepository,
            IRepository<UserRoles> userRolesRepository)
        {
            _repository = repository;
            _rolesRepository = rolesRepository;
            _userRolesRepository = userRolesRepository;
        }

        public async Task<int> CreateAsync(Users input)
        {
            return await _repository.InsertAsync(input);
        }

        public async Task<int> CreateAsync(List<Users> input)
        {
            return await _repository.InsertRangeAsync(input);
        }

        public async Task<int> UpdateAsync(Users input)
        {
            return await _repository.UpdateAsync(input);
        }

        public async Task<int> UpdateAsync(List<Users> input)
        {
            return await _repository.UpdateRangeAsync(input);
        }

        public async Task<int> DeleteAsync(Expression<Func<Users, bool>> predicate)
        {
            return await _repository.DeleteAsync(predicate);
        }

        public async Task<Users?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Users?> FirstOrDefaultAsync(Expression<Func<Users, bool>> predicate)
        {
            return await _repository.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<Users>> GetAllAsync()
        {
            return await _repository.GetListAsync();
        }

        public async Task<List<Users>> GetListAsync(Expression<Func<Users, bool>> predicate)
        {
            return await _repository.GetListAsync(predicate);
        }


        public async Task<int> CreateUserAsync(CreateUserRequest request)
        {
            // 检查用户名是否已存在
            if (await IsUsernameExistAsync(request.Account))
            {
                throw new InvalidOperationException($"用户名 '{request.Account}' 已存在");
            }


            // 验证角色是否存在
            if (request.RoleIds.Any())
            {
                var existingRoles = await _rolesRepository.GetListAsync(r => request.RoleIds.Contains(r.Id));
                if (existingRoles.Count != request.RoleIds.Count)
                {
                    throw new InvalidOperationException("部分角色不存在");
                }
            }

            var user = new Users
            {
                Account = request.Account,
                PasswordHash = ComputeMD5Hash(request.Password),
                DisplayName = request.DisplayName,
                IsActive = request.IsActive,
                CreatedAt = DateTime.Now
            };

            var userId = await _repository.InsertByIdentityAsync(user);
            if (userId > 0)
            {
                // 分配角色
                if (request.RoleIds.Any())
                {
                    await AssignRolesToUserAsync(userId, request.RoleIds);
                }
                return user.Id;
            }
            return 0;
        }

        public async Task<bool> UpdateUserAsync(UpdateUserRequest request)
        {
            // 检查用户是否存在
            var existingUser = await _repository.GetByIdAsync(request.Id);
            if (existingUser == null)
            {
                throw new InvalidOperationException("用户不存在");
            }

            // 检查用户名是否已被其他用户使用
            if (await IsUsernameExistAsync(request.Account, request.Id))
            {
                throw new InvalidOperationException($"用户名 '{request.Account}' 已被其他用户使用");
            }

            // 验证角色是否存在
            if (request.RoleIds.Any())
            {
                var existingRoles = await _rolesRepository.GetListAsync(r => request.RoleIds.Contains(r.Id));
                if (existingRoles.Count != request.RoleIds.Count)
                {
                    throw new InvalidOperationException("部分角色不存在");
                }
            }

            // 更新用户信息
            existingUser.Account = request.Account;
            existingUser.DisplayName = request.DisplayName;
            existingUser.IsActive = request.IsActive;
            existingUser.UpdatedAt = DateTime.Now;

            var result = await _repository.UpdateAsync(existingUser);
            if (result > 0)
            {
                // 重新分配角色
                await _userRolesRepository.DeleteAsync(ur => ur.UserId == request.Id);
                if (request.RoleIds.Any())
                {
                    await AssignRolesToUserAsync(request.Id, request.RoleIds);
                }
                return true;
            }
            return false;
        }
        public async Task<List<UserResponse>> GetUsersAsyncAll(GetUsersRequest? request = null)
        {
            var users = await _repository.GetListAsync();
            var allUserRoles = await _userRolesRepository.GetListAsync();
            var allRoles = await _rolesRepository.GetListAsync();

            // 应用查询条件
            if (request != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Username))
                {
                    users = users.Where(x => x.DisplayName.Contains(request.Username) || x.Account.Contains(request.Username)).ToList();
                }

                if (request.IsActive.HasValue)
                {
                    users = users.Where(x => x.IsActive == request.IsActive.Value).ToList();
                }

                if (request.RoleId.HasValue)
                {
                    var userIdsWithRole = allUserRoles
                        .Where(ur => ur.RoleId == request.RoleId.Value)
                        .Select(ur => ur.UserId)
                        .ToList();
                    users = users.Where(u => userIdsWithRole.Contains(u.Id)).ToList();
                }
            }

            return users.Select(user => new UserResponse
            {
                Id = user.Id,
                Account = user.Account,
                DisplayName = user.DisplayName,
                IsActive = user.IsActive,
                LastLoginAt = user.LastActivityTime,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = GetUserRoles(user.Id, allUserRoles, allRoles)
            }).OrderByDescending(x => x.CreatedAt).ToList();
        }
        public async Task<PagedResult<UserResponse>> GetUsersAsync(GetUsersRequest? request = null)
        {
            // 设置默认分页参数
            var pageIndex = request?.PageIndex ?? 1;
            var pageSize = request?.PageSize ?? 20;
            if (pageIndex <= 0) pageIndex = 1;
            if (pageSize <= 0) pageSize = 20;

            // 构建查询条件
            Expression<Func<Users, bool>>? predicate = null;

            if (request != null)
            {
                var conditions = new List<Expression<Func<Users, bool>>>();

                // 关键词查询（用户名或昵称）
                if (!string.IsNullOrWhiteSpace(request.Keyword))
                {
                    conditions.Add(x => x.DisplayName.Contains(request.Keyword) || x.Account.Contains(request.Keyword));
                }

                // 用户名查询（保持向后兼容）
                if (!string.IsNullOrWhiteSpace(request.Username))
                {
                    conditions.Add(x => x.DisplayName.Contains(request.Username) || x.Account.Contains(request.Username));
                }

                // 激活状态查询
                if (request.IsActive.HasValue)
                {
                    conditions.Add(x => x.IsActive == request.IsActive.Value);
                }

                // 角色查询
                if (request.RoleId.HasValue)
                {
                    var userIdsWithRole = await _userRolesRepository.GetListAsync(ur => ur.RoleId == request.RoleId.Value);
                    var userIds = userIdsWithRole.Select(ur => ur.UserId).ToList();
                    if (userIds.Any())
                    {
                        conditions.Add(u => userIds.Contains(u.Id));
                    }
                    else
                    {
                        // 如果没有用户拥有该角色，返回空结果
                        return new PagedResult<UserResponse>
                        {
                            Items = new List<UserResponse>(),
                            TotalCount = 0,
                            PageIndex = pageIndex,
                            PageSize = pageSize
                        };
                    }
                }

                // 合并所有条件
                if (conditions.Any())
                {
                    predicate = conditions.Aggregate((prev, next) =>
                        Expression.Lambda<Func<Users, bool>>(
                            Expression.AndAlso(prev.Body, next.Body),
                            prev.Parameters));
                }
            }

            // 执行分页查询
            var pagedUsers = await _repository.GetPagedListAsync(
                predicate: predicate,
                pageIndex: pageIndex,
                pageSize: pageSize,
                orderBy: x => x.CreatedAt,
                ascending: false
            );

            // 获取所有角色和用户角色关系（用于填充角色信息）
            var allUserRoles = await _userRolesRepository.GetListAsync();
            var allRoles = await _rolesRepository.GetListAsync();

            // 转换为响应DTO
            var userResponses = pagedUsers.Items.Select(user => new UserResponse
            {
                Id = user.Id,
                Account = user.Account,
                DisplayName = user.DisplayName,
                IsActive = user.IsActive,
                LastLoginAt = user.LastActivityTime,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = GetUserRoles(user.Id, allUserRoles, allRoles)
            }).ToList();

            return new PagedResult<UserResponse>
            {
                Items = userResponses,
                TotalCount = pagedUsers.TotalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }
        public async Task<bool> DeleteUserAsync(DeleteUserRequest request)
        {
            var user = await _repository.GetByIdAsync(request.Id);
            if (user == null)
            {
                throw new InvalidOperationException("用户不存在");
            }

            // 删除用户角色关联
            await _userRolesRepository.DeleteAsync(ur => ur.UserId == request.Id);

            // 删除用户
            var result = await _repository.DeleteAsync(x => x.Id == request.Id);
            return result > 0;
        }

        public async Task<bool> BatchDeleteUserAsync(BatchDeleteUserRequest request)
        {
            foreach (var id in request.Ids)
            {
                await DeleteUserAsync(new DeleteUserRequest { Id = id });
            }
            return true;
        }

        public async Task<UserResponse?> GetUserByIdAsync(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null) return null;

            var userRoles = await _userRolesRepository.GetListAsync(ur => ur.UserId == id);
            var allRoles = await _rolesRepository.GetListAsync();

            return new UserResponse
            {
                Id = user.Id,
                Account = user.Account,
                DisplayName = user.DisplayName,
                IsActive = user.IsActive,
                LastLoginAt = user.LastActivityTime,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = GetUserRoles(user.Id, userRoles, allRoles)
            };
        }

        public async Task<bool> IsUsernameExistAsync(string username, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _repository.ExistsAsync(x => x.Account == username && x.Id != excludeId.Value);
            }
            return await _repository.ExistsAsync(x => x.Account == username);
        }


        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _repository.GetByIdAsync(request.Id);
            if (user == null)
            {
                throw new InvalidOperationException("用户不存在");
            }

            user.PasswordHash = ComputeMD5Hash(request.NewPassword);
            user.UpdatedAt = DateTime.Now;

            var result = await _repository.UpdateAsync(user);
            return result > 0;
        }

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        private async Task AssignRolesToUserAsync(int userId, List<int> roleIds)
        {
            if (userId > 0 && roleIds.Any())
            {
                var userRoles = roleIds.Select(roleId => new UserRoles
                {
                    UserId = userId,
                    RoleId = roleId
                }).ToList();

                await _userRolesRepository.InsertRangeAsync(userRoles);

            } 
        }

        /// <summary>
        /// 获取用户角色列表
        /// </summary>
        private List<RoleResponse> GetUserRoles(int userId, List<UserRoles> allUserRoles, List<Roles> allRoles)
        {
            var userRoleIds = allUserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToList();

            return allRoles
                .Where(r => userRoleIds.Contains(r.Id))
                .Select(role => new RoleResponse
                {
                    Id = role.Id,
                    Name = role.Name,
                    Code = role.Code,
                    CreatedAt = role.CreatedAt,
                    UpdatedAt = role.UpdatedAt
                }).ToList();
        }

        /// <summary>
        /// 密码哈希
        /// </summary> 
        private string ComputeMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }
    }
}


