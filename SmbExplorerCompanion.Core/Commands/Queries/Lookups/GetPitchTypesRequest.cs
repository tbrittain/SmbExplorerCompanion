using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetPitchTypesRequest : IRequest<OneOf<List<PitchTypeDto>, Exception>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetAllPitcherRolesHandler : IRequestHandler<GetPitchTypesRequest, OneOf<List<PitchTypeDto>, Exception>>
    {
        private readonly IRepository<PitchTypeDto> _pitchTypeRepository;

        public GetAllPitcherRolesHandler(IRepository<PitchTypeDto> pitchTypeRepository)
        {
            _pitchTypeRepository = pitchTypeRepository;
        }

        public async Task<OneOf<List<PitchTypeDto>, Exception>> Handle(GetPitchTypesRequest request, CancellationToken cancellationToken)
        {
            var pitchTypesResult = await _pitchTypeRepository.GetAllAsync(cancellationToken);
            if (pitchTypesResult.TryPickT1(out var exception, out var pitchTypeDtos))
            {
                return exception;
            }

            return pitchTypeDtos.ToList();
        }
    }
}