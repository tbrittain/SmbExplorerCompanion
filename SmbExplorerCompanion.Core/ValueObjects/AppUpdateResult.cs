namespace SmbExplorerCompanion.Core.ValueObjects;

public readonly record struct AppUpdateResult(Version Version, string ReleasePageUrl, DateTime ReleaseDate)
{
    public int DaysSinceRelease => (DateTime.Now - ReleaseDate).Days;
}