using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmbExplorerCompanion.Database;
using SmbExplorerCompanion.WPF.ViewModels;
using SmbExplorerCompanion.WPF.Views;
using static SmbExplorerCompanion.Shared.Constants.FileExports;

namespace SmbExplorerCompanion.WPF;

public partial class App
{
    private IServiceProvider ServiceProvider { get; set; } = null!;
    private IServiceCollection Services { get; set; } = null!;

    public App()
    {
#if RELEASE
        DispatcherUnhandledException += App_DispatcherUnhandledException;
#endif
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        if (!Directory.Exists(BaseApplicationDirectory))
        {
            Directory.CreateDirectory(BaseApplicationDirectory);
        }

        Services = new ServiceCollection();
        await ConfigureServices(Services);
        ServiceProvider = Services.BuildServiceProvider();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    private static Task ConfigureServices(IServiceCollection services)
    {
        var connectionString = Path.Combine(BaseApplicationDirectory, "SmbExplorerCompanion.db");
        services.AddDbContext<SmbExplorerCompanionDbContext>(options =>
        {
            options.UseSqlite(new SqliteConnection(connectionString));
        });

        services.AddSingleton<MainWindow>(serviceProvider => new MainWindow
        {
            DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>()
        });

        services.AddTransient<MainWindowViewModel>();

        // NavigationService calls this Func to get the ViewModel instance
        services.AddSingleton<Func<Type, ViewModelBase>>(serviceProvider =>
            viewModelType => (ViewModelBase) serviceProvider.GetRequiredService(viewModelType));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Global exception handler. In debug mode, this will not be called and exceptions will be thrown.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        Shutdown();
    }
}