using FurBot.Enums;

namespace FurBot.Configuration;

internal sealed class FurBotContextOptions
{
    public const string ConfigurationSectionName = "Database";

    public DatabaseProvider Provider { get; init; } = DatabaseProvider.Sqlite;
    public string ConnectionString { get; init; } = "Data Source=furbot.db";
}
