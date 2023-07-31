using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Seasons;

public class GetSeasonsByFranchiseRequest : IRequest<OneOf<List<SeasonDto>, Exception>>
{
    public GetSeasonsByFranchiseRequest(int franchiseId)
    {
        FranchiseId = franchiseId;
    }

    private int FranchiseId { get; }
    
    // ReSharper disable once UnusedType.Global
    public class GetSeasonsByFranchiseHandler : IRequestHandler<GetSeasonsByFranchiseRequest, OneOf<List<SeasonDto>, Exception>>
    {
        private readonly IRepository<SeasonDto> _seasonRepository;

        public GetSeasonsByFranchiseHandler(IRepository<SeasonDto> seasonRepository)
        {
            _seasonRepository = seasonRepository;
        }

        public async Task<OneOf<List<SeasonDto>, Exception>> Handle(GetSeasonsByFranchiseRequest request, CancellationToken cancellationToken)
        {
            var seasonsResponse = await _seasonRepository.GetAllAsync(cancellationToken);
            if (seasonsResponse.TryPickT1(out var exception, out var seasons))
                return exception;

            return seasons.Where(s => s.FranchiseId == request.FranchiseId).ToList();
        }
    }
}