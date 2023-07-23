using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using SmbExplorerCompanion.Core;
using SmbExplorerCompanion.Database;
using SmbExplorerCompanion.WPF.Services;
using SmbExplorerCompanion.WPF.ViewModels;
using SmbExplorerCompanion.WPF.Views;
using static SmbExplorerCompanion.Shared.Constants.FileExports;

namespace SmbExplorerCompanion.WPF;

public partial class App
{
    public App()
    {
#if RELEASE
        DispatcherUnhandledException += App_DispatcherUnhandledException;
#endif
    }

    private IServiceProvider ServiceProvider { get; set; } = null!;
    private IServiceCollection Services { get; set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        if (!Directory.Exists(BaseApplicationDirectory)) Directory.CreateDirectory(BaseApplicationDirectory);

        Services = new ServiceCollection();
        await ConfigureServices(Services);
        ServiceProvider = Services.BuildServiceProvider();
        SmbExplorerCompanionDbContext.ApplyMigrations(ServiceProvider);
        SmbExplorerCompanionDbContext.SeedLookups(ServiceProvider);

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
        await ((MainWindowViewModel) mainWindow.DataContext).Initialize();

        base.OnStartup(e);
    }

    private static Task ConfigureServices(IServiceCollection services)
    {
        services
            .AddCore()
            .AddDatabase()
            .AddSingleton<MainWindow>(serviceProvider => new MainWindow
            {
                DataContext = serviceProvider.GetRequiredService<MainWindowViewModel>()
            })
            .AddTransient<MainWindowViewModel>()
            .AddTransient<FranchiseSelectViewModel>()
            .AddTransient<HomeViewModel>()
            .AddTransient<ImportCsvViewModel>()
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<IApplicationContext, ApplicationContext>()
            // NavigationService calls this Func to get the ViewModel instance
            .AddSingleton<Func<Type, ViewModelBase>>(serviceProvider =>
                viewModelType => (ViewModelBase) serviceProvider.GetRequiredService(viewModelType));

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Global exception handler. In debug mode, this will not be called and exceptions will be thrown.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        Shutdown();
    }
}