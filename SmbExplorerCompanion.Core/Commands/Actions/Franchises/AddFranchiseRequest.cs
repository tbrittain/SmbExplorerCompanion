using MediatR;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Actions.Franchises;

public class AddFranchiseRequest(string name) : IRequest<FranchiseDto>
{
    private string Name { get; } = name;

    // ReSharper disable once UnusedType.Global
    internal class AddFranchiseHandler(IAddRepository<FranchiseDto> franchiseRepository) : IRequestHandler<AddFranchiseRequest, FranchiseDto>
    {
        public Task<FranchiseDto> Handle(AddFranchiseRequest request, CancellationToken cancellationToken)
        {
            var franchise = new FranchiseDto
            {
                Name = request.Name
            };
            return franchiseRepository.AddAsync(franchise, cancellationToken);
        }
    }
}