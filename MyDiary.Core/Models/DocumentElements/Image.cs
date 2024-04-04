namespace MyDiary.Models
{
    public class Image : Block
    {
        public override string Type => TypeOfImage;
        public string Caption { get; set; }
        public int? DataId { get; set; }
    }
}