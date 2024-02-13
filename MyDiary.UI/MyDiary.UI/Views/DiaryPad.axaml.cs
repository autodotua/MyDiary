using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MyDiary.UI.ViewModels;
using MyDiary.UI.Views.DiaryDocElement;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyDiary.UI.Views;

public partial class DiaryPad : UserControl
{
    public static readonly StyledProperty<DateTime?> SelectedDateProperty
        = AvaloniaProperty.Register<DiaryPad, DateTime?>(nameof(SelectedDate), DateTime.Today);

    private DiaryPadVM viewModel = new DiaryPadVM();

    public DiaryPad()
    {
        DataContext = viewModel;
        InitializeComponent();
        stkBody.Children.Add(new DiaryPart() { Content = new DiaryTextBox() });
        stkBody.GetChild(0).GetControlContent().AddHandler(KeyDownEvent, TextBox_KeyDown, RoutingStrategies.Tunnel);
#if DEBUG        
        var table = new DiaryTable();
        stkBody.Children.Add(new DiaryPart() { Content = table });
        table.MakeEmptyTable(6, 6);
        stkBody.Children.Add(new DiaryPart() { Content = new DiaryImage() { ImageSource = new Bitmap(AssetLoader.Open(new Uri("avares://MyDiary.UI/Assets/avalonia-logo.ico"))) } });

#endif
    }

    public DateTime? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
    private  void TextBox_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
    {
        StackPanel stkBody = this.stkBody;
        var oldTextBox = sender as DiaryTextBox;
        string text = oldTextBox.Text;
        int textBoxIndex = stkBody.IndexOf(oldTextBox);
        DiaryTextBox newTextBox = null;
        e.Handled = true;
        switch (e.Key)
        {
            case Avalonia.Input.Key.Enter when !string.IsNullOrEmpty(text):
                newTextBox = new DiaryTextBox();
                newTextBox.KeyDown += TextBox_KeyDown;
                stkBody.InsertDiaryPart(textBoxIndex + 1, newTextBox);
                if (oldTextBox.SelectionEnd != text.Length)
                {
                    newTextBox.Text = text[oldTextBox.SelectionEnd..];
                    oldTextBox.Text = text[..oldTextBox.SelectionStart];
                }
                newTextBox.AddHandler(KeyDownEvent, TextBox_KeyDown, RoutingStrategies.Tunnel);
                break;

            case Avalonia.Input.Key.Back
            when oldTextBox.CaretIndex == 0
                    && !stkBody.IsFirstElement(oldTextBox)
                    && stkBody.GetPreviousControl(oldTextBox) is DiaryTextBox t1:
                //如果当前指针位置在最左侧，并且不是第一个段落，并且上一个段落也是TextBox
                newTextBox = t1;
                stkBody.Children.Remove(oldTextBox.GetParentDiaryPart());
                if (!string.IsNullOrEmpty(text))
                {
                    newTextBox.Text = newTextBox.Text == null ? text : newTextBox.Text + text;
                }
                break;

            case Avalonia.Input.Key.Delete
            when oldTextBox.CaretIndex == (text?.Length ?? 0)
                    && !stkBody.IsLastElement(oldTextBox)
                    && stkBody.GetNextControl(oldTextBox) is DiaryTextBox t2:
                //如果当前指针位置在最右侧，并且不是最后一个段落，并且下一个段落也是TextBox
                newTextBox = t2;
                stkBody.Children.Remove(oldTextBox.GetParentDiaryPart());
                int caretIndex = 0;
                if (!string.IsNullOrEmpty(text))
                {
                    caretIndex = text.Length;
                    newTextBox.Text = newTextBox.Text == null ? text : text + newTextBox.Text;
                }
                newTextBox.CaretIndex = caretIndex;
                break;

            case Avalonia.Input.Key.Left
                when oldTextBox.CaretIndex == 0
                    && !stkBody.IsFirstElement(oldTextBox)
                    && stkBody.GetPreviousControl(oldTextBox) is DiaryTextBox t3:
                newTextBox = t3;
                newTextBox.CaretIndex = newTextBox.Text?.Length ?? 0;
                break;

            case Avalonia.Input.Key.Right
              when oldTextBox.CaretIndex == (text?.Length ?? 0)
                    && !stkBody.IsLastElement(oldTextBox)
                    && stkBody.GetNextControl(oldTextBox) is DiaryTextBox t4:
                newTextBox = t4;
                newTextBox.Focus();
                newTextBox.CaretIndex = 0;
                break;

            case Avalonia.Input.Key.Up
               when !stkBody.IsFirstElement(oldTextBox)
                    && stkBody.GetPreviousControl(oldTextBox) is DiaryTextBox t5:
                newTextBox = t5;
                break;

            case Avalonia.Input.Key.Down
                when !stkBody.IsLastElement(oldTextBox)
                    && stkBody.GetNextControl(oldTextBox) is DiaryTextBox t6:
                newTextBox = t6;
                break;

            default:
                e.Handled = false;
                break;
        }

        if (newTextBox != null)
        {
            if (newTextBox.IsLoaded)
            {
                newTextBox.Focus();
            }
            else
            {
                newTextBox.Loaded += (s, e) => (s as Control).Focus();
            }
        }
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        stkBody.GetChild(0).GetControlContent().Focus();
    }

    public T InsertElementAfter<T>(AddPartBar element) where T : Control, IDiaryElement, new()
    {
        int index = stkBody.Children.IndexOf(element.GetParentDiaryPart());
        T newElement = new T();
        stkBody.InsertDiaryPart(index + 1, newElement);
        return newElement;
    }


    public static DiaryPad GetDiaryPad(Control control)
    {
        return control.GetLogicalAncestors().OfType<DiaryPad>().FirstOrDefault()
            ?? throw new Exception($"提供的{nameof(control)}非{nameof(DiaryPad)}子元素");
    }
}