using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MyDiary.Core.Models;
using MyDiary.Core.Services;
using MyDiary.UI.ViewModels;
using MyDiary.UI.Views.DiaryDocElement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace MyDiary.UI.Views;
/// <summary>
/// ��ʾ�ռ����������Container
/// </summary>
/// <remarks>
/// ������ݽ���
/// <br/>
/// һ��<see cref="IDiaryElement"/>��<see cref="EditBar"/>�����ݽ���
/// <br/>
/// 1��<see cref="IDiaryElement"/>��<see cref="EditBar"/>����
/// <br/>
/// ���ݽ���ͨ��<see cref="DiaryPad"/>��Ϊ�н������ת��
/// ���ĵ����Է����䶯ʱ��ͨ��<see cref="IDiaryElement.NotifyEditDataUpdated"/>�¼�����֪ͨ��
/// <see cref="DiaryPad"/>�����ÿ��<see cref="IDiaryElement"/>��֪ͨ��
/// ���ṩ��<see cref="EditBarVM"/>��Ϊ<see cref="EditBar"/>��DataContext
/// <br/>
/// 2��<see cref="EditBar"/>�򴫵�<see cref="IDiaryElement"/>
/// <br/>
/// ��һ���ֺܼ򵥡�<see cref="EditBar"/>�Ĳ�����ͬ�����󶨵�<see cref="EditBarVM"/>�е�<see cref="TextElementInfo"/>��
/// �������е���������ص�<see cref="DiaryTextBoxBase"/>�����˰󶨣��ܹ�ͬ����ӳ��
/// </remarks>
public partial class DiaryPad : UserControl
{
    public static readonly StyledProperty<DateTime?> SelectedDateProperty
        = AvaloniaProperty.Register<DiaryPad, DateTime?>(nameof(SelectedDate), null);

    private DiaryPadVM viewModel = new DiaryPadVM();

    public DiaryPad()
    {
        DataContext = viewModel;
        InitializeComponent();
        //var txt = CreateAndInsertElementBelow<DiaryTextBox>(null);
        //stkBody.GetChild(0).GetControlContent().AddHandler(KeyDownEvent, TextBox_KeyDown, RoutingStrategies.Tunnel);
        //#if DEBUG
        //        var table = CreateAndInsertElementBelow<DiaryTable>(txt);
        //        table.MakeEmptyTable(6, 6);
        //        var image = CreateAndInsertElementBelow<DiaryImage>(table);
        //        image.ImageSource = new Bitmap(AssetLoader.Open(new Uri("avares://MyDiary.UI/Assets/avalonia-logo.ico")));

        //#endif
    }

    public DateTime? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
    public static DiaryPad GetDiaryPad(Control control)
    {
        return control.GetLogicalAncestors().OfType<DiaryPad>().FirstOrDefault()
            ?? throw new Exception($"�ṩ��{nameof(control)}��{nameof(DiaryPad)}��Ԫ��");
    }

    public T CreateAndInsertElementBelow<T>(Control element) where T : Control, IDiaryElement, new()
    {
        int index = element == null ? -1 : stkBody.Children.IndexOf(element.GetParentDiaryPart());
        return CreateAndInsertElement<T>(index + 1);
    }
    public T CreateAndAppendElement<T>() where T : Control, IDiaryElement, new()
    {
        return CreateAndInsertElement<T>(int.MaxValue);
    }
    private T CreateAndInsertElement<T>(int index) where T : Control, IDiaryElement, new()
    {
        index = Math.Min(index, stkBody.Children.Count);
        T newElement = new T();
        newElement.NotifyEditDataUpdated += DiaryElement_EditPropertiesUpdated;
        stkBody.InsertDiaryPart(index, newElement);
        return newElement;
    }

    public void RemoveElement<T>(T element) where T : Control, IDiaryElement
    {
        element.NotifyEditDataUpdated -= DiaryElement_EditPropertiesUpdated;
        stkBody.Children.Remove(element.GetParentDiaryPart());
    }

    private void DiaryElement_EditPropertiesUpdated(object sender, EventArgs e)
    {
        IDiaryElement element = sender as IDiaryElement;
        editBar.DataContext = element.GetEditData();
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
                //�����ǰָ��λ��������࣬���Ҳ��ǵ�һ�����䣬������һ������Ҳ��TextBox
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
                //�����ǰָ��λ�������Ҳ࣬���Ҳ������һ�����䣬������һ������Ҳ��TextBox
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

    private void UserControl_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        stkBody.GetChild(0).GetControlContent().Focus();
    }

    protected override async void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == SelectedDateProperty)
        {
            (var oldValue, var newValue) = change.GetOldAndNewValue<DateTime?>();
            if (change.OldValue != null)
            {
                await SaveDocumentAsync(oldValue.Value);
            }
            stkBody.Children.Clear();
            if (change.NewValue != null)
            {
                await LoadDocumentAsync(newValue.Value);
            }
        }
    }
    DoumentManager docManager = new DoumentManager();

    private async Task SaveDocumentAsync(DateTime date)
    {
        List<Block> blocks = new List<Block>();
        foreach (var element in stkBody.Children)
        {
            blocks.Add((element as DiaryPart).GetDiaryElement().GetData());
        }
        await docManager.SetDocumentAsync(date, null, blocks);
    }
    private async Task LoadDocumentAsync(DateTime date)
    {
        var doc = await docManager.GetDocumentAsync(date, null);
        if (doc == null || doc.Count == 0)
        {
            var txt = CreateAndAppendElement<DiaryTextBox>();
            txt.AddHandler(KeyDownEvent, TextBox_KeyDown, RoutingStrategies.Tunnel);
        }
        else
        {
            foreach (var part in doc)
            {
                IDiaryElement element = null;
                switch (part.Type)
                {
                    case Block.TypeOfTextElement:
                        element = CreateAndAppendElement<DiaryTextBox>();
                        (element as DiaryTextBox).AddHandler(KeyDownEvent, TextBox_KeyDown, RoutingStrategies.Tunnel);
                        break;
                    case Block.TypeOfTable:
                        element = CreateAndAppendElement<DiaryTable>();
                        break;
                    case Block.TypeOfImage:
                        element = CreateAndAppendElement<DiaryImage>();
                        break;
                }
                element.LoadData(part);
            }
        }
    }
}