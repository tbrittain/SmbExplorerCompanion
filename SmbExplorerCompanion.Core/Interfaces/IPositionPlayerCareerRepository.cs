﻿using SmbExplorerCompanion.Core.Entities.Players;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IPositionPlayerCareerRepository
{
    public Task<List<PlayerCareerBattingDto>> GetBattingCareers(
        int? pageNumber = null,
        int? limit = null,
        string? orderBy = null,
        bool descending = true,
        int? playerId = null,
        bool onlyHallOfFamers = false,
        int? primaryPositionId = null,
        bool onlyActivePlayers = false,
        CancellationToken cancellationToken = default);
    
    public Task<List<PlayerCareerBattingDto>> GetHallOfFameCandidates(int seasonId,
        CancellationToken cancellationToken = default);
    
    public Task<List<SimilarPlayerDto>> GetSimilarBattingCareers(
        int playerId,
        CancellationToken cancellationToken = default);
}