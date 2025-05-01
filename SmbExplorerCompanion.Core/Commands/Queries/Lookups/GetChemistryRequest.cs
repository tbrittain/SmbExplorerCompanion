using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetChemistryRequest : IRequest<List<ChemistryDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetChemistryRequestHandler(IGetAllRepository<ChemistryDto> chemistryRepository)
        : IRequestHandler<GetChemistryRequest, List<ChemistryDto>>
    {
        public async Task<List<ChemistryDto>> Handle(GetChemistryRequest request, CancellationToken cancellationToken)
        {
            var chemistryResult = await chemistryRepository.GetAllAsync(cancellationToken);
            return chemistryResult.ToList();
        }
    }
}