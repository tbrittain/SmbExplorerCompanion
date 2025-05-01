using MediatR;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetPitcherRolesRequest : IRequest<List<PitcherRoleDto>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetAllPitcherRolesHandler(IGetAllRepository<PitcherRoleDto> pitcherRoleRepository)
        : IRequestHandler<GetPitcherRolesRequest, List<PitcherRoleDto>>
    {
        public async Task<List<PitcherRoleDto>> Handle(GetPitcherRolesRequest request, CancellationToken cancellationToken)
        {
            var pitcherRoleResult = await pitcherRoleRepository.GetAllAsync(cancellationToken);
            return pitcherRoleResult.ToList();
        }
    }
}