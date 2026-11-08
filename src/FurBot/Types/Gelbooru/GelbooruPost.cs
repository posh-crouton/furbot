using System.Text.Json.Serialization;

namespace FurBot.Types.Gelbooru;

internal sealed class GelbooruPost
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("file")] public GelbooruPostFile File { get; set; } = new();

    [JsonPropertyName("rating")] public string Rating { get; set; } = "";

    [JsonPropertyName("tags")] public GelbooruPostTags Tags { get; set; } = new();
}
