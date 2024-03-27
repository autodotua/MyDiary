using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace MyDiary.Models
{
    [Index(nameof(Level))]
    public class PresetStyle : ModelBase
    {
        public int Level { get; set; } = 0;
        public TextStyle Style { get; set; }
    }
}