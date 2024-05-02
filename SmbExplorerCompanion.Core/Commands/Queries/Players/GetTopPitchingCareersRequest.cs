using MediatR;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopPitchingCareersRequest : IRequest<List<PlayerCareerPitchingDto>>
{
    public GetTopPitchingCareersRequest(GetPitchingCareersFilters filters)
    {
        Filters = filters;
    }

    private GetPitchingCareersFilters Filters { get; }

    // ReSharper disable once UnusedType.Global
    internal class GetTopPitchingCareersHandler : IRequestHandler<GetTopPitchingCareersRequest, List<PlayerCareerPitchingDto>>
    {
        private readonly IPitcherCareerRepository _pitcherCareerRepository;

        public GetTopPitchingCareersHandler(IPitcherCareerRepository pitcherCareerRepository)
        {
            _pitcherCareerRepository = pitcherCareerRepository;
        }

        public async Task<List<PlayerCareerPitchingDto>> Handle(GetTopPitchingCareersRequest request,
            CancellationToken cancellationToken)
        {
            return await _pitcherCareerRepository.GetPitchingCareers(
                request.Filters,
                cancellationToken);
        }
    }
}