using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Players;

public class GetTopBattingCareersRequest : IRequest<OneOf<List<PlayerCareerDto>, Exception>>
{
    public GetTopBattingCareersRequest(int? pageNumber, string? orderBy, bool descending)
    {
        PageNumber = pageNumber;
        OrderBy = orderBy;
        Descending = descending;
    }

    private int? PageNumber { get; }
    private string? OrderBy { get; }
    private bool Descending { get; }
    
    // ReSharper disable once UnusedType.Global
    public class GetTopBattingCareersHandler : IRequestHandler<GetTopBattingCareersRequest, OneOf<List<PlayerCareerDto>, Exception>>
    {
        private readonly IPlayerRepository _playerRepository;

        public GetTopBattingCareersHandler(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<OneOf<List<PlayerCareerDto>, Exception>> Handle(GetTopBattingCareersRequest request, CancellationToken cancellationToken) =>
            await _playerRepository.GetTopBattingCareers(request.PageNumber, request.OrderBy, request.Descending, cancellationToken);
    }
}