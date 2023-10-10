using System.Globalization;
using CsvHelper;
using SmbExplorerCompanion.Csv.Models;

namespace SmbExplorerCompanion.Csv.Services;

public class CsvReaderService
{
    public async Task<List<OverallPlayer>> ReadOverallPlayersAsync(string filePath)
    {
        var players = new List<OverallPlayer>();

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<OverallPlayer.OverallPlayersCsvMapping>();

        var records = csv.GetRecordsAsync<OverallPlayer>();
        await foreach (var record in records)
        {
            players.Add(record);
        }

        return players;
    }

    public async Task<List<PlayoffSchedule>> ReadPlayoffScheduleAsync(string filePath)
    {
        var schedule = new List<PlayoffSchedule>();

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<PlayoffSchedule.PlayoffScheduleCsvMapping>();

        var records = csv.GetRecordsAsync<PlayoffSchedule>();
        await foreach (var record in records)
        {
            schedule.Add(record);
        }

        return schedule;
    }

    public async Task<List<SeasonSchedule>> ReadSeasonScheduleAsync(string filePath)
    {
        var schedule = new List<SeasonSchedule>();

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<SeasonSchedule.SeasonScheduleCsvMapping>();

        var records = csv.GetRecordsAsync<SeasonSchedule>();
        await foreach (var record in records)
        {
            schedule.Add(record);
        }

        return schedule;
    }

    // This applies to both the Season and Playoff versions of the stat
    public async Task<List<SeasonStatBatting>> ReadPlayerStatBattingAsync(string filePath)
    {
        var stats = new List<SeasonStatBatting>();

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<SeasonStatBatting.SeasonStatBattingCsvMapping>();

        var records = csv.GetRecordsAsync<SeasonStatBatting>();
        await foreach (var record in records)
        {
            stats.Add(record);
        }

        return stats;
    }

    // This applies to both the Season and Playoff versions of the stat
    public async Task<List<SeasonStatPitching>> ReadPlayerStatPitchingAsync(string filePath)
    {
        var stats = new List<SeasonStatPitching>();

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<SeasonStatPitching.SeasonStatPitchingCsvMapping>();

        var records = csv.GetRecordsAsync<SeasonStatPitching>();
        await foreach (var record in records)
        {
            stats.Add(record);
        }

        return stats;
    }

    public async Task<List<Team>> ReadTeamsAsync(string filePath)
    {
        var teams = new List<Team>();

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<Team.TeamCsvMapping>();

        var records = csv.GetRecordsAsync<Team>();
        await foreach (var record in records)
        {
            teams.Add(record);
        }

        return teams;
    }

    public async Task<int> GetSeasonIdFromPlayoffPitching(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<SeasonStatPitching.SeasonStatPitchingCsvMapping>();

        var records = csv.GetRecordsAsync<SeasonStatPitching>();
        var enumerator = records.GetAsyncEnumerator();
        await enumerator.MoveNextAsync();
        return enumerator.Current.SeasonId;
    }
}