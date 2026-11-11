using System.Text.Json.Serialization;

namespace FurBot.Types.Gelbooru;

internal sealed class GelbooruPostFile
{
    [JsonPropertyName("md5")]
    [JsonConverter(typeof(Md5JsonConverter))]
    public byte[] Md5 { get; set; } = [];

    [JsonPropertyName("url")] public string? Url { get; set; }

    [JsonPropertyName("width")] public int Width { get; set; }

    [JsonPropertyName("height")] public int Height { get; set; }

    [JsonPropertyName("ext")] public string? Extension { get; set; }
}
