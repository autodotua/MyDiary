using System.Text.Json.Serialization;

namespace MyDiary.Models
{
    [JsonDerivedType(typeof(TextParagraph))]
    [JsonDerivedType(typeof(TextStyle))]
    [JsonDerivedType(typeof(TableCell))]
    [JsonDerivedType(typeof(Table))]
    [JsonDerivedType(typeof(Image))]
    public abstract class Block
    {
        public const string TypeOfTextParagraph = nameof(TextParagraph);
        public const string TypeOfTextStyle = nameof(TextStyle);
        public const string TypeOfTableCell = nameof(TableCell);
        public const string TypeOfTable =nameof(Table);
        public const string TypeOfImage = nameof(Image);
        public abstract string Type { get; }
    }
}