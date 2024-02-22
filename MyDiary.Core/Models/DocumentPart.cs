namespace MyDiary.Core.Models
{
    public abstract class DocumentPart
    {
        public const string TypeOfTextElement = "Text";
        public const string TypeOfTable = "Table";
        public const string TypeOfImage = "Image";
        public abstract string Type { get; }
    }
}
