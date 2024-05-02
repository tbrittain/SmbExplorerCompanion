using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects;
using SmbExplorerCompanion.WPF.Models.Response;
using static SmbExplorerCompanion.Shared.Constants.Github;

namespace SmbExplorerCompanion.WPF.Services;

public class HttpService : IHttpService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AppUpdateResult?> CheckForUpdates()
    {
        var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version;
        if (currentVersion is null) throw new Exception("Unable to determine current version.");

        var httpClient = _httpClientFactory.CreateClient();

        httpClient.DefaultRequestHeaders.Add("User-Agent", $"SMBExplorerCompanion/{currentVersion.ToString()}");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        var response = await httpClient.GetAsync(LatestReleaseUrl);

        if (!response.IsSuccessStatusCode) throw new Exception(response.ReasonPhrase ?? "An unknown error occurred.");

        var result = await response.Content.ReadFromJsonAsync<GitHubReleaseResponse>();
        if (result is null) throw new Exception("Unable to parse latest release response.");

        var currentVersionWithoutRev =
            new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);

        var resultVersionWithoutRev =
            new Version(result.Version.Major, result.Version.Minor, result.Version.Build);

        if (currentVersionWithoutRev >= resultVersionWithoutRev) return null;

        return new AppUpdateResult
        {
            Version = resultVersionWithoutRev,
            ReleasePageUrl = result.HtmlUrl,
            ReleaseDate = result.PublishedAt
        };
    }
}