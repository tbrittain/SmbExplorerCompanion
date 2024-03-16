using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Lookups;

public class GetPitcherRolesRequest : IRequest<OneOf<List<PitcherRoleDto>, Exception>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetAllPitcherRolesHandler : IRequestHandler<GetPitcherRolesRequest, OneOf<List<PitcherRoleDto>, Exception>>
    {
        private readonly IRepository<PitcherRoleDto> _pitcherRoleRepository;

        public GetAllPitcherRolesHandler(IRepository<PitcherRoleDto> pitcherRoleRepository)
        {
            _pitcherRoleRepository = pitcherRoleRepository;
        }

        public async Task<OneOf<List<PitcherRoleDto>, Exception>> Handle(GetPitcherRolesRequest request, CancellationToken cancellationToken)
        {
            var pitcherRoleResult = await _pitcherRoleRepository.GetAllAsync(cancellationToken);
            if (pitcherRoleResult.TryPickT1(out var exception, out var pitcherRoleDtos))
            {
                return exception;
            }

            return pitcherRoleDtos.ToList();
        }
    }
}