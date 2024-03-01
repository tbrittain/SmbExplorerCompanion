using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetThrowHandednessRequest : IRequest<OneOf<List<ThrowHandednessDto>, Exception>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetThrowHandednessRequestHandler : IRequestHandler<GetThrowHandednessRequest, OneOf<List<ThrowHandednessDto>, Exception>>
    {
        private readonly IRepository<ThrowHandednessDto> _throwHandednessRepository;

        public GetThrowHandednessRequestHandler(IRepository<ThrowHandednessDto> throwHandednessRepository)
        {
            _throwHandednessRepository = throwHandednessRepository;
        }

        public async Task<OneOf<List<ThrowHandednessDto>, Exception>> Handle(GetThrowHandednessRequest request,
            CancellationToken cancellationToken)
        {
            var throwHandednessResult = await _throwHandednessRepository.GetAllAsync(cancellationToken);
            if (throwHandednessResult.TryPickT1(out var exception, out var throwHandednessDtos))
            {
                return exception;
            }

            return throwHandednessDtos.ToList();
        }
    }
}