namespace MyDiary.Core.Models
{
    public class Image : Block
    {
        public override string Type => TypeOfImage;
        public string ImageName { get; set; }
        public byte[] Data { get; set; }
    }
}
