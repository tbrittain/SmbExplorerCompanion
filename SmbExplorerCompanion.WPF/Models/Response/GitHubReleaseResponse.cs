using System;
using System.Text.Json.Serialization;

namespace SmbExplorerCompanion.WPF.Models.Response;

public class GitHubReleaseResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("tag_name")]
    public string TagName { private get; set; } = string.Empty;
    
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; } = string.Empty;
    
    public Version Version => Version.Parse(TagName.TrimStart('v'));
    
    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}