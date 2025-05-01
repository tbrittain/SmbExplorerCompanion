namespace SmbExplorerCompanion.Core.Interfaces;

public interface IGetAllRepository<T> where T : class
{
    public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IAddRepository<T> where T : class
{
    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
}