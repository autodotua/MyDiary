using MyDiary.Core.Models;
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
        public static async Task TestAsync()
        {
#if DEBUG
            using (var dm = new DocumentManager())
                dm.ClearDocumentsAsync().Wait();
#endif
            var options = new WordParserOptions(2024, [
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
                                Name="年度总结",
                                TimeHost=TimeUnit.Year,
                                DayNumberingType=NumberingType.ParagraphNumbering,
                            },
                ]);
            await ParseAsync(@"C:\Users\fz\OneDrive\旧事重提\日记\2021.docx", options);
        }

        public static async Task ParseAsync(string file, WordParserOptions options)
        {
            CheckOptions(options);
            using var fs = File.OpenRead(file);
            XWPFDocument doc = new XWPFDocument(fs);
            var ps = doc.Paragraphs;
            var segments = options.Segments.ToDictionary(p => p.Name);
            WordParserDiarySegment s = null;
            NullableDate date = default;
            int year = options.Year;
            List<WordParserError> errors = new List<WordParserError>();
            Dictionary<WordParserDiarySegment, Dictionary<NullableDate, Document>> documents = new();
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
                            date = new NullableDate(year);
                        }
                    }
                    else //currentSegment is not null
                    {
                        if (segments.TryGetValue(text, out WordParserDiarySegment value))
                        {
                            s = value;
                            date = new NullableDate(year);
                        }
                        else if (s.TimeHost is TimeUnit.Month or TimeUnit.Day
                           && CheckDateTitle(ref date, text, s.MonthPattern, errors, TimeUnit.Month, "月份"))
                        {
                            //跳转新月份
                        }
                        else if (s.TimeHost is TimeUnit.Day
                            && s.DayNumberingType == NumberingType.OutlineTitle
                            && CheckDateTitle(ref date, text, s.DayPattern, errors, TimeUnit.Day, "日"))
                        {
                            //跳转新日
                        }
                        else
                        {
                            AddParagraph();
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
                        if (date.Month.HasValue)
                        {
                            AddParagraph();
                        }
                    }
                    else if (s.TimeHost == TimeUnit.Day)
                    {
                        if (s.DayNumberingType == NumberingType.ParagraphNumbering
                            && HasNumberingEnabled(doc, p))//仅考虑启用了项目编号的段落
                        {
                            if (date.IsSpecified)
                            {
                                date = new NullableDate(date.Year, date.Month, date.Day + 1);
                            }
                            else
                            {
                                throw new NotImplementedException();
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
                        AddToDic(documents, s, date, t);
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
        private static bool HasNumberingEnabled(XWPFDocument doc, XWPFParagraph p)
        {
            return doc.GetCTStyle().GetStyleList().Where(s => s.styleId == p.StyleID).FirstOrDefault()?.pPr?.numPr != null;
        }
        private static void AddToDic(
            Dictionary<WordParserDiarySegment, Dictionary<NullableDate, Document>> docs4Seg,
            WordParserDiarySegment seg, NullableDate date, Block block)
        {
            if (!docs4Seg.ContainsKey(seg))
            {
                docs4Seg.Add(seg, new Dictionary<NullableDate, Document>());
            }
            Dictionary<NullableDate, Document> docs = docs4Seg[seg];
            Document doc;
            if (docs.TryGetValue(date, out Document value))
            {
                doc = value;
            }
            else
            {
                doc = new Document()
                {
                    Year = date.Year,
                    Month = seg.TimeHost is TimeUnit.Year ? null : date.Month,
                    Day = (seg.TimeHost is TimeUnit.Year or TimeUnit.Month) ? null : date.Day,
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

        private static bool CheckDateTitle(ref NullableDate value, string text, string pattern, IList<WordParserError> errors, TimeUnit type, string errorText)
        {
            if (Regex.IsMatch(text, pattern))
            {
                var match = Regex.Match(text, pattern);
                if (match.Groups.ContainsKey("value"))
                {
                    int tempValue = int.Parse(match.Groups["value"].ValueSpan);
                    if (type == TimeUnit.Month && (tempValue <= 0 || tempValue >= 13))
                    {
                        errors.Add(new WordParserError(errorText + "月超出范围", text));
                    }
                    else if (type == TimeUnit.Day && !value.Month.HasValue)
                    {
                        errors.Add(new WordParserError(errorText + "在指定日前未指定月", text));
                    }
                    else if (type == TimeUnit.Day && (tempValue <= 0 || tempValue > DateTime.DaysInMonth(value.Year, value.Month.Value)))
                    {
                        errors.Add(new WordParserError(errorText + "日超出范围", text));
                    }
                    else
                    {
                        switch (type)
                        {
                            case TimeUnit.Year:
                                break;
                            case TimeUnit.Month:
                                value = new NullableDate(value.Year, tempValue, 0);
                                break;
                            case TimeUnit.Day:
                                value = new NullableDate(value.Year, value.Month, tempValue);
                                break;
                        }
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
