using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetThrowHandednessRequest : IRequest<List<ThrowHandednessDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetThrowHandednessRequestHandler(IGetAllRepository<ThrowHandednessDto> throwHandednessRepository)
        : IRequestHandler<GetThrowHandednessRequest, List<ThrowHandednessDto>>
    {
        public async Task<List<ThrowHandednessDto>> Handle(GetThrowHandednessRequest request,
            CancellationToken cancellationToken)
        {
            var throwHandednessResult = await throwHandednessRepository.GetAllAsync(cancellationToken);
            return throwHandednessResult.ToList();
        }
    }
}