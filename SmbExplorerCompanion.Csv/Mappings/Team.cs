using CsvHelper.Configuration;

namespace SmbExplorerCompanion.Csv.Mappings;

public class Team
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = default!;
    public string DivisionName { get; set; } = default!;

    public string ConferenceName { get; set; } = default!;

    // TODO: Expose this property on the export
    public int SeasonId { get; set; }
    public int SeasonNum { get; set; }
    public int Budget { get; set; }
    public int Payroll { get; set; }
    public int Surplus { get; set; }
    public int SurplusPerGame { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int RunDifferential { get; set; }
    public int RunsFor { get; set; }
    public int RunsAgainst { get; set; }
    public double GamesBehind { get; set; }

    public double WinPercentage { get; set; }

    // TODO: Make sure the expected stats are on the database objects
    public double PythagoreanWinPercentage { get; set; }
    public int ExpectedWins { get; set; }
    public int ExpectedLosses { get; set; }
    public int TotalPower { get; set; }
    public int TotalContact { get; set; }
    public int TotalSpeed { get; set; }
    public int TotalFielding { get; set; }
    public int TotalArm { get; set; }
    public int TotalVelocity { get; set; }
    public int TotalJunk { get; set; }
    public int TotalAccuracy { get; set; }

    public sealed class TeamCsvMapping : ClassMap<Team>
    {
        public TeamCsvMapping()
        {
            Map(x => x.TeamId).Name("TeamId");
            Map(x => x.TeamName).Name("Team");
            Map(x => x.DivisionName).Name("Division");
            Map(x => x.ConferenceName).Name("Conference");
            Map(x => x.SeasonId).Name("SeasonId");
            Map(x => x.SeasonNum).Name("Season");
            Map(x => x.Budget).Name("Budget");
            Map(x => x.Payroll).Name("Payroll");
            Map(x => x.Surplus).Name("Surplus");
            Map(x => x.SurplusPerGame).Name("Surplus/Game");
            Map(x => x.Wins).Name("W");
            Map(x => x.Losses).Name("L");
            Map(x => x.RunDifferential).Name("Run Differential");
            Map(x => x.RunsFor).Name("Runs For");
            Map(x => x.RunsAgainst).Name("Runs Against");
            Map(x => x.GamesBehind).Name("GB");
            Map(x => x.WinPercentage).Name("WPCT");
            Map(x => x.PythagoreanWinPercentage).Name("Pythagorean WPCT");
            Map(x => x.ExpectedWins).Name("Expected W");
            Map(x => x.ExpectedLosses).Name("Expected L");
            Map(x => x.TotalPower).Name("Power");
            Map(x => x.TotalContact).Name("Contact");
            Map(x => x.TotalSpeed).Name("Speed");
            Map(x => x.TotalFielding).Name("Fielding");
            Map(x => x.TotalArm).Name("Arm");
            Map(x => x.TotalVelocity).Name("Velocity");
            Map(x => x.TotalJunk).Name("Junk");
            Map(x => x.TotalAccuracy).Name("Accuracy");
        }
    }
}