﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyDiary.Models.Converters
{
    public class JsonBlockConverter : JsonConverter<Block>
    {
        public override Block Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            if (doc.RootElement.TryGetProperty(nameof(Type), out JsonElement typeElement) && typeElement.ValueKind == JsonValueKind.String)
            {
                return typeElement.GetString() switch
                {
                    Block.TypeOfTextParagraph => JsonSerializer.Deserialize<TextParagraph>(doc.RootElement.GetRawText(), options),
                    Block.TypeOfTextStyle => JsonSerializer.Deserialize<TextStyle>(doc.RootElement.GetRawText(), options),
                    Block.TypeOfTableCell => JsonSerializer.Deserialize<TableCell>(doc.RootElement.GetRawText(), options),
                    Block.TypeOfTable => JsonSerializer.Deserialize<Table>(doc.RootElement.GetRawText(), options),
                    Block.TypeOfImage => JsonSerializer.Deserialize<Image>(doc.RootElement.GetRawText(), options),
                    _ => throw new NotImplementedException(),
                };
            }
            throw new FormatException("缺少Type属性");
        }

        public override void Write(Utf8JsonWriter writer, Block value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}