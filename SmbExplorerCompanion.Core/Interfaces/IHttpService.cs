using SmbExplorerCompanion.Core.ValueObjects;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IHttpService
{
    Task<AppUpdateResult?> CheckForUpdates();
}