namespace MyDiary.Core.Models
{
    public class Table : Block
    {
        public TableCell[,] Cells { get; set; }
        public string TableName { get; set; }
        public override string Type => TypeOfTable;
    }
}
