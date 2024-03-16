using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetBatHandednessRequest : IRequest<OneOf<List<BatHandednessDto>, Exception>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetBatHandednessRequestHandler : IRequestHandler<GetBatHandednessRequest, OneOf<List<BatHandednessDto>, Exception>>
    {
        private readonly IRepository<BatHandednessDto> _batHandednessRepository;

        public GetBatHandednessRequestHandler(IRepository<BatHandednessDto> batHandednessRepository)
        {
            _batHandednessRepository = batHandednessRepository;
        }

        public async Task<OneOf<List<BatHandednessDto>, Exception>> Handle(GetBatHandednessRequest request, CancellationToken cancellationToken)
        {
            var batHandednessResult = await _batHandednessRepository.GetAllAsync(cancellationToken);
            if (batHandednessResult.TryPickT1(out var exception, out var batHandednessDtos))
            {
                return exception;
            }

            return batHandednessDtos.ToList();
        }
    }
}