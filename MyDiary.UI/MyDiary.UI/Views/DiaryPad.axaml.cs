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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyDiary.UI.Views;
/// <summary>
/// 显示日记文章主体的Container
/// </summary>
/// <remarks>
/// 相关内容介绍
/// <br/>
/// 一、<see cref="IDiaryElement"/>和<see cref="EditBar"/>的数据交换
/// <br/>
/// 1、<see cref="IDiaryElement"/>向<see cref="EditBar"/>传递
/// <br/>
/// 数据交换通过<see cref="DiaryPad"/>作为中介进行中转。
/// 在文档属性发生变动时，通过<see cref="IDiaryElement.EditPropertiesUpdated"/>事件进行通知。
/// <see cref="DiaryPad"/>会接收每个<see cref="IDiaryElement"/>的通知，
/// 若发现发起通知的<see cref="IDiaryElement"/>被Focused，那么则进行处理。
/// <see cref="EditBar"/>具有<see cref="EditBar.EditProperties"/>依赖属性
/// 通过改变<see cref="EditBar.EditProperties"/>，实现通知到UI。
/// <br/>
/// 2、<see cref="EditBar"/>向传递<see cref="IDiaryElement"/>
/// <br/>
/// 这一部分很简单。在1中，<see cref="IDiaryElement"/>向<see cref="EditBar"/>发送了一个<see cref="EditBar.EditProperties"/>。
/// 通过监听<see cref="EditBar.EditProperties"/>的<see cref="INotifyPropertyChanged.PropertyChanged"/>事件，
/// 即可实现在<see cref="EditBar"/>的UI变化时，通知到<see cref="EditBar.EditProperties"/>，进而通知到<see cref="IDiaryElement"/>。
/// </remarks>
public partial class DiaryPad : UserControl
{
    public static readonly StyledProperty<DateTime?> SelectedDateProperty
        = AvaloniaProperty.Register<DiaryPad, DateTime?>(nameof(SelectedDate), DateTime.Today);

    private DiaryPadVM viewModel = new DiaryPadVM();

    public DiaryPad()
    {
        DataContext = viewModel;
        InitializeComponent();
        var txt = CreateAndInsertElementBelow<DiaryTextBox>(null);
        stkBody.GetChild(0).GetControlContent().AddHandler(KeyDownEvent, TextBox_KeyDown, RoutingStrategies.Tunnel);
#if DEBUG
        var table = CreateAndInsertElementBelow<DiaryTable>(txt);
        table.MakeEmptyTable(6, 6);
        var image = CreateAndInsertElementBelow<DiaryImage>(table);
        image.ImageSource = new Bitmap(AssetLoader.Open(new Uri("avares://MyDiary.UI/Assets/avalonia-logo.ico")));

#endif
    }

    public DateTime? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
    public static DiaryPad GetDiaryPad(Control control)
    {
        return control.GetLogicalAncestors().OfType<DiaryPad>().FirstOrDefault()
            ?? throw new Exception($"提供的{nameof(control)}非{nameof(DiaryPad)}子元素");
    }

    public T CreateAndInsertElementBelow<T>(Control element) where T : Control, IDiaryElement, new()
    {
        int index = element == null ? -1 : stkBody.Children.IndexOf(element.GetParentDiaryPart());
        T newElement = new T();
        newElement.EditPropertiesUpdated += DiaryElement_EditPropertiesUpdated;
        stkBody.InsertDiaryPart(index + 1, newElement);
        return newElement;
    }

    public void RemoveElement<T>(T element) where T : Control, IDiaryElement
    {
        element.EditPropertiesUpdated -= DiaryElement_EditPropertiesUpdated;
        stkBody.Children.Remove(element.GetParentDiaryPart());
    }

    private void DiaryElement_EditPropertiesUpdated(object sender, EventArgs e)
    {
        IDiaryElement element = sender as IDiaryElement;
        var focusedElement = TopLevel.GetTopLevel(this).FocusManager.GetFocusedElement();
        if (focusedElement is Control c && element is ILogical l)
        {
            if (c.GetLogicalAncestors().Contains(l) || c == l)
            {
                Debug.WriteLine("Updated Edit Properties");
                editBar.EditProperties = element.GetEditProperties();
            }
        }
    }

    private void TextBox_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
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
                newTextBox = CreateAndInsertElementBelow<DiaryTextBox>(oldTextBox);
                newTextBox.KeyDown += TextBox_KeyDown;
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
                RemoveElement(oldTextBox);
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
                RemoveElement(oldTextBox);
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
}