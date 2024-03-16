using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetBatHandednessRequest : IRequest<List<BatHandednessDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetBatHandednessRequestHandler : IRequestHandler<GetBatHandednessRequest, List<BatHandednessDto>>
    {
        private readonly IRepository<BatHandednessDto> _batHandednessRepository;

        public GetBatHandednessRequestHandler(IRepository<BatHandednessDto> batHandednessRepository)
        {
            _batHandednessRepository = batHandednessRepository;
        }

        public async Task<List<BatHandednessDto>> Handle(GetBatHandednessRequest request, CancellationToken cancellationToken)
        {
            var batHandednessResult = await _batHandednessRepository.GetAllAsync(cancellationToken);
            return batHandednessResult.ToList();
        }
    }
}