using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmbExplorerCompanion.Database.Entities;
using SmbExplorerCompanion.Database.Entities.Lookups;
using static SmbExplorerCompanion.Shared.Constants.FileExports;

namespace SmbExplorerCompanion.Database;

public class SmbExplorerCompanionDbContext : DbContext
{
    public SmbExplorerCompanionDbContext()
    {
    }

    public SmbExplorerCompanionDbContext(DbContextOptions<SmbExplorerCompanionDbContext> options) : base(options)
    {
    }

    public virtual DbSet<Chemistry> Chemistry { get; set; } = default!;
    public virtual DbSet<PitcherRole> PitcherRoles { get; set; } = default!;
    public virtual DbSet<PitchType> PitchTypes { get; set; } = default!;
    public virtual DbSet<PlayerAward> PlayerAwards { get; set; } = default!;
    public virtual DbSet<Position> Positions { get; set; } = default!;
    public virtual DbSet<Trait> Traits { get; set; } = default!;
    public virtual DbSet<BatHandedness> BatHandedness { get; set; } = default!;
    public virtual DbSet<ThrowHandedness> ThrowHandedness { get; set; } = default!;

    public virtual DbSet<Conference> Conferences { get; set; } = default!;
    public virtual DbSet<Division> Divisions { get; set; } = default!;
    public virtual DbSet<Franchise> Franchises { get; set; } = default!;
    public virtual DbSet<Player> Players { get; set; } = default!;
    public virtual DbSet<PlayerGameIdHistory> PlayerGameIdHistory { get; set; } = default!;
    public virtual DbSet<PlayerSeason> PlayerSeasons { get; set; } = default!;
    public virtual DbSet<PlayerSeasonBattingStat> PlayerSeasonBattingStats { get; set; } = default!;
    public virtual DbSet<PlayerSeasonGameStat> PlayerSeasonGameStats { get; set; } = default!;
    public virtual DbSet<PlayerSeasonPitchingStat> PlayerSeasonPitchingStats { get; set; } = default!;
    public virtual DbSet<PlayerTeamHistory> PlayerTeamHistory { get; set; } = default!;
    public virtual DbSet<Season> Seasons { get; set; } = default!;
    public virtual DbSet<SeasonTeamHistory> SeasonTeamHistory { get; set; } = default!;
    public virtual DbSet<Team> Teams { get; set; } = default!;
    public virtual DbSet<TeamSeasonSchedule> TeamSeasonSchedules { get; set; } = default!;
    public virtual DbSet<TeamPlayoffSchedule> TeamPlayoffSchedules { get; set; } = default!;
    public virtual DbSet<ChampionshipWinner> ChampionshipWinners { get; set; } = default!;
    public virtual DbSet<TeamGameIdHistory> TeamGameIdHistory { get; set; } = default!;
    public virtual DbSet<TeamLogoHistory> TeamLogoHistory { get; set; } = default!;
    public virtual DbSet<TeamNameHistory> TeamNameHistory { get; set; } = default!;
    public virtual DbSet<LookupSeed> LookupSeeds { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = Path.Combine(BaseApplicationDirectory, "SmbExplorerCompanion.db");
        optionsBuilder.UseSqlite($"Data Source={connectionString}");

#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
#endif

        base.OnConfiguring(optionsBuilder);
    }

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

        modelBuilder.Entity<TeamPlayoffSchedule>()
            .HasOne(x => x.HomeTeamHistory)
            .WithMany(x => x.HomePlayoffSchedule)
            .HasForeignKey(x => x.HomeTeamHistoryId);

        modelBuilder.Entity<TeamPlayoffSchedule>()
            .HasOne(x => x.AwayTeamHistory)
            .WithMany(x => x.AwayPlayoffSchedule)
            .HasForeignKey(x => x.AwayTeamHistoryId);

        modelBuilder.Entity<TeamPlayoffSchedule>()
            .HasOne(x => x.HomePitcherSeason)
            .WithMany(x => x.HomePitchingPlayoffSchedule)
            .HasForeignKey(x => x.HomePitcherSeasonId);

        modelBuilder.Entity<TeamPlayoffSchedule>()
            .HasOne(x => x.AwayPitcherSeason)
            .WithMany(x => x.AwayPitchingPlayoffSchedule)
            .HasForeignKey(x => x.AwayPitcherSeasonId);

        modelBuilder.Entity<PlayerSeason>()
            .HasOne(x => x.SecondaryPosition)
            .WithMany(x => x.SecondaryPositionPlayers)
            .HasForeignKey(x => x.SecondaryPositionId);

        modelBuilder.Entity<Player>()
            .HasOne(x => x.PrimaryPosition)
            .WithMany(x => x.PrimaryPositionPlayers)
            .HasForeignKey(x => x.PrimaryPositionId);

        modelBuilder.Entity<Season>()
            .HasOne(s => s.ChampionshipWinner)
            .WithOne(cw => cw.Season)
            .HasForeignKey<ChampionshipWinner>(cw => cw.SeasonId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        base.OnModelCreating(modelBuilder);
    }

    public static void SeedLookups(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmbExplorerCompanionDbContext>();

        var seedId = Guid.Parse("8833f86a-cf8d-4674-b621-bca065272ba2");
        if (dbContext.LookupSeeds.Any(x => x.Id == seedId)) return;

        using var transaction = dbContext.Database.BeginTransaction();

        dbContext.BatHandedness.AddRange(
            new BatHandedness {Name = "R"},
            new BatHandedness {Name = "L"},
            new BatHandedness {Name = "S"}
        );

        dbContext.ThrowHandedness.AddRange(
            new ThrowHandedness {Name = "R"},
            new ThrowHandedness {Name = "L"}
        );

        dbContext.PitcherRoles.AddRange(
            new PitcherRole {Name = "SP"},
            new PitcherRole {Name = "RP"},
            new PitcherRole {Name = "SP/RP"},
            new PitcherRole {Name = "CP"}
        );

        dbContext.Positions.AddRange(
            new Position {Name = "P", IsPrimaryPosition = true},
            new Position {Name = "C", IsPrimaryPosition = true},
            new Position {Name = "1B", IsPrimaryPosition = true},
            new Position {Name = "2B", IsPrimaryPosition = true},
            new Position {Name = "3B", IsPrimaryPosition = true},
            new Position {Name = "SS", IsPrimaryPosition = true},
            new Position {Name = "LF", IsPrimaryPosition = true},
            new Position {Name = "CF", IsPrimaryPosition = true},
            new Position {Name = "RF", IsPrimaryPosition = true},
            new Position {Name = "IF"},
            new Position {Name = "OF"},
            new Position {Name = "1B/OF"},
            new Position {Name = "IF/OF"}
        );

        var chemistryTypes = new List<Chemistry>
        {
            new() {Name = "Competitive"},
            new() {Name = "Spirited"},
            new() {Name = "Disciplined"},
            new() {Name = "Scholarly"},
            new() {Name = "Crafty"}
        };
        dbContext.Chemistry.AddRange(chemistryTypes);

        var playerAwards = new List<PlayerAward>
        {
            new()
            {
                Name = "MVP",
                OriginalName = "MVP",
                Importance = 0,
                IsBuiltIn = true,
                IsBattingAward = true,
                IsPitchingAward = true,
                IsUserAssignable = true
            },
            new()
            {
                Name = "Triple Crown (Batting)",
                OriginalName = "Triple Crown (Batting)",
                Importance = 0,
                IsBuiltIn = true,
                IsBattingAward = true
            },
            new()
            {
                Name = "Triple Crown (Pitching)",
                OriginalName = "Triple Crown (Pitching)",
                Importance = 0,
                IsBuiltIn = true,
                IsPitchingAward = true
            },
            new()
            {
                Name = "Cy Young",
                OriginalName = "Cy Young",
                Importance = 1,
                IsBuiltIn = true,
                IsPitchingAward = true,
                IsUserAssignable = true
            },
            new()
            {
                Name = "Silver Slugger",
                OriginalName = "Silver Slugger",
                Importance = 1,
                IsBuiltIn = true,
                IsBattingAward = true,
                IsUserAssignable = true
            },
            new()
            {
                Name = "ROY",
                OriginalName = "ROY",
                Importance = 1,
                IsBuiltIn = true,
                IsBattingAward = true,
                IsUserAssignable = true
            },
            new()
            {
                Name = "Gold Glove",
                OriginalName = "Gold Glove",
                Importance = 2,
                IsBuiltIn = true,
                IsFieldingAward = true,
                IsUserAssignable = true
            },
            new()
            {
                Name = "Playoff MVP",
                OriginalName = "Playoff MVP",
                Importance = 2,
                IsBuiltIn = true,
                IsBattingAward = true,
                IsPitchingAward = true,
                IsPlayoffAward = true,
                IsUserAssignable = true
            },
            new()
            {
                Name = "Championship MVP",
                OriginalName = "Championship MVP",
                Importance = 2,
                IsBuiltIn = true,
                IsBattingAward = true,
                IsPitchingAward = true,
                IsPlayoffAward = true,
                IsUserAssignable = true
            },
            new()
            {
                Name = "Batting Title",
                OriginalName = "Batting Title",
                Importance = 3,
                IsBuiltIn = true,
                IsBattingAward = true
            },
            new()
            {
                Name = "Home Run Title",
                OriginalName = "Home Run Title",
                Importance = 3,
                IsBuiltIn = true,
                IsBattingAward = true
            },
            new()
            {
                Name = "RBI Title",
                OriginalName = "RBI Title",
                Importance = 3,
                IsBuiltIn = true,
                IsBattingAward = true
            },
            new()
            {
                Name = "ERA Title",
                OriginalName = "ERA Title",
                Importance = 3,
                IsBuiltIn = true,
                IsPitchingAward = true
            },
            new()
            {
                Name = "Wins Title",
                OriginalName = "Wins Title",
                Importance = 3,
                IsBuiltIn = true,
                IsPitchingAward = true
            },
            new()
            {
                Name = "Strikeouts Title",
                OriginalName = "Strikeouts Title",
                Importance = 3,
                IsBuiltIn = true,
                IsPitchingAward = true
            },
            new()
            {
                Name = "All-Star",
                OriginalName = "All-Star",
                Importance = 4,
                IsBuiltIn = true,
                IsUserAssignable = true,
                IsPitchingAward = true,
                IsBattingAward = true,
            },
            new()
            {
                Name = "MVP-2",
                OriginalName = "MVP-2",
                Importance = 5,
                IsBuiltIn = true,
                OmitFromGroupings = true,
                IsUserAssignable = true,
                IsPitchingAward = true,
                IsBattingAward = true,
            },
            new()
            {
                Name = "MVP-3",
                OriginalName = "MVP-3",
                Importance = 5,
                IsBuiltIn = true,
                OmitFromGroupings = true,
                IsUserAssignable = true,
                IsPitchingAward = true,
                IsBattingAward = true,
            },
            new()
            {
                Name = "MVP-4",
                OriginalName = "MVP-4",
                Importance = 5,
                IsBuiltIn = true,
                OmitFromGroupings = true,
                IsUserAssignable = true,
                IsPitchingAward = true,
                IsBattingAward = true,
            },
            new()
            {
                Name = "MVP-5",
                OriginalName = "MVP-5",
                Importance = 5,
                IsBuiltIn = true,
                OmitFromGroupings = true,
                IsUserAssignable = true,
                IsPitchingAward = true,
                IsBattingAward = true,
            },
            new()
            {
                Name = "Cy Young-2",
                OriginalName = "Cy Young-2",
                Importance = 5,
                IsBuiltIn = true,
                OmitFromGroupings = true,
                IsUserAssignable = true,
                IsPitchingAward = true
            },
            new()
            {
                Name = "Cy Young-3",
                OriginalName = "Cy Young-3",
                Importance = 5,
                IsBuiltIn = true,
                OmitFromGroupings = true,
                IsUserAssignable = true,
                IsPitchingAward = true
            },
            new()
            {
                Name = "Cy Young-4",
                OriginalName = "Cy Young-4",
                Importance = 5,
                IsBuiltIn = true,
                OmitFromGroupings = true,
                IsUserAssignable = true,
                IsPitchingAward = true
            },
            new()
            {
                Name = "Cy Young-5",
                OriginalName = "Cy Young-5",
                Importance = 5,
                IsBuiltIn = true,
                OmitFromGroupings = true,
                IsUserAssignable = true,
                IsPitchingAward = true
            },
            new()
            {
                Name = "ROY-2",
                OriginalName = "ROY-2",
                Importance = 5,
                IsBuiltIn = true,
                OmitFromGroupings = true,
                IsUserAssignable = true,
                IsPitchingAward = true,
                IsBattingAward = true,
            },
            new()
            {
                Name = "ROY-3",
                OriginalName = "ROY-3",
                Importance = 5,
                IsBuiltIn = true,
                OmitFromGroupings = true,
                IsUserAssignable = true,
                IsPitchingAward = true,
                IsBattingAward = true,
            },
            new()
            {
                Name = "ROY-4",
                OriginalName = "ROY-4",
                Importance = 5,
                IsBuiltIn = true,
                OmitFromGroupings = true,
                IsUserAssignable = true,
                IsPitchingAward = true,
                IsBattingAward = true,
            },
            new()
            {
                Name = "ROY-5",
                OriginalName = "ROY-5",
                Importance = 5,
                IsBuiltIn = true,
                OmitFromGroupings = true,
                IsUserAssignable = true,
                IsPitchingAward = true,
                IsBattingAward = true,
            }
        };

        dbContext.PlayerAwards.AddRange(playerAwards);

        dbContext.SaveChanges();

        dbContext.PitchTypes.AddRange(
            new PitchType {Name = "4F"},
            new PitchType {Name = "2F"},
            new PitchType {Name = "SB"},
            new PitchType {Name = "CH"},
            new PitchType {Name = "FK"},
            new PitchType {Name = "CB"},
            new PitchType {Name = "SL"},
            new PitchType {Name = "CF"}
        );

        var competitiveChemistry = chemistryTypes.Single(x => x.Name == "Competitive");
        var spiritedChemistry = chemistryTypes.Single(x => x.Name == "Spirited");
        var disciplinedChemistry = chemistryTypes.Single(x => x.Name == "Disciplined");
        var scholarlyChemistry = chemistryTypes.Single(x => x.Name == "Scholarly");
        var craftyChemistry = chemistryTypes.Single(x => x.Name == "Crafty");

        var smb4Traits = new List<Trait>
        {
            new()
            {
                Name = "Ace Exterminator",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Bad Ball Hitter",
                Chemistry = craftyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Bad Jumps",
                Chemistry = craftyChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Base Jogger",
                Chemistry = disciplinedChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Base Rounder",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "BB Prone",
                Chemistry = disciplinedChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Big Hack",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Bunter",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Butter Fingers",
                Chemistry = disciplinedChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Cannon Arm",
                Chemistry = competitiveChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Choker",
                Chemistry = spiritedChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Clutch",
                Chemistry = spiritedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Composed",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "CON vs LHP",
                Chemistry = spiritedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "CON vs RHP",
                Chemistry = spiritedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Consistent",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Crossed Up",
                Chemistry = scholarlyChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Distractor",
                Chemistry = craftyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Dive Wizard",
                Chemistry = spiritedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Durable",
                Chemistry = competitiveChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Easy Jumps",
                Chemistry = craftyChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Easy Target",
                Chemistry = craftyChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Elite 2F",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Elite 4F",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Elite CB",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Elite CF",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Elite CH",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Elite FK",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Elite SB",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Elite SL",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Falls Behind",
                Chemistry = scholarlyChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Fastball Hitter",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "First Pitch Prayer",
                Chemistry = competitiveChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "First Pitch Slayer",
                Chemistry = competitiveChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Gets Ahead",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "High Pitch",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Injury Prone",
                Chemistry = competitiveChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Inside Pitch",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "K Collector",
                Chemistry = competitiveChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "K Neglector",
                Chemistry = competitiveChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Little Hack",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Low Pitch",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Magic Hands",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Meltdown",
                Chemistry = spiritedChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Metal Head",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Mind Gamer",
                Chemistry = craftyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Noodle Arm",
                Chemistry = competitiveChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Off-speed Hitter",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Outside Pitch",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Pick Officer",
                Chemistry = craftyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Pinch Perfect",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "POW vs LHP",
                Chemistry = spiritedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "POW vs RHP",
                Chemistry = spiritedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Rally Starter",
                Chemistry = spiritedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Rally Stopper",
                Chemistry = spiritedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "RBI Hero",
                Chemistry = spiritedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "RBI Zero",
                Chemistry = spiritedChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Reverse Splits",
                Chemistry = craftyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Sign Stealer",
                Chemistry = craftyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Slow Poke",
                Chemistry = competitiveChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Specialist",
                Chemistry = craftyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Sprinter",
                Chemistry = competitiveChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Stealer",
                Chemistry = craftyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Stimulated",
                Chemistry = craftyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Surrounded",
                Chemistry = spiritedChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Tough Out",
                Chemistry = competitiveChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Two Way (C)",
                Chemistry = spiritedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Two Way (IF)",
                Chemistry = spiritedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Two Way (OF)",
                Chemistry = spiritedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Utility",
                Chemistry = scholarlyChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Volatile",
                Chemistry = disciplinedChemistry,
                IsPositive = true,
                IsSmb3 = false
            },
            new()
            {
                Name = "Whiffer",
                Chemistry = competitiveChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Wild Thing",
                Chemistry = spiritedChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Wild Thrower",
                Chemistry = craftyChemistry,
                IsPositive = false,
                IsSmb3 = false
            },
            new()
            {
                Name = "Workhorse",
                Chemistry = competitiveChemistry,
                IsPositive = true,
                IsSmb3 = false
            }
        };

        dbContext.Traits.AddRange(smb4Traits);

        var smb3Traits = new List<Trait>
        {
            new()
            {
                Name = "POW vs RHP",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "POW vs LHP",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "CON vs RHP",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "CON vs LHP",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "RBI Man",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "RBI Dud",
                IsPositive = false,
                IsSmb3 = true
            },
            new()
            {
                Name = "High Pitch",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "Low Pitch",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "Inside Pitch",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "Outside Pitch",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "Tough Out",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "Whiffer",
                IsPositive = false,
                IsSmb3 = true
            },
            new()
            {
                Name = "Stealer",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "Bad Jumps",
                IsPositive = false,
                IsSmb3 = true
            },
            new()
            {
                Name = "Utility",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "Composed",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "BB Prone",
                IsPositive = false,
                IsSmb3 = true
            },
            new()
            {
                Name = "Specialist",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "K Man",
                IsPositive = true,
                IsSmb3 = true
            },
            new()
            {
                Name = "K Dud",
                IsPositive = false,
                IsSmb3 = true
            }
        };

        dbContext.Traits.AddRange(smb3Traits);
        dbContext.LookupSeeds.Add(new LookupSeed {Id = seedId});

        dbContext.SaveChanges();
        transaction.CommitAsync();
    }

    public static void ApplyMigrations(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmbExplorerCompanionDbContext>();
        if (dbContext.Database.GetPendingMigrations().Any()) dbContext.Database.Migrate();
    }
}