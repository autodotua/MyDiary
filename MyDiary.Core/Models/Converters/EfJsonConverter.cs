using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace MyDiary.Models.Converters
{
    public class EfJsonConverter<T> : ValueConverter<T, string>
    {
        public EfJsonConverter() : base(
            p => JsonSerializer.Serialize(p, jsonOptions),
            p => JsonSerializer.Deserialize<T>(p, jsonOptions)
            )
        {
        }

        public static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = false,
            Converters = {
                new JsonBlockConverter(),
                new Json2DArrayConverter()
            }
        };
    }
}