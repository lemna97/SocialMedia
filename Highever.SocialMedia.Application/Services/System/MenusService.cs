using Highever.SocialMedia.Application.Contracts.Services;
using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Application.Services
{
    /// <summary>
    /// 菜单服务实现
    /// </summary>
    public class MenusService : IMenusService
    {
        private readonly IRepository<Menus> _repository;

        public MenusService(IRepository<Menus> repository)
        {
            _repository = repository;
        }

        public async Task<bool> AddAsync(Menus entity)
        {
            var result = await _repository.InsertAsync(entity);
            return result > 0;
        }

        public async Task<bool> UpdateAsync(Menus entity)
        {
            var result = await _repository.UpdateAsync(entity);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var result = await _repository.DeleteAsync(x => x.Id == id);
            return result > 0;
        }

        public async Task<Menus?> GetByIdAsync(long id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Menus?> FirstOrDefaultAsync(Expression<Func<Menus, bool>> predicate)
        {
            return await _repository.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<Menus>> GetAllAsync()
        {
            return await _repository.GetListAsync();
        }

        public async Task<List<Menus>> GetListAsync(Expression<Func<Menus, bool>> predicate)
        {
            return await _repository.GetListAsync(predicate);
        }

        public async Task<List<Menus>> GetMenuTreeAsync()
        {
            var allMenus = await _repository.GetListAsync(x => x.IsActive);
            return allMenus.OrderBy(x => x.Sort).ToList();
        }
    }
}
