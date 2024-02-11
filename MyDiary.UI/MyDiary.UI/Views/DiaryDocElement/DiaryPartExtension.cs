using Avalonia.Controls;
using System;

namespace MyDiary.UI.Views.DiaryDocElement
{
    public static class DiaryPartExtension
    {
        public static Control GetControlContent(this DiaryPart container)
        {
            if (container.Content is Control control)
            {
                return control;
            }
            throw new ArgumentException($"{nameof(container.Content)}不是{nameof(Control)}");
        }

        public static DiaryPart AsDiaryPart(this Control control)
        {
            if (control.Parent is DiaryPart dp)
            {
                return dp;
            }
            else if (control.Parent == null)
            {
                return new DiaryPart() { Content = control };
            }
            throw new ArgumentException($"{nameof(control)}的{nameof(control.Parent)}已分配且不为{nameof(DiaryPart)}");
        }
    }
}
