using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.UI.ViewModels
{
    public partial class StringDataTableItem : ViewModelBase
    {
        public StringDataTableItem() : this(null)
        {
        }

        public StringDataTableItem(string text) : this(1, 1, text)
        {
        }

        public StringDataTableItem(int rowSpan, int columnSpan, string text)
        {
            RowSpan = rowSpan >= 1 ? rowSpan : throw new ArgumentOutOfRangeException(nameof(rowSpan));
            ColumnSpan = columnSpan >= 1 ? columnSpan : throw new ArgumentOutOfRangeException(nameof(columnSpan));
            Text = text;
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VisualRowSpan))]
        private int rowSpan=1;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VisualColumnSpan))]
        private int columnSpan=1;
        [ObservableProperty]
        private string text;
        [ObservableProperty]
        private double fontSize = 14;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FontWeight))]
        private bool bold;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FontStyle))]
        private bool italic;

        public FontStyle FontStyle
        {
            get => Italic ? FontStyle.Italic : FontStyle.Normal;
            set => Italic = value == FontStyle.Italic ? true : false;
        }
        public FontWeight FontWeight
        {
            get => Bold ? FontWeight.Bold : FontWeight.Normal;
            set => Bold = value > FontWeight.Normal ? true : false;
        }

        public int VisualRowSpan => RowSpan * 2 - 1;
        public int VisualColumnSpan => ColumnSpan * 2 - 1;
    }
}
