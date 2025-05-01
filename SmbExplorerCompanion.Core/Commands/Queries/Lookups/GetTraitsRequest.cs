using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetTraitsRequest : IRequest<List<TraitDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetTraitsRequestHandler(IGetAllRepository<TraitDto> traitRepository) : IRequestHandler<GetTraitsRequest, List<TraitDto>>
    {
        public async Task<List<TraitDto>> Handle(GetTraitsRequest request,
            CancellationToken cancellationToken)
        {
            var traitResult = await traitRepository.GetAllAsync(cancellationToken);
            return traitResult.ToList();
        }
    }
}