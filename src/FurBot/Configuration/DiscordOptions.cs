namespace FurBot.Configuration;

internal sealed class DiscordOptions
{
    public const string ConfigurationSectionName = "Discord";

    public string Token { get; init; } = string.Empty;
    public ulong[] ChannelIds { get; init; } = [];
}
