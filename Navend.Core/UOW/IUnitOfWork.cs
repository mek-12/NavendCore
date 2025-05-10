using Navend.Core.Data;

namespace Navend.Core.UOW;

public interface IUnitOfWork : IDisposable 
{
    Task StartTransactionAsync();
    void StartTransaction();
    Task CommitTransactionAsync();
    void CommitTransaction();
    Task RollbackTransactionAsync();
    void RollbackTransaction();
    IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity: class, IEntity<TKey>;
}