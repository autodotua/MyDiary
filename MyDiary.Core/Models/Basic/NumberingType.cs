using System.ComponentModel;

namespace MyDiary.Models
{
    public enum NumberingType
    {
        [Description("大纲级别标题")]
        OutlineTitle,
        [Description("项目编号")]
        ParagraphNumbering
    }
}