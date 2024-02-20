using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views.DiaryDocElement;

public abstract class DiaryTextBoxBase : TextBox, IDiaryElement
{
    public event EventHandler EditBarInfoUpdated;

    protected void RaiseEditBarInfoUpdated()
    {
        EditBarInfoUpdated?.Invoke(this, EventArgs.Empty);
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
                    return;
            }
            e.Handled = true;
            EditBarInfoUpdated?.Invoke(this, EventArgs.Empty);
        }
        base.OnKeyDown(e);
    }

    public static readonly StyledProperty<TextElementInfo> TextDataProperty =
        AvaloniaProperty.Register<DiaryTextBoxBase, TextElementInfo>(nameof(TextData));

    public TextElementInfo TextData
    {
        get => this.GetValue(TextDataProperty);
        set => SetValue(TextDataProperty, value);
    }

    public abstract EditBarInfo GetEditBarInfo(); protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TextDataProperty)
        {
            if (change.OldValue != null)
            {
                throw new InvalidOperationException($"{nameof(TextData)}只可设置一次");
            }
            var item = change.NewValue as TextElementInfo;
            BindToData(TextProperty, nameof(item.Text));
            BindToData(FontSizeProperty, nameof(item.FontSize));
            BindToData(FontWeightProperty, nameof(item.FontWeight));
            BindToData(FontStyleProperty, nameof(item.FontStyle));
            BindToData(TextAlignmentProperty, nameof(item.TextAlignment));


        }
    }
    protected void BindToData(AvaloniaProperty property, string propertyName)
    {
        this.Bind(property, new Binding
        {
            Source = TextData,
            Path = propertyName
        });
    }
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        EditBarInfoUpdated?.Invoke(this, EventArgs.Empty);
    }
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        EditBarInfoUpdated?.Invoke(this, EventArgs.Empty);
    }


}
