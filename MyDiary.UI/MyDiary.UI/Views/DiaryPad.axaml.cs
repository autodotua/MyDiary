using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views;

public partial class DiaryPad : UserControl
{
    private bool isChangingSelectedDate = false;
    public static readonly StyledProperty<DateTime?> SelectedDateProperty
        = AvaloniaProperty.Register<DiaryPad, DateTime?>(nameof(SelectedDate), DateTime.Today);

    public DateTime? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
    private DiaryPadVM viewModel = new DiaryPadVM();
    public DiaryPad()
    {
        DataContext = viewModel;
        InitializeComponent();

        stkBody.Children[0].AddHandler(KeyDownEvent, TextBox_KeyDown, RoutingStrategies.Tunnel);
    }

    private void TextBox_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        StackPanel stkBody = this.stkBody;
        var oldTextBox = sender as TextBox;
        string text = oldTextBox.Text;
        int textBoxIndex = stkBody.Children.IndexOf(oldTextBox);
        TextBox newTextBox = null;
        switch (e.Key)
        {
            case Avalonia.Input.Key.Enter when !string.IsNullOrEmpty(text):
                newTextBox = new TextBox();
                newTextBox.KeyDown += TextBox_KeyDown;
                stkBody.Children.Insert(textBoxIndex + 1, newTextBox);
                if (oldTextBox.SelectionEnd != text.Length)
                {
                    newTextBox.Text = text[oldTextBox.SelectionEnd..];
                    oldTextBox.Text = text[..oldTextBox.SelectionStart];
                }
                newTextBox.Focus();
                newTextBox.AddHandler(KeyDownEvent, TextBox_KeyDown, RoutingStrategies.Tunnel);
                e.Handled = true;
                break;

            case Avalonia.Input.Key.Back
            when oldTextBox.CaretIndex == 0
                    && textBoxIndex > 0
                    && stkBody.Children[textBoxIndex - 1] is TextBox:
                //如果当前指针位置在最左侧，并且不是第一个段落，并且上一个段落也是TextBox
                newTextBox = stkBody.Children[textBoxIndex - 1] as TextBox;
                stkBody.Children.Remove(oldTextBox);
                if (!string.IsNullOrEmpty(text))
                {
                    newTextBox.Text = newTextBox.Text == null ? text : newTextBox.Text + text;
                }
                newTextBox.Focus();
                e.Handled = true;
                break;

            case Avalonia.Input.Key.Delete
            when oldTextBox.CaretIndex == (text?.Length ?? 0)
                    && textBoxIndex <= stkBody.Children.Count - 2
                    && stkBody.Children[textBoxIndex + 1] is TextBox:
                //如果当前指针位置在最右侧，并且不是最后一个段落，并且下一个段落也是TextBox
                newTextBox = stkBody.Children[textBoxIndex + 1] as TextBox;
                stkBody.Children.Remove(oldTextBox);
                int caretIndex = 0;
                if (!string.IsNullOrEmpty(text))
                {
                    caretIndex = text.Length;
                    newTextBox.Text = newTextBox.Text == null ? text : text + newTextBox.Text;
                }
                newTextBox.Focus();
                newTextBox.CaretIndex = caretIndex;
                e.Handled = true;
                break;

            case Avalonia.Input.Key.Left
                when oldTextBox.CaretIndex == 0
                  && textBoxIndex > 0
                  && stkBody.Children[textBoxIndex - 1] is TextBox:
                newTextBox = stkBody.Children[textBoxIndex - 1] as TextBox;
                newTextBox.Focus();
                newTextBox.CaretIndex = newTextBox.Text?.Length ?? 0;
                e.Handled = true;
                break;

            case Avalonia.Input.Key.Right
              when oldTextBox.CaretIndex == (text?.Length ?? 0)
                  && textBoxIndex <= stkBody.Children.Count - 2
                  && stkBody.Children[textBoxIndex + 1] is TextBox:
                newTextBox = stkBody.Children[textBoxIndex + 1] as TextBox;
                newTextBox.Focus();
                newTextBox.CaretIndex = 0;
                e.Handled = true;
                break;

            case Avalonia.Input.Key.Up
               when textBoxIndex > 0 && stkBody.Children[textBoxIndex - 1] is TextBox:
                newTextBox = stkBody.Children[textBoxIndex - 1] as TextBox;
                newTextBox.Focus();
                e.Handled = true;
                break;

            case Avalonia.Input.Key.Down
                when textBoxIndex <= stkBody.Children.Count - 2
                && stkBody.Children[textBoxIndex + 1] is TextBox:
                newTextBox = stkBody.Children[textBoxIndex + 1] as TextBox;
                newTextBox.Focus();
                e.Handled = true;
                break;
        }
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        stkBody.Children[0].Focus();
    }
}