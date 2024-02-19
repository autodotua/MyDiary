using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using MyDiary.UI.ViewModels;
using System;
using System.Linq.Expressions;
using Avalonia.Layout;
using MyDiary.Core.Models;
using System.Diagnostics;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using System.Linq;
using FzLib;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.Generic;
using Avalonia.Controls.Primitives;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia.LogicalTree;
using Avalonia.Styling;

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class DiaryTable : Grid, IDiaryElement
{

    /**
     * Grid的ColumnDefinitions/RowDefinitions：
     * 10                                                  4           64               4        ……         4           10
     * 用来支持调整大小的多余空间    边框      TextBox       边框    ……       边框     用来支持调整大小的多余空间
     */

    public static readonly AttachedProperty<int> TableColumnProperty = AvaloniaProperty.RegisterAttached<DiaryTable, TextBox, int>("Column");

    public static readonly AttachedProperty<StringDataTableItem> TableDataProperty = AvaloniaProperty.RegisterAttached<DiaryTable, TextBox, StringDataTableItem>("RowSpan");

    public static readonly AttachedProperty<int> TableRowProperty = AvaloniaProperty.RegisterAttached<DiaryTable, TextBox, int>("Row");

    /// <summary>
    /// 边框实际（调整区）粗细
    /// </summary>
    private const double InnerBorderWidth = 2;


    private readonly DiaryTableVM viewModel;

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
    /// 选择后TextBox边界
    /// </summary>
    private int selectionLeftIndex, selectionRightIndex, selectionTopIndex, selectionBottomIndex;

    /// <summary>
    /// 网格对应的TextBox[行号,列号]
    /// </summary>
    private TextBox[,] textBoxes;

    public DiaryTable()
    {
        DataContext = viewModel = new DiaryTableVM();
        InitializeComponent();
    }

    public event EventHandler EditPropertiesUpdated;

    enum TableCellsSelectionMode
    {
        None,
        Selecting,
        Selected
    }

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
                EditPropertiesUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    public static int GetTableColumn(TextBox element) => element.GetValue(TableColumnProperty);

    public static StringDataTableItem GetTableData(TextBox element) => element.GetValue(TableDataProperty);

    public static int GetTableRow(TextBox element) => element.GetValue(TableRowProperty);

    public static void SetTableColumn(TextBox element, int value) => element.SetValue(TableColumnProperty, value);

    public static void SetTableData(TextBox element, StringDataTableItem value) => element.SetValue(TableDataProperty, value);

    public static void SetTableRow(TextBox element, int value) => element.SetValue(TableRowProperty, value);

    public EditProperties GetEditProperties()
    {
        var txts = GetSelectedCells().ToList();
        if (txts.Count == 0)
        {
            return null;
        }
        var data = GetTableData(txts[0]);
        Debug.WriteLine(data);
        var ep = new EditProperties()
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
            Bold = txts.All(txt => txt.FontWeight > FontWeight.Normal),
            Italic = txts.All(txt => txt.FontStyle == FontStyle.Italic),
            FontSize = txts.Select(p => p.FontSize).Min(),
        };

        ep.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(EditProperties.Bold):
                    txts.ForEach(txt => txt.FontWeight = ep.Bold ? FontWeight.Bold : FontWeight.Normal);
                    break;
                case nameof(EditProperties.Italic):
                    txts.ForEach(txt => txt.FontStyle = ep.Italic ? FontStyle.Italic : FontStyle.Normal);
                    break;
                case nameof(EditProperties.FontSize):
                    txts.ForEach(txt => txt.FontSize = ep.FontSize);
                    break;
                case nameof(EditProperties.CellsMerged) when ep.CellsMerged == true:
                    if (CellsSelectionMode != TableCellsSelectionMode.Selected)
                    {
                        throw new Exception($"{nameof(CellsSelectionMode)}状态错误");
                    }
                    var topLeftTextBox = textBoxes[selectionTopIndex, selectionLeftIndex];
                    SetRowSpan(topLeftTextBox, (selectionBottomIndex - selectionTopIndex) * 2 + 1);
                    SetColumnSpan(topLeftTextBox, (selectionRightIndex - selectionLeftIndex) * 2 + 1);
                    GetTableData(topLeftTextBox).ColumnSpan = selectionRightIndex - selectionLeftIndex + 1;
                    GetTableData(topLeftTextBox).RowSpan = selectionBottomIndex - selectionTopIndex + 1;
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
                    var data = GetTableItems();
                    CreateTableStructure(data);
                    ClearCellsSelection();
                    topLeftTextBox.Focus();
                    break;
                case nameof(EditProperties.CellsMerged) when ep.CellsMerged == false:
                    if (CellsSelectionMode != TableCellsSelectionMode.None)
                    {
                        throw new Exception($"{nameof(CellsSelectionMode)}状态错误");
                    }
                    if (txts.Count != 1)
                    {
                        throw new Exception($"查找到的TextBox数量错误");
                    }
                    var txt = txts[0];
                    bool first = true;
                    for (int r = GetTableRow(txt); r < GetTableRow(txt) + GetTableData(txt).RowSpan; r++)
                    {
                        for (int c = GetTableColumn(txt); c < GetTableColumn(txt) + GetTableData(txt).ColumnSpan; c++)
                        {
                            if (first)
                            {
                                first = false;
                                continue;
                            }
                            CreateAndInsetCellTextBox(r, c, new StringDataTableItem());
                        }
                    }
                    SetRowSpan(txt, 1);
                    SetColumnSpan(txt, 1);
                    GetTableData(txt).ColumnSpan = 1;
                    GetTableData(txt).RowSpan = 1;

                    var data2 = GetTableItems();
                    CreateTableStructure(data2);
                    EditPropertiesUpdated?.Invoke(this, EventArgs.Empty);
                    break;
            }
        };
        return ep;
    }

    public void MakeEmptyTable(int row, int column)
    {
        StringDataTableItem[,] data = new StringDataTableItem[row, column];
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                data[i, j] = new StringDataTableItem();
            }
        }
#if DEBUG
        data[1, 1] = new StringDataTableItem(2, 2, null);
        data[4, 0] = new StringDataTableItem(2, 3, null);
        data[2, 3] = new StringDataTableItem(2, 3, null);
#endif
        MakeTable(data);
    }

    public void MakeTable(StringDataTableItem[,] data)
    {
        CreateTableStructure(data);
        //向单元格中填充TextBox
        FillTextBoxes(data);
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

        //若焦点不在TextBox（例如在调整边框），则不执行操作
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

    /// <summary>
    /// Border (GridSplitter/Rectangle) Row/Column 转 Grid Row/Column
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private static int BID2GID(int index)
    {
        return index * 2 + 1;
    }

    private static int GID2BID(int index)
    {
        Debug.Assert(index % 2 == 1);
        return (index - 1) / 2;
    }

    private static int GID2TID(int index)
    {
        Debug.Assert(index % 2 == 0);
        return (index - 2) / 2;
    }

    /// <summary>
    /// TextBox Row/Column 转 Grid Row/Column
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private static int TID2GID(int index)
    {
        return index * 2 + 2;
    }

    private void ChangeSourceButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
        // new Avalonia.Platform.Storage.FilePickerOpenOptions
        // {
        //     FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
        // });

    }
    private int currentZIndex = 100;
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

    private TextBox CreateAndInsetCellTextBox(int row, int column, StringDataTableItem item)
    {
        var txt = new TextBox
        {
            CornerRadius = new CornerRadius(0),
#if DEBUG
            Text = $"{row},{column}",
#else
            Text = item.Text,
#endif
            FontWeight = item.Bold ? FontWeight.Bold : FontWeight.Normal,
            FontStyle = item.Italic ? FontStyle.Italic : FontStyle.Normal,
            ZIndex = 100,
            Theme = App.Current.FindResource("TableTextBoxTheme") as ControlTheme
        };
        txt[!TextBox.BorderBrushProperty] = new DynamicResourceExtension("Foreground0");
        SetTableRow(txt, row);
        SetTableColumn(txt, column);
        SetTableData(txt, item);

        txt.GotFocus += (s, e) =>
        {
            ClearCellsSelection();
            EditPropertiesUpdated?.Invoke(this, EventArgs.Empty);
            (s as Visual).ZIndex = ++currentZIndex;
        };
        txt.LostFocus += (s, e) =>
        {
            EditPropertiesUpdated?.Invoke(this, EventArgs.Empty);
        };

        textBoxes[row, column] = txt;
        SetRow(txt, TID2GID(row));
        SetColumn(txt, TID2GID(column));

        //合并单元格
        if (item.RowSpan * item.ColumnSpan > 1)
        {
            //TextBox跨行列
            SetRowSpan(txt, item.RowSpan * 2 - 1);
            SetColumnSpan(txt, item.ColumnSpan * 2 - 1);

            //标记已创建
            for (int a = row; a < row + item.RowSpan; a++)
            {
                for (int b = column; b < column + item.ColumnSpan; b++)
                {
                    textBoxes[a, b] = txt;
                }
            }

        }
        grd.Children.Add(txt);
        return txt;
    }

    private void CreateTableStructure(StringDataTableItem[,] data)
    {
        int row = data.GetLength(0);
        int column = data.GetLength(1);
        Panel[] horizontalLines = new Panel[row + 1];
        Panel[] verticalLines = new Panel[column + 1];
        //创建网格结构，包括绘制线
        MakeBorders(row, column);
        //处理合并单元格
        //MergeCells(data, row, column, horizontalLines, verticalLines);
    }

    private void DeleteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((Parent as Control).Parent as StackPanel).Children.Remove(this.GetParentDiaryPart());
    }

    private void FillTextBoxes(StringDataTableItem[,] data)
    {
        int row = data.GetLength(0);
        int column = data.GetLength(1);
        textBoxes = new TextBox[row, column];
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
                CreateAndInsetCellTextBox(r, c, item);

            }
        }
    }

    private TextBox GetFocusedCell()
    {
        var element = TopLevel.GetTopLevel(this).FocusManager.GetFocusedElement();
        if (element is TextBox txt)
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
    private IEnumerable<TextBox> GetSelectedCells(bool returnFocusedWhenNotSelecting = true)
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
            HashSet<TextBox> set = new HashSet<TextBox>();
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

    private StringDataTableItem GetStringDataTableItem(TextBox textBox)
    {
        StringDataTableItem item = GetTableData(textBox);
        item.Text = textBox.Text;
        item.Bold = textBox.FontWeight > FontWeight.Bold;
        item.Italic = textBox.FontStyle == FontStyle.Italic;
        return item;
    }

    private StringDataTableItem[,] GetTableItems()
    {
        bool[,] visited = new bool[textBoxes.GetLength(0), textBoxes.GetLength(1)];
        StringDataTableItem[,] items = new StringDataTableItem[textBoxes.GetLength(0), textBoxes.GetLength(1)];
        for (int r = 0; r < visited.GetLength(0); r++)
        {
            for (int c = 0; c < visited.GetLength(1); c++)
            {
                if (visited[r, c])
                {
                    continue;
                }
                StringDataTableItem item = GetStringDataTableItem(textBoxes[r, c]);
                items[r, c] = item;
                for (int rr = 0; rr < item.RowSpan; rr++)
                {
                    for (int cc = 0; cc < item.ColumnSpan; cc++)
                    {
                        visited[r, c] = true;
                    }
                }
            }
        }
        return items;
    }

    private void MakeBorders(int row, int column)
    {
        grd.ColumnDefinitions.Clear();
        grd.RowDefinitions.Clear();
        foreach (var child in grd.Children.OfType<Panel>().Cast<Control>().Concat(grd.Children.OfType<GridSplitter>()).ToList())
        {
            grd.Children.Remove(child);
        }

        grd.ColumnDefinitions.Add(new ColumnDefinition(10, GridUnitType.Pixel));
        for (int c = 0; c <= column; c++)
        {
            if (c > 0)
            {
                grd.ColumnDefinitions.Add(new ColumnDefinition(64, GridUnitType.Pixel));
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
                    selectionLeftIndex = Math.Min(selectionLeftIndex, GetTableColumn(txt));
                    selectionRightIndex = Math.Max(selectionRightIndex, GetTableColumn(txt) + GetTableData(txt).ColumnSpan - 1);
                    selectionTopIndex = Math.Min(selectionTopIndex, GetTableRow(txt));
                    selectionBottomIndex = Math.Max(selectionBottomIndex, GetTableRow(txt) + GetTableData(txt).RowSpan - 1);
                }
            }
            Debug.WriteLine($"{selectionLeftIndex},{selectionRightIndex},{selectionTopIndex},{selectionBottomIndex}");

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
}