using Highever.SocialMedia.Domain.Entity;
using Highever.SocialMedia.Domain.Repository;
using SqlSugar;
using System.Linq.Expressions;

namespace Highever.SocialMedia.Infrastructure.Repository
{
    /// <summary>
    /// 菜单仓储实现
    /// </summary>
    public class MenusRepository : IMenusRepository
    {
        private readonly ISqlSugarClient _db;

        public MenusRepository(ISqlSugarClient db)
        {
            _db = db;
        }

        public async Task<bool> AddAsync(Menus entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Menus entity)
        {
            entity.UpdatedAt = DateTime.Now;
            return await _db.Updateable(entity).ExecuteCommandAsync() > 0;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            return await _db.Deleteable<Menus>().In(id).ExecuteCommandAsync() > 0;
        }

        public async Task<Menus?> GetByIdAsync(long id)
        {
            return await _db.Queryable<Menus>().FirstAsync(x => x.Id == id);
        }

        public async Task<Menus?> FirstOrDefaultAsync(Expression<Func<Menus, bool>> predicate)
        {
            return await _db.Queryable<Menus>().FirstAsync(predicate);
        }

        public async Task<List<Menus>> GetAllAsync()
        {
            return await _db.Queryable<Menus>().ToListAsync();
        }

        public async Task<List<Menus>> GetListAsync(Expression<Func<Menus, bool>> predicate)
        {
            return await _db.Queryable<Menus>().Where(predicate).ToListAsync();
        }
    }
}