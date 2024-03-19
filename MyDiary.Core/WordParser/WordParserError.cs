namespace MyDiary.Core.WordParser
{
    public class WordParserError(string message, string paragraph)
    {
        public string Message { get; } = message;
        public string Paragraph { get; } = paragraph;
    }
}
