﻿using Mapster;
using MyDiary.Managers.Services;
using MyDiary.Models;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.SS.Formula.Functions;
using NPOI.XWPF.UserModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Document = MyDiary.Models.Document;

namespace MyDiary.WordParser
{
    public class WordReader(DocumentManager dm, PresetStyleManager pm)
    {
        private readonly DocumentManager dm = dm;
        private readonly PresetStyleManager pm = pm;

        public async Task ParseAsync(string file, WordParserOptions options)
        {
            CheckOptions(options);
            //using var fs = File.OpenRead(file);
            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using XWPFDocument doc = new XWPFDocument(fs);

            IDictionary<int, TextStyle> level2Style = await pm.GetAllAsync();

            List<WordParserError> errors = new List<WordParserError>();
            Dictionary<WordParserDiarySegment, Dictionary<NullableDate, Document>> documents = new();
            foreach (var s in options.Segments)
            {
                NullableDate date = new NullableDate(options.Year);
                bool hasCaptured = false;
                foreach (XWPFParagraph p in doc.Paragraphs)
                {
                    var outline = GetOutlineLevel(p);
                    //跳过目录
                    if (IsCatalogue(p))
                    {
                        continue;
                    }

                    //运行到此处，排除了目录

                    if (outline == 1)//一级大纲
                    {
                        if (p.Text == s.TitleInDocument) //找到了对应的Tag
                        {
                            hasCaptured = true;
                        }
                        else //不匹配，可能是还没找到，也可能是新的一节开始
                        {
                            if (hasCaptured)
                            {
                                //该部分结束，跳出
                                break;
                            }
                        }
                        continue;
                    }

                    //运行到此处，大纲级别为非1

                    if (outline > 1) //二级以上大纲
                    {
                        if (!hasCaptured) //还没定位到时间
                        {
                            continue;
                        }

                        //进入一个新的主题
                        if (s.TimeUnit is TimeUnit.Month or TimeUnit.Day
                            && outline == s.MonthTitleLevel
                          && CheckMonthTitle(ref date, p.Text, s.MonthPattern, errors, "月份"))
                        {
                            //跳转新月份
                        }
                        else if (s.TimeUnit is TimeUnit.Day
                            && outline == s.DayTitleLevel
                            && s.DayNumberingType == NumberingType.OutlineTitle
                            && CheckDayTitle(ref date, p.Text, s.DayPattern, errors, "日", out string title))
                        {
                            //跳转新日
                            GetOrCreateDocument(documents, s, date).Title = title;
                        }
                        else //不是月或日的标题
                        {
                            //如果允许插入标题并且设置的最大标题级别大于当前段落的级别
                            if (s.LargestInnerLevel > 0 && outline >= s.LargestInnerLevel)
                            {
                                if (s.TimeUnit == TimeUnit.Day && s.DayNumberingType == NumberingType.ParagraphNumbering)
                                {
                                    //序号形式不可能存在内部大纲级别
                                    continue;
                                }
                                AddParagraph();
                            }
                        }
                        continue;
                    }

                    //运行到此处，无大纲级别正文

                    if (!hasCaptured) //还没定位到时间
                    {
                        continue;
                    }

                    switch (s.TimeUnit)
                    {
                        case TimeUnit.Year:
                            AddParagraph();
                            break;
                        case TimeUnit.Month:
                            if (date.Month.HasValue)
                            {
                                AddParagraph();
                            }
                            break;
                        case TimeUnit.Day:
                            if (date.IsSpecified)
                            {
                                switch (s.DayNumberingType)
                                {
                                    case NumberingType.OutlineTitle:
                                        AddParagraph();
                                        break;
                                    case NumberingType.ParagraphNumbering:
                                        if (HasNumberingEnabled(doc, p))//仅考虑启用了项目编号的段落
                                        {
                                            date = new NullableDate(date.Year, date.Month, date.Day + 1);
                                            AddParagraph();
                                        }
                                        break;
                                }
                            }
                            break;
                    }

                    void AddParagraph()
                    {
                        foreach (var line in p.Text.Split('\r', '\n'))//正常情况下软换行\n
                        {
                            TextParagraph t = new TextParagraph() { Text = line };
                            if (outline > 0)
                            {
                                t.Level = outline - (s.LargestInnerLevel - 1);
                            }
                            if (level2Style.TryGetValue(t.Level, out TextStyle value))
                            {
                                value.Adapt(t);
                            }
                            GetOrCreateDocument(documents, s, date).Blocks.Add(t);
                        }
                    }
                }
            }
            List<Document> allDocuments = new List<Document>(documents.Select(p => p.Value.Count).Sum());
            foreach (var docs in documents.Values)
            {
                allDocuments.AddRange(docs.Values);
            }
            await dm.SetDocumentsAsync(allDocuments);
        }

        private static bool HasNumberingEnabled(XWPFDocument doc, XWPFParagraph p)
        {
            return p.GetCTP().pPr?.numPr != null
           || doc.GetCTStyle().GetStyleList().Where(s => s.styleId == p.StyleID).FirstOrDefault()?.pPr?.numPr != null;
        }

        private static Document GetOrCreateDocument(Dictionary<WordParserDiarySegment, Dictionary<NullableDate, Document>> docs4Seg, WordParserDiarySegment seg, NullableDate date)
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
                    Month = seg.TimeUnit is TimeUnit.Year ? null : date.Month,
                    Day = (seg.TimeUnit is TimeUnit.Year or TimeUnit.Month) ? null : date.Day,
                    Tag = seg.TargetTag,
                    Blocks = []
                };
                docs.Add(date, doc);
            }

            return doc;
        }

        private static void CheckOptions(WordParserOptions options)
        {
            foreach (var seg in options.Segments)
            {
                if (seg.TimeUnit is TimeUnit.Month or TimeUnit.Day && seg.MonthPattern == null)
                {
                    throw new NullReferenceException(nameof(WordParserDiarySegment.MonthPattern));
                }
                if (seg.TimeUnit is TimeUnit.Day && seg.DayNumberingType == NumberingType.OutlineTitle && seg.DayPattern == null)
                {
                    throw new NullReferenceException(nameof(WordParserDiarySegment.DayPattern));
                }
            }
        }

        private static bool CheckMonthTitle(ref NullableDate value, string text, string pattern, IList<WordParserError> errors, string errorText)
        {
            if (Regex.IsMatch(text, pattern))
            {
                var match = Regex.Match(text, pattern);
                if (match.Groups.ContainsKey("month"))
                {
                    int tempValue = int.Parse(match.Groups["month"].ValueSpan);
                    if (tempValue <= 0 || tempValue >= 13)
                    {
                        errors.Add(new WordParserError(errorText + "月超出范围", text));
                    }
                    else
                    {
                        value = new NullableDate(value.Year, tempValue, 0);
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

        private static bool CheckDayTitle(ref NullableDate value, string text, string pattern, IList<WordParserError> errors, string errorText, out string title)
        {
            title = null;
            if (Regex.IsMatch(text, pattern))
            {
                var match = Regex.Match(text, pattern);
                if (match.Groups.ContainsKey("day"))
                {
                    int tempValue = int.Parse(match.Groups["day"].ValueSpan);
                    if (!value.Month.HasValue)
                    {
                        errors.Add(new WordParserError(errorText + "在指定日前未指定月", text));
                    }
                    else if (tempValue <= 0 || tempValue > DateTime.DaysInMonth(value.Year, value.Month.Value))
                    {
                        errors.Add(new WordParserError(errorText + "日超出范围", text));
                    }
                    else
                    {
                        value = new NullableDate(value.Year, value.Month, tempValue);
                        if (match.Groups.ContainsKey("title"))
                        {
                            title = match.Groups["title"].Value;
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


        private static Regex rCatalogue = new Regex("\t\\w+$", RegexOptions.Compiled);

        private static bool IsCatalogue(XWPFParagraph p)
        {
            if (!rCatalogue.IsMatch(p.Text))
            {
                return false;
            }
            var outlineString = p.GetCTP().pPr.outlineLvl;
            return outlineString == null;
        }

        private static int GetOutlineLevel(XWPFParagraph p)
        {
            string level = GetStyle(p)?.pPr?.outlineLvl?.val;
            return level == null ? 0 : (int.Parse(level) + 1);
        }

        private static CT_Style GetStyle(XWPFParagraph p)
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