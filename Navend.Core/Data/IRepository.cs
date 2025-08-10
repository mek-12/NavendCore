using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Navend.Core.Data;

public interface IRepository<TEntity, in TKey> : IRepository where TEntity : class, IEntity<TKey>
{
    Task<TEntity?> GetAsync(TKey id);
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate);
    Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null,
                                    bool asNoTracking = false,
                                    int? take = null);
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(List<TEntity> entities, bool saveChanges = true);
    Task UpdateAsync(TEntity entity);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities);
    Task UpsertRangeAsync(List<TEntity> entities);
    Task UpdatePartialAsync(TKey id, Expression<Func<TEntity, object>> propertySelector, object newValue);
    Task DeleteAsync(TEntity entity);
    Task DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate);
    Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate);
    IQueryable<TEntity> AsQueryable(bool asNoTracking = false);
    Task<List<TResult>> SelectAsync<TResult>(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, TResult>> selector, bool asNoTracking = false, int? take = null);
    Task BulkInsertAsync(ConcurrentBag<TEntity> entities, int batchSize = 1000, bool clearTracker = true, CancellationToken cancellationToken = default);
}


public interface IRepository {}