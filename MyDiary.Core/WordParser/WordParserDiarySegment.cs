using MyDiary.Models;

namespace MyDiary.WordParser
{
    public class WordParserDiarySegment
    {
        /// <summary>
        /// 这一部分的主题是什么，标题是什么。需要有大纲级别
        /// </summary>
        public string TitleInDocument { get; set; }

        /// <summary>
        /// 导入后的分类标签
        /// </summary>
        public string TargetTag { get; set; }

        /// <summary>
        /// 这一部分以什么时间单位，就是一年一章，还是一个月一章，还是每天都有
        /// </summary>
        public TimeUnit TimeUnit { get; set; }

        /// <summary>
        /// 每日的内容是如何编号的，是提供了带大纲级别的标题，还是用段落编号
        /// </summary>
        public NumberingType DayNumberingType { get; set; }

        /// <summary>
        /// 月份标题的正则表达式
        /// </summary>
        public string MonthPattern { get; set; }

        /// <summary>
        /// 日标题的正则表达式
        /// </summary>
        public string DayPattern { get; set; }

        /// <summary>
        /// 若包含内部标题，设置最大内部标题的大纲级别。0表示内部都是正文
        /// </summary>
        public int LargestInnerLevel { get; set; } = 0;

        /// <summary>
        /// 月份标题的大纲级别
        /// </summary>
        public int MonthTitleLevel { get; set; } = 2;

        /// <summary>
        /// 日标题的大纲级别
        /// </summary>
        public int DayTitleLevel { get; set; } = 3;
    }
}