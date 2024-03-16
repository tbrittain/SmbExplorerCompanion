using MediatR;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Seasons;

public class GetSeasonsRequest : IRequest<List<SeasonDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetSeasonsHandler : IRequestHandler<GetSeasonsRequest, List<SeasonDto>>
    {
        private readonly IRepository<SeasonDto> _seasonRepository;

        public GetSeasonsHandler(IRepository<SeasonDto> seasonRepository)
        {
            _seasonRepository = seasonRepository;
        }

        public async Task<List<SeasonDto>> Handle(GetSeasonsRequest request, CancellationToken cancellationToken)
        {
            var seasonsResponse = await _seasonRepository.GetAllAsync(cancellationToken);
            return seasonsResponse.ToList();
        }
    }
}