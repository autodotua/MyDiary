﻿using System.Drawing;

namespace MyDiary.Models
{
    public class TextParagraph : TextElement
    {
        public override string Type => TypeOfTextParagraph;
        public int Level { get; set; }
    }
    public abstract class TextElement : Block
    {
        public string Text { get; set; }
        public double FontSize { get; set; } = 18;
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public int Alignment { get; set; }
        public Color TextColor { get; set; }
        public bool UseDefaultTextColor { get; set; } = true;
    }
}