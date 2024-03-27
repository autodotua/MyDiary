namespace MyDiary.WordParser
{
    public class WordParserOptions(int year, List<WordParserDiarySegment> segments)
    {
        public int Year { get; set; } = year;
        public List<WordParserDiarySegment> Segments { get; set; } = segments;
    }
}
