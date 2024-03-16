namespace SmbExplorerCompanion.Core.Interfaces;

public interface IRepository<T> where T : class
{
    public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
}