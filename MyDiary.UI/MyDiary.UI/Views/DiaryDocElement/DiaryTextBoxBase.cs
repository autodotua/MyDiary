using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using MyDiary.Managers.Services;
using MyDiary.Models;
using MyDiary.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MyDiary.UI.Views.DiaryDocElement;

public abstract class DiaryTextBoxBase : TextBox, IDiaryElement
{
    protected DiaryTextBoxBase()
    {
        FontFamily = ConfigManager.GetConfig(ConfigManager.FontFamilyKey, "等线");
    }

    public event EventHandler NotifyEditDataUpdated;

    protected void RaiseEditBarVMUpdated()
    {
        NotifyEditDataUpdated?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control)
        {
            switch (e.Key)
            {
                case Key.B:
                    TextData.Bold = !TextData.Bold;
                    break;

                case Key.I:
                    TextData.Italic = !TextData.Italic;
                    break;

                case Key.L:
                    TextData.Alignment = 0;
                    break;

                case Key.E:
                    TextData.Alignment = 1;
                    break;

                case Key.R:
                    TextData.Alignment = 2;
                    break;

                default:
                    base.OnKeyDown(e);
                    return;
            }
            NotifyEditDataUpdated?.Invoke(this, EventArgs.Empty);
        }
        base.OnKeyDown(e);
    }

    public static readonly StyledProperty<TextElementInfo> TextDataProperty =
        AvaloniaProperty.Register<DiaryTextBoxBase, TextElementInfo>(nameof(TextData));

    public TextElementInfo TextData
    {
        get => GetValue(TextDataProperty);
        set => SetValue(TextDataProperty, value);
    }

    public abstract EditBarVM GetEditData();

    private List<IDisposable> bindings = new List<IDisposable>();

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextDataProperty)
        {
            foreach (var b in bindings)
            {
                b.Dispose();
            }
            bindings.Clear();
            BindToData(TextProperty, nameof(TextElementInfo.Text));
            BindToData(FontSizeProperty, nameof(TextElementInfo.FontSize));
            BindToData(LineHeightProperty, nameof(TextElementInfo.FontSize), new MultiplyingFactorConverter(), 1.3);
            BindToData(FontWeightProperty, nameof(TextElementInfo.FontWeight));
            BindToData(FontStyleProperty, nameof(TextElementInfo.FontStyle));
            BindToData(TextAlignmentProperty, nameof(TextElementInfo.TextAlignment));
            BindToData(ForegroundProperty, nameof(TextElementInfo.Foreground));
            //BindToData(BackgroundProperty, nameof(TextElementInfo.Background));
        }
    }

    protected void BindToData(AvaloniaProperty property, string propertyName, IValueConverter converter = null, object converterParameter = null)
    {
        bindings.Add(this.Bind(property, new Binding
        {
            Source = TextData,
            Path = propertyName,
            Converter = converter,
            ConverterParameter = converterParameter,
        }));
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        NotifyEditDataUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void LoadData(Block data)
    {
        TextData = TextElementInfo.FromModel<TextElementInfo>(data as TextParagraph);
    }

    public Block GetData()
    {
        return TextData.ToModel();
    }
}

public class MultiplyingFactorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (double)value * (double)parameter;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}