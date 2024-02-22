namespace MyDiary.Core.Models
{
    public class Table : DocumentPart
    {
        public TableCell[,] Cells { get; set; }
        public override string Type => TypeOfTable;
    }
}
