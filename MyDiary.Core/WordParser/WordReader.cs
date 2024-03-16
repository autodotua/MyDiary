using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyDiary.Core.WordParser
{
    public enum TimeUnit
    {
        Year,
        Month,
        Day,
    }
    [Flags]
    public enum NumberingType
    {
        OutlineTitle = 0x01,
        ParagraphNumbering = 0x02,
        Both = OutlineTitle | ParagraphNumbering,
    }
    public class WordParserDiarySegment
    {
        /// <summary>
        /// 这一部分的主题是什么，标题是什么
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 这一部分以什么时间单位，就是一年一章，还是一个月一章，还是每天都有
        /// </summary>
        public TimeUnit TimeHost { get; set; }
        /// <summary>
        /// 每日的内容是如何编号的，是提供了带大纲级别的标题，还是用段落编号
        /// </summary>
        public NumberingType DayNumberingType { get; set; }

    }
    public class WordParserOptions(int year,List<WordParserDiarySegment> segments)
    {
        public int Year { get; set; } = year;
        public List<WordParserDiarySegment> Segments { get; set; } = segments;
    }
    public class WordReader
    {
        public static void Test()
        {
            var options = new WordParserOptions(2023,[
                    new WordParserDiarySegment(){
                        Name="日记",
                        TimeHost=TimeUnit.Day,
                        DayNumberingType=NumberingType.ParagraphNumbering
                    },
                      new WordParserDiarySegment(){
                                Name="科研日志",
                                TimeHost=TimeUnit.Day,
                                DayNumberingType=NumberingType.ParagraphNumbering
                            },
                      new WordParserDiarySegment(){
                                Name="年终总结",
                                TimeHost=TimeUnit.Year,
                                DayNumberingType=NumberingType.ParagraphNumbering
                            },
                ]);
            Parse(@"C:\Users\autod\OneDrive\临时\2023.docx", options);
        }

        public static void Parse(string file, WordParserOptions options)
        {
            using var fs = File.OpenRead(file);
            XWPFDocument doc = new XWPFDocument(fs);
            var ps = doc.Paragraphs;
            var segments = options.Segments.ToDictionary(p => p.Name);
            WordParserDiarySegment currentSegment = null;
            int month = 1;
            int day = 1;
            foreach (XWPFParagraph p in ps)
            {
                var text = p.Text;
                var outline = GetOutlineLevel(p);
                if (IsCatalogue(p))
                {
                    continue;
                }
                if (currentSegment == null 
                    && outline > 0 
                    && segments.ContainsKey(text))
                {
                    currentSegment = segments[text];
                }

                Debug.WriteLine(outline);
                Debug.WriteLine(p.Text);
            }
        }

        static Regex rCatalogue = new Regex("\t\\w+$", RegexOptions.Compiled);
        private static bool IsCatalogue(XWPFParagraph p)
        {
            if (!rCatalogue.IsMatch(p.Text))
            {
                return false;
            }
            var outlineString = p.GetCTP().pPr.outlineLvl;
            return outlineString == null;
        }
        static int GetOutlineLevel(XWPFParagraph p)
        {
            string level = GetStyle(p)?.pPr?.outlineLvl?.val;
            return level == null ? 0 : (int.Parse(level) + 1);
        }

        static CT_Style GetStyle(XWPFParagraph p)
        {
            var styles = p.Document.GetStyles();
            if (p.StyleID == null)
            {
                return null;
            }
            return styles.GetStyle(p.StyleID).GetCTStyle();
        }
    }
}
