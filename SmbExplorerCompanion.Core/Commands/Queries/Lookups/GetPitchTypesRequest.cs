using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetPitchTypesRequest : IRequest<List<PitchTypeDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetAllPitcherRolesHandler : IRequestHandler<GetPitchTypesRequest, List<PitchTypeDto>>
    {
        private readonly IRepository<PitchTypeDto> _pitchTypeRepository;

        public GetAllPitcherRolesHandler(IRepository<PitchTypeDto> pitchTypeRepository)
        {
            _pitchTypeRepository = pitchTypeRepository;
        }

        public async Task<List<PitchTypeDto>> Handle(GetPitchTypesRequest request, CancellationToken cancellationToken)
        {
            var pitchTypesResult = await _pitchTypeRepository.GetAllAsync(cancellationToken);
            return pitchTypesResult.ToList();
        }
    }
}