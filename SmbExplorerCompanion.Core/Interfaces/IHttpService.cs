using OneOf.Types;
using OneOf;
using SmbExplorerCompanion.Core.ValueObjects;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IHttpService
{
    Task<OneOf<AppUpdateResult, None, Error<string>>> CheckForUpdates();
}