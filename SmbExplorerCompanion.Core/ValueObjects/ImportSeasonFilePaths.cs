namespace SmbExplorerCompanion.Core.ValueObjects;

public record ImportSeasonFilePaths(
    string Teams,
    string OverallPlayers,
    string SeasonStatsPitching,
    string SeasonStatsBatting,
    string SeasonSchedule)
{
    public IEnumerator<string> GetEnumerator()
    {
        yield return Teams;
        yield return OverallPlayers;
        yield return SeasonStatsPitching;
        yield return SeasonStatsBatting;
        yield return SeasonSchedule;
    }
}