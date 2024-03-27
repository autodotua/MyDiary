namespace MyDiary.Models
{
    public class TableCell : TextElement
    {
        public override string Type => TypeOfTableCell;
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
    }
}