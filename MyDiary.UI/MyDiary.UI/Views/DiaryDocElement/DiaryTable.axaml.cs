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

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class DiaryTable : Grid, IDiaryElement
{

    /**
     * Grid的ColumnDefinitions/RowDefinitions：
     * 10                                                  4           64               4        ……         4           10
     * 用来支持调整大小的多余空间    边框      TextBox       边框    ……       边框     用来支持调整大小的多余空间
     */

    public static readonly AttachedProperty<int> TableColumnProperty = AvaloniaProperty.RegisterAttached<DiaryTable, DiaryTextBox, int>("Column");

    public static readonly AttachedProperty<StringDataTableItem> TableDataProperty = AvaloniaProperty.RegisterAttached<DiaryTable, DiaryTextBox, StringDataTableItem>("RowSpan");

    public static readonly AttachedProperty<int> TableRowProperty = AvaloniaProperty.RegisterAttached<DiaryTable, DiaryTextBox, int>("Row");

    /// <summary>
    /// 边框实际（调整区）粗细
    /// </summary>
    private const double InnerBorderWidth = 4;

    /// <summary>
    /// 边框线条粗细
    /// </summary>
    private const double InnerLineWidth = 1;

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
    private DiaryTextBox[,] textBoxes;

    public DiaryTable()
    {
        InitializeComponent();
    }
    public static int GetTableColumn(DiaryTextBox element) => element.GetValue(TableColumnProperty);

    public static StringDataTableItem GetTableData(DiaryTextBox element) => element.GetValue(TableDataProperty);

    public static int GetTableRow(DiaryTextBox element) => element.GetValue(TableRowProperty);

    public static void SetTableColumn(DiaryTextBox element, int value) => element.SetValue(TableColumnProperty, value);

    public static void SetTableData(DiaryTextBox element, StringDataTableItem value) => element.SetValue(TableDataProperty, value);

    public static void SetTableRow(DiaryTextBox element, int value) => element.SetValue(TableRowProperty, value);

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
        if (TopLevel.GetTopLevel(this).FocusManager.GetFocusedElement() is not TextBox)
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

    private static DiaryTextBox StringDataTableItem2DiaryTextBox(int row, int column, StringDataTableItem item)
    {
        var txt = new DiaryTextBox()
        {
            CornerRadius = new CornerRadius(0),
            Text = item.Text,
            FontWeight = item.Bold ? FontWeight.Bold : FontWeight.Normal,
            FontStyle = item.Italic ? FontStyle.Italic : FontStyle.Normal
        };
        SetTableRow(txt, row);
        SetTableColumn(txt, column);
        SetTableData(txt, item);
        return txt;
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

    private void CreateTableStructure(StringDataTableItem[,] data)
    {
        int row = data.GetLength(0);
        int column = data.GetLength(1);
        Panel[] horizontalLines = new Panel[row + 1];
        Panel[] verticalLines = new Panel[column + 1];
        //创建网格结构，包括绘制线
        MakeBorders(row, column, horizontalLines, verticalLines);
        //处理合并单元格
        MergeCells(data, row, column, horizontalLines, verticalLines);
    }

    private void DeleteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((Parent as Control).Parent as StackPanel).Children.Remove(this.GetParentDiaryPart());
    }

    private StringDataTableItem DiaryTextBox2StringDataTableItem(DiaryTextBox textBox)
    {
        StringDataTableItem item = GetTableData(textBox);
        item.Text = textBox.Text;
        item.Bold = textBox.FontWeight > FontWeight.Bold;
        item.Italic = textBox.FontStyle == FontStyle.Italic;
        return item;
    }

    private void FillTextBoxes(StringDataTableItem[,] data)
    {
        int row = data.GetLength(0);
        int column = data.GetLength(1);
        bool[,] cellCreated = new bool[row, column];
        textBoxes = new DiaryTextBox[row, column];
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                //对于已经合并的单元格不需要再添加
                if (cellCreated[i, j])
                {
                    continue;
                }
                cellCreated[i, j] = true;
                var item = data[i, j];
                DiaryTextBox txt = StringDataTableItem2DiaryTextBox(i, j, item);

                textBoxes[i, j] = txt;
                SetRow(txt, TID2GID(i));
                SetColumn(txt, TID2GID(j));

                //合并单元格
                if (item.RowSpan * item.ColumnSpan > 1)
                {
                    //TextBox跨行列
                    SetRowSpan(txt, item.RowSpan * 2 - 1);
                    SetColumnSpan(txt, item.ColumnSpan * 2 - 1);

                    //标记已创建
                    for (int a = i; a < i + item.RowSpan; a++)
                    {
                        for (int b = j; b < j + item.ColumnSpan; b++)
                        {
                            cellCreated[a, b] = true;
                            textBoxes[a, b] = txt;
                        }
                    }

                }
                grd.Children.Add(txt);
            }
        }
    }
    private void Flyout_Closed(object sender, EventArgs e)
    {
        (sender as Flyout).Closed -= Flyout_Closed;
        pointerDownPosition = null;
        selectionBorder = null;
        pnlSelection.Children.Clear();
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
                StringDataTableItem item = DiaryTextBox2StringDataTableItem(textBoxes[r, c]);
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
    private void MakeBorders(int row, int column, Panel[] horizontalLines, Panel[] verticalLines)
    {
        grd.ColumnDefinitions.Clear();
        grd.RowDefinitions.Clear();
        foreach (var child in grd.Children.OfType<Panel>().Cast<Control>().Concat(grd.Children.OfType<GridSplitter>()).ToList())
        {
            grd.Children.Remove(child);
        }

        grd.ColumnDefinitions.Add(new ColumnDefinition(10, GridUnitType.Pixel));
        for (int j = 0; j <= column; j++)
        {
            if (j > 0)
            {
                grd.ColumnDefinitions.Add(new ColumnDefinition(64, GridUnitType.Pixel));
            }

            grd.ColumnDefinitions.Add(new ColumnDefinition(4, GridUnitType.Pixel));

            var splitter = new GridSplitter() { Background = Brushes.Transparent };
            SetRowSpan(splitter, int.MaxValue);
            SetColumn(splitter, BID2GID(j));
            //if (j == 0 || j == column)
            //{
            //    splitter[!BackgroundProperty] = new DynamicResourceExtension("Foreground0");
            //}
            grd.Children.Add(splitter);

            var rect = new Panel()
            {
                Width = 1,
                HorizontalAlignment = j == 0 ? HorizontalAlignment.Left : (j == column ? HorizontalAlignment.Right : HorizontalAlignment.Center),
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0, (InnerBorderWidth - InnerLineWidth) / 2, 0, (InnerBorderWidth - InnerLineWidth) / 2)
            };
            rect[!BackgroundProperty] = new DynamicResourceExtension("Foreground0");
            SetRow(rect, 1);
            SetRowSpan(rect, BID2GID(row));
            SetColumn(rect, BID2GID(j));
            grd.Children.Add(rect);
            verticalLines[j] = rect;
        }
        grd.ColumnDefinitions.Add(new ColumnDefinition(10, GridUnitType.Pixel));


        grd.RowDefinitions.Add(new RowDefinition(10, GridUnitType.Pixel));
        for (int i = 0; i <= row; i++)
        {
            if (i > 0)
            {
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
            }
            grd.RowDefinitions.Add(new RowDefinition(4, GridUnitType.Pixel));

            var splitter = new GridSplitter() { Background = Brushes.Transparent };
            SetRow(splitter, BID2GID(i));
            grd.Children.Add(splitter);

            var rect = new Panel()
            {
                Height = 1,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = i == 0 ? VerticalAlignment.Top : (i == row ? VerticalAlignment.Bottom : VerticalAlignment.Center),
                Margin = new Thickness((InnerBorderWidth - InnerLineWidth) / 2, 0, (InnerBorderWidth - InnerLineWidth) / 2, 0)
            };
            SetRow(rect, BID2GID(i));
            SetColumn(rect, 1);
            SetColumnSpan(rect, BID2GID(column));
            rect[!BackgroundProperty] = new DynamicResourceExtension("Foreground0");
            grd.Children.Add(rect);
            horizontalLines[i] = rect;
        }
        grd.RowDefinitions.Add(new RowDefinition(10, GridUnitType.Pixel));
    }

    private void MergeCellButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
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
    }

    private bool[,] MergeCells(StringDataTableItem[,] data, int row, int column, Panel[] horizontalLines, Panel[] verticalLines)
    {
        bool[,] cellProcessed = new bool[row, column];
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                //对于已经合并的单元格不需要再添加
                if (cellProcessed[i, j])
                {
                    continue;
                }
                cellProcessed[i, j] = true;
                var item = data[i, j];


                //合并单元格
                if (item.RowSpan * item.ColumnSpan > 1)
                {
                    //标记已创建
                    for (int a = i; a < i + item.RowSpan; a++)
                    {
                        for (int b = j; b < j + item.ColumnSpan; b++)
                        {
                            cellProcessed[a, b] = true;
                        }
                    }

                    //处理边框
                    //水平
                    for (int k = i + 1; k <= i + item.RowSpan - 1; k++)
                    {
                        //将原来的线截短到TextBox左侧。如果该单元格在最左侧，则删除左侧线。
                        if (j > 0)
                        {
                            SetColumnSpan(horizontalLines[k], TID2GID(j) - GetColumn(horizontalLines[k]));
                        }
                        else
                        {
                            grd.Children.Remove(horizontalLines[k]);
                        }
                        //TextBox右侧新线的长度
                        int newLineLong = TID2GID(column) - TID2GID(j + item.ColumnSpan) + 1;
                        //如果长度<1，说明已经到了最右端，那么右侧不再需要画线。同时为了检测错误，设置horizontalLines[k]为空。
                        if (newLineLong <= 1)
                        {
                            horizontalLines[k] = null;
                            continue;
                        }
                        //右侧新线
                        var rect = new Panel()
                        {
                            Height = 1,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness((InnerBorderWidth - InnerLineWidth) / 2, 0, (InnerBorderWidth - InnerLineWidth) / 2, 0)
                        };
                        //设置行列
                        SetRow(rect, BID2GID(k));
                        SetColumn(rect, BID2GID(j + item.ColumnSpan));
                        SetColumnSpan(rect, newLineLong);
                        rect[!BackgroundProperty] = new DynamicResourceExtension("Foreground0");
                        grd.Children.Add(rect);
                        //将horizontalLines[k]的值设置为最新的线。由于遍历的时候是从小到大，因此已经被截短的线可以保证不会再用到。而新增的线可能被再次截短。
                        horizontalLines[k] = rect;
                    }
                    //垂直
                    for (int k = j + 1; k <= j + item.ColumnSpan - 1; k++)
                    {
                        if (i > 0)
                        {
                            SetRowSpan(verticalLines[k], TID2GID(i) - GetRow(verticalLines[k]));
                        }
                        else
                        {
                            grd.Children.Remove(verticalLines[k]);
                        }

                        int newLineLong = TID2GID(row) - TID2GID(i + item.RowSpan) + 1;
                        if (newLineLong <= 1)
                        {
                            verticalLines[k] = null;
                            continue;
                        }
                        var rect = new Panel()
                        {
                            Width = 1,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            Margin = new Thickness(0, (InnerBorderWidth - InnerLineWidth) / 2, 0, (InnerBorderWidth - InnerLineWidth) / 2)
                        };
                        SetColumn(rect, BID2GID(k));
                        SetRow(rect, BID2GID(i + item.RowSpan));
                        SetRowSpan(rect, newLineLong);
                        rect[!BackgroundProperty] = new DynamicResourceExtension("Foreground0");
                        grd.Children.Add(rect);
                        verticalLines[k] = rect;
                    }
                }
            }
        }

        return cellProcessed;
    }

    private void SelectCells(PointerPoint point)
    {
        //创建框选显示框
        if (selectionBorder == null)
        {
            selectionBorder = new Border()
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(4),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsVisible = false
            };
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


        if (leftIndex < 0 || rightIndex < 0 || topIndex < 0 || bottomIndex < 0 //没框到
            || topIndex == bottomIndex && leftIndex == rightIndex) //只框了一个，此时认为在选择TextBox内的文字
        {
            selectionBorder.IsVisible = false;
            return;
        }

        //确定最终范围。上述的范围可能会截断一些合并的单元格，需要确保框选范围内都是完整的单元格。
        selectionLeftIndex = leftIndex;
        selectionRightIndex = rightIndex;
        selectionTopIndex = topIndex;
        selectionBottomIndex = bottomIndex;
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
        if (pointerDownPosition.HasValue)
        {
            var flyout = FlyoutBase.GetAttachedFlyout(pnlSelection);
            if (flyout.IsOpen)
            {
                return;
            }
            flyout.Closed += Flyout_Closed;
            FlyoutBase.ShowAttachedFlyout(pnlSelection);
        }
    }
}