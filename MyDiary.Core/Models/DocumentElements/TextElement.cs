using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.Core.Models
{
    public class TextElement: Block
    {
        public override string Type => TypeOfTextElement;
        public string Text { get; set; }
        public double FontSize { get; set; } = 14;
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public int Alignment { get; set; }
        public Color TextColor { get; set; }
        public bool UseDefaultTextColor { get; set; }
    }
}
