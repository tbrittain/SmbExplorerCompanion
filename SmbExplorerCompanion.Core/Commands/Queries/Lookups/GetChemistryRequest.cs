using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetChemistryRequest : IRequest<List<ChemistryDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetChemistryRequestHandler : IRequestHandler<GetChemistryRequest, List<ChemistryDto>>
    {
        private readonly IRepository<ChemistryDto> _chemistryRepository;

        public GetChemistryRequestHandler(IRepository<ChemistryDto> chemistryRepository)
        {
            _chemistryRepository = chemistryRepository;
        }

        public async Task<List<ChemistryDto>> Handle(GetChemistryRequest request, CancellationToken cancellationToken)
        {
            var chemistryResult = await _chemistryRepository.GetAllAsync(cancellationToken);
            return chemistryResult.ToList();
        }
    }
}