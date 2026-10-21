using Newtonsoft.Json;

namespace App.Shared.Infrastructure;

/// <summary>Deserializa TimeOnly desde "HH:mm" o "HH:mm:ss" (inputs HTML y legacy).</summary>
public sealed class TimeOnlyJsonConverter : JsonConverter<TimeOnly?>
{
    public override TimeOnly? ReadJson(
        JsonReader reader,
        Type objectType,
        TimeOnly? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;
        var raw = reader.Value?.ToString();
        if (string.IsNullOrWhiteSpace(raw)) return null;
        if (TimeOnly.TryParse(raw, out var time)) return time;
        throw new JsonSerializationException($"Hora inválida: {raw}");
    }

    public override void WriteJson(JsonWriter writer, TimeOnly? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteValue(value.Value.ToString("HH:mm:ss"));
    }
}
