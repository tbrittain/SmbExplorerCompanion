using MediatR;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Franchises;

public class GetAllFranchisesRequest : IRequest<IEnumerable<FranchiseDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetAllFranchisesHandler : IRequestHandler<GetAllFranchisesRequest, IEnumerable<FranchiseDto>>
    {
        private readonly IRepository<FranchiseDto> _franchiseRepository;

        public GetAllFranchisesHandler(IRepository<FranchiseDto> franchiseRepository)
        {
            _franchiseRepository = franchiseRepository;
        }

        public async Task<IEnumerable<FranchiseDto>> Handle(GetAllFranchisesRequest request, CancellationToken cancellationToken) =>
            await _franchiseRepository.GetAllAsync(cancellationToken);
    }
}