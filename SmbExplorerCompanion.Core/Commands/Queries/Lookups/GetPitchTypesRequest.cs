using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetPitchTypesRequest : IRequest<List<PitchTypeDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetAllPitcherRolesHandler(IGetAllRepository<PitchTypeDto> pitchTypeRepository)
        : IRequestHandler<GetPitchTypesRequest, List<PitchTypeDto>>
    {
        public async Task<List<PitchTypeDto>> Handle(GetPitchTypesRequest request, CancellationToken cancellationToken)
        {
            var pitchTypesResult = await pitchTypeRepository.GetAllAsync(cancellationToken);
            return pitchTypesResult.ToList();
        }
    }
}