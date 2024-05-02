using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetPitcherRolesRequest : IRequest<List<PitcherRoleDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetAllPitcherRolesHandler : IRequestHandler<GetPitcherRolesRequest, List<PitcherRoleDto>>
    {
        private readonly IRepository<PitcherRoleDto> _pitcherRoleRepository;

        public GetAllPitcherRolesHandler(IRepository<PitcherRoleDto> pitcherRoleRepository)
        {
            _pitcherRoleRepository = pitcherRoleRepository;
        }

        public async Task<List<PitcherRoleDto>> Handle(GetPitcherRolesRequest request, CancellationToken cancellationToken)
        {
            var pitcherRoleResult = await _pitcherRoleRepository.GetAllAsync(cancellationToken);
            return pitcherRoleResult.ToList();
        }
    }
}