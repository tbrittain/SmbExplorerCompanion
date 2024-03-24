using MediatR;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Actions.Franchises;

public class AddFranchiseRequest : IRequest<FranchiseDto>
{
    public AddFranchiseRequest(string name)
    {
        Name = name;
    }

    private string Name { get; }

    // ReSharper disable once UnusedType.Global
    internal class AddFranchiseHandler : IRequestHandler<AddFranchiseRequest, FranchiseDto>
    {
        private readonly IRepository<FranchiseDto> _franchiseRepository;

        public AddFranchiseHandler(IRepository<FranchiseDto> franchiseRepository)
        {
            _franchiseRepository = franchiseRepository;
        }

        public Task<FranchiseDto> Handle(AddFranchiseRequest request, CancellationToken cancellationToken)
        {
            var franchise = new FranchiseDto
            {
                Name = request.Name
            };
            return _franchiseRepository.AddAsync(franchise, cancellationToken);
        }
    }
}