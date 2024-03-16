using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetTraitsRequest : IRequest<List<TraitDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetTraitsRequestHandler : IRequestHandler<GetTraitsRequest, List<TraitDto>>
    {
        private readonly IRepository<TraitDto> _traitRepository;

        public GetTraitsRequestHandler(IRepository<TraitDto> traitRepository)
        {
            _traitRepository = traitRepository;
        }

        public async Task<List<TraitDto>> Handle(GetTraitsRequest request,
            CancellationToken cancellationToken)
        {
            var traitResult = await _traitRepository.GetAllAsync(cancellationToken);
            return traitResult.ToList();
        }
    }
}