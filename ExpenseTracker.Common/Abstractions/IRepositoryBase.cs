using Ardalis.Specification;

namespace ExpenseTracker.Common.Abstractions
{
    public interface IRepositoryBase<T>
    {
        Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken);
        Task UpdateAsync(T entity, CancellationToken cancellationToken);
        Task DeleteAsync(T entity, CancellationToken cancellationToken);

        // Query methods
        Task<IReadOnlyList<T>> FirstOrDefault(ISpecification<T> spec, CancellationToken cancellationToken);
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken);
        Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken);
        Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken cancellationToken);
    }
}
