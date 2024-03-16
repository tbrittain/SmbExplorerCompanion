using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetTraitsRequest : IRequest<OneOf<List<TraitDto>, Exception>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetTraitsRequestHandler : IRequestHandler<GetTraitsRequest, OneOf<List<TraitDto>, Exception>>
    {
        private readonly IRepository<TraitDto> _traitRepository;

        public GetTraitsRequestHandler(IRepository<TraitDto> traitRepository)
        {
            _traitRepository = traitRepository;
        }

        public async Task<OneOf<List<TraitDto>, Exception>> Handle(GetTraitsRequest request,
            CancellationToken cancellationToken)
        {
            var traitResult = await _traitRepository.GetAllAsync(cancellationToken);
            if (traitResult.TryPickT1(out var exception, out var traitDtos))
            {
                return exception;
            }

            return traitDtos.ToList();
        }
    }
}