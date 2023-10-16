using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Win32;
using SmbExplorerCompanion.Core.Commands.Queries.Seasons;
using SmbExplorerCompanion.Core.Entities.Seasons;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects;
using SmbExplorerCompanion.Core.ValueObjects.Progress;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Seasons;
using SmbExplorerCompanion.WPF.Models.Seasons;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class ImportCsvViewModel : ViewModelBase
{
    private string _overallPlayersCsvPath = string.Empty;
    private string _playoffBattingCsvPath = string.Empty;
    private string _playoffPitchingCsvPath = string.Empty;
    private string _playoffScheduleCsvPath = string.Empty;
    private string _seasonBattingCsvPath = string.Empty;
    private string _seasonPitchingCsvPath = string.Empty;
    private string _seasonScheduleCsvPath = string.Empty;
    private string _teamsCsvPath = string.Empty;
    private readonly ICsvImportRepository _csvImportRepository;
    private readonly IApplicationContext _applicationContext;
    private Season? _selectedSeason;

    public ImportCsvViewModel(ICsvImportRepository csvImportRepository, ISender mediator, IApplicationContext applicationContext)
    {
        _csvImportRepository = csvImportRepository;
        _applicationContext = applicationContext;

        var seasonsResponse = mediator.Send(new GetSeasonsRequest()).Result;

        if (seasonsResponse.TryPickT1(out var exception, out var seasons))
        {
            MessageBox.Show(exception.Message);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        var seasonMapper = new SeasonMapping();
        Seasons.AddRange(seasons.Select(s => seasonMapper.FromDto(s)));
    }

    public Season? SelectedSeason
    {
        get => _selectedSeason;
        set
        {
            SetField(ref _selectedSeason, value);
            AddSeasonCommand.NotifyCanExecuteChanged();
            
            OverallPlayersCsvPath = string.Empty;
            PlayoffBattingCsvPath = string.Empty;
            PlayoffPitchingCsvPath = string.Empty;
            PlayoffScheduleCsvPath = string.Empty;
            SeasonBattingCsvPath = string.Empty;
            SeasonPitchingCsvPath = string.Empty;
            SeasonScheduleCsvPath = string.Empty;
            TeamsCsvPath = string.Empty;

            ImportSeasonDataCommand.NotifyCanExecuteChanged();
            ImportPlayoffDataCommand.NotifyCanExecuteChanged();
        }
    }

    public ObservableCollection<Season> Seasons { get; } = new();

    private bool HasAddedSeason { get; set; }

    private bool CanAddNewSeason => !HasAddedSeason;

    // This will ensure that the user clicks the "Add Season" button before they can import CSVs
    // and does not click the "Add Season" button again after they have already added a season.
    [RelayCommand(CanExecute = nameof(CanAddNewSeason))]
    private void AddSeason()
    {
        var lastSeason = Seasons.MaxBy(s => s.Number);
        var newSeason = new Season
        {
            Id = default,
            FranchiseId = _applicationContext.SelectedFranchiseId!.Value,
            Number = lastSeason is not null ? lastSeason.Number + 1 : 1,
            IsNewSeason = true
        };

        Seasons.Add(newSeason);
        SelectedSeason = newSeason;
        HasAddedSeason = true;
    }

    public string TeamsCsvPath
    {
        get => _teamsCsvPath;
        private set
        {
            SetField(ref _teamsCsvPath, value);
            ImportSeasonDataCommand.NotifyCanExecuteChanged();
        }
    }

    public string OverallPlayersCsvPath
    {
        get => _overallPlayersCsvPath;
        private set
        {
            SetField(ref _overallPlayersCsvPath, value);
            ImportSeasonDataCommand.NotifyCanExecuteChanged();
        }
    }

    public string SeasonBattingCsvPath
    {
        get => _seasonBattingCsvPath;
        private set
        {
            SetField(ref _seasonBattingCsvPath, value);
            ImportSeasonDataCommand.NotifyCanExecuteChanged();
        }
    }

    public string SeasonPitchingCsvPath
    {
        get => _seasonPitchingCsvPath;
        private set
        {
            SetField(ref _seasonPitchingCsvPath, value);
            ImportSeasonDataCommand.NotifyCanExecuteChanged();
        }
    }

    public string SeasonScheduleCsvPath
    {
        get => _seasonScheduleCsvPath;
        private set
        {
            SetField(ref _seasonScheduleCsvPath, value);
            ImportSeasonDataCommand.NotifyCanExecuteChanged();
        }
    }

    public bool CanImportSeasonCsvs => SelectedSeason is not null &&
                                       !string.IsNullOrWhiteSpace(TeamsCsvPath) &&
                                       !string.IsNullOrWhiteSpace(OverallPlayersCsvPath) &&
                                       !string.IsNullOrWhiteSpace(SeasonBattingCsvPath) &&
                                       !string.IsNullOrWhiteSpace(SeasonPitchingCsvPath) &&
                                       !string.IsNullOrWhiteSpace(SeasonScheduleCsvPath);

    public string PlayoffPitchingCsvPath
    {
        get => _playoffPitchingCsvPath;
        private set
        {
            SetField(ref _playoffPitchingCsvPath, value);
            ImportPlayoffDataCommand.NotifyCanExecuteChanged();
        }
    }

    public string PlayoffBattingCsvPath
    {
        get => _playoffBattingCsvPath;
        private set
        {
            SetField(ref _playoffBattingCsvPath, value);
            ImportPlayoffDataCommand.NotifyCanExecuteChanged();
        }
    }

    public string PlayoffScheduleCsvPath
    {
        get => _playoffScheduleCsvPath;
        private set
        {
            SetField(ref _playoffScheduleCsvPath, value);
            ImportPlayoffDataCommand.NotifyCanExecuteChanged();
        }
    }

    public bool CanImportPlayoffCsvs => SelectedSeason is not null &&
                                        !string.IsNullOrWhiteSpace(PlayoffPitchingCsvPath) &&
                                        !string.IsNullOrWhiteSpace(PlayoffBattingCsvPath) &&
                                        !string.IsNullOrWhiteSpace(PlayoffScheduleCsvPath);

    public ObservableCollection<ImportProgress> ImportProgress { get; } = new();

    [RelayCommand(CanExecute = nameof(CanImportSeasonCsvs))]
    private async Task ImportSeasonData()
    {
        if (SelectedSeason is null)
        {
            MessageBox.Show("Please select a season.");
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        ImportProgress.Clear();
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

        var progressChannel = Channel.CreateUnbounded<ImportProgress>();
        var filePaths = new ImportSeasonFilePaths(
            TeamsCsvPath,
            OverallPlayersCsvPath,
            SeasonPitchingCsvPath,
            SeasonBattingCsvPath,
            SeasonScheduleCsvPath);

        try
        {
            var coreSeason = new SeasonDto
            {
                Id = SelectedSeason.Id,
                Number = SelectedSeason!.Number
            };

            var task = Task.Run(() => _csvImportRepository.ImportSeason(filePaths, progressChannel.Writer, coreSeason, default));

            await foreach (var progress in progressChannel.Reader.ReadAllAsync()) 
                ImportProgress.Add(progress);
            await task;

            if (SelectedSeason.IsNewSeason)
            {
                SelectedSeason.Id = task.Result.Id;
            }

            MessageBox.Show("Successfully imported season data!");
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
        }
    }

    [RelayCommand(CanExecute = nameof(CanImportPlayoffCsvs))]
    private async Task ImportPlayoffData()
    {
        if (SelectedSeason is null)
        {
            MessageBox.Show("Please select a season.");
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        ImportProgress.Clear();
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

        var progressChannel = Channel.CreateUnbounded<ImportProgress>();

        var filePaths = new ImportPlayoffFilePaths(
            PlayoffPitchingCsvPath,
            PlayoffBattingCsvPath,
            PlayoffScheduleCsvPath);

        try
        {
            var coreSeason = new SeasonDto
            {
                Id = SelectedSeason.Id,
                Number = SelectedSeason!.Number
            };

            var task = Task.Run(() => _csvImportRepository.ImportPlayoffs(filePaths, progressChannel.Writer, coreSeason, default));

            await foreach (var progress in progressChannel.Reader.ReadAllAsync()) 
                ImportProgress.Add(progress);
            await task;

            MessageBox.Show("Successfully imported playoff data!");
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
        }
    }

    [RelayCommand]
    private void SelectSeasonTeamsCsv()
    {
        var filePath = GetUserProvidedCsv("Teams");
        if (filePath is null) return;

        TeamsCsvPath = filePath;
    }

    [RelayCommand]
    private void SelectSeasonOverallPlayersCsv()
    {
        var filePath = GetUserProvidedCsv("Overall Players");
        if (filePath is null) return;

        OverallPlayersCsvPath = filePath;
    }

    [RelayCommand]
    private void SelectSeasonBattingCsv()
    {
        var filePath = GetUserProvidedCsv("Batting");
        if (filePath is null) return;

        SeasonBattingCsvPath = filePath;
    }

    [RelayCommand]
    private void SelectSeasonPitchingCsv()
    {
        var filePath = GetUserProvidedCsv("Pitching");
        if (filePath is null) return;

        SeasonPitchingCsvPath = filePath;
    }

    [RelayCommand]
    private void SelectSeasonScheduleCsv()
    {
        var filePath = GetUserProvidedCsv("Schedule");
        if (filePath is null) return;

        SeasonScheduleCsvPath = filePath;
    }

    [RelayCommand]
    private void SelectPlayoffPitchingCsv()
    {
        var filePath = GetUserProvidedCsv("Playoff Pitching");
        if (filePath is null) return;

        PlayoffPitchingCsvPath = filePath;
    }

    [RelayCommand]
    private void SelectPlayoffBattingCsv()
    {
        var filePath = GetUserProvidedCsv("Playoff Batting");
        if (filePath is null) return;

        PlayoffBattingCsvPath = filePath;
    }

    [RelayCommand]
    private void SelectPlayoffScheduleCsv()
    {
        var filePath = GetUserProvidedCsv("Playoff Schedule");
        if (filePath is null) return;

        PlayoffScheduleCsvPath = filePath;
    }

    private static string? GetUserProvidedCsv(string csvTitle)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "CSV Files (*.csv)|*.csv",
            Title = $"Select {csvTitle} CSV File"
        };

        return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
    }
}