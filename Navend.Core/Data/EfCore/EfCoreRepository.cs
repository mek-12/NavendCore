using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Navend.Core.Data.EfCore;

public class EfCoreRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    protected readonly DbContext dbContext;
    protected readonly DbSet<TEntity> dbSet;

    public EfCoreRepository(DbContext dbContext)
    {
        this.dbContext = dbContext;
        dbSet = dbContext.Set<TEntity>();
    }
    public async Task AddAsync(TEntity entity)
    {
        await dbSet.AddAsync(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task AddRangeAsync(List<TEntity> entities)
    {
        await dbSet.AddRangeAsync(entities);
        await dbContext.SaveChangesAsync();
    }

    public IQueryable<TEntity> AsQueryable()
    {
        return dbSet.AsQueryable();
    }

    public async Task DeleteAsync(TEntity entity)
    {
        dbSet.Remove(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate)
    {
        dbSet.RemoveRange(dbSet.Where(predicate));
        await dbContext.SaveChangesAsync();
    }

    public async Task<TEntity?> GetAsync(TKey id)
    {
        // TODO: We can use NolockDbContext for this method. 
        // Once I add the Navend.Core library, I will be able to use this idea.
        return await dbSet.FindAsync(id);
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await dbSet.FindAsync(predicate);
    }

    public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await dbSet.CountAsync(predicate);
    }

    public async Task UpdateAsync(TEntity entity)
    {
        dbSet.Attach(entity);
        dbContext.Entry(entity).State = EntityState.Modified;
        await dbContext.SaveChangesAsync();
    }

    public async Task UpsertRangeAsync(List<TEntity> entities)
    {
        if (entities == null || entities.Count == 0)
            return;

        var ids = entities.Select(e => e.Id).ToList();

        var existingIds = await dbSet
            .Where(e => ids.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync();

        var existingIdSet = new HashSet<TKey>(existingIds);

        var toAdd = entities.Where(e => !existingIdSet.Contains(e.Id)).ToList();
        var toUpdate = entities.Where(e => existingIdSet.Contains(e.Id)).ToList();

        if (toAdd.Count > 0)
            await AddRangeAsync(toAdd);

        if (toUpdate.Count > 0)
            await UpdateRangeAsync(toUpdate);
    }

    public async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            dbSet.Attach(entity);
            dbContext.Entry(entity).State = EntityState.Modified;
        }
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdatePartialAsync(TKey id, Expression<Func<TEntity, object>> propertySelector, object newValue)
    {

        TEntity? entity = (TEntity?)Activator.CreateInstance(typeof(TEntity));
        if (entity is null)
            return;
        dbSet.Attach(entity);

        string propertyName = GetPropertyName(propertySelector);

        var entry = dbContext.Entry(entity);
        entry.Property(propertyName).CurrentValue = newValue;
        entry.Property(propertyName).IsModified = true;

        await dbContext.SaveChangesAsync();
    }
    private string GetPropertyName(Expression<Func<TEntity, object>> propertyLambda)
    {
        return propertyLambda.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression unary when unary.Operand is MemberExpression member => member.Member.Name,
            _ => throw new ArgumentException("Invalid property expression")
        };
    }

    public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return await dbSet.ToListAsync();
    }

    public async Task<List<TResult>> SelectAsync<TResult>(Expression<Func<TEntity, bool>>? predicate, Expression<Func<TEntity, TResult>> selector)
    {
        IQueryable<TEntity> query = dbSet;

        if (predicate != null)
            query = query.Where(predicate);

        return await query.Select(selector).ToListAsync();
    }
}