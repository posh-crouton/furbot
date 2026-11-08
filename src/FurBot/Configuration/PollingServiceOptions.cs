namespace FurBot.Configuration;

internal sealed class PollingServiceOptions
{
    public const string ConfigurationSectionName = "PollingService";

    public int IntervalMs { get; init; } = 300_000;
}
