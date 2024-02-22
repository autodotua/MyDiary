using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views;

public partial class EditBar : UserControl
{
    public EditBar()
    {
        InitializeComponent();
    }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if(change.Property==DataContextProperty)
        {
            if(change.OldValue is IDisposable d)
            {
                d.Dispose();
            }
        }
    }
}