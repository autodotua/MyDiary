using Microsoft.EntityFrameworkCore;

namespace MyDiary.Models
{
    [Index(nameof(Year),nameof(Month),nameof(Day),nameof(Tag))]
    public class Document : ModelBase
    {
        public IList<Block> Blocks { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public string Title { get; set; }
        public string Tag { get; set; }
    }
}
