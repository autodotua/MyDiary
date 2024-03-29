using Avalonia;
using Avalonia.Controls;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class DiaryTableCell : DiaryTextBoxBase
{
    public DiaryTableCell()
    {
        InitializeComponent();
        //���ô�TableRow/Column��Grid.Row/Column�ĵ���󶨡�
        //ע�⣬��Ҫ����TableRow/Column��Ĭ��ֵΪ<0������ֹ���õ�ֵ��Ĭ��ֵ��ͬ���²�֪ͨ
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

    public TableCellInfo CellData
    {
        get => GetValue(TextDataProperty) as TableCellInfo;
        set => SetValue(TextDataProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextDataProperty)
        {
            BindToData(Grid.RowSpanProperty, nameof(TableCellInfo.VisualRowSpan));
            BindToData(Grid.ColumnSpanProperty, nameof(TableCellInfo.VisualColumnSpan));
        }
    }

    public override EditBarVM GetEditData()
    {
        throw new NotImplementedException();
    }
}