﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmbExplorerCompanion.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BatHandedness",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatHandedness", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chemistry",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chemistry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Franchises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Franchises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PitcherRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PitcherRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PitchTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PitchTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerAwards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalName = table.Column<string>(type: "TEXT", nullable: false),
                    IsBuiltIn = table.Column<bool>(type: "INTEGER", nullable: false),
                    Importance = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAwards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThrowHandedness",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThrowHandedness", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Traits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsSmb3 = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPositive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ChemistryId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Traits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Traits_Chemistry_ChemistryId",
                        column: x => x.ChemistryId,
                        principalTable: "Chemistry",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Conferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsDesignatedHitter = table.Column<bool>(type: "INTEGER", nullable: false),
                    FranchiseId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conferences_Franchises_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchises",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    NumGames = table.Column<int>(type: "INTEGER", nullable: false),
                    FranchiseId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_Franchises_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    BatHandednessId = table.Column<int>(type: "INTEGER", nullable: false),
                    ThrowHandednessId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrimaryPositionId = table.Column<int>(type: "INTEGER", nullable: false),
                    PitcherRoleId = table.Column<int>(type: "INTEGER", nullable: true),
                    ChemistryId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_BatHandedness_BatHandednessId",
                        column: x => x.BatHandednessId,
                        principalTable: "BatHandedness",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Players_Chemistry_ChemistryId",
                        column: x => x.ChemistryId,
                        principalTable: "Chemistry",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Players_PitcherRoles_PitcherRoleId",
                        column: x => x.PitcherRoleId,
                        principalTable: "PitcherRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Players_Positions_PrimaryPositionId",
                        column: x => x.PrimaryPositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Players_ThrowHandedness_ThrowHandednessId",
                        column: x => x.ThrowHandednessId,
                        principalTable: "ThrowHandedness",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Divisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ConferenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    FranchiseId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Divisions_Conferences_ConferenceId",
                        column: x => x.ConferenceId,
                        principalTable: "Conferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Divisions_Franchises_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerGameIdHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGameIdHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerGameIdHistory_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSeasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    Salary = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSeasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSeasons_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerSeasons_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DivisionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PitcherPitchTypeHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    PitchTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PitcherPitchTypeHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PitcherPitchTypeHistory_PitchTypes_PitchTypeId",
                        column: x => x.PitchTypeId,
                        principalTable: "PitchTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PitcherPitchTypeHistory_PlayerSeasons_PlayerSeasonId",
                        column: x => x.PlayerSeasonId,
                        principalTable: "PlayerSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSeasonAwards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayerAwardId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSeasonAwards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonAwards_PlayerAwards_PlayerAwardId",
                        column: x => x.PlayerAwardId,
                        principalTable: "PlayerAwards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonAwards_PlayerSeasons_PlayerSeasonId",
                        column: x => x.PlayerSeasonId,
                        principalTable: "PlayerSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSeasonBattingStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    GamesPlayed = table.Column<int>(type: "INTEGER", nullable: false),
                    GamesBatting = table.Column<int>(type: "INTEGER", nullable: false),
                    AtBats = table.Column<int>(type: "INTEGER", nullable: false),
                    PlateAppearances = table.Column<int>(type: "INTEGER", nullable: false),
                    Runs = table.Column<int>(type: "INTEGER", nullable: false),
                    Hits = table.Column<int>(type: "INTEGER", nullable: false),
                    Singles = table.Column<int>(type: "INTEGER", nullable: false),
                    Doubles = table.Column<int>(type: "INTEGER", nullable: false),
                    Triples = table.Column<int>(type: "INTEGER", nullable: false),
                    HomeRuns = table.Column<int>(type: "INTEGER", nullable: false),
                    RunsBattedIn = table.Column<int>(type: "INTEGER", nullable: false),
                    ExtraBaseHits = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalBases = table.Column<int>(type: "INTEGER", nullable: false),
                    StolenBases = table.Column<int>(type: "INTEGER", nullable: false),
                    CaughtStealing = table.Column<int>(type: "INTEGER", nullable: false),
                    Walks = table.Column<int>(type: "INTEGER", nullable: false),
                    Strikeouts = table.Column<int>(type: "INTEGER", nullable: false),
                    HitByPitch = table.Column<int>(type: "INTEGER", nullable: false),
                    Obp = table.Column<double>(type: "REAL", nullable: false),
                    Slg = table.Column<double>(type: "REAL", nullable: false),
                    Ops = table.Column<double>(type: "REAL", nullable: false),
                    Woba = table.Column<double>(type: "REAL", nullable: false),
                    Iso = table.Column<double>(type: "REAL", nullable: false),
                    Babip = table.Column<double>(type: "REAL", nullable: false),
                    SacrificeHits = table.Column<int>(type: "INTEGER", nullable: false),
                    SacrificeFlies = table.Column<int>(type: "INTEGER", nullable: false),
                    BattingAverage = table.Column<double>(type: "REAL", nullable: false),
                    Errors = table.Column<int>(type: "INTEGER", nullable: false),
                    PassedBalls = table.Column<int>(type: "INTEGER", nullable: false),
                    PaPerGame = table.Column<double>(type: "REAL", nullable: false),
                    AbPerHomeRun = table.Column<double>(type: "REAL", nullable: false),
                    StrikeoutPercentage = table.Column<double>(type: "REAL", nullable: false),
                    WalkPercentage = table.Column<double>(type: "REAL", nullable: false),
                    ExtraBaseHitPercentage = table.Column<double>(type: "REAL", nullable: false),
                    OpsPlus = table.Column<double>(type: "REAL", nullable: false),
                    IsRegularSeason = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSeasonBattingStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonBattingStats_PlayerSeasons_PlayerSeasonId",
                        column: x => x.PlayerSeasonId,
                        principalTable: "PlayerSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSeasonGameStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    Power = table.Column<int>(type: "INTEGER", nullable: false),
                    Contact = table.Column<int>(type: "INTEGER", nullable: false),
                    Speed = table.Column<int>(type: "INTEGER", nullable: false),
                    Fielding = table.Column<int>(type: "INTEGER", nullable: false),
                    Arm = table.Column<int>(type: "INTEGER", nullable: false),
                    Velocity = table.Column<int>(type: "INTEGER", nullable: false),
                    Junk = table.Column<int>(type: "INTEGER", nullable: false),
                    Accuracy = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSeasonGameStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonGameStats_PlayerSeasons_PlayerSeasonId",
                        column: x => x.PlayerSeasonId,
                        principalTable: "PlayerSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSeasonPitchingStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    Wins = table.Column<int>(type: "INTEGER", nullable: false),
                    Losses = table.Column<int>(type: "INTEGER", nullable: false),
                    CompleteGames = table.Column<int>(type: "INTEGER", nullable: false),
                    Shutouts = table.Column<int>(type: "INTEGER", nullable: false),
                    Hits = table.Column<int>(type: "INTEGER", nullable: false),
                    EarnedRuns = table.Column<int>(type: "INTEGER", nullable: false),
                    HomeRuns = table.Column<int>(type: "INTEGER", nullable: false),
                    Walks = table.Column<int>(type: "INTEGER", nullable: false),
                    Strikeouts = table.Column<int>(type: "INTEGER", nullable: false),
                    InningsPitched = table.Column<double>(type: "REAL", nullable: false),
                    EarnedRunAverage = table.Column<double>(type: "REAL", nullable: false),
                    TotalPitches = table.Column<int>(type: "INTEGER", nullable: false),
                    Saves = table.Column<int>(type: "INTEGER", nullable: false),
                    HitByPitch = table.Column<int>(type: "INTEGER", nullable: false),
                    BattersFaced = table.Column<int>(type: "INTEGER", nullable: false),
                    GamesPlayed = table.Column<int>(type: "INTEGER", nullable: false),
                    GamesStarted = table.Column<int>(type: "INTEGER", nullable: false),
                    GamesFinished = table.Column<int>(type: "INTEGER", nullable: false),
                    RunsAllowed = table.Column<int>(type: "INTEGER", nullable: false),
                    WildPitches = table.Column<int>(type: "INTEGER", nullable: false),
                    BattingAverageAgainst = table.Column<double>(type: "REAL", nullable: false),
                    Fip = table.Column<double>(type: "REAL", nullable: false),
                    Whip = table.Column<double>(type: "REAL", nullable: false),
                    WinPercentage = table.Column<double>(type: "REAL", nullable: false),
                    OpponentObp = table.Column<double>(type: "REAL", nullable: false),
                    StrikeoutsPerNine = table.Column<double>(type: "REAL", nullable: false),
                    WalksPerNine = table.Column<double>(type: "REAL", nullable: false),
                    HitsPerNine = table.Column<double>(type: "REAL", nullable: false),
                    HomeRunsPerNine = table.Column<double>(type: "REAL", nullable: false),
                    PitchesPerInning = table.Column<double>(type: "REAL", nullable: false),
                    PitcherPerGame = table.Column<double>(type: "REAL", nullable: false),
                    EraMinus = table.Column<double>(type: "REAL", nullable: false),
                    FipMinus = table.Column<double>(type: "REAL", nullable: false),
                    IsRegularSeason = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSeasonPitchingStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonPitchingStats_PlayerSeasons_PlayerSeasonId",
                        column: x => x.PlayerSeasonId,
                        principalTable: "PlayerSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerSecondaryPositionHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    PositionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSecondaryPositionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSecondaryPositionHistory_PlayerSeasons_PlayerSeasonId",
                        column: x => x.PlayerSeasonId,
                        principalTable: "PlayerSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerSecondaryPositionHistory_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeasonTeamHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamId = table.Column<int>(type: "INTEGER", nullable: false),
                    Budget = table.Column<long>(type: "INTEGER", nullable: false),
                    Payroll = table.Column<long>(type: "INTEGER", nullable: false),
                    PayrollPerGame = table.Column<double>(type: "REAL", nullable: false),
                    Wins = table.Column<int>(type: "INTEGER", nullable: false),
                    Losses = table.Column<int>(type: "INTEGER", nullable: false),
                    GamesBehind = table.Column<long>(type: "INTEGER", nullable: false),
                    RunsScored = table.Column<int>(type: "INTEGER", nullable: false),
                    RunsAllowed = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalPower = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalContact = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalSpeed = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalFielding = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalArm = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalVelocity = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalJunk = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAccuracy = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayoffWins = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayoffLosses = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayoffRunsScored = table.Column<int>(type: "INTEGER", nullable: false),
                    PlayoffRunsAllowed = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonTeamHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonTeamHistory_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonTeamHistory_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerTeamHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    SeasonTeamHistoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerTeamHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerTeamHistory_PlayerSeasons_PlayerSeasonId",
                        column: x => x.PlayerSeasonId,
                        principalTable: "PlayerSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerTeamHistory_SeasonTeamHistory_SeasonTeamHistoryId",
                        column: x => x.SeasonTeamHistoryId,
                        principalTable: "SeasonTeamHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamDivisionHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeasonTeamHistoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    DivisionId = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamDivisionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamDivisionHistory_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamDivisionHistory_SeasonTeamHistory_SeasonTeamHistoryId",
                        column: x => x.SeasonTeamHistoryId,
                        principalTable: "SeasonTeamHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamDivisionHistory_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeamSeasonSchedule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HomeTeamHistoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    AwayTeamHistoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    HomePitcherSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    AwayPitcherSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    HomeScore = table.Column<int>(type: "INTEGER", nullable: false),
                    AwayScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamSeasonSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamSeasonSchedule_PlayerSeasons_AwayPitcherSeasonId",
                        column: x => x.AwayPitcherSeasonId,
                        principalTable: "PlayerSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamSeasonSchedule_PlayerSeasons_HomePitcherSeasonId",
                        column: x => x.HomePitcherSeasonId,
                        principalTable: "PlayerSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamSeasonSchedule_SeasonTeamHistory_AwayTeamHistoryId",
                        column: x => x.AwayTeamHistoryId,
                        principalTable: "SeasonTeamHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamSeasonSchedule_SeasonTeamHistory_HomeTeamHistoryId",
                        column: x => x.HomeTeamHistoryId,
                        principalTable: "SeasonTeamHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conferences_FranchiseId",
                table: "Conferences",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_ConferenceId",
                table: "Divisions",
                column: "ConferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_FranchiseId",
                table: "Divisions",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_PitcherPitchTypeHistory_PitchTypeId",
                table: "PitcherPitchTypeHistory",
                column: "PitchTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PitcherPitchTypeHistory_PlayerSeasonId",
                table: "PitcherPitchTypeHistory",
                column: "PlayerSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGameIdHistory_PlayerId",
                table: "PlayerGameIdHistory",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_BatHandednessId",
                table: "Players",
                column: "BatHandednessId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_ChemistryId",
                table: "Players",
                column: "ChemistryId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_PitcherRoleId",
                table: "Players",
                column: "PitcherRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_PrimaryPositionId",
                table: "Players",
                column: "PrimaryPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_ThrowHandednessId",
                table: "Players",
                column: "ThrowHandednessId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonAwards_PlayerAwardId",
                table: "PlayerSeasonAwards",
                column: "PlayerAwardId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonAwards_PlayerSeasonId",
                table: "PlayerSeasonAwards",
                column: "PlayerSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonBattingStats_PlayerSeasonId",
                table: "PlayerSeasonBattingStats",
                column: "PlayerSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonGameStats_PlayerSeasonId",
                table: "PlayerSeasonGameStats",
                column: "PlayerSeasonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonPitchingStats_PlayerSeasonId",
                table: "PlayerSeasonPitchingStats",
                column: "PlayerSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasons_PlayerId",
                table: "PlayerSeasons",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasons_SeasonId",
                table: "PlayerSeasons",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSecondaryPositionHistory_PlayerSeasonId",
                table: "PlayerSecondaryPositionHistory",
                column: "PlayerSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSecondaryPositionHistory_PositionId",
                table: "PlayerSecondaryPositionHistory",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTeamHistory_PlayerSeasonId",
                table: "PlayerTeamHistory",
                column: "PlayerSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTeamHistory_SeasonTeamHistoryId",
                table: "PlayerTeamHistory",
                column: "SeasonTeamHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_FranchiseId",
                table: "Seasons",
                column: "FranchiseId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonTeamHistory_SeasonId",
                table: "SeasonTeamHistory",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonTeamHistory_TeamId",
                table: "SeasonTeamHistory",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamDivisionHistory_DivisionId",
                table: "TeamDivisionHistory",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamDivisionHistory_SeasonTeamHistoryId",
                table: "TeamDivisionHistory",
                column: "SeasonTeamHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamDivisionHistory_TeamId",
                table: "TeamDivisionHistory",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_DivisionId",
                table: "Teams",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamSeasonSchedule_AwayPitcherSeasonId",
                table: "TeamSeasonSchedule",
                column: "AwayPitcherSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamSeasonSchedule_AwayTeamHistoryId",
                table: "TeamSeasonSchedule",
                column: "AwayTeamHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamSeasonSchedule_HomePitcherSeasonId",
                table: "TeamSeasonSchedule",
                column: "HomePitcherSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamSeasonSchedule_HomeTeamHistoryId",
                table: "TeamSeasonSchedule",
                column: "HomeTeamHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Traits_ChemistryId",
                table: "Traits",
                column: "ChemistryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PitcherPitchTypeHistory");

            migrationBuilder.DropTable(
                name: "PlayerGameIdHistory");

            migrationBuilder.DropTable(
                name: "PlayerSeasonAwards");

            migrationBuilder.DropTable(
                name: "PlayerSeasonBattingStats");

            migrationBuilder.DropTable(
                name: "PlayerSeasonGameStats");

            migrationBuilder.DropTable(
                name: "PlayerSeasonPitchingStats");

            migrationBuilder.DropTable(
                name: "PlayerSecondaryPositionHistory");

            migrationBuilder.DropTable(
                name: "PlayerTeamHistory");

            migrationBuilder.DropTable(
                name: "TeamDivisionHistory");

            migrationBuilder.DropTable(
                name: "TeamSeasonSchedule");

            migrationBuilder.DropTable(
                name: "Traits");

            migrationBuilder.DropTable(
                name: "PitchTypes");

            migrationBuilder.DropTable(
                name: "PlayerAwards");

            migrationBuilder.DropTable(
                name: "PlayerSeasons");

            migrationBuilder.DropTable(
                name: "SeasonTeamHistory");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "BatHandedness");

            migrationBuilder.DropTable(
                name: "Chemistry");

            migrationBuilder.DropTable(
                name: "PitcherRoles");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropTable(
                name: "ThrowHandedness");

            migrationBuilder.DropTable(
                name: "Divisions");

            migrationBuilder.DropTable(
                name: "Conferences");

            migrationBuilder.DropTable(
                name: "Franchises");
        }
    }
}
