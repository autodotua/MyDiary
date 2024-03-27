using Avalonia.Controls;
using System;

namespace MyDiary.UI.Views.DiaryDocElement
{
    public static class DiaryPadStackPanelExtension
    {
        public static DiaryPart GetChild(this StackPanel panel, int index)
        {
            if (panel.Children[index] is DiaryPart element)
            {
                return element;
            }
            throw new ArgumentException($"指定位置的元素不是{nameof(DiaryPart)}");
        }

        public static Control GetNextControl(this StackPanel panel, Control element)
        {
            int index = IndexOf(panel, element);
            return index + 1 >= panel.Children.Count ? null : (panel.Children[index + 1] as DiaryPart).GetControlContent();
        }

        public static Control GetPreviousControl(this StackPanel panel, Control element)
        {
            int index = IndexOf(panel, element);
            return index - 1 < 0 ? null : (panel.Children[index - 1] as DiaryPart).GetControlContent();
        }

        public static void InsertDiaryPart(this StackPanel panel, int index, Control control)
        {
            panel.Children.Insert(index, control.CreateDiaryPart());
        }

        public static bool IsFirstElement(this StackPanel panel, Control element)
        {
            int index = IndexOf(panel, element);
            return index == 0;
        }

        public static bool IsLastElement(this StackPanel panel, Control element)
        {
            int index = IndexOf(panel, element);
            return index == panel.Children.Count - 1;
        }

        public static int IndexOf(this StackPanel panel, Control element)
        {
            if (element.Parent is not DiaryPart part)
            {
                throw new ArgumentException($"{nameof(element)}不在{nameof(DiaryPart)}内", nameof(element));
            }
            int index = panel.Children.IndexOf(part);
            if (index < 0)
            {
                throw new ArgumentException($"{nameof(DiaryPart)}不在{nameof(panel)}内", nameof(element));
            }

            return index;
        }
    }
}