using System.Text.Json.Serialization;

namespace FurBot.Types.Gelbooru;

internal sealed class GelbooruPost
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("file")] public GelbooruPostFile File { get; set; } = new();

    [JsonIgnore] public byte[] Md5 => File.Md5;

    [JsonPropertyName("rating")] public string Rating { get; set; } = "";

    [JsonPropertyName("tags")] public GelbooruPostTags Tags { get; set; } = new();
}
