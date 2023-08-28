using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Win32;
using SmbExplorerCompanion.Core.Commands.Actions.Csv;
using SmbExplorerCompanion.Core.ValueObjects.Progress;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class ImportCsvViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private string _overallPlayersCsvPath = string.Empty;
    private string _playoffBattingCsvPath = string.Empty;
    private string _playoffPitchingCsvPath = string.Empty;
    private string _playoffScheduleCsvPath = string.Empty;
    private string _seasonBattingCsvPath = string.Empty;
    private string _seasonPitchingCsvPath = string.Empty;
    private string _seasonScheduleCsvPath = string.Empty;
    private string _teamsCsvPath = string.Empty;

    public ImportCsvViewModel(IMediator mediator)
    {
        _mediator = mediator;
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

    [RelayCommand(CanExecute = nameof(CanImportSeasonCsvs))]
    private async Task ImportSeasonData()
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        
        var progressChannel = Channel.CreateUnbounded<ImportProgress>();
        var response = await _mediator.Send(new ImportSeasonDataRequest(
            TeamsCsvPath,
            OverallPlayersCsvPath,
            SeasonPitchingCsvPath,
            SeasonBattingCsvPath,
            SeasonScheduleCsvPath,
            progressChannel.Writer));

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
        if (response.TryPickT1(out var exception, out _)) MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        else MessageBox.Show("Successfully imported season data!");
    }

    [RelayCommand(CanExecute = nameof(CanImportPlayoffCsvs))]
    private async Task ImportPlayoffData()
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        
        var progressChannel = Channel.CreateUnbounded<ImportProgress>();
        var response = await _mediator.Send(new ImportPlayoffDataRequest(
            PlayoffPitchingCsvPath,
            PlayoffBattingCsvPath,
            PlayoffScheduleCsvPath,
            progressChannel.Writer));

        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
        if (response.TryPickT1(out var exception, out _)) MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        else MessageBox.Show("Successfully imported playoff data!");
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