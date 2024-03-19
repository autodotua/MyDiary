using MyDiary.Managers.Services;
using MyDiary.Models;
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
using Document = MyDiary.Models.Document;

namespace MyDiary.Core.WordParser
{

    public class WordReader
    {
        public static void Test()
        {
#if DEBUG
            using (var dm = new DocumentManager())
                dm.ClearDocumentsAsync().Wait();
#endif
            var options = new WordParserOptions(2023, [
                    new WordParserDiarySegment(){
                        Name="日记",
                        TimeHost=TimeUnit.Day,
                        DayNumberingType=NumberingType.ParagraphNumbering,
                        MonthPattern="(?<value>[0-1]?[0-9])月",
                    },
                      new WordParserDiarySegment(){
                                Name="科研日志",
                                TimeHost=TimeUnit.Day,
                                DayNumberingType=NumberingType.ParagraphNumbering,
                        MonthPattern="(?<value>[0-1]?[0-9])月",
                            },  new WordParserDiarySegment(){
                                Name="求职日志",
                                TimeHost=TimeUnit.Day,
                                DayNumberingType=NumberingType.ParagraphNumbering,
                        MonthPattern="(?<value>[0-1]?[0-9])月",
                            },
                      new WordParserDiarySegment(){
                                Name="年终总结",
                                TimeHost=TimeUnit.Year,
                                DayNumberingType=NumberingType.ParagraphNumbering,
                            },
                ]);
            ParseAsync(@"C:\Users\autod\OneDrive\旧事重提\日记\2023.docx", options).Wait();
        }

        public static async Task ParseAsync(string file, WordParserOptions options)
        {
            CheckOptions(options);
            using var fs = File.OpenRead(file);
            XWPFDocument doc = new XWPFDocument(fs);
            var ps = doc.Paragraphs;
            var segments = options.Segments.ToDictionary(p => p.Name);
            WordParserDiarySegment s = null;
            int month = 0;
            int day = 0;
            List<WordParserError> errors = new List<WordParserError>();
            Dictionary<WordParserDiarySegment, Dictionary<(int, int, int), Document>> documents = new();
            foreach (XWPFParagraph p in ps)
            {
                var text = p.Text;
                var outline = GetOutlineLevel(p);
                if (IsCatalogue(p))
                {
                    continue;
                }
                Debug.WriteLine(text);
                if (outline > 0)
                {
                    //进入一个新的主题
                    if (s == null)
                    {
                        if (segments.TryGetValue(text, out WordParserDiarySegment value))
                        {
                            s = value;
                            month = 0;
                            day = 0;
                        }
                    }
                    else //currentSegment is not null
                    {
                        if (segments.TryGetValue(text, out WordParserDiarySegment value))
                        {
                            s = value;
                            month = 0;
                            day = 0;
                        }
                        else if (s.TimeHost is TimeUnit.Month or TimeUnit.Day
                           && CheckDateTitle(ref month, text, s.MonthPattern, errors, "月份"))
                        {
                            day = 0;
                            //跳转新月份
                        }
                        else if (s.TimeHost is TimeUnit.Day
                            && s.DayNumberingType == NumberingType.OutlineTitle
                            && CheckDateTitle(ref day, text, s.DayPattern, errors, "日"))
                        {
                            //跳转新日期
                            if (day > DateTime.DaysInMonth(options.Year, month))
                            {
                                throw new Exception("日期已大于本月应有日期");
                            }
                        }
                    }
                }
                else //outline == 0
                {
                    if (s == null) //还没定位到时间
                    {
                        continue;
                    }
                    if (s.TimeHost == TimeUnit.Year)
                    {
                        AddParagraph();
                    }
                    else if (s.TimeHost == TimeUnit.Month)
                    {
                        if (month > 0)
                        {
                            AddParagraph();
                        }
                    }
                    else if (s.TimeHost == TimeUnit.Day)
                    {
                        if (s.DayNumberingType == NumberingType.ParagraphNumbering)
                        {
                            day++;
                            if (day > DateTime.DaysInMonth(options.Year, month))
                            {
                                //throw new Exception("日期已大于本月应有日期");
                            }
                        }
                        AddParagraph();
                    }
                }
                void AddParagraph()
                {
                    foreach (var line in text.Split('\r', '\n'))//正常情况下软换行\n
                    {
                        TextElement t = new TextElement() { Text = line };
                        AddToDic(documents, s, options.Year, month, day, t);
                    }
                }
            }

            using var dm = new DocumentManager();
            List<Document> allDocuments = new List<Document>(documents.Select(p => p.Value.Count).Sum());
            foreach (var docs in documents.Values)
            {
                allDocuments.AddRange(docs.Values);
            }
            await dm.SetDocumentsAsync(allDocuments);
        }

        private static void AddToDic(
            Dictionary<WordParserDiarySegment, Dictionary<(int, int, int), Document>> docs4Seg,
            WordParserDiarySegment seg,
            int year, int month, int day, Block block)
        {
            if (!docs4Seg.ContainsKey(seg))
            {
                docs4Seg.Add(seg, new Dictionary<(int, int, int), Document>());
            }
            Dictionary<(int, int, int), Document> docs = docs4Seg[seg];
            var date = (year, month, day);
            Document doc;
            if (docs.TryGetValue(date, out Document value))
            {
                doc = value;
            }
            else
            {
                doc = new Document()
                {
                    Year = year,
                    Month = month,
                    Day = day,
                    Tag = seg.Name,
                    Blocks = []
                };
                docs.Add(date, doc);
            }
            doc.Blocks.Add(block);
        }

        private static void CheckOptions(WordParserOptions options)
        {
            foreach (var seg in options.Segments)
            {
                if (seg.TimeHost is TimeUnit.Month or TimeUnit.Day && seg.MonthPattern == null)
                {
                    throw new NullReferenceException(nameof(WordParserDiarySegment.MonthPattern));
                }
                if (seg.TimeHost is TimeUnit.Day && seg.DayNumberingType == NumberingType.OutlineTitle && seg.DayPattern == null)
                {
                    throw new NullReferenceException(nameof(WordParserDiarySegment.DayPattern));
                }
            }
        }

        private static bool CheckDateTitle(ref int value, string text, string pattern, IList<WordParserError> errors, string errorText)
        {
            if (Regex.IsMatch(text, pattern))
            {
                var match = Regex.Match(text, pattern);
                if (match.Groups.ContainsKey("value"))
                {
                    int tempValue = int.Parse(match.Groups["value"].ValueSpan);
                    if (tempValue <= 0 || tempValue >= 13)
                    {
                        errors.Add(new WordParserError(errorText + "范围错误", text));
                    }
                    else if (tempValue <= value)
                    {
                        errors.Add(new WordParserError(errorText + "早于先前的" + errorText, text));
                    }
                    else
                    {
                        value = tempValue;
                        return true;
                    }
                }
                else //月份内无value组
                {
                    errors.Add(new WordParserError("找不到" + errorText, text));
                }
            }
            return false;

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
