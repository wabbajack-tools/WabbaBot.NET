using System.Text.Json;
using System.Text.Json.Serialization;
using Wabbajack.DTOs;

namespace WabbaBot.Converters;

public class GameJsonConverter : JsonConverter<Game>
{
    public override Game Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string gameJson = reader.GetString();
        if (Enum.TryParse<Game>(gameJson, ignoreCase: true, out var game))
            return game;
        else return Game.ModdingTools;
    }

    public override void Write(Utf8JsonWriter writer, Game value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
