using MediatR;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Seasons;

public class GetSeasonsRequest : IRequest<List<SeasonDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetSeasonsHandler(IGetAllRepository<SeasonDto> seasonRepository) : IRequestHandler<GetSeasonsRequest, List<SeasonDto>>
    {
        public async Task<List<SeasonDto>> Handle(GetSeasonsRequest request, CancellationToken cancellationToken)
        {
            var seasonsResponse = await seasonRepository.GetAllAsync(cancellationToken);
            return seasonsResponse.ToList();
        }
    }
}