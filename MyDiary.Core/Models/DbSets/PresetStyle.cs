using System.Drawing;

namespace MyDiary.Models
{
    public class PresetStyle
    {
        public double FontSize { get; set; } = 18;
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public int Alignment { get; set; }
        public Color TextColor { get; set; }
        public bool UseDefaultTextColor { get; set; } = true;
        public int Level { get; set; }
    }
}