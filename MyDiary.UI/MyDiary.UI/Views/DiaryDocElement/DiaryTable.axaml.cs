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
     * Grid��ColumnDefinitions/RowDefinitions��
     * 10                                                  4           64               4        ����         4           10
     * ����֧�ֵ�����С�Ķ���ռ�    �߿�      TextBox       �߿�    ����       �߿�     ����֧�ֵ�����С�Ķ���ռ�
     */

    public static readonly AttachedProperty<int> TableColumnProperty = AvaloniaProperty.RegisterAttached<DiaryTable, DiaryTextBox, int>("Column");

    public static readonly AttachedProperty<StringDataTableItem> TableDataProperty = AvaloniaProperty.RegisterAttached<DiaryTable, DiaryTextBox, StringDataTableItem>("RowSpan");

    public static readonly AttachedProperty<int> TableRowProperty = AvaloniaProperty.RegisterAttached<DiaryTable, DiaryTextBox, int>("Row");

    /// <summary>
    /// �߿�ʵ�ʣ�����������ϸ
    /// </summary>
    private const double InnerBorderWidth = 4;

    /// <summary>
    /// �߿�������ϸ
    /// </summary>
    private const double InnerLineWidth = 1;

    /// <summary>
    /// ����״ΰ���ʱ��λ��
    /// </summary>
    private Point? pointerDownPosition;

    /// <summary>
    /// ��ɫ��ѡ��Ԫ��Ŀ�
    /// </summary>
    private Border selectionBorder = null;

    /// <summary>
    /// ѡ���TextBox�߽�
    /// </summary>
    private int selectionLeftIndex, selectionRightIndex, selectionTopIndex, selectionBottomIndex;

    /// <summary>
    /// �����Ӧ��TextBox[�к�,�к�]
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
        //��Ԫ�������TextBox
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

        //���û�а��£���ִ�в�������֮ǰ��ѡ��״̬�������
        if (!point.Properties.IsLeftButtonPressed)
        {
            StopSelectingCells();
            return;
        }

        //�����㲻��TextBox�������ڵ����߿򣩣���ִ�в���
        if (TopLevel.GetTopLevel(this).FocusManager.GetFocusedElement() is not TextBox)
        {
            return;
        }
        //�״ΰ��£���¼
        if (pointerDownPosition == null)
        {
            //�����޷�ʹ��OnPointerPressed��ȡλ�ã�������Moved�л�ȡ
            pointerDownPosition = point.Position;
            return;
        }

        //��ʼ��ѡ
        SelectCells(point);
    }

    /// <summary>
    /// Border (GridSplitter/Rectangle) Row/Column ת Grid Row/Column
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
    /// TextBox Row/Column ת Grid Row/Column
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
        //��������ṹ������������
        MakeBorders(row, column, horizontalLines, verticalLines);
        //����ϲ���Ԫ��
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
                //�����Ѿ��ϲ��ĵ�Ԫ����Ҫ�����
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

                //�ϲ���Ԫ��
                if (item.RowSpan * item.ColumnSpan > 1)
                {
                    //TextBox������
                    SetRowSpan(txt, item.RowSpan * 2 - 1);
                    SetColumnSpan(txt, item.ColumnSpan * 2 - 1);

                    //����Ѵ���
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
                //�����Ѿ��ϲ��ĵ�Ԫ����Ҫ�����
                if (cellProcessed[i, j])
                {
                    continue;
                }
                cellProcessed[i, j] = true;
                var item = data[i, j];


                //�ϲ���Ԫ��
                if (item.RowSpan * item.ColumnSpan > 1)
                {
                    //����Ѵ���
                    for (int a = i; a < i + item.RowSpan; a++)
                    {
                        for (int b = j; b < j + item.ColumnSpan; b++)
                        {
                            cellProcessed[a, b] = true;
                        }
                    }

                    //����߿�
                    //ˮƽ
                    for (int k = i + 1; k <= i + item.RowSpan - 1; k++)
                    {
                        //��ԭ�����߽ض̵�TextBox��ࡣ����õ�Ԫ��������࣬��ɾ������ߡ�
                        if (j > 0)
                        {
                            SetColumnSpan(horizontalLines[k], TID2GID(j) - GetColumn(horizontalLines[k]));
                        }
                        else
                        {
                            grd.Children.Remove(horizontalLines[k]);
                        }
                        //TextBox�Ҳ����ߵĳ���
                        int newLineLong = TID2GID(column) - TID2GID(j + item.ColumnSpan) + 1;
                        //�������<1��˵���Ѿ��������Ҷˣ���ô�Ҳ಻����Ҫ���ߡ�ͬʱΪ�˼���������horizontalLines[k]Ϊ�ա�
                        if (newLineLong <= 1)
                        {
                            horizontalLines[k] = null;
                            continue;
                        }
                        //�Ҳ�����
                        var rect = new Panel()
                        {
                            Height = 1,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness((InnerBorderWidth - InnerLineWidth) / 2, 0, (InnerBorderWidth - InnerLineWidth) / 2, 0)
                        };
                        //��������
                        SetRow(rect, BID2GID(k));
                        SetColumn(rect, BID2GID(j + item.ColumnSpan));
                        SetColumnSpan(rect, newLineLong);
                        rect[!BackgroundProperty] = new DynamicResourceExtension("Foreground0");
                        grd.Children.Add(rect);
                        //��horizontalLines[k]��ֵ����Ϊ���µ��ߡ����ڱ�����ʱ���Ǵ�С��������Ѿ����ض̵��߿��Ա�֤�������õ������������߿��ܱ��ٴνض̡�
                        horizontalLines[k] = rect;
                    }
                    //��ֱ
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
        //������ѡ��ʾ��
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

        //����Grid��ÿ�����о��붥���������ۼƾ���
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

        //��������ѡʵ�ʷ�Χ�ı߽�
        var left = Math.Min(pointerDownPosition.Value.X, point.Position.X);
        var right = Math.Max(pointerDownPosition.Value.X, point.Position.X);
        var top = Math.Min(pointerDownPosition.Value.Y, point.Position.Y);
        var bottom = Math.Max(pointerDownPosition.Value.Y, point.Position.Y);

        //���������ʵ�ʿ�ѡ��Χ�ཻ�ĵ�Ԫ��
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


        if (leftIndex < 0 || rightIndex < 0 || topIndex < 0 || bottomIndex < 0 //û��
            || topIndex == bottomIndex && leftIndex == rightIndex) //ֻ����һ������ʱ��Ϊ��ѡ��TextBox�ڵ�����
        {
            selectionBorder.IsVisible = false;
            return;
        }

        //ȷ�����շ�Χ�������ķ�Χ���ܻ�ض�һЩ�ϲ��ĵ�Ԫ����Ҫȷ����ѡ��Χ�ڶ��������ĵ�Ԫ��
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

        //���ÿ��λ��
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