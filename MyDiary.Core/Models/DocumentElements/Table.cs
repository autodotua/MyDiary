namespace MyDiary.Models
{
    public class Table : Block
    {
        public TableCell[,] Cells { get; set; }
        public string Caption { get; set; }
        public override string Type => TypeOfTable;
    }
}