﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using MyDiary.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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

    public abstract EditBarInfo GetEditBarInfo();
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
            BindToData(FontWeightProperty, nameof(TextElementInfo.FontWeight));
            BindToData(FontStyleProperty, nameof(TextElementInfo.FontStyle));
            BindToData(TextAlignmentProperty, nameof(TextElementInfo.TextAlignment));
            BindToData(ForegroundProperty, nameof(TextElementInfo.Foreground));
            BindToData(BackgroundProperty, nameof(TextElementInfo.Background));
        }
    }
    protected void BindToData(AvaloniaProperty property, string propertyName)
    {
        bindings.Add(this.Bind(property, new Binding
        {
            Source = TextData,
            Path = propertyName
        }));
    }
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        EditBarInfoUpdated?.Invoke(this, EventArgs.Empty);
    }
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        //if(TopLevel.GetTopLevel(this).FocusManager.GetFocusedElement() is not DiaryTextBoxBase)
        //{
        //    this.Focus();
        //    e.Handled = true;
        //    return;
        //}
        base.OnLostFocus(e);
        EditBarInfoUpdated?.Invoke(this, EventArgs.Empty);
    }

}
