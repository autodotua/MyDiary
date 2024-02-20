using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MyDiary.UI.ViewModels;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace MyDiary.UI.Views.DiaryDocElement;
public partial class DiaryTableCell : TextBox
{
    public DiaryTableCell()
    {
        InitializeComponent();
        //设置从TableRow/Column到Grid.Row/Column的单向绑定。
        //注意，需要设置TableRow/Column的默认值为<0，来防止设置的值与默认值相同导致不通知
        TableRowProperty.Changed.AddClassHandler<DiaryTableCell>((s, e) =>
        {
            Grid.SetRow(s, DiaryTable.TID2GID((int)e.NewValue));
        });
        TableColumnProperty.Changed.AddClassHandler<DiaryTableCell>((s, e) =>
        {
            Grid.SetColumn(s, DiaryTable.TID2GID((int)e.NewValue));
        });
    }


    public static readonly StyledProperty<int> TableRowProperty =
        AvaloniaProperty.Register<DiaryTableCell, int>(nameof(TableRow), -1);

    public int TableRow
    {
        get => this.GetValue(TableRowProperty);
        set => SetValue(TableRowProperty, value);
    }

    public static readonly StyledProperty<int> TableColumnProperty =
        AvaloniaProperty.Register<DiaryTableCell, int>(nameof(TableColumn), -1);

    public int TableColumn
    {
        get => this.GetValue(TableColumnProperty);
        set => SetValue(TableColumnProperty, value);
    }

    public static readonly StyledProperty<TableCellInfo> CellDataProperty =
        AvaloniaProperty.Register<DiaryTableCell, TableCellInfo>(nameof(CellData));

    public TableCellInfo CellData
    {
        get => this.GetValue(CellDataProperty);
        set => SetValue(CellDataProperty, value);
    }

    public void SetBold()
    {
        CellData.Bold = !CellData.Bold;
        EditBarInfoUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void SetItalic()
    {
        CellData.Italic = !CellData.Italic;
        EditBarInfoUpdated?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        EditBarInfoUpdated?.Invoke(this, EventArgs.Empty);
    }
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        EditBarInfoUpdated?.Invoke(this, EventArgs.Empty);
    }
    public event EventHandler EditBarInfoUpdated;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.B)
        {
            SetBold();
            e.Handled = true;
        }
        if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.I)
        {
            SetItalic();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CellDataProperty)
        {
            if(change.OldValue != null)
            {
                throw new InvalidOperationException($"{nameof(CellData)}只可设置一次");
            }
            var item = change.NewValue as TableCellInfo;
            BindToData(TextProperty, nameof(item.Text));
            BindToData(FontSizeProperty, nameof(item.FontSize));
            BindToData(FontWeightProperty, nameof(item.FontWeight));
            BindToData(FontStyleProperty, nameof(item.FontStyle));
            BindToData(TextAlignmentProperty, nameof(item.TextAlignment));
            BindToData(Grid.RowSpanProperty, nameof(item.VisualRowSpan));
            BindToData(Grid.ColumnSpanProperty, nameof(item.VisualColumnSpan));

            void BindToData(AvaloniaProperty property, string propertyName)
            {
                this.Bind(property, new Binding
                {
                    Source = item,
                    Path = propertyName
                });
            }
        }
    }
}