using System.Text.Json;
using System.Text.Json.Serialization;

namespace FurBot.Types.Gelbooru;

internal sealed class Md5JsonConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        if (value is null || value.Length != 32)
            throw new JsonException("MD5 must contain 32 hexadecimal characters.");

        try { return Convert.FromHexString(value); }
        catch (FormatException exception) { throw new JsonException("Invalid MD5 hexadecimal value.", exception); }
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options) =>
        writer.WriteStringValue(Convert.ToHexString(value).ToLowerInvariant());
}
