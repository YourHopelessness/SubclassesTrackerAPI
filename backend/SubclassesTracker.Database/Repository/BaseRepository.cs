using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SubclassesTracker.Database.Context;
using System.Linq;

namespace SubclassesTracker.Database.Repository
{
    public interface IBaseRepository<T> where T : class
    {
        IQueryable<T> GetList(Expression<Func<T, bool>> predicate);
        Task<T?> GetByParam(Expression<Func<T, bool>> predicate, CancellationToken token);
        Task AddAsync(T entity, CancellationToken token);
        Task UpdateAsync(T entity, CancellationToken token);
        Task DeleteAsync(T entity, CancellationToken token);
    }

    public class BaseRepository<T>(EsoContext context) : IBaseRepository<T> where T : class
    {
        protected readonly EsoContext _context = context;

        public IQueryable<T> GetList(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().Where(predicate);
        }

        public async Task<T?> GetByParam(Expression<Func<T, bool>> predicate, CancellationToken token)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate, token);
        }

        public async Task AddAsync(T entity, CancellationToken token)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync(token);
        }

        public async Task UpdateAsync(T entity, CancellationToken token)
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync(token);
        }

        public async Task DeleteAsync(T entity, CancellationToken token)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync(token);
        }
    }
}
