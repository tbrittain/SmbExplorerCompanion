using OneOf;
using OneOf.Types;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IRepository<T> where T : class
{
    public Task<OneOf<IEnumerable<T>, Exception>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<OneOf<T, None, Exception>> GetByIdAsync(int Id, CancellationToken cancellationToken = default);
    public Task<OneOf<T, Exception>> AddAsync(T entity, CancellationToken cancellationToken = default);

    public Task<OneOf<IEnumerable<T>, Exception>> AddRangeAsync(IEnumerable<T> entities,
        CancellationToken cancellationToken = default);

    public Task<OneOf<T, None, Exception>> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    public Task<OneOf<T, None, Exception>> DeleteAsync(int Id, CancellationToken cancellationToken = default);
}