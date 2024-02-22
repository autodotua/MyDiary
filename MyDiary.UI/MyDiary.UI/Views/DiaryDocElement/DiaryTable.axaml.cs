using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using FzLib;
using MyDiary.Core.Models;
using MyDiary.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class DiaryTable : Grid, IDiaryElement
{

    /**
     * Grid的ColumnDefinitions/RowDefinitions：
     * 10                                                  4           64               4        ……         4           10
     * 用来支持调整大小的多余空间    边框      DiaryTableCell       边框    ……       边框     用来支持调整大小的多余空间
     */

    /// <summary>
    /// 边框实际（调整区）粗细
    /// </summary>
    private const double InnerBorderWidth = 2;

    private const double DefaultColumnWidth = 64;

    private readonly DiaryTableVM viewModel;

    public DiaryTable()
    {
        DataContext = viewModel = new DiaryTableVM();

        InitializeComponent();
    }

    #region 和EditBar的数据交换

    public event EventHandler NotifyEditDataUpdated;

    public EditBarVM GetEditData()
    {
        var txts = GetSelectedCells().ToList();
        if (txts.Count == 0)
        {
            return null;
        }
        var data = txts[0].CellData;
        var datas = txts.Select(p => p.CellData).ToHashSet();

        Debug.WriteLine(datas.Count);
        var ep = new EditBarVM(datas.Cast<TextElementInfo>().ToList())
        {
            CanMergeCell = CellsSelectionMode switch
            {
                TableCellsSelectionMode.None => data.RowSpan * data.ColumnSpan > 1,
                TableCellsSelectionMode.Selecting => false,
                TableCellsSelectionMode.Selected => true,
                _ => false
            },
            CellsMerged = CellsSelectionMode switch
            {
                TableCellsSelectionMode.None => data.RowSpan * data.ColumnSpan > 1,
                _ => false
            },
        };

        ep.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(EditBarVM.CellsMerged) when ep.CellsMerged == true:
                    if (CellsSelectionMode != TableCellsSelectionMode.Selected)
                    {
                        throw new Exception($"{nameof(CellsSelectionMode)}状态错误");
                    }
                    MergeCells();
                    break;
                case nameof(EditBarVM.CellsMerged) when ep.CellsMerged == false:
                    if (CellsSelectionMode != TableCellsSelectionMode.None)
                    {
                        throw new Exception($"{nameof(CellsSelectionMode)}状态错误");
                    }
                    if (txts.Count != 1)
                    {
                        throw new Exception($"查找到的DiaryTableCell数量错误");
                    }
                    SplitMergedCell(txts[0]);
                    break;
            }
        };
        return ep;
    }

    private void SplitMergedCell(DiaryTableCell txt)
    {
        bool first = true;
        for (int r = txt.TableRow; r < txt.TableRow + txt.CellData.RowSpan; r++)
        {
            for (int c = txt.TableColumn; c < txt.TableColumn + txt.CellData.ColumnSpan; c++)
            {
                if (first)
                {
                    first = false;
                    continue;
                }
                CreateAndInsertCellTextBox(r, c, new TableCellInfo());
            }
        }
        txt.CellData.ColumnSpan = 1;
        txt.CellData.RowSpan = 1;

        var data2 = GetCellsData();
        CreateTableStructure(data2);
        NotifyEditDataUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void MergeCells()
    {
        var topLeftTextBox = textBoxes[selectionTopIndex, selectionLeftIndex];
        topLeftTextBox.CellData.ColumnSpan = selectionRightIndex - selectionLeftIndex + 1;
        topLeftTextBox.CellData.RowSpan = selectionBottomIndex - selectionTopIndex + 1;
        for (int r = selectionTopIndex; r <= selectionBottomIndex; r++)
        {
            for (int c = selectionLeftIndex; c <= selectionRightIndex; c++)
            {
                if (textBoxes[r, c] != topLeftTextBox)
                {
                    grd.Children.Remove(textBoxes[r, c]);
                    textBoxes[r, c] = topLeftTextBox;
                }
            }
        }
        var data = GetCellsData();
        CreateTableStructure(data);
        ClearCellsSelection();
        topLeftTextBox.Focus();
    }
    #endregion


    #region 建立表格
    public void MakeEmptyTable(int row, int column)
    {
        TableCellInfo[,] data = new TableCellInfo[row, column];
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                data[i, j] = new TableCellInfo();
            }
        }
#if DEBUG
        data[1, 1] = new TableCellInfo(2, 2, null);
        data[4, 0] = new TableCellInfo(2, 3, null);
        data[2, 3] = new TableCellInfo(2, 3, null);
#endif
        MakeTable(data);
    }

    public void MakeTable(TableCellInfo[,] data)
    {
        CreateTableStructure(data);
        //向单元格中填充DiaryTableCell
        FillTextBoxes(data);
    }



    private void CreateTableStructure(TableCellInfo[,] data)
    {
        int row = data.GetLength(0);
        int column = data.GetLength(1);

        //清除边框结构
        grd.ColumnDefinitions.Clear();
        grd.RowDefinitions.Clear();
        foreach (var child in grd.Children.OfType<GridSplitter>().ToList())
        {
            grd.Children.Remove(child);
        }

        //纵向
        grd.ColumnDefinitions.Add(new ColumnDefinition(10, GridUnitType.Pixel));
        for (int c = 0; c <= column; c++)
        {
            if (c > 0)
            {
                grd.ColumnDefinitions.Add(new ColumnDefinition(DefaultColumnWidth, GridUnitType.Pixel));
            }
            grd.ColumnDefinitions.Add(new ColumnDefinition(InnerBorderWidth, GridUnitType.Pixel));


            var splitter = new GridSplitter()
            {
                Background = Brushes.Transparent,
                ZIndex = 1,
            };
            SetRowSpan(splitter, int.MaxValue);
            SetColumn(splitter, BID2GID(c));
            grd.Children.Add(splitter);

        }
        grd.ColumnDefinitions.Add(new ColumnDefinition(10, GridUnitType.Pixel));

        //横向
        grd.RowDefinitions.Add(new RowDefinition(10, GridUnitType.Pixel));
        for (int r = 0; r <= row; r++)
        {
            if (r > 0)
            {
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
            }
            grd.RowDefinitions.Add(new RowDefinition(InnerBorderWidth, GridUnitType.Pixel));
            var splitter = new GridSplitter()
            {
                Background = Brushes.Transparent,
                ZIndex = 1
            };
            SetRow(splitter, BID2GID(r));
            grd.Children.Add(splitter);

        }
        grd.RowDefinitions.Add(new RowDefinition(10, GridUnitType.Pixel));
    }
    #endregion

    #region 框选

    enum TableCellsSelectionMode
    {
        None,
        Selecting,
        Selected
    }

    private TableCellsSelectionMode cellsSelectionMode = TableCellsSelectionMode.None;

    /// <summary>
    /// 鼠标首次按下时的位置
    /// </summary>
    private Point? pointerDownPosition;

    /// <summary>
    /// 红色的选择单元格的框
    /// </summary>
    private Border selectionBorder = null;

    /// <summary>
    /// 选择后DiaryTableCell边界
    /// </summary>
    private int selectionLeftIndex, selectionRightIndex, selectionTopIndex, selectionBottomIndex;

    private TableCellsSelectionMode CellsSelectionMode
    {
        get => cellsSelectionMode;
        set
        {
            var oldValue = cellsSelectionMode;
            cellsSelectionMode = value;
            if (value != oldValue)
            {
                grd.Children.OfType<GridSplitter>()
                    .ForEach(p => p.IsEnabled = value == TableCellsSelectionMode.None);
                NotifyEditDataUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        StopSelectingCells();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var point = e.GetCurrentPoint(pnlSelection);

        //左键没有按下，不执行操作，若之前在选择状态，则清除
        if (!point.Properties.IsLeftButtonPressed)
        {
            StopSelectingCells();
            return;
        }

        //若焦点不在DiaryTableCell（例如在调整边框），则不执行操作
        if (TopLevel.GetTopLevel(this).FocusManager.GetFocusedElement() is GridSplitter)
        {
            return;
        }
        //首次按下，记录
        if (pointerDownPosition == null)
        {
            //由于无法使用OnPointerPressed获取位置，所以在Moved中获取
            pointerDownPosition = point.Position;
            return;
        }

        //开始框选
        SelectCells(point);
    }

    private void ClearCellsSelection()
    {
        if (CellsSelectionMode != TableCellsSelectionMode.None)
        {
            CellsSelectionMode = TableCellsSelectionMode.None;
            selectionBorder = null;
            pointerDownPosition = null;
            pnlSelection.Children.Clear();
        }
    }

    private DiaryTableCell GetFocusedCell()
    {
        var element = TopLevel.GetTopLevel(this).FocusManager.GetFocusedElement();
        if (element is DiaryTableCell txt)
        {
            if (txt.GetLogicalAncestors().Contains(this))
            {
                return txt;
            }
            return null;
        }
        return null;
    }

    /// <summary>
    /// 获取被选择的范围内的
    /// </summary>
    /// <param name="returnFocusedWhenNotSelecting">若开启，则在未选择状态会返回焦点所在文本框</param>
    /// <returns></returns>
    private IEnumerable<DiaryTableCell> GetSelectedCells(bool returnFocusedWhenNotSelecting = true)
    {

        if (CellsSelectionMode == TableCellsSelectionMode.None)
        {
            if (returnFocusedWhenNotSelecting)
            {
                var txt = GetFocusedCell();
                if (txt != null)
                {
                    yield return txt;
                }
            }
        }
        else
        {
            HashSet<DiaryTableCell> set = new HashSet<DiaryTableCell>();
            for (int r = selectionTopIndex; r <= selectionBottomIndex; r++)
            {
                for (int c = selectionLeftIndex; c <= selectionRightIndex; c++)
                {
                    if (!set.Contains(textBoxes[r, c]))
                    {
                        set.Add(textBoxes[r, c]);
                        yield return textBoxes[r, c];
                    }
                }
            }
        }
    }

    private void SelectCells(PointerPoint point)
    {
        CellsSelectionMode = TableCellsSelectionMode.Selecting;

        //创建框选显示框
        if (selectionBorder == null)
        {
            selectionBorder = new Border()
            {
                BorderThickness = new Thickness(4),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsVisible = false
            };
            selectionBorder[!Border.BorderBrushProperty] = new DynamicResourceExtension("Accent0");
            pnlSelection.Children.Add(selectionBorder);
        }

        //整理Grid的每个行列距离顶部和左侧的累计距离
        var gridLefts = grd.ColumnDefinitions.Select(p => p.ActualWidth).ToList();
        gridLefts[0] = grd.Bounds.Left;
        for (int i = 1; i < gridLefts.Count; i++)
        {
            gridLefts[i] = gridLefts[i] + gridLefts[i - 1];
        }
        var gridTops = grd.RowDefinitions.Select(p => p.ActualHeight).ToList();
        gridTops[0] = grd.Bounds.Top;
        for (int i = 1; i < gridTops.Count; i++)
        {
            gridTops[i] = gridTops[i] + gridTops[i - 1];
        }

        //计算鼠标框选实际范围的边界
        var left = Math.Min(pointerDownPosition.Value.X, point.Position.X);
        var right = Math.Max(pointerDownPosition.Value.X, point.Position.X);
        var top = Math.Min(pointerDownPosition.Value.Y, point.Position.Y);
        var bottom = Math.Max(pointerDownPosition.Value.Y, point.Position.Y);

        //计算与鼠标实际框选范围相交的单元格
        int leftIndex = -1, rightIndex = -1, topIndex = -1, bottomIndex = -1;
        for (int i = 2; i <= gridLefts.Count - 2; i += 2)
        {
            if (left > gridLefts[i - 2] && left < gridLefts[i + 1])
            {
                leftIndex = GID2TID(i);
            }
            if (right > gridLefts[i - 2] && right < gridLefts[i + 1])
            {
                rightIndex = GID2TID(i);
            }
        }
        for (int i = 2; i <= gridTops.Count - 2; i += 2)
        {
            if (top > gridTops[i - 2] && top < gridTops[i + 1])
            {
                topIndex = GID2TID(i);
            }
            if (bottom > gridTops[i - 2] && bottom < gridTops[i + 1])
            {
                bottomIndex = GID2TID(i);
            }
        }
        //没框到
        if (leftIndex < 0 || rightIndex < 0 || topIndex < 0 || bottomIndex < 0)
        {
            selectionBorder.IsVisible = false;
            return;
        }

        //确定最终范围。上述的范围可能会截断一些合并的单元格，需要确保框选范围内都是完整的单元格。
        selectionLeftIndex = leftIndex;
        selectionRightIndex = rightIndex;
        selectionTopIndex = topIndex;
        selectionBottomIndex = bottomIndex;
        bool needCalculateRange = true;
        while (needCalculateRange)
        {
            for (int i = leftIndex; i <= rightIndex; i++)
            {
                for (int j = topIndex; j <= bottomIndex; j++)
                {
                    var txt = textBoxes[j, i];
                    selectionLeftIndex = Math.Min(selectionLeftIndex, txt.TableColumn);
                    selectionRightIndex = Math.Max(selectionRightIndex, txt.TableColumn + txt.CellData.ColumnSpan - 1);
                    selectionTopIndex = Math.Min(selectionTopIndex, txt.TableRow);
                    selectionBottomIndex = Math.Max(selectionBottomIndex, txt.TableRow + txt.CellData.RowSpan - 1);
                }
            }
            //Debug.WriteLine($"{selectionLeftIndex},{selectionRightIndex},{selectionTopIndex},{selectionBottomIndex}");

            //在范围更新、保证了不截断合并的单元格后，
            //新扩充的范围内可能又包含了新的被截断的合并单元格，
            //因此需要再次进行判断更新。
            if (selectionLeftIndex != leftIndex
                || selectionRightIndex != rightIndex
                || selectionTopIndex != topIndex
                || selectionBottomIndex != bottomIndex)
            {
                leftIndex = selectionLeftIndex;
                rightIndex = selectionRightIndex;
                topIndex = selectionTopIndex;
                bottomIndex = selectionBottomIndex;
                needCalculateRange = true;
            }
            else
            {
                needCalculateRange = false;
            }
        }

        //如果只框到一个，那么认为是在内部选择文字，不处理
        if (!GetSelectedCells(false).Skip(1).Any())
        {
            selectionBorder.IsVisible = false;
            return;
        }

        //设置框的位置
        selectionBorder.IsVisible = true;
        selectionBorder.Margin = new Thickness(
            grd.Bounds.Left + textBoxes[selectionTopIndex, selectionLeftIndex].Bounds.Left - InnerBorderWidth,
            grd.Bounds.Top + textBoxes[selectionTopIndex, selectionLeftIndex].Bounds.Top - InnerBorderWidth,
            Bounds.Width - grd.Bounds.Left - textBoxes[selectionBottomIndex, selectionRightIndex].Bounds.Right - InnerBorderWidth,
            Bounds.Height - grd.Bounds.Top - textBoxes[selectionBottomIndex, selectionRightIndex].Bounds.Bottom - InnerBorderWidth
            );
    }

    private void StopSelectingCells()
    {
        if (selectionBorder != null && selectionBorder.IsVisible)
        {
            CellsSelectionMode = TableCellsSelectionMode.Selected;
        }
        else
        {
            CellsSelectionMode = TableCellsSelectionMode.None;
            if (selectionBorder != null)
            {
                pnlSelection.Children.Remove(selectionBorder);
                pointerDownPosition = null;
            }
        }
    }
    #endregion

    #region 索引转换

    /// <summary>
    /// Border (GridSplitter/Rectangle) Row/Column 转 Grid Row/Column
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    internal static int BID2GID(int index)
    {
        return index * 2 + 1;
    }

    internal static int GID2BID(int index)
    {
        Debug.Assert(index % 2 == 1);
        return (index - 1) / 2;
    }

    internal static int GID2TID(int index)
    {
        Debug.Assert(index % 2 == 0);
        return (index - 2) / 2;
    }

    /// <summary>
    /// DiaryTableCell Row/Column 转 Grid Row/Column
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    internal static int TID2GID(int index)
    {
        return index * 2 + 2;
    }

    #endregion

    #region 文本框处理

    /// <summary>
    /// 网格对应的DiaryTableCell[行号,列号]。
    /// </summary>
    /// <remarks>
    /// 程序中有两个与表格对应的数组，分别是<see cref="DiaryTableCell"/>[]和<see cref="TableCellInfo"/>[]。
    /// 前者通过<see cref="textBoxes"/>保存在对象中，保持与实际显示的一致，
    /// 而后者需要时通过<see cref="GetCellsData"/>进行获取。
    /// <br/>
    /// 两者有一个不同点，对于合并单元格，
    /// <see cref="textBoxes"/>的每个子单元格都为相同值，即合并的单元格；
    /// 而<see cref="TableCellInfo"/>[]仅左上角保存值，其余均设置为null。
    /// </remarks>
    private DiaryTableCell[,] textBoxes;

    private void AddTextBoxMenuEvents(DiaryTableCell txt)
    {
        var menu = txt.ContextFlyout as MenuFlyout;

        Debug.Assert(menu != null);
        var items = menu.Items.Cast<MenuItem>().Skip(3).Where(p => !p.Header.Equals("-")).ToList();
        foreach (var item in items)
        {
            switch (item.Name)
            {
                //在上方插入
                case "InsertUp":
                    item.Click += (s, e) =>
                     {
                         e.Handled = true;
                         InsertRow(s, true);
                     };
                    break;
                //在下方插入
                case "InsertDown":
                    item.Click += (s, e) =>
                     {
                         e.Handled = true;
                         InsertRow(s, false);
                     };
                    break;
                //在左侧插入
                case "InsertLeft":
                    item.Click += (s, e) =>
                     {
                         e.Handled = true;
                         InsertColumn(s, true);
                     };
                    break;
                //在右侧插入
                case "InsertRight":
                    item.Click += (s, e) =>
                     {
                         e.Handled = true;
                         InsertColumn(s, false);
                     };
                    break;
                //删除行
                case "DeleteRow":
                    item.Click += (s, e) =>
                     {
                         e.Handled = true;
                         DeleteRow(s);
                     };
                    break;
                //删除列
                case "DeleteColumn":
                    item.Click += (s, e) =>
                     {
                         e.Handled = true;
                         DeleteColumn(s);
                     };
                    break;
            }
        }


        void InsertRow(object sender, bool up)
        {
            //如果直接拿上面的txt，永远都是[0,0]那个，不确定是什么原因，可能是闭包相关问题
            LoadData(sender, out TableCellInfo[,] data, out int insertRow, out int insertColumn, out int height, out int width);
            if (!up)
            {
                insertRow += textBoxes[insertRow, insertColumn].CellData.RowSpan;
            }

            //网格加两行，一行内容一行边框
            grd.RowDefinitions.Insert(grd.RowDefinitions.Count - 2, new RowDefinition(InnerBorderWidth, GridUnitType.Pixel));
            grd.RowDefinitions.Insert(grd.RowDefinitions.Count - 2, new RowDefinition(1, GridUnitType.Auto));
            //新的文本框数组
            var newTextBoxes = new DiaryTableCell[height + 1, width];
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    var d = data[r, c];
                    var t = textBoxes[r, c];
                    if (d == null)
                    {
                        continue;
                    }
                    if (r >= insertRow) //在插入线下方
                    {
                        //那么整体下移一格
                        t.TableRow = r + 1;
                    }
                    else //在插入线上方
                     if (r + d.RowSpan > insertRow) //如果这个合并的单元格跨越了插入线
                    {
                        //这个合并单元格跨越的行需要多一行
                        d.RowSpan++;
                    }
                    //新的行号
                    int newR = r >= insertRow ? r + 1 : r;

                    //更新文本框数组
                    for (int rr = newR; rr < newR + d.RowSpan; rr++)
                    {
                        for (int cc = c; cc < c + d.ColumnSpan; cc++)
                        {
                            newTextBoxes[rr, cc] = t;
                        }
                    }

                }
            }
            textBoxes = newTextBoxes;
            //复制新增行，填充没有被合并单元格占据的单元格
            for (int i = 0; i < width; i++)
            {
                if (textBoxes[insertRow, i] == null)
                {
                    CreateAndInsertCellTextBox(insertRow, i, new TableCellInfo()
                    {
#if DEBUG
                        Text = "Insert"
#endif
                    });
                }
            }
        }

        void DeleteRow(object sender)
        {
            //如果直接拿上面的txt，永远都是[0,0]那个，不确定是什么原因，可能是闭包相关问题
            var txt = LoadData(sender, out TableCellInfo[,] data, out int deleteRow, out int deleteColumn, out int height, out int width);
            int rowSpan = txt.CellData.RowSpan; //选中的如果是合并的单元格，可以同时删除多行
            grd.RowDefinitions.RemoveRange(2, rowSpan * 2);
            //新的文本框数组
            var newTextBoxes = new DiaryTableCell[height - rowSpan, width];
            var newTextBoxesSet = new HashSet<DiaryTableCell>(); //用来保存需要保留的DiaryTableCell
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    var d = data[r, c];
                    var t = textBoxes[r, c];
                    if (d == null)
                    {
                        continue;
                    }
                    int newR = -1;
                    if (r >= deleteRow + rowSpan) //在删除行下方
                    {
                        //那么整体上移一格
                        t.TableRow = r - rowSpan;
                        newR = r - rowSpan;
                    }
                    else if (r >= deleteRow && r < deleteRow + rowSpan) //就是要删除的那一行
                    {
                        if (d.RowSpan > rowSpan + deleteRow - r) //如果周围某一个已合并单元格的一部分在要删除的行中
                        {
                            d.RowSpan -= rowSpan + deleteRow - r; //计算新的行号，rowSpan + deleteRow - r指的是从要删除的n行中与当前合并单元格不重叠的行数
                                                                  //设置新行，在这种情况下，一定是欲删除的行
                            t.TableRow = deleteRow;
                            newR = deleteRow;
                        }
                        else//其他情况，则准备删除单元格
                        {
                            continue;
                        }
                    }
                    else//在插入线上方
                    {
                        if (r + d.RowSpan > deleteRow) //如果这个合并的单元格跨越了插入线
                        {
                            //删除的行数等于这个单元格和要删除的行重叠的行数
                            d.RowSpan -= r + d.RowSpan - deleteRow;
                        }
                        newR = r;
                    }

                    //更新文本框数组
                    for (int rr = newR; rr < newR + d.RowSpan; rr++)
                    {
                        for (int cc = c; cc < c + d.ColumnSpan; cc++)
                        {
                            newTextBoxes[rr, cc] = t;
                        }
                    }
                    newTextBoxesSet.Add(t);
                }
            }
            //删除不再使用的DiaryTableCell
            foreach (var t in textBoxes)
            {
                if (!newTextBoxesSet.Contains(t))
                {
                    grd.Children.Remove(t);
                }
            }
            textBoxes = newTextBoxes;
        }

        void InsertColumn(object sender, bool left)
        {
            LoadData(sender, out TableCellInfo[,] data, out int insertRow, out int insertColumn, out int height, out int width);

            if (!left)
            {
                insertColumn += textBoxes[insertRow, insertColumn].CellData.ColumnSpan;
            }

            grd.ColumnDefinitions.Insert(grd.ColumnDefinitions.Count - 2, new ColumnDefinition(InnerBorderWidth, GridUnitType.Pixel));
            grd.ColumnDefinitions.Insert(grd.ColumnDefinitions.Count - 2, new ColumnDefinition(DefaultColumnWidth, GridUnitType.Pixel));

            var newTextBoxes = new DiaryTableCell[height, width + 1];

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    var d = data[r, c];
                    var t = textBoxes[r, c];

                    if (d == null)
                    {
                        continue;
                    }

                    if (c >= insertColumn)
                    {
                        t.TableColumn = c + 1;
                    }
                    else if (c + d.ColumnSpan > insertColumn)
                    {
                        d.ColumnSpan++;
                    }

                    int newC = c >= insertColumn ? c + 1 : c;

                    for (int cc = newC; cc < newC + d.ColumnSpan; cc++)
                    {
                        for (int rr = r; rr < r + d.RowSpan; rr++)
                        {
                            newTextBoxes[rr, cc] = t;
                        }
                    }
                }
            }

            textBoxes = newTextBoxes;

            for (int i = 0; i < height; i++)
            {
                if (textBoxes[i, insertColumn] == null)
                {
                    CreateAndInsertCellTextBox(i, insertColumn, new TableCellInfo()
                    {
#if DEBUG
                        Text = "Insert"
#endif
                    });
                }
            }
        }

        void DeleteColumn(object sender)
        {
            var txt = LoadData(sender, out TableCellInfo[,] data, out int deleteRow, out int deleteColumn, out int height, out int width);
            int columnSpan = txt.CellData.ColumnSpan;
            grd.ColumnDefinitions.RemoveRange(2, columnSpan * 2);

            var newTextBoxes = new DiaryTableCell[height, width - columnSpan];
            var newTextBoxesSet = new HashSet<DiaryTableCell>();

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    var d = data[r, c];
                    var t = textBoxes[r, c];

                    if (d == null)
                    {
                        continue;
                    }

                    int newC = -1;

                    if (c >= deleteColumn + columnSpan)
                    {
                        t.TableColumn = c - columnSpan;
                        newC = c - columnSpan;
                    }
                    else if (c >= deleteColumn && c < deleteColumn + columnSpan)
                    {
                        if (d.ColumnSpan > columnSpan + deleteColumn - c)
                        {
                            d.ColumnSpan -= columnSpan + deleteColumn - c;
                            t.TableColumn = deleteColumn;
                            newC = deleteColumn;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (c + d.ColumnSpan > deleteColumn)
                        {
                            d.ColumnSpan -= c + d.ColumnSpan - deleteRow;
                        }
                        newC = c;
                    }

                    for (int cc = newC; cc < newC + d.ColumnSpan; cc++)
                    {
                        for (int rr = r; rr < r + d.RowSpan; rr++)
                        {
                            newTextBoxes[rr, cc] = t;
                        }
                    }
                    newTextBoxesSet.Add(t);
                }
            }

            foreach (var t in textBoxes)
            {
                if (!newTextBoxesSet.Contains(t))
                {
                    grd.Children.Remove(t);
                }
            }

            textBoxes = newTextBoxes;
        }

        DiaryTableCell LoadData(object s, out TableCellInfo[,] data, out int r, out int c, out int rr, out int cc)
        {
            var txt = (s as Visual).GetLogicalAncestors().OfType<DiaryTableCell>().First();
            data = GetCellsData();
            r = txt.TableRow;
            c = txt.TableColumn;
            rr = data.GetLength(0);
            cc = data.GetLength(1);
            return txt;
        }
    }


    private DiaryTableCell CreateAndInsertCellTextBox(int row, int column, TableCellInfo item)
    {
        var txt = new DiaryTableCell
        {
            ZIndex = 100,
        };
#if DEBUG
        item.Text ??= $"{row},{column}";
#endif

        txt.TableRow = row;
        txt.TableColumn = column;
        txt.CellData = item;


        txt.GotFocus += (s, e) =>
        {
            ClearCellsSelection();
        };

        txt.Loaded += (s, e) =>
        {
            AddTextBoxMenuEvents(s as DiaryTableCell);
        };
        txt.NotifyEditDataUpdated += (s, e) => NotifyEditDataUpdated?.Invoke(this, EventArgs.Empty);

        //标记已创建
        for (int r = row; r < row + item.RowSpan; r++)
        {
            for (int c = column; c < column + item.ColumnSpan; c++)
            {
                textBoxes[r, c] = txt;
            }
        }
        grd.Children.Add(txt);
        return txt;
    }

    private void FillTextBoxes(TableCellInfo[,] data)
    {
        grd.Children.OfType<DiaryTableCell>().ToList().ForEach(p => grd.Children.Remove(p));
        int row = data.GetLength(0);
        int column = data.GetLength(1);
        textBoxes = new DiaryTableCell[row, column];
        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                //对于已经合并的单元格不需要再添加
                if (textBoxes[r, c] != null)
                {
                    continue;
                }
                var item = data[r, c];
                CreateAndInsertCellTextBox(r, c, item);

            }
        }
    }

    private TableCellInfo[,] GetCellsData()
    {
        bool[,] visited = new bool[textBoxes.GetLength(0), textBoxes.GetLength(1)];
        TableCellInfo[,] items = new TableCellInfo[textBoxes.GetLength(0), textBoxes.GetLength(1)];
        for (int r = 0; r < visited.GetLength(0); r++)
        {
            for (int c = 0; c < visited.GetLength(1); c++)
            {
                if (visited[r, c])
                {
                    continue;
                }
                TableCellInfo item = textBoxes[r, c].CellData;
                items[r, c] = item;
                for (int rr = 0; rr < item.RowSpan; rr++)
                {
                    for (int cc = 0; cc < item.ColumnSpan; cc++)
                    {
                        visited[r + rr, c + cc] = true;
                    }
                }
            }
        }
        return items;
    }
    #endregion

    #region 按钮操作
    private void ChangeSourceButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
        // new Avalonia.Platform.Storage.FilePickerOpenOptions
        // {
        //     FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
        // });

    }

    private void DeleteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((Parent as Control).Parent as StackPanel).Children.Remove(this.GetParentDiaryPart());
    }

    public void LoadData(DocumentPart data)
    {
        throw new NotImplementedException();
    }
    #endregion

}