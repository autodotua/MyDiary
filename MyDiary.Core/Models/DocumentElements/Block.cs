using System.Text.Json.Serialization;

namespace MyDiary.Models
{
    [JsonDerivedType(typeof(TextElement))]
    [JsonDerivedType(typeof(Table))]
    [JsonDerivedType(typeof(Image))]
    public abstract class Block
    {
        public const string TypeOfTextElement = "Text";
        public const string TypeOfTable = "Table";
        public const string TypeOfImage = "Image";
        public abstract string Type { get; }

    }

}

