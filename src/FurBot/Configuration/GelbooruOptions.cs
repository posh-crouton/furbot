namespace FurBot.Configuration;

internal sealed class GelbooruOptions
{
    public const string ConfigurationSectionName = "Gelbooru";

    public string ContactInfo { get; init; } = string.Empty;
    public string InstanceUri { get; init; } = "https://e621.net";
    public string InstanceName { get; init; } = "e621";
    public string Query { get; init; } = "rating:s";
    public int PostLimit { get; set; } = 5;
    public string Username { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
