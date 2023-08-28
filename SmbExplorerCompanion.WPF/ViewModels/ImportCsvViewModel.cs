using System;
using System.Collections.ObjectModel;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects;
using SmbExplorerCompanion.Core.ValueObjects.Progress;

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

    public ImportCsvViewModel(ICsvImportRepository csvImportRepository)
    {
        _csvImportRepository = csvImportRepository;
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

    public bool CanImportSeasonCsvs => !string.IsNullOrWhiteSpace(TeamsCsvPath) &&
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

    public bool CanImportPlayoffCsvs => !string.IsNullOrWhiteSpace(PlayoffPitchingCsvPath) &&
                                        !string.IsNullOrWhiteSpace(PlayoffBattingCsvPath) &&
                                        !string.IsNullOrWhiteSpace(PlayoffScheduleCsvPath);
    
    public ObservableCollection<ImportProgress> ImportProgress { get; } = new();

    [RelayCommand(CanExecute = nameof(CanImportSeasonCsvs))]
    private async Task ImportSeasonData()
    {
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
            _ = _csvImportRepository.ImportSeason(filePaths, progressChannel.Writer, default);
            
            await foreach (var progress in progressChannel.Reader.ReadAllAsync())
            {
                ImportProgress.Add(progress);
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
        ImportProgress.Clear();
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        
        var progressChannel = Channel.CreateUnbounded<ImportProgress>();
        
        var filePaths = new ImportPlayoffFilePaths(
            PlayoffPitchingCsvPath,
            PlayoffBattingCsvPath,
            PlayoffScheduleCsvPath);

        try
        {
            _ = _csvImportRepository.ImportPlayoffs(filePaths, progressChannel.Writer, default);
            
            await foreach (var progress in progressChannel.Reader.ReadAllAsync())
            {
                ImportProgress.Add(progress);
            }
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