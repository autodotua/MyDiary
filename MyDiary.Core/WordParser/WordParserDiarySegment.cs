using MyDiary.Models;

namespace MyDiary.WordParser
{
    public class WordParserDiarySegment
    {
        /// <summary>
        /// 这一部分的主题是什么，标题是什么
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 这一部分以什么时间单位，就是一年一章，还是一个月一章，还是每天都有
        /// </summary>
        public TimeUnit TimeUnit { get; set; }
        /// <summary>
        /// 每日的内容是如何编号的，是提供了带大纲级别的标题，还是用段落编号
        /// </summary>
        public NumberingType DayNumberingType { get; set; }
        public string MonthPattern { get; set; }
        public string DayPattern { get; set; }

    }
}
