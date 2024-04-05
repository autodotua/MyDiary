namespace MyDiary.Models
{
    public class TableCell : TextElement
    {
        public override string Type => TypeOfTableCell;
        public int RowSpan { get; set; } = 1;
        public int ColumnSpan { get; set; } = 1;
    }
}