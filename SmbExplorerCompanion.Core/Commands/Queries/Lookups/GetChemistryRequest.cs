using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetChemistryRequest : IRequest<OneOf<List<ChemistryDto>, Exception>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetChemistryRequestHandler : IRequestHandler<GetChemistryRequest, OneOf<List<ChemistryDto>, Exception>>
    {
        private readonly IRepository<ChemistryDto> _chemistryRepository;

        public GetChemistryRequestHandler(IRepository<ChemistryDto> chemistryRepository)
        {
            _chemistryRepository = chemistryRepository;
        }

        public async Task<OneOf<List<ChemistryDto>, Exception>> Handle(GetChemistryRequest request, CancellationToken cancellationToken)
        {
            var chemistryResult = await _chemistryRepository.GetAllAsync(cancellationToken);
            if (chemistryResult.TryPickT1(out var exception, out var chemistryDtos))
            {
                return exception;
            }

            return chemistryDtos.ToList();
        }
    }
}