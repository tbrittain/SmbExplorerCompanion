using SmbExplorerCompanion.Database.Entities.Lookups;

namespace SmbExplorerCompanion.Database.Entities;

public class PlayerSeason
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public virtual Player Player { get; set; } = default!;
    public int SeasonId { get; set; }
    public virtual Season Season { get; set; } = default!;
    public int Age { get; set; }
    public int Salary { get; set; }

    public virtual ICollection<PitcherPitchTypeHistory> PitcherPitchTypeHistory { get; set; } =
        new HashSet<PitcherPitchTypeHistory>();

    public int SecondaryPositionId { get; set; }
    public virtual Position SecondaryPosition { get; set; } = default!;

    public virtual ICollection<PlayerTeamHistory> TeamHistory { get; set; } =
        new HashSet<PlayerTeamHistory>();

    // This is one-to-one because a player's game stats are not modified during the postseason
    public virtual PlayerSeasonGameStat GameStats { get; set; } = default!;

    public virtual ICollection<Trait> Traits { get; set; } = new HashSet<Trait>();

    public virtual ICollection<PlayerSeasonBattingStat> BattingStats { get; set; } =
        new HashSet<PlayerSeasonBattingStat>();

    public virtual ICollection<PlayerSeasonPitchingStat> PitchingStats { get; set; } =
        new HashSet<PlayerSeasonPitchingStat>();

    public virtual ICollection<PlayerAward> Awards { get; set; } =
        new HashSet<PlayerAward>();

    public virtual ICollection<TeamSeasonSchedule> HomePitchingSchedule { get; set; } =
        new HashSet<TeamSeasonSchedule>();

    public virtual ICollection<TeamSeasonSchedule> AwayPitchingSchedule { get; set; } =
        new HashSet<TeamSeasonSchedule>();

    public virtual ICollection<TeamPlayoffSchedule> HomePitchingPlayoffSchedule { get; set; } =
        new HashSet<TeamPlayoffSchedule>();

    public virtual ICollection<TeamPlayoffSchedule> AwayPitchingPlayoffSchedule { get; set; } =
        new HashSet<TeamPlayoffSchedule>();

    public virtual ChampionshipWinner? ChampionshipWinner { get; set; }
}