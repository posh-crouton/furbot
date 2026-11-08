using System.Text.Json.Serialization;

namespace FurBot.Types.Gelbooru;

internal sealed class GelbooruPostTags
{
    [JsonPropertyName("general")] public List<string> General { get; set; } = [];

    [JsonPropertyName("artist")] public List<string> Artist { get; set; } = [];

    [JsonPropertyName("character")] public List<string> Character { get; set; } = [];

    [JsonPropertyName("copyright")] public List<string> Copyright { get; set; } = [];

    [JsonPropertyName("species")] public List<string> Species { get; set; } = [];
}