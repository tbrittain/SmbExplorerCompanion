using CsvHelper.Configuration;

namespace SmbExplorerCompanion.Csv.Models;

public class OverallPlayer
{
    public Guid PlayerId { get; set; }
    public int SeasonId { get; set; }
    public int SeasonNum { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Position { get; set; } = default!;
    public string SecondaryPosition { get; set; } = default!;
    public string PitcherRole { get; set; } = default!;
    public int Power { get; set; }
    public int Contact { get; set; }
    public int Speed { get; set; }
    public int Fielding { get; set; }
    public int? Arm { get; set; }
    public int? Velocity { get; set; }
    public int? Junk { get; set; }
    public int? Accuracy { get; set; }

    public int Age { get; set; }

    // We should only count the salary if the player is on the active roster
    public int Salary { get; set; }
    public string? Trait1 { get; set; }
    public string? Trait2 { get; set; }
    public string Chemistry { get; set; } = default!;
    public string ThrowHand { get; set; } = default!;
    public string BatHand { get; set; } = default!;
    public string? Pitch1 { get; set; }
    public string? Pitch2 { get; set; }
    public string? Pitch3 { get; set; }
    public string? Pitch4 { get; set; }
    public string? Pitch5 { get; set; }

    public sealed class OverallPlayersCsvMapping : ClassMap<OverallPlayer>
    {
        public OverallPlayersCsvMapping()
        {
            // TODO: Need to expose the following two on the export
            Map(x => x.PlayerId).Name("PlayerId");
            Map(x => x.SeasonId).Name("SeasonId");
            Map(x => x.SeasonNum).Name("Season");
            Map(x => x.FirstName).Name("First Name");
            Map(x => x.LastName).Name("Last Name");
            Map(x => x.Position).Name("Position");
            Map(x => x.SecondaryPosition).Name("Secondary Position");
            Map(x => x.PitcherRole).Name("Pitcher Role");
            Map(x => x.Power).Name("Power");
            Map(x => x.Contact).Name("Contact");
            Map(x => x.Speed).Name("Speed");
            Map(x => x.Fielding).Name("Fielding");
            Map(x => x.Arm).Name("Arm");
            Map(x => x.Velocity).Name("Velocity");
            Map(x => x.Junk).Name("Junk");
            Map(x => x.Accuracy).Name("Accuracy");
            Map(x => x.Age).Name("Age");
            Map(x => x.Salary).Name("Salary");
            Map(x => x.Trait1).Name("Trait 1");
            Map(x => x.Trait2).Name("Trait 2");
            Map(x => x.Chemistry).Name("Chemistry");
            Map(x => x.ThrowHand).Name("Throw Hand");
            Map(x => x.BatHand).Name("Bat Hand");
            Map(x => x.Pitch1).Name("Pitch 1");
            Map(x => x.Pitch2).Name("Pitch 2");
            Map(x => x.Pitch3).Name("Pitch 3");
            Map(x => x.Pitch4).Name("Pitch 4");
            Map(x => x.Pitch5).Name("Pitch 5");
        }
    }
}