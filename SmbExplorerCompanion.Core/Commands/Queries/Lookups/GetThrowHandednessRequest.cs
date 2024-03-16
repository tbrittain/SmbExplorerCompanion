using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetThrowHandednessRequest : IRequest<List<ThrowHandednessDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetThrowHandednessRequestHandler : IRequestHandler<GetThrowHandednessRequest, List<ThrowHandednessDto>>
    {
        private readonly IRepository<ThrowHandednessDto> _throwHandednessRepository;

        public GetThrowHandednessRequestHandler(IRepository<ThrowHandednessDto> throwHandednessRepository)
        {
            _throwHandednessRepository = throwHandednessRepository;
        }

        public async Task<List<ThrowHandednessDto>> Handle(GetThrowHandednessRequest request,
            CancellationToken cancellationToken)
        {
            var throwHandednessResult = await _throwHandednessRepository.GetAllAsync(cancellationToken);
            return throwHandednessResult.ToList();
        }
    }
}