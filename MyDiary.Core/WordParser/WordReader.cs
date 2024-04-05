using Mapster;
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
    public class WordReader(DocumentManager dm, PresetStyleManager pm, BinaryManager bm)
    {
        private readonly DocumentManager dm = dm;
        private readonly PresetStyleManager pm = pm;
        private readonly BinaryManager bm = bm;
        private readonly Regex rPicCaption = new Regex(@"^图 ?(([0-9]+)|([:：])).*$", RegexOptions.Compiled);

        public async Task<IList<Document>> ParseAsync(string file, WordParserOptions options)
        {
            CheckOptions(options);
            //using var fs = File.OpenRead(file);
            using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using XWPFDocument doc = new XWPFDocument(fs);

            IDictionary<int, TextStyle> level2Style = await pm.GetAllAsync();
            var tables = doc.Tables.ToDictionary(doc.GetPosOfTable);

            List<WordParserError> errors = new List<WordParserError>();
            Dictionary<WordParserDiarySegment, Dictionary<NullableDate, Document>> documents = new();
            Block lastBlock = null;
            foreach (var s in options.Segments)
            {
                NullableDate date = new NullableDate(options.Year);
                bool hasCaptured = false;
                for (int i = 0; i < doc.Paragraphs.Count; i++)
                {
                    XWPFParagraph p = doc.Paragraphs[i];
                    var outline = GetOutlineLevel(p);
                    //跳过目录
                    if (IsCatalogue(p))
                    {
                        lastBlock = null;
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
                        lastBlock = null;
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
                            GetOrCreateDocument(documents, s, date).Caption = title;
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
                        lastBlock = null;
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

                    async void AddParagraph()
                    {

                        if (tables.TryGetValue(i, out XWPFTable table))
                        {
                            Table newTable = new Table();
                            int columnCount = GetColumnCount(table);
                            newTable.Cells = new TableCell[table.NumberOfRows, columnCount];
                            for (int r = 0; r < table.NumberOfRows; r++)
                            {
                                for (int c1 = 0, c2 = 0; c1 < columnCount; c2++)
                                {
                                    var cell = table.GetRow(r).GetCell(c2);


                                    int columnSpan = GetCellColumnSpan(cell);
                                    if (cell.GetCTTc().tcPr.vMerge != null //当前单元格是上面合并下来的
                                        && cell.GetCTTc().tcPr.vMerge.val == ST_Merge.@continue)
                                    {
                                        int tempR = r - 1;
                                        while (tempR >= 0 && newTable.Cells[tempR, c1] == null)
                                        {
                                            tempR--;
                                        }
                                        newTable.Cells[tempR, c1].RowSpan++;
                                    }
                                    else//新建单元格
                                    {
                                        newTable.Cells[r, c1] = new TableCell() { Text = cell.GetText() };
                                        newTable.Cells[r, c1].ColumnSpan = columnSpan;
                                    }

                                    c1 += columnSpan;
                                }
                            }

                            GetOrCreateDocument(documents, s, date).Blocks.Add(newTable);
                            lastBlock = newTable;
                        }
                        else if (!string.IsNullOrEmpty(p.Text))//文本
                        {
                            if (lastBlock is Image && rPicCaption.IsMatch(p.Text))//图注
                            {
                                (lastBlock as Image).Caption = p.Text;
                            }
                            else//普通文本段落
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
                                    lastBlock = t;
                                }
                            }
                        }
                        else
                        {
                            foreach (var run in p.Runs)
                            {
                                if (run.GetEmbeddedPictures().Count != 0)//图片
                                {
                                    foreach (var pic in run.GetEmbeddedPictures())
                                    {
                                        var data = pic.GetPictureData().Data;
                                        var imageID = await bm.AddBinaryAsync(data);
                                        Image image = new Image()
                                        {
                                            DataId = imageID,
                                        };
                                        GetOrCreateDocument(documents, s, date).Blocks.Add(image);
                                        lastBlock = image;
                                    }
                                }
                            }
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
#if DEBUG
            foreach (var tempDoc in allDocuments)
            {
                var checkDoc = await dm.GetDocumentAsync(new NullableDate(tempDoc.Year, tempDoc.Month, tempDoc.Day), tempDoc.Tag);
                Debug.Assert(checkDoc.Equals(tempDoc));
            }
#endif
            return allDocuments;
        }
        private static int GetColumnCount(XWPFTable table)
        {
            int maxColumnCount = 0;

            // 遍历表格的每一行
            foreach (var row in table.Rows)
            {
                int columnCount = 0;

                // 遍历当前行的每一个单元格
                foreach (var cell in row.GetTableCells().OfType<XWPFTableCell>())
                {
                    int colSpan = GetCellColumnSpan(cell);
                    columnCount += colSpan;
                }

                // 更新最大列数
                if (columnCount > maxColumnCount)
                {
                    maxColumnCount = columnCount;
                }
            }

            return maxColumnCount;
        }

        private static int GetCellColumnSpan(XWPFTableCell cell)
        {
            // 获取合并单元格的列数
            return cell.GetCTTc().tcPr.gridSpan == null ? 1 : int.Parse(cell.GetCTTc().tcPr.gridSpan.val);
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