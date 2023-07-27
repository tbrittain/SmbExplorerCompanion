using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Actions.Franchises;

public class AddFranchiseRequest : IRequest<OneOf<FranchiseDto, Exception>>
{
    public AddFranchiseRequest(string name)
    {
        Name = name;
    }

    private string Name { get; }
    
    // ReSharper disable once UnusedType.Global
    public class AddFranchiseHandler : IRequestHandler<AddFranchiseRequest, OneOf<FranchiseDto, Exception>>
    {
        private readonly IRepository<FranchiseDto> _franchiseRepository;

        public AddFranchiseHandler(IRepository<FranchiseDto> franchiseRepository)
        {
            _franchiseRepository = franchiseRepository;
        }

        public async Task<OneOf<FranchiseDto, Exception>> Handle(AddFranchiseRequest request, CancellationToken cancellationToken)
        {
            var franchise = new FranchiseDto
            {
                Name = request.Name
            };
            return await _franchiseRepository.AddAsync(franchise, cancellationToken);
        }
    }
}