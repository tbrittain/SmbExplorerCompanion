﻿using MediatR;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Franchises;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Core.Commands.Queries.Franchises;

public class GetAllFranchisesRequest : IRequest<OneOf<IEnumerable<FranchiseDto>, Exception>>
{
    // ReSharper disable once UnusedType.Global
    internal class GetAllFranchisesHandler : IRequestHandler<GetAllFranchisesRequest, OneOf<IEnumerable<FranchiseDto>, Exception>>
    {
        private readonly IRepository<FranchiseDto> _franchiseRepository;

        public GetAllFranchisesHandler(IRepository<FranchiseDto> franchiseRepository)
        {
            _franchiseRepository = franchiseRepository;
        }

        public async Task<OneOf<IEnumerable<FranchiseDto>, Exception>>
            Handle(GetAllFranchisesRequest request, CancellationToken cancellationToken) =>
            await _franchiseRepository.GetAllAsync(cancellationToken);
    }
}