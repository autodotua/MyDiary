using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using FzLib.Avalonia.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using MyDiary.Models;
using MyDiary.UI.ViewModels;
using MyDiary.UI.Views.DiaryDocElement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

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
    public static readonly StyledProperty<NullableDate> SelectedDateProperty
        = AvaloniaProperty.Register<DiaryPad, NullableDate>(nameof(SelectedDate), default);

    private bool dbLoaded = false;
    //DocumentManager docManager = new DocumentManager();
    private DiaryPadVM viewModel = new DiaryPadVM();
    public DiaryPad()
    {
        DataContext = viewModel;
        InitializeComponent();
        viewModel.PropertyChanging += ViewModel_PropertyChanging;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    public NullableDate SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    protected override async void OnUnloaded(RoutedEventArgs e)
    {
        await SaveDocumentAsync(SelectedDate, viewModel.SelectedTag);
        base.OnUnloaded(e);
    }

    private void DiaryElement_EditPropertiesUpdated(object sender, EventArgs e)
    {
        IDiaryElement element = sender as IDiaryElement;
        editBar.DataContext = element.GetEditData();
    }
    private async void AddTagButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        string newTag = await this.ShowInputTextDialogAsync("������ǩ", "������Ҫ�����ı�ǩ��", validation: t =>
          {
              if (viewModel.Tags.Contains(t))
              {
                  throw new Exception("�Ѵ�����ͬ���Ƶı�ǩ");
              }
          });
        if (!string.IsNullOrWhiteSpace(newTag))
        {
            await App.ServiceProvider.GetRequiredService<IDataProvider>().AddTagAsync(newTag, SelectedDate.TimeUnit);
            viewModel.Tags.Add(newTag);
            viewModel.SelectedTag = newTag;
        }
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        //using TagManager tm = new TagManager();
        viewModel.Tags = new ObservableCollection<string>(await App.ServiceProvider.GetRequiredService<IDataProvider>().GetTagsAsync(TimeUnit.Day));
        viewModel.SelectedTag = viewModel.Tags[0];
        await LoadDocumentAsync(SelectedDate, viewModel.SelectedTag);
        dbLoaded = true;
    }

    #region ���غͱ�������
    protected override async void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (dbLoaded && change.Property == SelectedDateProperty)
        {
            try
            {
                dbLoaded = false;
                (var oldValue, var newValue) = change.GetOldAndNewValue<NullableDate>();

                await SaveDocumentAsync(oldValue, viewModel.SelectedTag);

                var oldTag = viewModel.SelectedTag;
                viewModel.Tags = new ObservableCollection<string>(await App.ServiceProvider.GetRequiredService<IDataProvider>().GetTagsAsync(newValue.TimeUnit));
                if (viewModel.Tags.Contains(oldTag))
                {
                    viewModel.SelectedTag = oldTag;
                }
                else
                {
                    viewModel.SelectedTag = viewModel.Tags[0];
                }
                await LoadDocumentAsync(newValue, viewModel.SelectedTag);
            }
            finally
            {
                dbLoaded = true;
            }
        }
    }

    private async Task LoadDocumentAsync(NullableDate date, string tag)
    {
        stkBody.Children.Clear();
        var cts = LoadingOverlay.ShowLoading(this, TimeSpan.FromSeconds(0.5));
        var doc = await App.ServiceProvider.GetRequiredService<IDataProvider>().GetDocumentAsync(date, tag);
        if (doc == null || doc.Blocks.Count == 0)
        {
            var txt = CreateAndAppendElement<DiaryTextBox>();
            txt.AddHandler(KeyDownEvent, TextBox_KeyDown, RoutingStrategies.Tunnel);
        }
        else
        {
            foreach (var part in doc.Blocks)
            {
                IDiaryElement element = null;
                switch (part.Type)
                {
                    case Block.TypeOfTextParagraph:
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
        viewModel.Title = doc?.Title;
        cts.Cancel();
    }

    private async Task SaveDocumentAsync(NullableDate date, string tag)
    {
        List<Block> blocks = new List<Block>();
        foreach (var element in stkBody.Children)
        {
            blocks.Add((element as DiaryPart).GetDiaryElement().GetData());
        }
        await App.ServiceProvider.GetRequiredService<IDataProvider>().SetDocumentAsync(date, tag, blocks, viewModel.Title);
    }

    private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (dbLoaded && e.PropertyName == nameof(DiaryPadVM.SelectedTag))
        {
            await LoadDocumentAsync(SelectedDate, viewModel.SelectedTag);
        }
    }

    private async void ViewModel_PropertyChanging(object sender, PropertyChangingEventArgs e)
    {
        if (dbLoaded && e.PropertyName == nameof(DiaryPadVM.SelectedTag))
        {
            await SaveDocumentAsync(SelectedDate, viewModel.SelectedTag);
        }
    }
    #endregion ���غͱ�������

    #region �ĵ���������

    public static DiaryPad GetDiaryPad(Control control)
    {
        return control.GetLogicalAncestors().OfType<DiaryPad>().FirstOrDefault()
            ?? throw new Exception($"�ṩ��{nameof(control)}��{nameof(DiaryPad)}��Ԫ��");
    }

    public T CreateAndAppendElement<T>() where T : Control, IDiaryElement, new()
    {
        return CreateAndInsertElement<T>(int.MaxValue);
    }

    public T CreateAndInsertElementBelow<T>(Control element) where T : Control, IDiaryElement, new()
    {
        int index = element == null ? -1 : stkBody.Children.IndexOf(element.GetParentDiaryPart());
        return CreateAndInsertElement<T>(index + 1);
    }
    public void RemoveElement<T>(T element) where T : Control, IDiaryElement
    {
        element.NotifyEditDataUpdated -= DiaryElement_EditPropertiesUpdated;
        stkBody.Children.Remove(element.GetParentDiaryPart());
    }

    private T CreateAndInsertElement<T>(int index) where T : Control, IDiaryElement, new()
    {
        index = Math.Min(index, stkBody.Children.Count);
        T newElement = new T();
        newElement.NotifyEditDataUpdated += DiaryElement_EditPropertiesUpdated;
        stkBody.InsertDiaryPart(index, newElement);
        return newElement;
    }
    #endregion �ĵ���������

    #region �����ı�
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

    #endregion �����ı�
}