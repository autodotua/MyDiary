using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.Core.Models
{

    public class StringDataTableItem
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

        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
        public string Text { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
    }
}
