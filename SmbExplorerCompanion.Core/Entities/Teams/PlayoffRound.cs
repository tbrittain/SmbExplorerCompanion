namespace SmbExplorerCompanion.Core.Entities.Teams;

public enum PlayoffRound
{
    WildCard,
    DivisionSeries,
    ConferenceSeries,
    ChampionshipSeries
}

public static class PlayoffSeries
{
    /// <summary>
    ///     Given the max series number, we can infer the type of series that is being played.
    /// </summary>
    public static readonly Dictionary<int, List<PlayoffRoundMaxGame>> SeriesLengths = new()
    {
        {
            15, new List<PlayoffRoundMaxGame>
            {
                new(PlayoffRound.WildCard, 8),
                new(PlayoffRound.DivisionSeries, 12),
                new(PlayoffRound.ConferenceSeries, 14),
                new(PlayoffRound.ChampionshipSeries, 15)
            }
        },
        {
            7, new List<PlayoffRoundMaxGame>
            {
                new(PlayoffRound.DivisionSeries, 4),
                new(PlayoffRound.ConferenceSeries, 6),
                new(PlayoffRound.ChampionshipSeries, 7)
            }
        },
        {
            3, new List<PlayoffRoundMaxGame>
            {
                new(PlayoffRound.ConferenceSeries, 2),
                new(PlayoffRound.ChampionshipSeries, 3)
            }
        }
    };
}

public record PlayoffRoundMaxGame(PlayoffRound Round, int MaxSeriesNumber);