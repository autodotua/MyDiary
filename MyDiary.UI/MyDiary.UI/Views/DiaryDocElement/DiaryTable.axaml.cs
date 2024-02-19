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
using Avalonia.Data;
using FzLib.Avalonia.Dialogs;

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
    private const double InnerBorderWidth = 2;

    private readonly DiaryTableVM viewModel;

    public DiaryTable()
    {
        DataContext = viewModel = new DiaryTableVM();
        InitializeComponent();
    }

    #region ��EditBar�����ݽ���

    public event EventHandler EditPropertiesUpdated;

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
                        throw new Exception($"{nameof(CellsSelectionMode)}״̬����");
                    }
                    var topLeftTextBox = textBoxes[selectionTopIndex, selectionLeftIndex];
                    //SetRowSpan(topLeftTextBox, (selectionBottomIndex - selectionTopIndex) * 2 + 1);
                    //SetColumnSpan(topLeftTextBox, (selectionRightIndex - selectionLeftIndex) * 2 + 1);
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
                        throw new Exception($"{nameof(CellsSelectionMode)}״̬����");
                    }
                    if (txts.Count != 1)
                    {
                        throw new Exception($"���ҵ���TextBox��������");
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
    #endregion

    #region ��������


    public static readonly AttachedProperty<int> TableColumnProperty = AvaloniaProperty.RegisterAttached<DiaryTable, TextBox, int>("Column");

    public static readonly AttachedProperty<StringDataTableItem> TableDataProperty = AvaloniaProperty.RegisterAttached<DiaryTable, TextBox, StringDataTableItem>("RowSpan");

    public static readonly AttachedProperty<int> TableRowProperty = AvaloniaProperty.RegisterAttached<DiaryTable, TextBox, int>("Row");

    public static int GetTableColumn(TextBox element) => element.GetValue(TableColumnProperty);

    public static StringDataTableItem GetTableData(TextBox element) => element.GetValue(TableDataProperty);

    public static int GetTableRow(TextBox element) => element.GetValue(TableRowProperty);

    public static void SetTableColumn(TextBox element, int value) => element.SetValue(TableColumnProperty, value);

    public static void SetTableData(TextBox element, StringDataTableItem value) => element.SetValue(TableDataProperty, value);

    public static void SetTableRow(TextBox element, int value) => element.SetValue(TableRowProperty, value);

    #endregion

    #region �������
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



    private void CreateTableStructure(StringDataTableItem[,] data)
    {
        int row = data.GetLength(0);
        int column = data.GetLength(1);

        //����߿�ṹ
        grd.ColumnDefinitions.Clear();
        grd.RowDefinitions.Clear();
        foreach (var child in grd.Children.OfType<GridSplitter>().ToList())
        {
            grd.Children.Remove(child);
        }

        //����
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

        //����
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

    #region ��ѡ

    enum TableCellsSelectionMode
    {
        None,
        Selecting,
        Selected
    }

    private TableCellsSelectionMode cellsSelectionMode = TableCellsSelectionMode.None;

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
        if (TopLevel.GetTopLevel(this).FocusManager.GetFocusedElement() is GridSplitter)
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
    /// ��ȡ��ѡ��ķ�Χ�ڵ�
    /// </summary>
    /// <param name="returnFocusedWhenNotSelecting">������������δѡ��״̬�᷵�ؽ��������ı���</param>
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

    private void SelectCells(PointerPoint point)
    {
        CellsSelectionMode = TableCellsSelectionMode.Selecting;

        //������ѡ��ʾ��
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
        //û��
        if (leftIndex < 0 || rightIndex < 0 || topIndex < 0 || bottomIndex < 0)
        {
            selectionBorder.IsVisible = false;
            return;
        }

        //ȷ�����շ�Χ�������ķ�Χ���ܻ�ض�һЩ�ϲ��ĵ�Ԫ����Ҫȷ����ѡ��Χ�ڶ��������ĵ�Ԫ��
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

            //�ڷ�Χ���¡���֤�˲��ضϺϲ��ĵ�Ԫ���
            //������ķ�Χ�ڿ����ְ������µı��ضϵĺϲ���Ԫ��
            //�����Ҫ�ٴν����жϸ��¡�
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

        //���ֻ��һ������ô��Ϊ�����ڲ�ѡ�����֣�������
        if (!GetSelectedCells(false).Skip(1).Any())
        {
            selectionBorder.IsVisible = false;
            return;
        }

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

    #region ����ת��

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

    #endregion

    #region �ı�����

    /// <summary>
    /// �����Ӧ��TextBox[�к�,�к�]
    /// </summary>
    private TextBox[,] textBoxes;

    private void AddTextBoxMenuEvents(TextBox txt)
    {
        var menu = txt.ContextFlyout as MenuFlyout;

        Debug.Assert(menu != null);
        var items = menu.Items.Cast<MenuItem>().Skip(3).Where(p => !p.Header.Equals("-")).ToList();

        //���Ϸ�����
        items[0].Click += async (s, e) =>
        {
            //��Ҫ���������Ȼ���ظ�����Click�¼�
            e.Handled = true;
            
            //���ֱ���������txt����Զ����[0,0]�Ǹ�����ȷ����ʲôԭ�򣬿����Ǳհ��������
            LoadData(s, out StringDataTableItem[,] data, out int r, out _, out int rr, out int cc);
           
            //���ϲ���Ԫ�������Ͽ���ֱ�ӽ��ϲ���Ԫ������ģ�������������ô�����ˣ�����������ϲ���Ԫ��ֱ�ӱ���
            for (int i = 0; i < cc;)
            {
                //��ǰ�����У��ϲ���Ԫ��ֻ�����Ͻ���ֵ����������null��
                //�������null����ζ����Ҫ��ֺϲ���Ԫ��
                if (data[r, i] == null)
                {
                    await this.ShowErrorDialogAsync("�޷�����", "��Ҫ������д��ںϲ���Ԫ��");
                    return;
                }
                //��Ϊ��ǰ�ǲ����У���˺���ϲ�����ע��
                //���ÿ�����󣬸��ݿ��е�����������Ծ
                i += data[r, i].ColumnSpan;
            }
            var newData = new StringDataTableItem[rr + 1, cc];
            //����������
            for (int i = 0; i < cc; i++)
            {
                newData[r, i] = new StringDataTableItem()
#if DEBUG
                { Text = "add" }
#endif
                ;
            }
            //�����ϰ벿��
            CopySubArray(data, 0, 0, r, cc, newData, 0, 0);
            //�����°벿��
            CopySubArray(data, r, 0, rr - r, cc, newData, r + 1, 0);
            //�ؽ�
            MakeTable(newData);
        };

        void LoadData(object s, out StringDataTableItem[,] data, out int r, out int c, out int rr, out int cc)
        {
            var txt = (s as Visual).GetLogicalAncestors().OfType<TextBox>().First();
            data = GetTableItems();
            r = GetTableRow(txt);
            c = GetTableColumn(txt);
            rr = data.GetLength(0);
            cc = data.GetLength(1);
        }
        static void CopySubArray<T>(T[,] source, int sourceStartRow, int sourceStartCol, int numRows, int numCols, T[,] target, int targetStartRow, int targetStartCol)
        {
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    target[targetStartRow + i, targetStartCol + j] = source[sourceStartRow + i, sourceStartCol + j];
                }
            }
        }
    }


    private TextBox CreateAndInsetCellTextBox(int row, int column, StringDataTableItem item)
    {
        var txt = new TextBox
        {
            CornerRadius = new CornerRadius(0),
            ZIndex = 100,
            Theme = App.Current.FindResource("TableTextBoxTheme") as ControlTheme
        };
#if DEBUG
        item.Text ??= $"{row},{column}";
#endif
        txt.Bind(TextBox.TextProperty, new Binding
        {
            Source = item,
            Path = nameof(item.Text)
        });
        txt.Bind(TextBox.FontSizeProperty, new Binding
        {
            Source = item,
            Path = nameof(item.FontSize)
        });
        txt.Bind(TextBox.FontWeightProperty, new Binding
        {
            Source = item,
            Path = nameof(item.FontWeight)
        });
        txt.Bind(TextBox.FontStyleProperty, new Binding
        {
            Source = item,
            Path = nameof(item.FontStyle)
        });
        txt.Bind(RowSpanProperty, new Binding
        {
            Source = item,
            Path = nameof(item.VisualRowSpan)
        });
        txt.Bind(ColumnSpanProperty, new Binding
        {
            Source = item,
            Path = nameof(item.VisualColumnSpan)
        });
        txt[!TextBox.BorderBrushProperty] = new DynamicResourceExtension("Foreground0");
        SetTableRow(txt, row);
        SetTableColumn(txt, column);
        SetTableData(txt, item);


        txt.GotFocus += (s, e) =>
        {
            ClearCellsSelection();
            EditPropertiesUpdated?.Invoke(this, EventArgs.Empty);
        };
        txt.LostFocus += (s, e) =>
        {
            EditPropertiesUpdated?.Invoke(this, EventArgs.Empty);
        };
        txt.Loaded += (s, e) =>
        {
            AddTextBoxMenuEvents(s as TextBox);
        };

        textBoxes[row, column] = txt;
        SetRow(txt, TID2GID(row));
        SetColumn(txt, TID2GID(column));

        //�ϲ���Ԫ��
        if (item.RowSpan * item.ColumnSpan > 1)
        {
            //����Ѵ���
            for (int r = row; r < row + item.RowSpan; r++)
            {
                for (int c = column; c < column + item.ColumnSpan; c++)
                {
                    textBoxes[r, c] = txt;
                }
            }

        }
        grd.Children.Add(txt);
        return txt;
    }

    private void FillTextBoxes(StringDataTableItem[,] data)
    {
        grd.Children.OfType<TextBox>().ToList().ForEach(p => grd.Children.Remove(p));
        int row = data.GetLength(0);
        int column = data.GetLength(1);
        textBoxes = new TextBox[row, column];
        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                //�����Ѿ��ϲ��ĵ�Ԫ����Ҫ�����
                if (textBoxes[r, c] != null)
                {
                    continue;
                }
                var item = data[r, c];
                CreateAndInsetCellTextBox(r, c, item);

            }
        }
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
                StringDataTableItem item = GetTableData(textBoxes[r, c]);
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

    #region ��ť����
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
    #endregion

}