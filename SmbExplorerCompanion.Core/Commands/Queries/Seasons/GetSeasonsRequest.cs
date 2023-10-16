using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Seasons;

public class GetSeasonsRequest : IRequest<OneOf<List<SeasonDto>, Exception>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetSeasonsHandler : IRequestHandler<GetSeasonsRequest, OneOf<List<SeasonDto>, Exception>>
    {
        private readonly IRepository<SeasonDto> _seasonRepository;

        public GetSeasonsHandler(IRepository<SeasonDto> seasonRepository)
        {
            _seasonRepository = seasonRepository;
        }

        public async Task<OneOf<List<SeasonDto>, Exception>> Handle(GetSeasonsRequest request, CancellationToken cancellationToken)
        {
            var seasonsResponse = await _seasonRepository.GetAllAsync(cancellationToken);
            if (seasonsResponse.TryPickT1(out var exception, out var seasons))
                return exception;

            return seasons.ToList();
        }
    }
}