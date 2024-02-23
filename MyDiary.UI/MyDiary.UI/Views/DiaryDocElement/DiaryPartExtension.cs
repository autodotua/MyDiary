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

        public static IDiaryElement GetDiaryElement(this DiaryPart container)
        {
            if (container.Content is IDiaryElement d)
            {
                return d;
            }
            throw new ArgumentException($"{nameof(container.Content)}不是{nameof(IDiaryElement)}");
        }

        public static DiaryPart GetParentDiaryPart(this Control control)
        {
            if (control.Parent is DiaryPart dp)
            {
                return dp;
            }
            if (control.Parent is Control c && c.Parent is DiaryPart dp2)
            {
                return dp2;
            }
            throw new ArgumentException($"{nameof(control)}的{nameof(control.Parent)}不为{nameof(DiaryPart)}");
        }
        public static DiaryPart CreateDiaryPart(this Control control)
        {
            if (control.Parent == null)
            {
                return new DiaryPart() { Content = control };
            }
            throw new ArgumentException($"{nameof(control)}的{nameof(control.Parent)}已分配");
        }
    }
}
