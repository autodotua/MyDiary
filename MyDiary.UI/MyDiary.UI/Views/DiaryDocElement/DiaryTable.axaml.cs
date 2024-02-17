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

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class DiaryTable : Grid, IDiaryElement
{

    /**
     * Grid��ColumnDefinitions/RowDefinitions��
     * 10                                                  4           64               4        ����         4           10
     * ����֧�ֵ�����С�Ķ���ռ�    �߿�      TextBox       �߿�    ����       �߿�     ����֧�ֵ�����С�Ķ���ռ�
     */

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
    private Point? mouseDownPosition;

    /// <summary>
    /// ��ɫ��ѡ��Ԫ��Ŀ�
    /// </summary>
    private Border selectionBorder = null;

    /// <summary>
    /// �����Ӧ��TextBox[�к�,�к�]
    /// </summary>
    private TextBox[,] textBoxes;

    public DiaryTable()
    {
        InitializeComponent();
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
        grd.ColumnDefinitions.Clear();
        grd.RowDefinitions.Clear();
        grd.ColumnDefinitions.Add(new ColumnDefinition(10, GridUnitType.Pixel));
        int row = data.GetLength(0);
        int column = data.GetLength(1);
        Panel[] horizontalLines = new Panel[row + 1];
        Panel[] verticalLines = new Panel[column + 1];
        //��������ṹ������������
        CreateTableStructure(row, column, horizontalLines, verticalLines);
        //����ϲ���Ԫ��
        MergeCells(data, row, column, horizontalLines, verticalLines);
        //��Ԫ�������TextBox
        FillTextBoxes(data, row, column);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        ClearSelection();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var point = e.GetCurrentPoint(pnlSelection);
        if (point.Properties.IsLeftButtonPressed)
        {
            if (mouseDownPosition == null)
            {
                //�����޷�ʹ��OnPointerPressed��ȡλ�ã�������Moved�л�ȡ
                mouseDownPosition = point.Position;
            }
            else
            {
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

                //����ע�Ͳ���Ϊ����ʵ�����λ�û���
                //bool right = point.Position.X - mouseDownPosition.Value.X > 0;
                //bool down = point.Position.Y - mouseDownPosition.Value.Y > 0;
                //var marginLeft = right ? mouseDownPosition.Value.X : point.Position.X;
                //var marginTop = down ? mouseDownPosition.Value.Y : point.Position.Y;
                //var marginRight = pnlSelection.Bounds.Width - (right ? point.Position.X : mouseDownPosition.Value.X);
                //var marginBottom = pnlSelection.Bounds.Height - (down ? point.Position.Y : mouseDownPosition.Value.Y);
                //if (pnlSelection.Bounds.Width- marginRight - marginBottom > 8 && pnlSelection.Bounds.Height - marginBottom - marginTop > 8)
                //{
                //    selectionBorder.Margin = new Thickness(marginLeft, marginTop, marginRight, marginBottom);
                //}

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
                var left = Math.Min(mouseDownPosition.Value.X, point.Position.X);
                var right = Math.Max(mouseDownPosition.Value.X, point.Position.X);
                var top = Math.Min(mouseDownPosition.Value.Y, point.Position.Y);
                var bottom = Math.Max(mouseDownPosition.Value.Y, point.Position.Y);
                if (right - left < 8 || bottom - top < 8)
                {
                    return;
                }

                //���������ʵ�ʿ�ѡ��Χ�ཻ�ĵ�Ԫ��
                int leftIndex = -1, rightIndex = -1, topIndex = -1, bottomIndex = -1;
                for (int i = 2; i <= gridLefts.Count - 2; i += 2)
                {
                    if (left > gridLefts[i - 2] && left < gridLefts[i+1])
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
                //for (int i = leftIndex; i <= rightIndex; i++)
                //{
                //    for (int j = topIndex; j <= bottomIndex; j++)
                //    {
                //        textBoxes[j, i].Background = Brushes.Red;
                //    }
                //}
                if (leftIndex < 0 || rightIndex < 0 || topIndex < 0 || bottomIndex < 0)
                {
                    selectionBorder.IsVisible = false;
                    return;
                }

                //��ʾ��Ԫ���
                selectionBorder.IsVisible = true;
                selectionBorder.Margin = new Thickness(
                    grd.Bounds.Left + textBoxes[topIndex, leftIndex].Bounds.Left-InnerBorderWidth,
                    grd.Bounds.Top + textBoxes[topIndex, leftIndex].Bounds.Top - InnerBorderWidth,
                    Bounds.Width - grd.Bounds.Left - textBoxes[bottomIndex, rightIndex].Bounds.Right - InnerBorderWidth,
                    Bounds.Height - grd.Bounds.Top - textBoxes[bottomIndex, rightIndex].Bounds.Bottom - InnerBorderWidth
                    );
                Debug.WriteLine(selectionBorder.Margin);
            }
        }
        else
        {
            ClearSelection();
        }
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

    private void ClearSelection()
    {
        if (mouseDownPosition.HasValue)
        {
            mouseDownPosition = null;
        }
        if (selectionBorder != null)
        {
            pnlSelection.Children.Clear();
            selectionBorder = null;
        }
    }
    private void CreateTableStructure(int row, int column, Panel[] horizontalLines, Panel[] verticalLines)
    {
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

    private void DeleteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((Parent as Control).Parent as StackPanel).Children.Remove(this.GetParentDiaryPart());
    }
    private void FillTextBoxes(StringDataTableItem[,] data, int row, int column)
    {
        bool[,] cellCreated = new bool[row, column];
        textBoxes = new TextBox[row, column];
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
                var txt = new DiaryTextBox()
                {
                    CornerRadius = new CornerRadius(0),
                    Text = item.Text,
                };
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
}