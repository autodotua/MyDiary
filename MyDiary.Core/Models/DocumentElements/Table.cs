namespace MyDiary.Models
{
    public class Table : Block
    {
        public TableCell[,] Cells { get; set; }
        public string Title { get; set; }
        public override string Type => TypeOfTable;
    }
}