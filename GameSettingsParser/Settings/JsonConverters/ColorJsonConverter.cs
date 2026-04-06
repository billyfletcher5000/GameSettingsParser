using System.Windows.Media;
using Newtonsoft.Json;

namespace GameSettingsParser.Settings.JsonConverters
{
    public sealed class ColorJsonConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            var hex = $"#{value.A:X2}{value.R:X2}{value.G:X2}{value.B:X2}";
            writer.WriteValue(hex);
        }

        public override Color ReadJson(
            JsonReader reader,
            Type objectType,
            Color existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return Colors.Transparent;

            var text = reader.Value?.ToString();

            if (string.IsNullOrWhiteSpace(text))
                return Colors.Transparent;

            try
            {
                return (Color)ColorConverter.ConvertFromString(text)!;
            }
            catch
            {
                return Colors.Transparent;
            }
        }
    }
}