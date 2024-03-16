using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetPlayerAwardsRequest : IRequest<List<PlayerAwardDto>>
{
    private GetPlayerAwardsRequest(bool onlyUserAssignable, bool isRegularSeason)
    {
        OnlyUserAssignable = onlyUserAssignable;
        IsRegularSeason = isRegularSeason;
    }

    private GetPlayerAwardsRequest()
    {
        All = true;
    }

    private bool OnlyUserAssignable { get; }
    private bool IsRegularSeason { get; }
    private bool All { get; }

    private static GetPlayerAwardsRequest RegularSeason(bool onlyUserAssignable) => new(onlyUserAssignable, true);
    private static GetPlayerAwardsRequest Playoffs(bool onlyUserAssignable) => new(onlyUserAssignable, false);
    public static GetPlayerAwardsRequest Default => RegularSeason(true);
    public static GetPlayerAwardsRequest DefaultPlayoffs => Playoffs(true);
    public static GetPlayerAwardsRequest AllAwards => new();

    // ReSharper disable once UnusedType.Global
    internal class GetPlayerAwardsHandler : IRequestHandler<GetPlayerAwardsRequest, List<PlayerAwardDto>>
    {
        private readonly IRepository<PlayerAwardDto> _playerAwardRepository;

        public GetPlayerAwardsHandler(IRepository<PlayerAwardDto> playerAwardRepository)
        {
            _playerAwardRepository = playerAwardRepository;
        }

        public async Task<List<PlayerAwardDto>> Handle(GetPlayerAwardsRequest request, CancellationToken cancellationToken)
        {
            var awardResult = await _playerAwardRepository.GetAllAsync(cancellationToken);
            return awardResult
                .Where(x => request.All || x.IsPlayoffAward != request.IsRegularSeason)
                .Where(x => request.All || x.IsUserAssignable == request.OnlyUserAssignable)
                .ToList();
        }
    }
}