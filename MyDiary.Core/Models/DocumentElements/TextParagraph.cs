namespace MyDiary.Models
{
    public class TextParagraph : TextElement
    {
        public override string Type => TypeOfTextParagraph;
        public int Level { get; set; }
    }
}