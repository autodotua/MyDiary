namespace MyDiary.WordParser
{
    public class WordParserOptions(int year, List<WordParserDiarySegment> segments)
    {
        public int Year { get; set; } = year;
        public IList<WordParserDiarySegment> Segments { get; set; } = segments;
    }
}