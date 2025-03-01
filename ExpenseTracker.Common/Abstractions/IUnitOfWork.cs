using ExpenseTracker.Common.Models;

namespace ExpenseTracker.Common.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {
        IRepositoryBase<Result<T>> GetRepository<T>() where T : class;
        Task<Result<int>> SaveChangesAsync(CancellationToken cancellation);
        Task CommitTransactionAsync(CancellationToken cancellationToken);
        Task RollbackTransactionAsync(CancellationToken cancellationToken);
        Task BeginTransactionAsync(CancellationToken cancellationToken);
    }
}
