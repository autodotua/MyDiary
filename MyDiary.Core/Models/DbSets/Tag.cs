namespace MyDiary.Models
{
    public class Tag : ModelBase
    {
        public string Name { get; set; }
        public TimeUnit TimeUnit { get; set; } = TimeUnit.Day;
    }
}