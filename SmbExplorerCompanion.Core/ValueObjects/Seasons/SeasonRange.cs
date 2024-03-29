﻿namespace SmbExplorerCompanion.Core.ValueObjects.Seasons;

public record struct SeasonRange
{
    public SeasonRange(int startSeasonId, int endSeasonId)
    {
        if (startSeasonId > endSeasonId)
            throw new InvalidOperationException("Start cannot be greater than End");

        StartSeasonId = startSeasonId;
        EndSeasonId = endSeasonId;
    }

    public SeasonRange(int startSeasonId)
    {
        StartSeasonId = startSeasonId;
        EndSeasonId = startSeasonId;
    }

    public int StartSeasonId { get; }
    public int EndSeasonId { get; }
}