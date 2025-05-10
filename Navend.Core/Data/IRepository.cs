using System.Linq.Expressions;

namespace Navend.Core.Data;

public interface IRepository<TEntity, in TKey>: IRepository where TEntity: class, IEntity<TKey> {
    Task<TEntity?> GetAsync(TKey id);
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate);
    Task AddAsync(TEntity entity);
    Task AddRangeAsync(List<TEntity> entities);
    Task UpdateAsync(TEntity entity);
    Task UpdateRangeAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate);
    Task UpdatePartialAsync(TKey id, Expression<Func<TEntity, object>> propertySelector, object newValue);
    Task DeleteAsync(TEntity entity);
    Task DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate);
    Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate);
    IQueryable<TEntity> AsQueryable();
}


public interface IRepository {}