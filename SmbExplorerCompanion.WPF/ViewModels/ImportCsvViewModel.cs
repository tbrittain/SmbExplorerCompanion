using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Actions.Csv;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class ImportCsvViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationContext _applicationContext;
    private string _teamsCsvPath = string.Empty;
    private string _overallPlayersCsvPath = string.Empty;
    private string _seasonBattingCsvPath = string.Empty;
    private string _seasonPitchingCsvPath = string.Empty;
    private string _seasonScheduleCsvPath = string.Empty;
    private string _playoffPitchingCsvPath = string.Empty;
    private string _playoffBattingCsvPath = string.Empty;
    private string _playoffScheduleCsvPath = string.Empty;

    public ImportCsvViewModel(IMediator mediator, IApplicationContext applicationContext)
    {
        _mediator = mediator;
        _applicationContext = applicationContext;
    }

    public string TeamsCsvPath
    {
        get => _teamsCsvPath;
        set
        {
            SetField(ref _teamsCsvPath, value);
            ImportSeasonDataCommand.NotifyCanExecuteChanged();
        }
    }

    public string OverallPlayersCsvPath
    {
        get => _overallPlayersCsvPath;
        set
        {
            SetField(ref _overallPlayersCsvPath, value);
            ImportSeasonDataCommand.NotifyCanExecuteChanged();
        }
    }

    public string SeasonBattingCsvPath
    {
        get => _seasonBattingCsvPath;
        set
        {
            SetField(ref _seasonBattingCsvPath, value);
            ImportSeasonDataCommand.NotifyCanExecuteChanged();
        }
    }

    public string SeasonPitchingCsvPath
    {
        get => _seasonPitchingCsvPath;
        set
        {
            SetField(ref _seasonPitchingCsvPath, value);
            ImportSeasonDataCommand.NotifyCanExecuteChanged();
        }
    }

    public string SeasonScheduleCsvPath
    {
        get => _seasonScheduleCsvPath;
        set
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
        set
        {
            SetField(ref _playoffPitchingCsvPath, value);
            ImportPlayoffDataCommand.NotifyCanExecuteChanged();
        }
    }

    public string PlayoffBattingCsvPath
    {
        get => _playoffBattingCsvPath;
        set
        {
            SetField(ref _playoffBattingCsvPath, value);
            ImportPlayoffDataCommand.NotifyCanExecuteChanged();
        }
    }

    public string PlayoffScheduleCsvPath
    {
        get => _playoffScheduleCsvPath;
        set
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
        var response = await _mediator.Send(new ImportSeasonDataRequest(
            _applicationContext.SelectedFranchiseId!.Value,
            TeamsCsvPath,
            OverallPlayersCsvPath,
            SeasonPitchingCsvPath,
            SeasonBattingCsvPath,
            SeasonScheduleCsvPath));

        if (response.TryPickT1(out var exception, out _))
        {
            MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            MessageBox.Show("Successfully imported season data!");
        }
    }

    [RelayCommand(CanExecute = nameof(CanImportPlayoffCsvs))]
    private async Task ImportPlayoffData()
    {
        var response = await _mediator.Send(new ImportPlayoffDataRequest(
            PlayoffPitchingCsvPath,
            PlayoffBattingCsvPath,
            PlayoffScheduleCsvPath));

        if (response.TryPickT1(out var exception, out _))
        {
            MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            MessageBox.Show("Successfully imported playoff data!");
        }
    }
}