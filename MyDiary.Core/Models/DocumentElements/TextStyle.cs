using System.Drawing;

namespace MyDiary.Models
{
    public class TextStyle : Block
    {
        public override string Type => TypeOfTextStyle;
        public double FontSize { get; set; } = 18;
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public int Alignment { get; set; }
        public Color TextColor { get; set; }
        public bool UseDefaultTextColor { get; set; } = true;
    }
}