using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MyDiary.UI.ViewModels
{
    public partial class TableCellInfo : TextElementInfo
    {
        public TableCellInfo() : this(null)
        {
        }

        public TableCellInfo(string text) : this(1, 1, text)
        {
        }

        public TableCellInfo(int rowSpan, int columnSpan, string text)
        {
            RowSpan = rowSpan >= 1 ? rowSpan : throw new ArgumentOutOfRangeException(nameof(rowSpan));
            ColumnSpan = columnSpan >= 1 ? columnSpan : throw new ArgumentOutOfRangeException(nameof(columnSpan));
            Text = text;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VisualRowSpan))]
        private int rowSpan = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VisualColumnSpan))]
        private int columnSpan = 1;

        public int VisualRowSpan => RowSpan * 2 - 1;
        public int VisualColumnSpan => ColumnSpan * 2 - 1;
    }
}