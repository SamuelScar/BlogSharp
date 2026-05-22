namespace BlogSharp.Api.Services.IA;

public class IAOptions
{
    public bool Enabled { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string SiteUrl { get; set; } = string.Empty;

    public string AppName { get; set; } = string.Empty;
}
