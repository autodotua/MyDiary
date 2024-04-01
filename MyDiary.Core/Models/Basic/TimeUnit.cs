using System.ComponentModel;

namespace MyDiary.Models
{
    public enum TimeUnit
    {
        [Description("年")]
        Year,
        [Description("月")]
        Month,
        [Description("日")]
        Day,
    }
}