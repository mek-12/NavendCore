using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Navend.Core.UOW;

namespace Navend.Core.Data.EfCore;

public class EfCoreUnitOfWork<TContext>: IUnitOfWork where TContext : DbContext {
    protected readonly ConcurrentDictionary<string,IRepository> Repositories = new();
    private TContext Context { get; }
    private IDbContextTransaction? Transaction;

    public EfCoreUnitOfWork(TContext context) {
        Context = context;
    }

    public void CommitTransaction()
    {
        Transaction?.Commit();
        Context.ChangeTracker.Clear();
        Transaction?.Dispose();
        Transaction = null;
    }

    public async Task CommitTransactionAsync() {
        if (Transaction is not null)
        {
            await Transaction.CommitAsync();
            Context.ChangeTracker.Clear();
            Transaction.Dispose(); // Kaynağı serbest bırak
            Transaction = null;
        }
    }

    public void Dispose() => Transaction?.Dispose();

    public void RollbackTransaction()
    {
        Transaction?.Rollback();
        Transaction?.Dispose();
        Transaction = null;
    } 

    public async Task RollbackTransactionAsync() {
        if (Transaction is not null)
        {
            await Transaction.RollbackAsync();
            Transaction.Dispose();
            Transaction = null;
        }
    }

    public void StartTransaction() => Transaction = Transaction is null ? Context.Database.BeginTransaction() : Transaction;

    public async Task StartTransactionAsync() {
        if (Context is not null) {
            Transaction = await Context.Database.BeginTransactionAsync();
        }
    }

    IRepository<TEntity, TKey> IUnitOfWork.GetRepository<TEntity, TKey>()
    {
        if (Repositories.ContainsKey(typeof(TKey).Name)){
            return (IRepository<TEntity,TKey>)Repositories[typeof(TKey).Name];
        }
        var repository = new EfCoreRepository<TEntity,TKey>(Context);
        Repositories.TryAdd(typeof(TEntity).Name, repository);
        return repository;
    }
}