using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace MyDiary.Core.Models.Converters
{
    public class EfJsonConverter<T> : ValueConverter<T, string>
    {
        public EfJsonConverter() : base(
            p => JsonSerializer.Serialize(p, DiaryDbContext.jsonOptions),
            p => JsonSerializer.Deserialize<T>(p, DiaryDbContext.jsonOptions)
            )
        {
        }
    }
}
