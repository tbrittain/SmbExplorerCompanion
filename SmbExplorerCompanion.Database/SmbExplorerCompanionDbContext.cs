using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Database.Entities;
using SmbExplorerCompanion.Database.Entities.Lookups;

namespace SmbExplorerCompanion.Database;

public class SmbExplorerCompanionDbContext : DbContext
{
    public virtual DbSet<Chemistry> Chemistry { get; set; } = default!;
    public virtual DbSet<PitcherRole> PitcherRoles { get; set; } = default!;
    public virtual DbSet<PitchType> PitchTypes { get; set; } = default!;
    public virtual DbSet<PlayerAward> PlayerAwards { get; set; } = default!;
    public virtual DbSet<Position> Positions { get; set; } = default!;
    public virtual DbSet<Trait> Traits { get; set; } = default!;
    
    public virtual DbSet<Conference> Conferences { get; set; } = default!;
    public virtual DbSet<Division> Divisions { get; set; } = default!;
    public virtual DbSet<Franchise> Franchises { get; set; } = default!;
    public virtual DbSet<PitcherPitchTypeHistory> PitcherPitchTypeHistory { get; set; } = default!;
    public virtual DbSet<Player> Players { get; set; } = default!;
    public virtual DbSet<PlayerGameIdHistory> PlayerGameIdHistory { get; set; } = default!;
    public virtual DbSet<PlayerSeason> PlayerSeasons { get; set; } = default!;
    public virtual DbSet<PlayerSeasonAward> PlayerSeasonAwards { get; set; } = default!;
    public virtual DbSet<PlayerSeasonBattingStat> PlayerSeasonBattingStats { get; set; } = default!;
    public virtual DbSet<PlayerSeasonGameStat> PlayerSeasonGameStats { get; set; } = default!;
    public virtual DbSet<PlayerSeasonPitchingStat> PlayerSeasonPitchingStats { get; set; } = default!;
    public virtual DbSet<PlayerSecondaryPositionHistory> PlayerSecondaryPositionHistory { get; set; } = default!;
    public virtual DbSet<PlayerTeamHistory> PlayerTeamHistory { get; set; } = default!;
    public virtual DbSet<Season> Seasons { get; set; } = default!;
    public virtual DbSet<SeasonTeamHistory> SeasonTeamHistory { get; set; } = default!;
    public virtual DbSet<Team> Teams { get; set; } = default!;
    public virtual DbSet<TeamDivisionHistory> TeamDivisionHistory { get; set; } = default!;
    public virtual DbSet<TeamSeasonSchedule> TeamSeasonSchedule { get; set; } = default!;
    public virtual DbSet<BatHandedness> BatHandedness { get; set; } = default!;
    public virtual DbSet<ThrowHandedness> ThrowHandedness { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeamSeasonSchedule>()
            .HasOne(x => x.HomeTeamHistory)
            .WithMany(x => x.HomeSeasonSchedule)
            .HasForeignKey(x => x.HomeTeamHistoryId);
        
        modelBuilder.Entity<TeamSeasonSchedule>()
            .HasOne(x => x.AwayTeamHistory)
            .WithMany(x => x.AwaySeasonSchedule)
            .HasForeignKey(x => x.AwayTeamHistoryId);
        
        modelBuilder.Entity<TeamSeasonSchedule>()
            .HasOne(x => x.HomePitcherSeason)
            .WithMany(x => x.HomePitchingSchedule)
            .HasForeignKey(x => x.HomePitcherSeasonId);
        
        modelBuilder.Entity<TeamSeasonSchedule>()
            .HasOne(x => x.AwayPitcherSeason)
            .WithMany(x => x.AwayPitchingSchedule)
            .HasForeignKey(x => x.AwayPitcherSeasonId);

        base.OnModelCreating(modelBuilder);
    }
}