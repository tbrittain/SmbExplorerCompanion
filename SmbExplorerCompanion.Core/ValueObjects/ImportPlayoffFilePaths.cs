namespace SmbExplorerCompanion.Core.ValueObjects;

public record ImportPlayoffFilePaths(string PlayoffStatsPitching, string PlayoffStatsBatting, string PlayoffSchedule)
{
    public IEnumerator<string> GetEnumerator()
    {
        yield return PlayoffStatsPitching;
        yield return PlayoffStatsBatting;
        yield return PlayoffSchedule;
    }
}