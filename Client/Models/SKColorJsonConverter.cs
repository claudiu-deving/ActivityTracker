using SkiaSharp;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client.Models;

public class SKColorJsonConverter : JsonConverter<SKColor>
{
    public override SKColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        byte alpha = 255, red = 0, green = 0, blue = 0;
        float hue = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new SKColor(red, green, blue, alpha);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string propertyName = reader.GetString();
            reader.Read();

            switch (propertyName.ToLower())
            {
                case "alpha":
                    alpha = (byte)reader.GetInt32();
                    break;
                case "red":
                    red = (byte)reader.GetInt32();
                    break;
                case "green":
                    green = (byte)reader.GetInt32();
                    break;
                case "blue":
                    blue = (byte)reader.GetInt32();
                    break;
                case "hue":
                    hue = (float)reader.GetDouble();
                    break;
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, SKColor value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("Alpha", value.Alpha);
        writer.WriteNumber("Red", value.Red);
        writer.WriteNumber("Green", value.Green);
        writer.WriteNumber("Blue", value.Blue);
        writer.WriteNumber("Hue", value.Hue);
        writer.WriteEndObject();
    }
}
