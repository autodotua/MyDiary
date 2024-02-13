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

        public int RowSpan { get; }
        public int ColumnSpan { get; }
        public string Text { get; set; }
    }
}
