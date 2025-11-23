using Microsoft.EntityFrameworkCore;
using SubclassesTracker.Database.Context;
using SubclassesTracker.Database.Entity;
using System.Linq.Expressions;

namespace SubclassesTracker.Database.Repository
{
    public interface IBaseRepository<T> where T : class
    {
        /// <summary>
        /// Get list by the predicate
        /// </summary>
        /// <param name="predicate">The list selection condition</param>
        /// <returns>List of T entities</returns>
        IQueryable<T> GetList(Expression<Func<T, bool>> predicate, bool noTracking = true);
        /// <summary>
        /// Get one entity by the condition
        /// </summary>
        /// <param name="predicate">condition</param>
        /// <returns>The single value or empty</returns>
        Task<T?> GetByParam(Expression<Func<T, bool>> predicate, CancellationToken token, bool noTracking = true);
        /// <summary>
        /// Add the new entity
        /// </summary>
        /// <param name="entity">Added entity</param>
        Task AddAsync(T entity, CancellationToken token);
        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Updated enities</param>
        Task UpdateAsync(T entity, CancellationToken token);
        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">id of the removed enity</param>
        Task DeleteAsync(int entityId, CancellationToken token);
        /// <summary>
        /// Get all enities
        /// </summary>
        /// <returns>List of enity</returns>
        IQueryable<T> GetAll(bool noTracking = true);
    }

    public class BaseRepository<T>(EsoContext context) : IBaseRepository<T>
        where T : class, IHaveIdentifier, new()
    {
        public IQueryable<T> GetList(Expression<Func<T, bool>> predicate, bool noTracking = true)
        {
            return context.Set<T>().Where(predicate).Detach(noTracking);
        }

        public IQueryable<T> GetAll(bool noTracking = true)
        {
            return context.Set<T>().Detach(noTracking);
        }

        public async Task<T?> GetByParam(
            Expression<Func<T, bool>> predicate,
            CancellationToken token,
            bool noTracking = true)
        {
            return await context.Set<T>().Detach(noTracking).FirstOrDefaultAsync(predicate, token);
        }

        public async Task AddAsync(T entity, CancellationToken token)
        {
            await context.Set<T>().AddAsync(entity, token);

            await context.SaveChangesAsync(token);
        }

        public async Task UpdateAsync(T entity, CancellationToken token)
        {
            context.Entry(entity).State = EntityState.Modified;
            context.Set<T>().Update(entity);

            await context.SaveChangesAsync(token);
        }

        public async Task DeleteAsync(int entityId, CancellationToken token)
        {
            var deletedEnity = new T { Id = entityId };
            context.Entry(deletedEnity).State = EntityState.Deleted;

            await context.SaveChangesAsync(token);
        }
    }
}
